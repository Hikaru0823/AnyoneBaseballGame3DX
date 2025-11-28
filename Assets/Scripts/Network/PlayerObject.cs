using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using KanKikuchi.AudioManager;

public class PlayerObject : NetworkBehaviour
{
    public enum EPlayerState { None, Batter, Defence }
	public static PlayerObject Local { get; private set; }
	
	// Metadata
	[Networked]
	public PlayerRef Ref { get; set; }
    [Networked]
	public PlayerController Controller { get; set; }
	[Networked]
	public byte Index { get; set; } = 255;
    [Networked, OnChangedRender(nameof(TeamChanged))]
	public byte TeamIndex { get; set; } = 255;
	[Networked]
	public EPlayerState State { get; set; } = EPlayerState.None;

    // User Settings
    [Networked, OnChangedRender(nameof(StatChanged))]
	public string Nickname { get; set; }
	[Networked, Capacity(30)]
	public string CharacterName { get; set; }
	// State & Gameplay Info
	[Networked, OnChangedRender(nameof(StatChanged))]
	public bool IsReady { get; set; }
    [Networked]
	public bool IsWhite { get; set; }
	[Networked]
	public bool IsLoaded { get; set; }
	[Networked]
	public bool IsImageSelected { get; set; }
	[Networked]
	public bool IsSpectator { get; set; }
	[Networked]
	public int BatterCount { get; set; } = 0;
	[Networked]
	public int HitCount { get; set; } = 0;
	[Networked]
	public int Score { get; set; } = 0;
	[Networked]
	public int HRCount { get; set; } = 0;
	[Networked, Capacity(10)]
	public NetworkArray<GameManager.EJudge> Judges => default;
	[Networked, Capacity(10)]
	public NetworkArray<float> EvaScores => default;

	public event System.Action OnStatChanged;
	public event System.Action OnTeamChanged;

	public void Server_Init(PlayerRef pRef, byte index)
	{
		Debug.Assert(Runner.IsServer);

		Ref = pRef;
		Index = index;
	}

	public override void Spawned()
	{
		if (Object.HasStateAuthority)
		{
			PlayerRegistry.Server_Add(Runner, Object.InputAuthority, this);
		}

		if (Object.HasInputAuthority)
		{
			Local = this;
			if (ResourcesManager.Instance.IsSelected)
			{
				Rpc_ToggleSelected();
			}
			Rpc_SetNickname(ResourcesManager.Instance.userName);
			Rpc_SetCharacterName(ResourcesManager.Instance.characterName);
		}

		PlayerRegistry.PlayerJoined(Object.InputAuthority);

		if (GameManager.Instance != null && !GameManager.IsOnline)
		{
			Index = 0;
			TeamIndex = 0;
			IsWhite = true;
			InterfaceManager.Instance.currentBatterUI.SetBatter(this);
			IsReady = true;
		}
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (Local == this) Local = null;

		if (!runner.IsShutdown)
		{
			// if (Controller)
			// {
			// 	runner.Despawn(Controller.Object);
			// }

			// if (GameManager.State.Current == GameState.EGameState.Game && PlayerRegistry.All(p => p.Controller == null))
			// {
			// 	GameManager.State.Server_SetState(GameState.EGameState.Outro);
			// }
		}

	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_OnInput()
	{
		if (Controller == null) return;
		Controller.OnInput();
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetNickname(string nick)
	{
		Nickname = nick;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	void Rpc_SetCharacterName(string name)
	{
		CharacterName = name;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetReadyState(bool isReady)
	{
		IsReady = isReady;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_SetLoadState(bool isLoaded)
	{
		IsLoaded = isLoaded;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_ToggleSpectate()
	{
		IsSpectator = !IsSpectator;
	}

	[Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
	public void Rpc_ToggleSelected()
	{
		IsImageSelected = !IsImageSelected;
	}

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    public void Rpc_ToggleIsWhite(bool isWhite)
    {
        IsWhite = isWhite;
        if(PlayerRegistry.Instance.GetAvailableOfTeam(isWhite, out byte index))
		{
			TeamIndex = index;
		}
	}

	[Rpc(RpcSources.All, RpcTargets.InputAuthority)]
	public void Rpc_SendImage()
	{
		ReliableKey reliableKey = ReliableKey.FromInts(0, Index, 0, 0);
		Runner.SendReliableDataToServer(reliableKey, ResourcesManager.Instance.customImageBytes);
	}

	void StatChanged()
	{
		OnStatChanged?.Invoke();
	}

	void TeamChanged()
	{
		OnTeamChanged?.Invoke();
	}
}
