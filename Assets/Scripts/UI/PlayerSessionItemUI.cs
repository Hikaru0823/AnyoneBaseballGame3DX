using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;
using System.Linq;
using Unity.VisualScripting;
using KanKikuchi.AudioManager;
using System;

public class PlayerSessionItemUI : NetworkBehaviour
{
	public TMP_Text usernameText;
	public RawImage playerImage;
	public Image readyImage;

	bool isHierarchyChanging = false;
	public bool isImageLoaded = false;
	private Coroutine setImageCoroutine = null;
	private GameObject parentObject;

	PlayerObject _player = null;
	PlayerObject Player
	{
		get
		{
			if (_player == null) _player = PlayerRegistry.GetPlayer(Object.InputAuthority);
			return _player;
		}
	}

	private void OnBeforeTransformParentChanged()
	{
		isHierarchyChanging = true;
	}

	private void OnTransformParentChanged()
	{
		isHierarchyChanging = false;
	}

	private void OnDisable()
	{
		if (!isHierarchyChanging && Runner?.IsRunning == true) Runner.Despawn(Object);
	}

	public void Init()
	{
		if (Player == null)
		{
			return;
		}
		Player.OnStatChanged += UpdateStats;
		Player.OnTeamChanged += SetPosition;
		if (!InterfaceManager.Instance.sessionScreen.playerItems.ContainsKey(Player.Ref))
			InterfaceManager.Instance.sessionScreen.playerItems.Add(Player.Ref, this);
		UpdateStats();
	}

    public override void Spawned()
	{
		//SEManager.Instance.Play(SEPath.ENTER_SESSION);
		Init();
	}

	public override void Despawned(NetworkRunner runner, bool hasState)
	{
		if (Player)
		{
			Player.OnStatChanged -= UpdateStats;
			Player.OnTeamChanged -= SetPosition;
		}
	}

	void UpdateStats()
	{
		SetUsername(Player.Nickname);
		SetReady(Player.IsReady);
		SetPosition();
	}

	void SetPosition()
	{
		if (Player)
		{
			if (!Player.Nickname.IsUnityNull() && Player.Nickname != "" )
			{
				if (Player.TeamIndex != 255)
				{
					var holder = Player.IsWhite ? InterfaceManager.Instance.sessionScreen.whitePlayerItemHolder[Player.TeamIndex] : InterfaceManager.Instance.sessionScreen.redPlayerItemHolder[Player.TeamIndex];
					transform.SetParent(holder, false);
				}
				if (!Player.IsSpectator)
				{
					// プレイヤーのイメージ設定
					if (!Player.IsImageSelected)
					{
						SetPlayerImage(ResourcesManager.Instance.CharacterTextureByName[Player.CharacterName]);
						isImageLoaded = true;
					}
					else if (!GameManager.Instance.PlayerCustomImages.ContainsKey(Player.Ref))
					{
						isImageLoaded = false;
						if (setImageCoroutine == null)
							setImageCoroutine = StartCoroutine(SetPlayerCustomImageRoutine(Player));
					}
				}
				else
				{
					isImageLoaded = true;
				}
			}

			//観戦者の数などを更新
			var whitePlayerCount = PlayerRegistry.Players.Count(p => p.IsWhite && p.TeamIndex != 255);
			var redPlayerCount = PlayerRegistry.Players.Count(p => !p.IsWhite && p.TeamIndex != 255);
			var spectatorCount = PlayerRegistry.Everyone.Count(p => p.IsSpectator);
			InterfaceManager.Instance.sessionScreen.spectatorCountText.text = spectatorCount.ToString();
			if (Runner.IsServer)
			{
				Runner.SessionInfo.UpdateCustomProperties(new Dictionary<string, SessionProperty>
				{
					{ "MaxPlayers", 6 },
					{ "WhitePlayers", whitePlayerCount },
					{ "RedPlayers", redPlayerCount },
					{ "SpectatorPlayers", spectatorCount }
				});
			}
		}
		// bool anySpectators = InterfaceManager.Instance.sessionScreen.spectatorItemHolder.childCount > 0;
		// InterfaceManager.Instance.sessionScreen.spectatorHeader.SetActive(anySpectators);
		// InterfaceManager.Instance.sessionScreen.spectatorPanel.gameObject.SetActive(anySpectators);
	}

	public void SetUsername(string name)
	{
		usernameText.text = name;
		if (PlayerObject.Local == Player)
			usernameText.color = new Color(0.6f, 0.6f, 1.0f, 1f);
	}

	public void SetReady(bool set)
	{
		readyImage.enabled = set;
	}

	public void SetPlayerImage(Texture2D texture)
	{
		playerImage.texture = texture;
	}

	IEnumerator SetPlayerCustomImageRoutine(PlayerObject player)
	{
		player.Rpc_SendImage();
		yield return new WaitUntil(() => GameManager.Instance?.PlayerCustomImages.ContainsKey(player.Ref) == true);
		SetPlayerImage(GameManager.Instance.PlayerCustomImages[player.Ref]);
		isImageLoaded = true;
		setImageCoroutine = null;
	}
}