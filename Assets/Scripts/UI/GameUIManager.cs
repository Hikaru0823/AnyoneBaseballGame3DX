// using System.Collections;
// using KanKikuchi.AudioManager;
// using TMPro;
// using UnityEngine;
// using UnityEngine.PlayerLoop;
// using UnityEngine.UI;
// using System.Collections.Generic;
// using System.Linq;
// using System.Runtime.CompilerServices;

// public class GameUIManager : MonoBehaviour
// {
//     public static GameUIManager Instance;
//     public Animator gameAnim;

//     [Header("ChangePlayer")]
//     public Animator changeTextAnim;
//     public Animator changePlayerAnim;
//     public Transform nextPlayerContent;
//     public Image ProgressImage;
//     public GameObject[] teamImages;
//     [Space]
//     [Header("VS")]
//     public GameObject[] VSScorePanels;
//     public TextMeshProUGUI[] scoreText;
//     public TextMeshProUGUI singelScoreText;
//     public TextMeshProUGUI singelScoreNameText;
//     public TextMeshProUGUI boxText;
//     public GameObject[] OmoteUraImage;

//     [Header("ChangeBox")]
//     public Animator boxAnim;
//     public GameObject boxchildPrefab;
//     public GameObject totalBoxChildPrefab;
//     public Transform boxContent;
//     public GameObject[] boxTeamPlatePanels;
//     public TextMeshProUGUI boxTeeamNameTex;

//     [Header("ResultTop")]
//     public Animator resultTop;

//     [Header("PrivateResult")]
//     public Animator privateResultAnim;
//     public GameObject[] privateResultPanels;
//     public Transform[] teamPrivateContent;
//     public Transform singlePrivateContent;
//     public GameObject privateResultPrefab;
//     public TextMeshProUGUI privateResultUserNameText;

//     [Header("Observer")]
//     public Animator observerUIAnim;
//     [Header("Player")]
//     public Animator playerUIAnim;
//     public RectTransform playerButonTransform;
    
//     [Header("FieldText")]
//     public Canvas[] fieldcanvases;

//     [Header("OutCount")]
//     public Animator outCountAnim;
//     public Image[] outImages;

//     private float[] evaluationScores;

//     [Header("KeyAsign")]
//     public GameObject[] keyAsignObjects;

//     [Header("DuoPitcher")]
//     public Canvas pitchTypeCanvas;
//     public Sprite[] pitchTypeImages;
//     public TextMeshProUGUI pitchTypeText;
//     public Image pitchTypeImage;

//     [Header("NetPitcher")]
//     public Animator netPitchanim;
//     public Slider netPitchSlider;

//     void Awake()
//     {
//         Instance = this;
//         VisibleFieldText(false);
//     }
    
//     public void Init()
//     {
//         InitOutCount();
//         VisibleOutCount(false);
//         VisibleObserverUI(false);
//         VisiblePlayerUI(false);
//         VisibleFieldText(false);
//         #if !UNITY_ANDROID && !UNITY_IOS
//         if (boxAnim.GetCurrentAnimatorStateInfo(0).IsName("Result"))
//             boxAnim.Play("ResultOut");
//         #endif
//         resultTop.Play("TopPanelOut");
//         privateResultAnim.Play("Panel Out");
//     }

//     public void VisibleGameUI(bool isVisible)
//     {
//         if(isVisible)
//         {
//             gameAnim.Play("Panel In");
//         }
//         else
//         {
//             gameAnim.Play("Panel Out");
//         }
//     }

