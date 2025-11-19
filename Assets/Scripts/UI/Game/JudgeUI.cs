using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JudgeUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Sprite[] judgeSprites;
    [SerializeField] private TMP_Text evaScoreText;
    [SerializeField] private Image stateImage;

    public void SetJudge(GameManager.EJudge judge)
    {
        stateImage.sprite = judgeSprites[(int)judge];
        if (ResourcesManager.Instance.CurrentMode == GameManager.EMode.Evaluation)
        {
            evaScoreText.enabled = true;
            evaScoreText.text = PlayerRegistry.GetPlayer(GameManager.Instance.CurrentBatter).EvaScores[GameManager.Instance.CurrentBoxCount - 1].ToString("F1");
        }
    }
}
