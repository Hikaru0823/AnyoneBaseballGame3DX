using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using UnityEngine.Profiling;

public class StartUpLogStamp : MonoBehaviour
{
    const string BUILD_DATE_KEY = "build_date_key";
    // アプリのバージョンが記載されているURLを記入する
    const string VersionCheckPath = "https://www.poran.net/eyemot/version/Version.txt";
    // アプリのバージョンを確認する方法
    const string VersionCheckMethod = "Date";//"Version"
    // アプリのダウンロードリンク
    const string AppDownLoadURL = "https://www.poran.net/ito/download/vibemanapp";

    //日時
    [SerializeField]
    private string buildDate_str = "";
    //ゲーム名
    [SerializeField]
    private string gameCode_str = "";
    //ビルド日自動更新するかどうか
    [SerializeField]
    bool Auto_buildDate_Update = true;
    //ビルド日のテキスト
    [SerializeField]
    private TMPro.TextMeshProUGUI buildtext;
    //バージョンが古い場合に出す注意書き
    [SerializeField]
    private Canvas warningVersion_obj;

    //一回でも起動したかどうか
    private static bool startUp_bool = false;

    // Start is called before the first frame update
    private void Start()
    {
        //ビルド日自動更新プログラム
        //var info = new System.IO.FileInfo(Application.dataPath);
        //string[] st = info.CreationTime.ToString().Split(' ');
        string[] str =  new string[3];
        if (Auto_buildDate_Update)
        {
            //buildDate_str = BayatGames.SaveGameFree.SaveGame.Load<string>(BUILD_DATE_KEY);//str[0] + str[1] + str[2];
            var sr = new System.IO.StreamReader(Application.streamingAssetsPath + "\\" + BUILD_DATE_KEY + ".txt");
            buildDate_str = sr.ReadLine();
            str[0] = buildDate_str.Substring(0, 4);
            str[1] = buildDate_str.Substring(4, 2);
            str[2] = buildDate_str.Substring(6, 2);
        }
        else
        {
            str[0] = buildDate_str.Substring(0, 4);
            str[1] = buildDate_str.Substring(4, 2);
            str[2] = buildDate_str.Substring(6, 2);
        }


        switch (str[1])
        {
            case "01":
                str[1] = "Jan";
                break;
            case "02":
                str[1] = "Feb";
                break;
            case "03":
                str[1] = "Mar";
                break;
            case "04":
                str[1] = "Apr";
                break;
            case "05":
                str[1] = "May";
                break;
            case "06":
                str[1] = "Jun";
                break;
            case "07":
                str[1] = "Jul";
                break;
            case "08":
                str[1] = "Aug";
                break;
            case "09":
                str[1] = "Sep";
                break;
            case "10":
                str[1] = "Oct";
                break;
            case "11":
                str[1] = "Nov";
                break;
            case "12":
                str[1] = "Dec";
                break;
        }
        if(buildtext != null) 
        {
            buildtext.text = "Build " + str[2] + " " + str[1] + "," + str[0];
        }


        if (!startUp_bool)
        {
            startUp_bool = true;
#if UNITY_STANDALONE && !UNITY_EDITOR
            StartCoroutine(LogStamp());
#endif
            StartCoroutine(CheckApplicationVertion());
        }
    }

    IEnumerator LogStamp()
    {
        string url = "https://www.poran.net/eyemot/";
        /*UnityWebRequest request = UnityWebRequest.Get(url);
        yield return request.Send();*/
        Dictionary<string, string> dic = new Dictionary<string, string>(); // 必要な情報 : Data,Game,Version,Width,Height,IP Adress,UserName,Host,OS,CPU,GPU,GPU_API,Memory,Memory_Usage,FPS;
        WWWForm wwwForm = new WWWForm();
        string date = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
        string gameName = gameCode_str;
        string version = buildDate_str;
        string screenWidth = Screen.width.ToString();
        string screenHeight = Screen.height.ToString();
        string UserName = Environment.UserName;
        string machineName = Environment.MachineName;
        dic.Add("Date", date);
        dic.Add("Game", gameName);
        dic.Add("Version", version);
        dic.Add("Width", screenWidth);
        dic.Add("Height", screenHeight);
        dic.Add("UserName", UserName);
        dic.Add("Host", machineName);
        dic.Add("OS", SystemInfo.operatingSystem.ToString());
        dic.Add("CPU", SystemInfo.processorType.ToString());
        dic.Add("GPU", SystemInfo.graphicsDeviceName.ToString());
        dic.Add("GPU_API", SystemInfo.graphicsDeviceType.ToString());
        dic.Add("Memory", SystemInfo.systemMemorySize + "MB");
        const uint mega = 1024 * 1024;
        dic.Add("Memory_Usage", Profiler.usedHeapSize / (float)mega + "MB");
        Resolution reso = Screen.currentResolution;
        dic.Add("FPS", reso.refreshRate.ToString());
        foreach (KeyValuePair<string, string> post in dic)
        {
            wwwForm.AddField(post.Key, post.Value);
        }
        Debug.Log("wwwForm : " + wwwForm.ToString());
        UnityWebRequest www = UnityWebRequest.Post(url, wwwForm);
        if (www.error != null)
        {
            Debug.Log("error : " + www.error);
        }
#pragma warning disable CS0618 // 型またはメンバーが古い形式です
        yield return www.Send();
#pragma warning restore CS0618 // 型またはメンバーが古い形式です
    }

