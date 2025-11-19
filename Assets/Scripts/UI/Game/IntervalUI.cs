using System.Collections;
using Fusion;
using KanKikuchi.AudioManager;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntervalUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private GameObject scoreItemPrefab;
    [SerializeField] private GameObject totalScoreItemPrefab;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject[] modeStatePlates;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Image gage;

    private GameObject[] items;
    private GameObject totalItem;

    public void InitBoxPanel(int totalBoxcount)
    {
        Debug.Log($"InitBoxPanel: {totalBoxcount}");
        items = new GameObject[totalBoxcount];
        for (int i = 0; i < totalBoxcount; i++)
        {
            var boxObj = Instantiate(scoreItemPrefab, content);
            boxObj.transform.Find("BoxText").GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            items[i] = boxObj;
        }
        totalItem = Instantiate(totalScoreItemPrefab, content);
    }

    public void SetName(string name)
    {
        modeStatePlates[0].SetActive(true);
        modeStatePlates[1].SetActive(false);
        nameText.text = name;
    }

    public IEnumerator SetCurrentPointsRoutine(int whitePoint, int redPoint, int currentBoxCount)
    {
        Transform targetChild = items[(currentBoxCount / 2) - 1].transform;
        yield return new WaitForSeconds(1f);
        targetChild.Find("WhiteText").GetComponent<TextMeshProUGUI>().text = whitePoint.ToString();
        SEManager.Instance.Play(SEPath.ADD_POINT);
        yield return new WaitForSeconds(1f);
        targetChild.Find("RedText").GetComponent<TextMeshProUGUI>().text = redPoint.ToString();
        SEManager.Instance.Play(SEPath.ADD_POINT);
    }

    public IEnumerator SetTotalPointsRoutine(int whitePoint, int redPoint)
    {
        Transform targetChild = totalItem.transform;
        yield return new WaitForSeconds(1f);
        targetChild.Find("WhiteText").GetComponent<TextMeshProUGUI>().text = whitePoint.ToString();
        SEManager.Instance.Play(SEPath.ADD_POINT);
        yield return new WaitForSeconds(1f);
        targetChild.Find("RedText").GetComponent<TextMeshProUGUI>().text = redPoint.ToString();
        SEManager.Instance.Play(SEPath.ADD_POINT);
        yield return new WaitForSeconds(1f);
        animator.Play("Result");
    }

    public IEnumerator SetTotalPointsRoutineSingle(int whitePoint)
    {
        SetPointsAll();
        Transform targetChild = totalItem.transform;
        targetChild.Find("WhiteText").GetComponent<TextMeshProUGUI>().text = "-";
        yield return new WaitForSeconds(1f);
        targetChild.Find("RedText").GetComponent<TextMeshProUGUI>().text = whitePoint.ToString();
        SEManager.Instance.Play(SEPath.ADD_POINT);
        yield return new WaitForSeconds(1f);
        animator.Play("Result");
        if(ES3.Load<bool>(SaveKeys.IsINF, false))
        {
            for (int i = 0; i < 5; i++)
            {
                yield return new WaitForSeconds(1f);
                gage.fillAmount += 0.2f;
            }
            GameManager.Instance.Runner.Shutdown();
            SceneManager.LoadScene(1);
        }
    }

    public void SetPointsAll()
    {
        for (int i = 0; i < items.Length; i++)
        {
            float points = 0;
            string judgeText = "";
            var text = items[i].transform.Find("WhiteText").GetComponent<TextMeshProUGUI>();
            switch (PlayerObject.Local.Judges[i])
            {
                case GameManager.EJudge.HomeRun:
                    points = 4;
                    judgeText = "HR";
                    text.color = Color.red;
                    break;
                case GameManager.EJudge.BH3:
                    points = 3;
                    judgeText = "3BH";
                    text.color = Color.blue;
                    break;
                case GameManager.EJudge.BH2:
                    points = 2;
                    judgeText = "2BH";
                    text.color = new Color(1f, 0.5f, 0f); // orange
                    break;
                case GameManager.EJudge.Hit:
                    points = 1;
                    judgeText = "HIT";
                    text.color = Color.yellow;
                    break;
                case GameManager.EJudge.Strike:
                    points = 0;
                    judgeText = "S";
                    text.color = Color.yellow;
                    break;
                case GameManager.EJudge.Faul:
                    points = 0;
                    judgeText = "F";
                    text.color = Color.gray;
                    break;
                case GameManager.EJudge.Out:
                    points = 0;
                    judgeText = "OUT";
                    text.color = Color.gray;
                    break;
                case GameManager.EJudge.Ball:
                    points = 0;
                    judgeText = "B";
                    text.color = Color.green;
                    break;
            }
            if (ResourcesManager.Instance.CurrentMode == GameManager.EMode.Evaluation)
            {
                points = PlayerRegistry.GetPlayer(GameManager.Instance.CurrentBatter).EvaScores[i];
            }
            items[i].transform.Find("RedText").GetComponent<TextMeshProUGUI>().text = points.ToString("F1");
            text.text = judgeText;
        }
    }

    public void OnButtonClicked()
    {
        animator.Play(ResourcesManager.PANEL_OUT);
        if (InterfaceManager.Instance != null)
        {
            InterfaceManager.Instance.privateResultUI.animator.Play(ResourcesManager.PANEL_IN);
        }
    }

}