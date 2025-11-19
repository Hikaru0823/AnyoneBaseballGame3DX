using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using Fusion.Addons.Physics;
using Fusion.Sockets;
using KanKikuchi.AudioManager;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

/// <summary>
/// ネットワーク対戦野球ゲームのメインマネージャークラス
/// ゲーム進行・プレイヤー管理・UI制御などを担当
/// </summary>
public class GameManager : NetworkBehaviour, INetworkRunnerCallbacks
{
    public enum EMode {BarrierFree, Normal, Duo, Online_BarrierFree, Evaluation, Derby}
    public enum EJudge { HomeRun, BH3, BH2, Hit, Faul, Out, Strike, Ball, PlayerLeft, None }
    public static GameState State => Instance._gameState;
	[SerializeField] private GameState _gameState;
    public static GameManager Instance; // シングルトンインスタンス

    [Networked]
    public PlayerRef CurrentBatter { get; set; }
    [Networked]
    public bool IsTopInning { get; set; } = true; // trueなら表、falseなら裏 ちな、これは野球用語の表裏ね
    [Networked, OnChangedRender(nameof(OnChangeOffenceDefence))]
    public int CurrentBoxCount { get; set; } = 1;
    [Networked, OnChangedRender(nameof(OnUpdateOutCount))]
    public int CurrentOutCount { get; set; } = 0;
    [Networked]
    public int CurrentWhitePoints { get; set; } = 0;
    [Networked]
    public int CurrentRedPoints { get; set; } = 0;
    [Networked, OnChangedRender(nameof(OnUpdateScore))]
    public int TotalWhitePoints { get; set; } = 0;
    [Networked, OnChangedRender(nameof(OnUpdateScore))]
    public int TotalRedPoints { get; set; } = 0;
    [Networked]
    public int CurrentWhiteBatterCount { get; set; } = -1;
    [Networked]
    public int CurrentRedBatterCount { get; set; } = -1;

    public static bool IsOnline => Instance.Runner != null && Instance.Runner.GameMode != GameMode.Single;

    public Dictionary<PlayerRef, Texture2D> PlayerCustomImages { get; private set; } = new();
    public Dictionary<PlayerRef, NetworkObject> CurrentDefenceAreas { get; set; } = new();
    private readonly Dictionary<EMode, Vector3> BallPosByMode = new Dictionary<EMode, Vector3>()
    {
        { EMode.BarrierFree, new Vector3(0, -3.25f, 10.3f) },
        { EMode.Normal, new Vector3(0, -3.25f, 7f) },
        { EMode.Duo, new Vector3(0, -3.25f, 10.3f) },
        { EMode.Online_BarrierFree, new Vector3(0, -3.25f, 10.3f) },
        { EMode.Evaluation, new Vector3(0, -3.25f, 10.3f) },
        { EMode.Derby, new Vector3(0, -3.25f, 7f) }
    };

    public NetBall netBall;

    public override void Spawned()
    {
        Instance = this;
        Runner.AddCallbacks(this);
        InterfaceManager.Instance.sessionScreen.AddSubscriptions();
        InterfaceManager.Instance.teamSelectScreen.AddSubscriptions();
    }