    IEnumerator CheckApplicationVertion()
    {
        var cor = GetVersionData();
        yield return StartCoroutine(cor);
        var result = (string)cor.Current;

        Debug.Log("Version_Result: " + result);

        switch (VersionCheckMethod) 
        {
            case "Date":
                //ビルド日から判定する方法
                if (int.Parse(buildDate_str) >= int.Parse(result))
                {
                    //最新バージョン　"This App is the current verison."
                    Debug.Log("This App is the current verison.");
                    warningVersion_obj.enabled = (false);
                }
                else
                {
                    //古いバージョン　"This App is the old verison."
                    Debug.Log("This App is the old verison.");
                    warningVersion_obj.enabled = (true);
                }
            break;
            case "Version":
                //アプリのバージョンから判定する方法
                var new_ver = Version(result);
                var ratest_ver = Version(Application.version);

                if (new_ver[0] == ratest_ver[0] && new_ver[1] == ratest_ver[1] && new_ver[2] == ratest_ver[2])
                {
                    //バージョン一致
                    Debug.Log("This App is the current verison.");
                    warningVersion_obj.enabled = (false);
                }
                else
                {

                    for (int j = 0; j < new_ver.Length; j++)
                    {
                        if (new_ver[j] < ratest_ver[j])
                        {
                            //最新版　"This App is the current verison."
                            Debug.Log("This App is the current verison.");
                            warningVersion_obj.enabled = (false);
                            break;
                        }
                        else
                        {
                            //古いバージョン　"This App is the old verison."
                            Debug.Log("This App is the old verison.");
                            warningVersion_obj.enabled = (true);
                        }
                    }
                }
             break;
        }     
    }

    /// <summary>
    /// サーバーから、新しいバージョンを取得する
    /// </summary>
    /// <returns></returns>
    IEnumerator GetVersionData()
    {
        /*取得したいサイトURLを指定*/
        var www = UnityWebRequest.Get(VersionCheckPath);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            Debug.Log(www.error);
            yield return "Error";
        }
        else
        {
            var text = www.downloadHandler.text;
            var array1 = text.Split('\n');
            int len = array1.Length - 1;
            if (array1[len] == null)
                len = len - 1;
            var Data = new Dictionary<string, string>();
            for (int i = 0; i < len; i++)
            {
                Debug.Log("www.downloadHandler" + i + ":" + array1[i].Split(',')[0] + ":" + array1[i].Split(',')[1]);
                Data.Add(array1[i].Split(',')[0], array1[i].Split(',')[1]);
            }

            var value = Data[gameCode_str];
            Debug.Log("www.downloadHandler.text:" + value);

            // 結果をテキストとして表示します
            yield return value;

            //  または、結果をバイナリデータとして取得します
            //results = www.downloadHandler.data;
        }
    }

    /// <summary>
    /// バージョンを識別するための配列に変換する
    /// </summary>
    /// <param name="version"></param>
    /// <returns></returns>
    private int[] Version(string version)
    {
        Debug.Log(version);
        string[] ver = version.Split('.');
        var ver_int = new int[ver.Length];
        for (int i = 0; i < ver.Length; i++)
            ver_int[i] = int.Parse(ver[i]);

        return ver_int;
    }


    public void OpenApplicationDownloadURL()
    {
        Application.OpenURL(AppDownLoadURL);
    }

}
