using System.Collections.Generic;
using System.Linq;
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

    [SerializeField] private Transform itemHolderW;
    [SerializeField] private Transform itemHolderR;
    [SerializeField] private PrivateresultItemUI itemPrefab;
    private Dictionary<PlayerRef, PrivateresultItemUI> itemDict = new Dictionary<PlayerRef, PrivateresultItemUI>();

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

    public void SetItems()
    {
        foreach (var item in itemDict.Values)
        {
            Destroy(item.gameObject);
        }
        itemDict.Clear();
        var whitePlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => p.IsWhite).ToArray();
        foreach (var player in whitePlayers)
        {
            PrivateresultItemUI item = Instantiate(itemPrefab, itemHolderW);
            item.SetItem(player, true);
            itemDict.Add(player.Ref, item);
        }

        var redPlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => !p.IsWhite).ToArray();
        foreach (var player in redPlayers)
        {
            PrivateresultItemUI item = Instantiate(itemPrefab, itemHolderR);
            item.SetItem(player, true);
            itemDict.Add(player.Ref, item);
        }
    }

    public void OnPlayerLeft(PlayerRef player)
    {
        if (itemDict.TryGetValue(player, out PrivateresultItemUI item))
        {
            Destroy(item.gameObject);
            itemDict.Remove(player);
        }
        else
        {
            Debug.LogWarning($"{player} not found in VSPanel");
        }
    }

    [SerializeField] private GameObject whitePlayerList;
    [SerializeField] private GameObject redPlayerList;
    bool isVisible_W = false;
    bool isVisible_R = false;

    public void OnVisible(bool isWhite)
    {
        if (isWhite)
        {
            isVisible_W = !isVisible_W;
            whitePlayerList.SetActive(isVisible_W);
        }
        else
        {
            isVisible_R = !isVisible_R;
            redPlayerList.SetActive(isVisible_R);
        }
    }
}
