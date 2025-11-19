using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ReadyButton : MonoBehaviour
{
    [Header("Resources")]
    public CanvasGroup canvasGroup;
    public Image buttonBackGround;
    public Image buttonAImage;
    public TextMeshProUGUI text;

    private bool isClicked = false;

    public void OnButtonClicked()
    {
        if (isClicked)
        {
            buttonBackGround.color = Color.white;
            buttonAImage.color = Color.white;
            text.text = "準備\n完了";
        }
        else
        {
            buttonBackGround.color = Color.grey;
            buttonAImage.color = Color.grey;
            text.text = "キャンセル";
        }
        isClicked = !isClicked;
        PlayerObject.Local.Rpc_SetReadyState(isClicked);
    }

    public void InitVisual()
    {
        isClicked  =false;
        buttonBackGround.color = Color.white;
        buttonAImage.color = Color.white;
        text.text = "準備\n完了";
    }

    public void IsVisual(bool isVisual)
    {
        if(isVisual)
        {    
            canvasGroup.alpha = 1;
            canvasGroup.blocksRaycasts = true;
        }
        else
        {    
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
