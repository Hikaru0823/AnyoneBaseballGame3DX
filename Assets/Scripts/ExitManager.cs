using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitManager : Singleton<ExitManager>
{
    [Header("Resouces")]
    public Button exitButton;

    void Start()
    {
        #if UNITY_WEBGL
        exitButton.gameObject.SetActive(false);
        #endif
    }

    public void OnReturnButtonClicked()
    {
        if (InGame)
        {
            PopupUI.OnVisible("タイトルに戻りますか？", "ゲームへの途中参加は出来ません。", onClose: () =>
            {
                ReturnToTitle();
            });
        }
        else
        {
            ReturnToTitle();
        }
    }


    public void ReturnToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }

    public void OnExitButtonClicked()
    {
#if UNITY_WEBGL
        // WebGLの場合は何もしない
        return;
#elif UNITY_EDITOR
        // エディタの場合は再生停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド版の場合はアプリケーション終了
        if (FindFirstObjectByType<NetworkRunner>())
        {
            if (InGame)
            {
                PopupUI.OnVisible("ゲームを終了しますか？", "ゲームへの途中参加は出来ません。", onClose: () =>
                {
                    Application.Quit();
                });
            }
            else
            {
                Application.Quit();
            }
        }
        else
        {
            Application.Quit();
        }
#endif
    }


    bool InGame => FindFirstObjectByType<NetworkRunner>()?.GameMode != GameMode.Single && GameManager.Instance != null && (GameManager.State.Current != GameState.EGameState.Pregame && GameManager.State.Current != GameState.EGameState.Off);
}
