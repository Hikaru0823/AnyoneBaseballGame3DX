using Fusion;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Fusion.Sockets;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System.Linq;
using ExitGames.Client.Photon.StructWrapping;
using KanKikuchi.AudioManager;
using EyeMoT.Fusion;

public class LobbyManager : SimulationBehaviour, INetworkRunnerCallbacks
{
    public static LobbyManager Instance;
    public enum State{
        RoomSelect,
        TeamSelect,
        InSession,
        InGame,
    }
    public State CurrentState { get; set; }
    [SerializeField] NetworkRunner runnerPrefab;
	[SerializeField] NetworkObject managerPrefab;
    [SerializeField] private SessionListView _sessionListView;
    [SerializeField] public SessionHolder SessionHolder;


    public new NetworkRunner  Runner { get; private set; }

    async void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        SessionHolder.init();
    }

    void Start()
    {
        if(ResourcesManager.Instance.CurrentMode != GameManager.EMode.Online_BarrierFree && ResourcesManager.Instance.CurrentMode != GameManager.EMode.Online_Universal)
        {
            CurrentState = State.InGame;
            StartCoroutine(SingleSessionRoutine());
        }
        else
            TryJoinLobby();
    }
    
    IEnumerator SingleSessionRoutine(string sessionCode = null, System.Action successCallback = null)
	{
		if (!Runner)
		{
			Runner = Instantiate(runnerPrefab);
		}

		Runner.GetComponent<NetworkEvents>().PlayerJoined.AddListener((runner, player) =>
			{
				if (runner.IsServer && runner.LocalPlayer == player)
				{
					runner.Spawn(managerPrefab);
				}
			});
		Runner.AddCallbacks(this);

		Task<StartGameResult> task = Runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Single,
			SessionName = "1",
			SceneManager = Runner.GetComponent<INetworkSceneManager>(),
		});
		
		while (!task.IsCompleted)
		{
			yield return null;
		}
		StartGameResult result = task.Result;
		if (result.Ok)
		{
			//Debug.Log($"Connected to Session {code} by Host with custom properties.");
			//Runner.SessionInfo.IsVisible = !_private;
			//Debug.Log($"Private bool is {_private}. Session is {(Runner.SessionInfo.IsVisible ? "public" : "private")}");
			if (successCallback != null)
				successCallback.Invoke();
		}
		else
		{
			DisconnectUI.OnShutdown(result.ShutdownReason);
		}
	}
    
    public void TryJoinLobby()
    {
        BGMManager.Instance.Play(BGMPath.READY);
        InterfaceManager.Instance.topAnimator.Play(ResourcesManager.PANEL_IN);
        InterfaceManager.Instance.lobbyAnimator.Play(ResourcesManager.PANEL_IN);
        StartCoroutine(JoinLobbyRoutine());
    }

    IEnumerator JoinLobbyRoutine()
    {
        Loading.Instance.SetVisible(true);
        //onTryJoinLobby?.Invoke();
        Runner = Instantiate(runnerPrefab);
        Runner.AddCallbacks(this);
        Task<StartGameResult> task = Runner.JoinSessionLobby(SessionLobby.ClientServer);
        while (!task.IsCompleted)
        {
            yield return null;
        }
        StartGameResult result = task.Result;
        Loading.Instance.SetVisible(false);
        if (result.Ok)
        {
            Debug.Log("Connected to lobby.");
            CurrentState = State.RoomSelect;
            InterfaceManager.Instance.lobbyAnimator.Play(ResourcesManager.PANEL_IN);
        }
        else
        {
            DisconnectUI.OnShutdown(result.ShutdownReason);
        }
    }

    //UI hook
    public void CreateHostSession()
    {
        SessionDef.Name? availableSession = GetAvailableSessionName();
        if (!availableSession.HasValue)
        {
            PopupUI.OnVisible("空きルームがありません", "利用可能なルーム名がすべて使用中です。");
            return;
        }

        SessionHolder.TryGet(availableSession.Value, out SessionData data);
        var sessionCode = SessionCodeUtility.BuildSessionCode(data.Name);
        var description = $"ホストとしてルーム<color=green>「{data.UIName}」</color>を作成します。\nホストがゲーム終了するとルームも閉じます。";
        PopupUI.OnVisible($"ルームを作成しますか？", description, data.Sprite, onClose: () =>
        {
            TryHostSession(sessionCode);
        });
    }

    public void TryHostSession(string sessionCode = null, System.Action successCallback = null)
    {
        StartCoroutine(HostSessionRoutine(sessionCode, successCallback));
    }

	IEnumerator HostSessionRoutine(string sessionCode = null, System.Action successCallback = null)
	{
		if (!Runner)
		{
			Runner = Instantiate(runnerPrefab);
		}

		Runner.GetComponent<NetworkEvents>().PlayerJoined.AddListener((runner, player) =>
			{
				if (runner.IsServer && runner.LocalPlayer == player)
				{
					runner.Spawn(managerPrefab);
				}
			});
		Runner.AddCallbacks(this);

		string code = sessionCode;

		// カスタムセッションプロパティの設定
		Dictionary<string, SessionProperty> customProperties = new Dictionary<string, SessionProperty>
		{
			// 例: 最大プレイヤー数を設定
			{ "MaxPlayers", 20 },
            { "WhitePlayers", 0 },
            { "RedPlayers", 0 },
			{ "SpectatorPlayers", 0 },
		};

		Loading.Instance.SetVisible(true);
		Task<StartGameResult> task = Runner.StartGame(new StartGameArgs()
		{
			GameMode = GameMode.Host,
			SessionName = code,
			SceneManager = Runner.GetComponent<INetworkSceneManager>(),
			PlayerCount = 20,
			SessionProperties = customProperties  // カスタムプロパティを設定
		});
		
		while (!task.IsCompleted)
		{
			yield return null;
		}
		StartGameResult result = task.Result;
		Loading.Instance.SetVisible(false);
		if (result.Ok)
		{
			Debug.Log($"Connected to Session {code} by Host with custom properties.");
			InterfaceManager.Instance.lobbyAnimator.Play(ResourcesManager.PANEL_OUT);
            InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_IN);
			CurrentState = State.TeamSelect;
			if (successCallback != null)
                successCallback.Invoke();
		}
		else
		{
			DisconnectUI.OnShutdown(result.ShutdownReason);
		}
	}

    public void TryJoinSession(string sessionCode, System.Action successCallback = null)
    {
        StartCoroutine(JoinSessionRoutine(sessionCode, successCallback));
    }

    IEnumerator JoinSessionRoutine(string sessionCode, System.Action successCallback)
    {
        if (Runner) Runner.Shutdown();
        Runner = Instantiate(runnerPrefab);
        Runner.AddCallbacks(this);

        Loading.Instance.SetVisible(true);
        Task<StartGameResult> task = Runner.StartGame(new StartGameArgs()
        {
            GameMode = GameMode.AutoHostOrClient,
            SessionName = sessionCode,
            SceneManager = Runner.GetComponent<INetworkSceneManager>(),
        });
        while (!task.IsCompleted)
        {
            yield return null;
        }
        StartGameResult result = task.Result;

        Loading.Instance.SetVisible(false);
        if (result.Ok)
        {
            InterfaceManager.Instance.lobbyAnimator.Play(ResourcesManager.PANEL_OUT);
            InterfaceManager.Instance.teamSelectAnimator.Play(ResourcesManager.PANEL_IN);
            CurrentState = State.TeamSelect;
            if (successCallback != null)
                successCallback.Invoke();
        }
        else
        {
            DisconnectUI.OnShutdown(result.ShutdownReason);
        }
    }

    //ロビー内セッションの人数処理
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
        _sessionListView.UpdateSessions(sessionList);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log($"LobbyManager.OnShutdown: {shutdownReason}");
        Runner = null;
        DisconnectUI.OnShutdown(shutdownReason);
    }

    SessionDef.Name? GetAvailableSessionName()
    {
        return _sessionListView != null ? _sessionListView.GetAvailableSessionName() : null;
    }

    #region NetworkCallBacks
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    { }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    { }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    { 
        Debug.Log($"LobbyManager.OnDisconnectedFromServer: {reason}");
        DisconnectUI.OnDisconnectedFromServer(reason);
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    { }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    { 
        Debug.Log($"LobbyManager.OnDisconnectedFromServer: {reason}");
        DisconnectUI.OnConnectFailed(reason);
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    { }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    { }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    { }

    public void OnConnectedToServer(NetworkRunner runner)
    { }


    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    { }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    { }

    public void OnSceneLoadDone(NetworkRunner runner)
    { }

    public void OnSceneLoadStart(NetworkRunner runner)
    { }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player Left: {player.IsMasterClient}");
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    #endregion
}

[System.Serializable]
public class FixedSessionInfo
{
	public string Name;
	public int MaxPlayers;
}