    public void ForwardGameProgress()
    {
        CurrentBatter = default;
        // ボールが残っていたら削除する
        if (netBall != null)
        {
            netBall.Despawn();
            netBall = null;
        }
        netBall = Instance.Runner.Spawn(ResourcesManager.Instance.netBallPrefab, BallPosByMode[ResourcesManager.Instance.CurrentMode]);

        IsTopInning = GameManager.IsOnline ? CurrentBoxCount % 2 == 1 : true;

        //バッターを決定！！
        var whitePlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => p.IsWhite).ToArray();
        var redPlayers = PlayerRegistry.OrderAsc(p => p.TeamIndex, p => !p.IsWhite).ToArray();
        if (IsTopInning)
        {
            if (whitePlayers.Length == 0)
            {
                Runner.Shutdown();
                return;
            }
            CurrentWhiteBatterCount++;
            if (CurrentWhiteBatterCount >= whitePlayers.Length)
                CurrentWhiteBatterCount = 0;
            CurrentBatter = whitePlayers[CurrentWhiteBatterCount].Ref;
            whitePlayers[CurrentWhiteBatterCount].State = PlayerObject.EPlayerState.Batter;
            Instance.Runner.Spawn(ResourcesManager.Instance.netBaseballBatPrefab, new Vector3(0.7f, -3.2f, 10.3f));
            foreach (var player in whitePlayers)
                if (player.Ref != CurrentBatter) player.State = PlayerObject.EPlayerState.None;
            foreach (var player in redPlayers)
            {
                player.State = PlayerObject.EPlayerState.Defence;
                if (!CurrentDefenceAreas.ContainsKey(player.Ref))
                {
                    CurrentDefenceAreas[player.Ref] = Instance.Runner.Spawn(ResourcesManager.Instance.gravityFieldPrefab, inputAuthority: player.Ref);
                }
            }
        }
        else
        {
            if (redPlayers.Length == 0)
            {
                Runner.Shutdown();
                return;
            }
            CurrentRedBatterCount++;
            if (CurrentRedBatterCount >= redPlayers.Length)
                CurrentRedBatterCount = 0;
            CurrentBatter = redPlayers[CurrentRedBatterCount].Ref;
            redPlayers[CurrentRedBatterCount].State = PlayerObject.EPlayerState.Batter;
            Instance.Runner.Spawn(ResourcesManager.Instance.netBaseballBatPrefab, new Vector3(0.7f, -3.2f, 10.3f));
            foreach (var player in whitePlayers)
            {
                player.State = PlayerObject.EPlayerState.Defence;
                if (!CurrentDefenceAreas.ContainsKey(player.Ref))
                {
                    CurrentDefenceAreas[player.Ref] = Instance.Runner.Spawn(ResourcesManager.Instance.gravityFieldPrefab, inputAuthority: player.Ref);
                }
            }
            foreach (var player in redPlayers)
                if (player.Ref != CurrentBatter) player.State = PlayerObject.EPlayerState.None;
        }
    }

    public void OnChangeOffenceDefence()
    {
        InterfaceManager.Instance.vsPanel.SetGameCount(CurrentBoxCount);
    }

    public void UpdateGameParamater(int point, GameManager.EJudge judge)
    {
        var CurrentObj = PlayerRegistry.GetPlayer(CurrentBatter);
        if (CurrentObj != null)
        {
            CurrentObj.BatterCount++;
            CurrentObj.Score += point;
            CurrentObj.HitCount += point > 0 ? 1 : 0;
            CurrentObj.HRCount += point == 4 ? 1 : 0;
            CurrentObj.Judges.Set(CurrentObj.BatterCount - 1, judge);
            if (CurrentObj?.Controller != null) 
            {
                Instance.Runner.Despawn(CurrentObj.Controller.Object);
            }
        }

        if (IsTopInning)
        {
            TotalWhitePoints += point;
            CurrentWhitePoints += point;
        }
        else
        {
            CurrentRedPoints += point;
            TotalRedPoints += point;
        }

        CurrentOutCount++;
        var isChange = CurrentOutCount >= (IsOnline ? 3 : 1);
        if (isChange)
        {
            CurrentOutCount = 0;
            CurrentBoxCount++;
            if (CurrentDefenceAreas.Count > 0)
            {
                foreach (var area in CurrentDefenceAreas)
                    Instance.Runner.Despawn(area.Value);
                CurrentDefenceAreas.Clear();
            }
        }
        StartCoroutine(ChangeStateRoutine(judge, isChange));
    }

    public IEnumerator ChangeStateRoutine(GameManager.EJudge judge, bool isChange)
    {
        State.Server_SetState(GameState.EGameState.Judging);
        yield return new WaitForSeconds(3f);

        if (!IsOnline)
        {
            if (CurrentBoxCount > ResourcesManager.Instance.MaxBoxCountSingle)
                State.Server_SetState(GameState.EGameState.Postgame);
            else
                State.Server_SetState(GameState.EGameState.BeforeHit);
            yield break;
        }

        if (isChange)
        {
            if (IsTopInning)
            {
                State.Server_SetState(GameState.EGameState.DefenceOffenceChange);
                yield return new WaitForSeconds(3f);
            }
            else
            {
                State.Server_SetState(GameState.EGameState.Interval);
                yield return new WaitForSeconds(4f);
                if (CurrentBoxCount >= ResourcesManager.Instance.MaxBoxCount)
                {
                    State.Server_SetState(GameState.EGameState.Postgame);
                    yield break;
                }
            }
        }
        
        State.Server_SetState(GameState.EGameState.BatterChange);
    }

    public void OnUpdateOutCount()
    {
        InterfaceManager.Instance.outCountUI.SetOutCount(CurrentOutCount);
    }

    public void OnUpdateScore()
    {
        InterfaceManager.Instance.vsPanel.SetPoints(TotalWhitePoints, TotalRedPoints);
    }

    public void BatterChanged(PlayerObject batter)
    {
        InterfaceManager.Instance.currentBatterUI.SetBatter(batter);
        InterfaceManager.Instance.changeBatterUI.ChangeBatter(batter, 10f);
    }

    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data)
	{
		key.GetInts(out int a, out int b, out int c, out int d);
		if (a == 0 && Runner.IsServer) // Client to Server
		{
			ReliableKey reliableKey = ReliableKey.FromInts(1, b, 0, 0);
			foreach (var p in Runner.ActivePlayers)
			{
				Runner.SendReliableDataToPlayer(p, reliableKey, data.ToArray());
			}
		}
		else if (a == 1) // Server to Client
		{
			if (data.Count > 0)
			{
				Texture2D texture = new Texture2D(2, 2);
				texture.LoadImage(data.Array);
				if (texture != null)
				{
					Debug.Log($"Received custom image for player {b}: {texture.width}x{texture.height}");
					PlayerCustomImages[PlayerRegistry.Where(p => p.Index == b).FirstOrDefault().Ref] = texture;
				}
			}
		}
	}

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (PlayerCustomImages.ContainsKey(player))
        {
            PlayerCustomImages.Remove(player);
            Debug.Log($"Removed custom image for player {player}");
        }

        if ((State.Current == GameState.EGameState.BatterChange ||State.Current == GameState.EGameState.BeforeHit || State.Current == GameState.EGameState.AfterHit) && CurrentBatter == player && Runner.IsServer)
        {
            InterfaceManager.Instance.changeBatterUI.Stop();
            if (netBall != null)
            {
                netBall.RPC_SendJudge(EJudge.PlayerLeft);
            }
            var bat = FindFirstObjectByType<NetBaseballBat>();
            if (bat != null)
            {
                Runner.Despawn(bat.Object);
            }
            CurrentBatter = default;
        }
    }

    /// <summary>
    /// シーン遷移時の処理（タイトルに戻ったらRunner停止）
    /// </summary>
    public void OnSceneLoades(Scene preScene, Scene nextScene)
    {
        if (nextScene.buildIndex == 0)
            Runner.Shutdown();
    }

    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player)
    {
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
    }

    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason)
    {
    }

    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token)
    {
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
    }

    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
    {
    }

    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress)
    {
    }

    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input)
    {
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
    {
    }

    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
    {
    }

    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
    {
    }

    public void OnSceneLoadDone(NetworkRunner runner)
    {
    }

    public void OnSceneLoadStart(NetworkRunner runner)
    {
    }
}