//     #region "NextPlayer"
//     bool previswhite = true;
//     public IEnumerator ChangePlayer(GameObject plate, bool isWhite)
//     {
//         if (previswhite != isWhite && previswhite)
//         {
//             changeTextAnim.Play("Panel In");
//             yield return StartCoroutine(GameCameraController.Instance.ChangeRoutine(3f));
//             changeTextAnim.Play("Panel Out");
//         }
//         previswhite = isWhite;
//         changePlayerAnim.Play("Panel In");
//         var nextPlayer = Instantiate(plate, nextPlayerContent);
//         nextPlayer.transform.localPosition = Vector3.zero;
//         teamImages[0].SetActive(isWhite);
//         teamImages[1].SetActive(!isWhite);
//         var isMe = false;
//         if (plate.transform.Find("UserName").GetComponent<TextMeshProUGUI>().text == ES3.Load<string>("UserName", defaultValue: ""))
//             isMe = true;
//         yield return StartCoroutine(FlashingImage(nextPlayer.transform.Find("ReadyImage").GetComponent<Image>(), isMe));
//         Destroy(nextPlayerContent.GetChild(0).gameObject);
//         changePlayerAnim.Play("Panel Out");
//     }
//     IEnumerator FlashingImage(Image targetImage, bool isNotFlash)
//     {
//         targetImage.enabled = false;
//         for(int i = 0; i < 20; i++)
//         {
//             if(isNotFlash)
//             {
//                 targetImage.enabled = !targetImage.enabled;
//                 if(targetImage.enabled)
//                     SEManager.Instance.Play(SEPath.HIGH_LIGHT);
//             }
//             ProgressImage.fillAmount = (i + 1) / 20f;
//             yield return new WaitForSeconds(0.5f);
            
//         }
//     }
//     #endregion

//     #region "VSScore"
//     public void SetScore(bool isWhite, int point)
//     {
//         if(isWhite)
//         {
//             scoreText[0].text = (int.Parse(scoreText[0].text) + point).ToString();
//         }
//         else
//         {
//             scoreText[1].text = (int.Parse(scoreText[1].text) + point).ToString();
//         }
//     }
//     public void SetSingleScore(int point)
//     {
//         singelScoreText.text = (int.Parse(singelScoreText.text) + point).ToString();
//         scoreText[0].text = (int.Parse(scoreText[0].text) + point).ToString();
//     }
    
//     public void InitVSScore(string mode)
//     {
//         foreach(var panel in VSScorePanels)
//             panel.SetActive(panel.name == mode);
//         if(mode == "Duo")
//         {
//             scoreText[0].text = "0";
//             scoreText[1].text = "0";
//         }
//         else if(mode == "Single")
//         {
//             singelScoreText.text = "0";
//             singelScoreNameText.text = ES3.Load<string>("UserName");
//         }
//     }

//     public void UpdateBoxCount(int count, bool isWhite)
//     {
//         if (isWhite)
//             boxText.text = count + "回表";
//         else
//             boxText.text = count + "回裏";
//         if (ES3.Load<SceneController.GameMode>("GameMode", defaultValue: SceneController.GameMode.Online) != SceneController.GameMode.Online) return;
//         OmoteUraImage[0].SetActive(isWhite);
//         OmoteUraImage[1].SetActive(!isWhite);
//     }

//     #endregion

//     #region "Box"
//     public void InitBoxPanel(int totalBoxcount, string mode)
//     {
//         evaluationScores = new float[totalBoxcount];
//         foreach(var panel in boxTeamPlatePanels)
//             panel.SetActive(panel.name == mode);
//         if(mode == "Single")
//             boxTeeamNameTex.text = ES3.Load<string>("UserName");
//         if(boxContent.childCount > 0)
//         {
//             for(int i=0; i < boxContent.childCount; i++)
//                 Destroy(boxContent.GetChild(i).gameObject);
//         }

//         for(int i=0; i<totalBoxcount; i++)
//         {
//             var boxObj = Instantiate(boxchildPrefab, boxContent);
//             boxObj.transform.Find("BoxText").GetComponent<TextMeshProUGUI>().text = (i+1).ToString();
//         }
//         Instantiate(totalBoxChildPrefab, boxContent);
//     }

