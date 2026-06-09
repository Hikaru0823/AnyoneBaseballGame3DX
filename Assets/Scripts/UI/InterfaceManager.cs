using System.Collections;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance { get; private set; }
    public SessionScreenUI sessionScreen;
    public TeamSelectScreenUI teamSelectScreen;
    public Animator topAnimator;
    public Animator lobbyAnimator;
    public Animator teamSelectAnimator;
    public Animator sessionScreenAnimator;
    public Animator gameAnimator;
    public Animator resultTopAnimator;

    [Header("Game UI")]
    public VSPanel vsPanel;
    public OutCountUI outCountUI;
    public SpectatorUI spectatorUI;
    public PlayerUI playerUI;
    public JudgeUI judgeUI;
    public ChangeBatterUI changeBatterUI;
    public Animator changeOffenceDefenceAnimator;
    public IntervalUI intervalUI;
    public PrivateResultUI privateResultUI;
    public CurrentBatterUI currentBatterUI;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public IEnumerator StartIntroViewRoutine()
    {
        yield return GameCameraController.Instance.IntroRoutine();
        if (GameManager.Instance.Runner.IsServer)
        {
            GameManager.State.Server_SetState(GameState.EGameState.BatterChange);
        }
    }

    public static void ShowPanel(Animator targetPanel)
    {
        if (!targetPanel.GetCurrentAnimatorStateInfo(0).IsName(ResourcesManager.PANEL_IN))
        {
            targetPanel.Play(ResourcesManager.PANEL_IN);
        }
    }

    public static void HidePanel(Animator targetPanel)
    {
        if (targetPanel.GetCurrentAnimatorStateInfo(0).IsName(ResourcesManager.PANEL_IN))
        {
            targetPanel.Play(ResourcesManager.PANEL_OUT);
        }
    }

    public void OnReturnButtonClicked()
    {
        switch(LobbyManager.Instance.CurrentState)
        {
            case LobbyManager.State.RoomSelect:
                ExitManager.Instance.OnReturnButtonClicked();
                break;
            case LobbyManager.State.TeamSelect:
                ExitManager.Instance.OnReturnButtonClicked();
                break;
            case LobbyManager.State.InSession:
                PlayerObject.Local.Rpc_TReTeamSelect();
                teamSelectAnimator.Play(ResourcesManager.PANEL_IN);
                sessionScreenAnimator.Play(ResourcesManager.PANEL_OUT);
                LobbyManager.Instance.CurrentState = LobbyManager.State.TeamSelect;
                break;
            case LobbyManager.State.InGame:
                ExitManager.Instance.OnReturnButtonClicked();
                break;
        }
    }
}
