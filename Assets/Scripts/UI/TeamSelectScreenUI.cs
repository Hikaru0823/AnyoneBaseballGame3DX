using System;
using System.Linq;
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TeamSelectScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject whiteBlocker;
    [SerializeField] private GameObject redBlocker;
    [SerializeField] private TMP_Text whitePlayerCountText;
    [SerializeField] private TMP_Text redPlayerCountText;
    [SerializeField] private TMP_Text spectatorCountText;
    [SerializeField] private TMP_Text whitePlayerNamesText;
    [SerializeField] private TMP_Text redPlayerNamesText;
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
        whitePlayerNamesText.text = string.Join(", ", GetPlayerNames(true));
        redPlayerNamesText.text = string.Join(", ", GetPlayerNames(false));
        whiteBlocker.SetActive(whitePlayers >= ResourcesManager.Instance.MaxPlayerCount);
        redBlocker.SetActive(redPlayers >= ResourcesManager.Instance.MaxPlayerCount);
        whiteBlocker.transform.parent.GetComponent<Button>().interactable = whitePlayers < ResourcesManager.Instance.MaxPlayerCount;
        redBlocker.transform.parent.GetComponent<Button>().interactable = redPlayers < ResourcesManager.Instance.MaxPlayerCount;
    }

    private void PlayerJoined(NetworkRunner runner, PlayerRef @ref)
    {
        int whitePlayers = runner.SessionInfo.Properties["WhitePlayers"];
        int redPlayers = runner.SessionInfo.Properties["RedPlayers"];
        int spectatorPlayers = runner.SessionInfo.Properties["SpectatorPlayers"];

        whitePlayerCountText.text = whitePlayers.ToString();
        redPlayerCountText.text = redPlayers.ToString();
        spectatorCountText.text = spectatorPlayers.ToString();
        whitePlayerNamesText.text = string.Join(", ", GetPlayerNames(true));
        redPlayerNamesText.text = string.Join(", ", GetPlayerNames(false));
        whiteBlocker.SetActive(whitePlayers >= ResourcesManager.Instance.MaxPlayerCount);
        redBlocker.SetActive(redPlayers >= ResourcesManager.Instance.MaxPlayerCount);
        whiteBlocker.transform.parent.GetComponent<Button>().interactable = whitePlayers < ResourcesManager.Instance.MaxPlayerCount;
        redBlocker.transform.parent.GetComponent<Button>().interactable = redPlayers < ResourcesManager.Instance.MaxPlayerCount;
    }

    public void OnPlayerTeamSelected(bool isWhite)
    {
        InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_OUT);
        InterfaceManager.Instance.sessionScreenAnimator.Play(ResourcesManager.PANEL_IN);
        LobbyManager.Instance.CurrentState = LobbyManager.State.InSession;
        PlayerObject.Local.Rpc_ToggleIsWhite(isWhite);
    }

    public void OnSpectatorSelected()
    {
        InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_OUT);
        InterfaceManager.Instance.sessionScreenAnimator.Play(ResourcesManager.PANEL_IN);
        InterfaceManager.Instance.sessionScreen.ReadyButton.gameObject.SetActive(false);
        LobbyManager.Instance.CurrentState = LobbyManager.State.InSession;
        PlayerObject.Local.Rpc_ToggleSpectate();
    }

    private string[] GetPlayerNames(bool isWhite)
    {
        var players = PlayerRegistry.Where(p => p.IsWhite == isWhite && p.TeamIndex != 255);
        string[] playerNames = new string[players.Count()];
        int index = 0;
        foreach (var player in players)
        {
            playerNames[index] = player.Nickname;
            index++;
        }
        return playerNames;
    }
}