//     public IEnumerator SetBoxPanel(int whitePoint, int redPoint, int boxCount, bool islast = false)
//     {
//         var targetchild = boxContent.GetChild(boxCount-1).transform;
//         boxAnim.Play("Panel In");
//         yield return new WaitForSeconds(1f);
//         targetchild.Find("WhiteText").GetComponent<TextMeshProUGUI>().text = whitePoint.ToString();
//         SEManager.Instance.Play(SEPath.ADD_POINT);
//         yield return new WaitForSeconds(1f);
//         targetchild.Find("RedText").GetComponent<TextMeshProUGUI>().text = redPoint.ToString();
//         SEManager.Instance.Play(SEPath.ADD_POINT);
//         if(boxCount + 1 == boxContent.childCount)
//         {    
//             List<PlayerObject> objects = new ();
//             foreach(var player in LobbyManager.Instance.runner.ActivePlayers)
//                 if(LobbyManager.Instance.runner.TryGetPlayerObject(player, out var plObj))
//                     objects.Add(plObj.GetComponent<PlayerObject>());
//             SetPrivateScore(objects.ToArray());
//             ShowResult();
//             yield break;
//         }
//         yield return new WaitForSeconds(3f);

//         if(!islast)
//             boxAnim.Play("Panel Out");
//     }

//     public void SetBoxPanelSingle(string judge, int point, int boxCount, bool islast = false, float evaScore = 0)
//     {
//         if(islast)
//             boxAnim.Play("Panel In");
//         var targetchild = boxContent.GetChild(boxCount-1).transform;
//         targetchild.Find("WhiteText").GetComponent<TextMeshProUGUI>().text = judge;
//         if(evaScore > 0)
//         {    
//             if(boxCount-1 < evaluationScores.Length)
//             {    
//                 evaluationScores[boxCount-1] = evaScore;
//                 targetchild.Find("RedText").GetComponent<TextMeshProUGUI>().text = evaScore.ToString("F1");
//             }
//             else
//             {
//                 targetchild.Find("RedText").GetComponent<TextMeshProUGUI>().text = evaluationScores.Average().ToString("F1") + "\n±" + evaScore.ToString("F1");
//             }
//         }
//         else
//             targetchild.Find("RedText").GetComponent<TextMeshProUGUI>().text = point.ToString();
//         if(boxCount + 1 == boxContent.childCount)
//         {    
//             SetSinglePrivateScore(GameManager.Instance.param);
//             ShowResultSingle();
//             return;
//         }
//     }
//     #endregion

//     #region "Result"
//     public void ShowResult()
//     {
//         resultTop.Play("TopPanelIn");
//         SEManager.Instance.Play(SEPath.OGI_GAMESET);
//         StartCoroutine(SetBoxPanel(int.Parse(scoreText[0].text), int.Parse(scoreText[1].text), boxContent.childCount, true));
//         boxAnim.Play("Result");
//     }

//     public void ShowResultSingle()
//     {
//         resultTop.Play("TopPanelIn");
//         SEManager.Instance.Play(SEPath.OGI_GAMESET);
//         if(ES3.Load<SceneController.GameMode>("GameMode") == SceneController.GameMode.Evaluation)
//         {
//             SetBoxPanelSingle("-", int.Parse(scoreText[0].text), boxContent.childCount, true, StdDev(evaluationScores));
//         }
//         else
//             SetBoxPanelSingle("-", int.Parse(scoreText[0].text), boxContent.childCount, true);
//         boxAnim.Play("Result");
//     }

//     #endregion

//     #region  "PrivateResult"
//     public void InitPrivateResult(string mode, string name = "")
//     {
//         foreach(var panel in privateResultPanels)
//             panel.SetActive(panel.name == mode);
//         if(mode == "Single")
//             privateResultUserNameText.text = name;

