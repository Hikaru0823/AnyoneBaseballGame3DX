using System.Linq;
using UnityEngine;

public class PrivateResultUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Transform itemHolderW;
    [SerializeField] private Transform itemHolderR;
    [SerializeField] private PrivateresultItemUI itemPrefab;

    public void SetItem()
    {
        var whitePlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => p.IsWhite).ToArray();
        foreach (var player in whitePlayers)
        {
            PrivateresultItemUI item = Instantiate(itemPrefab, itemHolderW);
            item.SetItem(player);
        }

        var redPlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => !p.IsWhite).ToArray();
        foreach (var player in redPlayers)
        {
            PrivateresultItemUI item = Instantiate(itemPrefab, itemHolderR);
            item.SetItem(player);
        }
        //animator.Play(ResourcesManager.PANEL_IN);
    }

    public void OnButtonClicked()
    {
        animator.Play(ResourcesManager.PANEL_OUT);
        InterfaceManager.Instance.intervalUI.animator.Play("Result");
    }
}
