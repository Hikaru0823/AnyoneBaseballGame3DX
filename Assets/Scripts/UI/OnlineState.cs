using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

public class OnlineState : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField][Tooltip("0: Session, 1: Lobby")] private TextMeshProUGUI[] texts;
    [SerializeField][Tooltip("0: Normal, 1: BarrierFree")] private Sprite[] backGrounds;
    [SerializeField][Tooltip("0: Normal, 1: BarrierFree")] private Sprite[] icons;
    [SerializeField] private Image backGroundImage;
    [SerializeField] private Image iconImage;

    private string setSession = "SetSession";
    private string setLobby = "SetLobby";

    public void SetSessionPanel(int num)
    {
        iconImage.sprite = icons[num];
        backGroundImage.sprite = backGrounds[num];
        texts[0].text = "ルーム" + (num + 1).ToString();
        animator.Play(setSession);
    }

    public void SetLobbypanel()
    {
        if(animator != null)
            animator.Play(setLobby);
    }

    public void SetLobbyTotalCount(int num)
    {
        texts[1].text = num.ToString();
    }
}
