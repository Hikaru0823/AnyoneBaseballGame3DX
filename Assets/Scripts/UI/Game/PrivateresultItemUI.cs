using System.Linq;
using TMPro;
using UnityEngine;

public class PrivateresultItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text batAveText;
    [SerializeField] private TMP_Text homeRunText;
    [SerializeField] private TMP_Text pointText;

    public void SetItem(PlayerObject player, bool isNameOnly = false)
    {
        playerNameText.text = player.Nickname;
        if(player == PlayerObject.Local)
            playerNameText.color = Color.green;
        if (isNameOnly) return;
        batAveText.text = ((float)player.HitCount/(float)player.BatterCount).ToString("F3");
        homeRunText.text = player.HRCount.ToString();
        pointText.text = (player.Score).ToString();
    }

}