//         if(teamPrivateContent[0].childCount > 0)
//         {
//             for(int i=0; i < teamPrivateContent[0].childCount; i++)
//                 Destroy(teamPrivateContent[0].GetChild(i).gameObject);
//         }
//         if(teamPrivateContent[1].childCount > 0)
//         {
//             for(int i=0; i < teamPrivateContent[1].childCount; i++)
//                 Destroy(teamPrivateContent[1].GetChild(i).gameObject);
//         }
//     }

//     public void SetSinglePrivateScore(GameManager.PlayerParam obj)
//     {
//         GameObject plate = Instantiate(privateResultPrefab, singlePrivateContent);
//         var texts = plate.transform.GetComponentsInChildren<TextMeshProUGUI>();
//         foreach(var text in texts)
//         {
//             switch(text.name)
//             {
//                 case "NameText":
//                     text.text = privateResultUserNameText.text;
//                     break;
//                 case "BatAveText":
//                     float result = (float)obj.hitCount / (float)obj.boxCount;
//                     if(obj.hitCount == obj.boxCount)
//                         text.text = "1";
//                     else
//                         text.text = result.ToString("F3").Remove(0, 1);
//                     break;
//                 case "HRCountText":
//                     text.text = obj.HRCount.ToString();
//                         break;
//                 case "PointText":
//                     text.text = obj.point.ToString();
//                     break;
//                 }
//             }
//     }

//     public void SetPrivateScore(PlayerObject[] plObj)
//     {
//         foreach(var obj in plObj)
//         {
//             GameObject plate;
//             if(obj.team == PlayerRegistry.UserTeam.White)
//                 plate = Instantiate(privateResultPrefab, teamPrivateContent[0]);
//             else if(obj.team == PlayerRegistry.UserTeam.Red)
//                 plate = Instantiate(privateResultPrefab, teamPrivateContent[1]);
//             else
//                 continue;
//             var texts = plate.transform.GetComponentsInChildren<TextMeshProUGUI>();
//             foreach(var text in texts)
//             {
//                 switch(text.name)
//                 {
//                     case "NameText":
//                         text.text = obj.userName.ToString();
//                         break;
//                     case "BatAveText":
//                         float result = (float)obj.hitCount / (float)obj.boxCount;
//                         if(obj.hitCount == obj.boxCount)
//                             text.text = "1";
//                         else
//                             text.text = result.ToString("F3").Remove(0, 1);
//                         break;
//                     case "HRCountText":
//                         text.text = obj.HRCount.ToString();
//                         break;
//                     case "PointText":
//                         text.text = obj.point.ToString();
//                         break;
//                 }
//             }
//         }
//     }

//     #endregion

//     #region Observer
//     public void VisibleObserverUI(bool isVisible)
//     {
//         if(isVisible)
//         {
//             observerUIAnim.Play("Panel In");
//         }
//         else
//         {
//             observerUIAnim.Play("Panel Out");
//         }
//     }
//     #endregion

//     #region Player
//     public void VisiblePlayerUI(bool isVisible, PlayerRegistry.PlayerState state = PlayerRegistry.PlayerState.Butter)
//     {
//         if (isVisible)
//         {
//             playerUIAnim.Play("Panel In");
//             playerButonTransform.GetComponent<PlayerButton>().SetState(state);
//         }
//         else
//         {
//             playerUIAnim.Play("Panel Out");
//         }
//     }

//     public void SetPlayerButtonTransform(Vector3 pos)
//     {
//         playerButonTransform.anchoredPosition  =pos;
//     }
//     #endregion
    
//     #region "Fieldtexts"
//     public void VisibleFieldText(bool isVisible)
//     {
//         foreach(var canvs in fieldcanvases)
//         {
//             canvs.enabled = isVisible;
//         }
//     }
    
//     public List<Vector3> GetFieldTextPositions()
//     {
//         List<Vector3> positions = new();
//         foreach(var canvs in fieldcanvases)
//         {
//             if(canvs.name == "DefenceText")
//             {
//                 positions.Add(canvs.transform.position);
//             }
//         }
//         return positions;
//     }
//     #endregion

