using Fusion;
using TMPro;
using UnityEngine;

public class VSPanel : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private GameObject[] OffenceDeffenceState;
    [SerializeField] private GameObject redContent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text whitePointText;
    [SerializeField] private TMP_Text redPointText;
    [SerializeField] private TMP_Text gameCountText;

    public void SetGameCount(int count)
    {
        Debug.Log($"SetGameCount: {count}");
        bool isWhite;
        string state;
        if (!GameManager.IsOnline)
        {
            isWhite = true;
            state = "回表";
            gameCountText.text = count.ToString() + state;
            return;
        }
        else
        {
            isWhite = count % 2 == 1;
            state = isWhite ? "回表" : "回裏";
            gameCountText.text = ((count + 1) / 2).ToString() + state;
        }
        OffenceDeffenceState[0].SetActive(isWhite);
        OffenceDeffenceState[1].SetActive(!isWhite);
    }

    public void SetSingleState(string name)
    {
        foreach (var item in OffenceDeffenceState)
            item.SetActive(false);
        redContent.SetActive(false);
        nameText.text = name;
    }

    public void SetPoints(int whitePoints, int redPoints)
    {
        whitePointText.text = whitePoints.ToString();
        redPointText.text = redPoints.ToString();
    }
}
