using System;
using Fusion;
using TMPro;
using UnityEngine;

public class TeamSelectScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text whitePlayerCountText;
    [SerializeField] private TMP_Text redPlayerCountText;
    [SerializeField] private TMP_Text spectatorCountText;
    public void AddSubscriptions()
    {
        // イベント購読の重複を防ぐため、一度解除してから購読
        PlayerRegistry.OnPlayerJoined -= PlayerJoined;
        PlayerRegistry.OnPlayerLeft -= PlayerLeft;
        PlayerRegistry.OnPlayerJoined += PlayerJoined;
        PlayerRegistry.OnPlayerLeft += PlayerLeft;
    }

    private void PlayerLeft(NetworkRunner runner, PlayerRef @ref)
    {
        int whitePlayers = runner.SessionInfo.Properties["WhitePlayers"];
        int redPlayers = runner.SessionInfo.Properties["RedPlayers"];
        int spectatorPlayers = runner.SessionInfo.Properties["SpectatorPlayers"];

        whitePlayerCountText.text = whitePlayers.ToString();
        redPlayerCountText.text = redPlayers.ToString();
        spectatorCountText.text = spectatorPlayers.ToString();
    }

    private void PlayerJoined(NetworkRunner runner, PlayerRef @ref)
    {
        int whitePlayers = runner.SessionInfo.Properties["WhitePlayers"];
        int redPlayers = runner.SessionInfo.Properties["RedPlayers"];
        int spectatorPlayers = runner.SessionInfo.Properties["SpectatorPlayers"];

        whitePlayerCountText.text = whitePlayers.ToString();
        redPlayerCountText.text = redPlayers.ToString();
        spectatorCountText.text = spectatorPlayers.ToString();
    }

    public void OnPlayerTeamSelected(bool isWhite)
    {
        InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_OUT);
        InterfaceManager.Instance.sessionScreenAnimator.Play(ResourcesManager.PANEL_IN);
        PlayerObject.Local.Rpc_ToggleIsWhite(isWhite);
    }

    public void OnSpectatorSelected()
    {
        InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_OUT);
        InterfaceManager.Instance.sessionScreenAnimator.Play(ResourcesManager.PANEL_IN);
        InterfaceManager.Instance.sessionScreen.ReadyButton.gameObject.SetActive(false);
        PlayerObject.Local.Rpc_ToggleSpectate();
    }
}