//     #region  "OutCount"
//     public void InitOutCount()
//     {
//         foreach (var image in outImages)
//         {
//             image.color = Color.black;
//         }
//     }

//     public void VisibleOutCount(bool isVisible)
//     {
//         if(isVisible)
//         {
//             outCountAnim.Play("Panel In");
//         }
//         else
//         {
//             outCountAnim.Play("Panel Out");
//         }
//     }

//     public void SetOutCount(int outCount)
//     {
//         for(int i = 0; i < outCount; i++)
//         {
//             outImages[i].color = Color.red;
//         }
//     }
//     #endregion

//     #region "KeyAsign"
//     public void VisibleKeAsign(string[] objNamea)
//     {
//         foreach(var obj in keyAsignObjects)
//             foreach(var name in objNamea)
//                 if(obj.name == name)
//                     obj.SetActive(true);
//     }

//     #endregion

//     #region "NetPitcher"
//     bool isNetPitchActive  =false;
//     bool isRight  = true;
//     public void VisibleNetPitch(bool isVisible)
//     {
//         if(isVisible)
//         {
//             netPitchanim.Play("Panel In");
//             isNetPitchActive  = true;
//         }
//         else
//         {
//             netPitchanim.Play("Panel Out");
//             isNetPitchActive  =false;
//         }
//     }

//     void FixedUpdate()
//     {
//         if(isNetPitchActive)
//         {
//             if(isRight)
//                 netPitchSlider.value += 0.006f;
//             else
//                 netPitchSlider.value -= 0.006f;
//             if(netPitchSlider.value >= 1)
//                 isRight = false;
//             if(netPitchSlider.value <= 0)
//                 isRight  = true;
//         }
//     }

//     public GameBall.PitchMode GetNetPitchType()
//     {
//         var result = netPitchSlider.value;
//         Debug.Log(result);
//         if (result < 0.25f)
//             return GameBall.PitchMode.Curve;
//         else if (result > 0.75f)
//             return GameBall.PitchMode.Slow;
//         else
//             return GameBall.PitchMode.Straight;
//     }

//     #endregion

//     #region "DuoPitcher"
//     public void VisiblePitchType(bool isVisible)
//     {
//         pitchTypeCanvas.enabled = isVisible;
//     }

//     public void OnChangePitchType(GameBall.PitchMode mode)
//     {
//         switch(mode)
//         {
//             case GameBall.PitchMode.Straight:
//                 pitchTypeImage.sprite =pitchTypeImages[0];
//                 pitchTypeText.text  ="ストレート";
//                 break;
//             case GameBall.PitchMode.Curve:
//                 pitchTypeImage.sprite =pitchTypeImages[1];
//                 pitchTypeText.text  ="カーブ";
//                 break;
//             case GameBall.PitchMode.Slow:
//                 pitchTypeImage.sprite =pitchTypeImages[2];
//                 pitchTypeText.text  ="スロー";
//                 break;
//         }
//     }

//     #endregion

//     #region Button Event
//     public void OnObserverCameraChange(Button button)
//     {
//         GameCameraController.Instance.ChangeCamera(button.name);
//     }

//     public void OnPrivateResultButton()
//     {
//         privateResultAnim.Play("Panel In");
//         boxAnim.Play("Panel Out");
//     }

//     public void OnResultButton()
//     {
//         privateResultAnim.Play("Panel Out");
//         boxAnim.Play("Panel In");
//         boxAnim.Play("Result");
//     }
//     #endregion

//     public float StdDev(float[] data)
//     {
//         int n = data.Length;
//         if (n <= 1) return 0f;

//         float mean = data.Average();                   // 平均
//         float sumSq = data.Sum(x => (x - mean) * (x - mean)); // 偏差平方和
//         float stdev = Mathf.Sqrt(sumSq / (n - 1));  // 不偏分散 → √
//         return stdev;
//     }
// }
