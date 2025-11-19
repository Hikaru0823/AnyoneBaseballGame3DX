using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private PlayerButton playerButton;

    private readonly Dictionary<GameManager.EMode, Vector3> BatterUIPosByMode = new Dictionary<GameManager.EMode, Vector3>()
    {
        { GameManager.EMode.BarrierFree, new Vector3(0, 280, 0) },
        { GameManager.EMode.Normal, new Vector3(-420, -200, 0) },
        { GameManager.EMode.Duo, new Vector3(0, -3.25f, 10.3f) },
        { GameManager.EMode.Online_BarrierFree, new Vector3(0, 280, 0) },
        { GameManager.EMode.Evaluation, new Vector3(0, 150, 0) },
        { GameManager.EMode.Derby,new Vector3(0, -10000, 0) }
    };

    private void ChangePlayerButtonPosition(PlayerObject.EPlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerObject.EPlayerState.Batter:
                InterfaceManager.ShowPanel(animator);
                playerButton.transform.localPosition = BatterUIPosByMode[ResourcesManager.Instance.CurrentMode];
                break;
            case PlayerObject.EPlayerState.Defence:
                InterfaceManager.ShowPanel(animator);
                playerButton.transform.localPosition = new Vector3(0, -200, 0); // Example position for Defence
                break;
            case PlayerObject.EPlayerState.None:
                InterfaceManager.ShowPanel(animator);
                break;
        }
    }

    public void SetButtonState(PlayerObject.EPlayerState playerState)
    {
        playerButton.SetState(playerState);
        ChangePlayerButtonPosition(playerState);
    }
}
