using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using Fusion;
using KanKikuchi.AudioManager;
using Unity.VisualScripting;
using UnityEngine;

public class GameState : NetworkBehaviour
{
	public enum EGameState { Off, Pregame, Intro_View, BatterChange, BeforeHit, AfterHit, Judging, DefenceOffenceChange, Interval, Postgame }

	[Networked][field: ReadOnly] public EGameState Previous { get; set; }
	[Networked] [field: ReadOnly] public EGameState Current { get; set; }

	[Networked] TickTimer Delay { get; set; }
	[Networked] EGameState DelayedState { get; set; }

	protected StateMachine<EGameState> StateMachine = new StateMachine<EGameState>();

	private Coroutine introViewCoroutine;

	public override void Spawned()
	{
		if (Runner.IsServer)
		{
			Server_SetState(EGameState.Pregame);
		}

		StateMachine[EGameState.Pregame].onUpdate = () =>
		{
			if (Runner.IsServer)
			{
				if (PlayerRegistry.Players.Count() == 0)
					return;
				if (PlayerRegistry.All(p => p.IsReady))
				{
					if (GameManager.IsOnline)
						Server_SetState(EGameState.Intro_View);
					else
						Server_SetState(EGameState.BeforeHit);
				}
			}
		};

		StateMachine[EGameState.Pregame].onExit = next =>
		{
			InterfaceManager.HidePanel(InterfaceManager.Instance.topAnimator);
			InterfaceManager.HidePanel(InterfaceManager.Instance.lobbyAnimator);
			InterfaceManager.HidePanel(InterfaceManager.Instance.sessionScreenAnimator);
			BGMManager.Instance.Play(BGMPath.MAJOUR, 0.2f);
			Loading.Instance.SetVisible(false);
			if (GameManager.IsOnline)
			{
				InterfaceManager.Instance.intervalUI.InitBoxPanel(ResourcesManager.Instance.MaxBoxCount / 2);
			}
			else
			{
				ExitManager.Instance.returnButtonAnimation.Play(ResourcesManager.PANEL_IN);
				InterfaceManager.Instance.intervalUI.InitBoxPanel(ResourcesManager.Instance.MaxBoxCountSingle);
				InterfaceManager.Instance.intervalUI.SetName(ResourcesManager.Instance.userName);
				InterfaceManager.Instance.vsPanel.SetSingleState(ResourcesManager.Instance.userName);
			}
			if (Runner.SessionInfo.IsOpen) Runner.SessionInfo.IsOpen = false;
		};

		StateMachine[EGameState.Intro_View].onEnter = prev =>
		{
			introViewCoroutine = StartCoroutine(InterfaceManager.Instance.StartIntroViewRoutine());
		};

		StateMachine[EGameState.Intro_View].onExit = next =>
		{
			if (introViewCoroutine != null)
			{
				StopCoroutine(introViewCoroutine);
				introViewCoroutine = null;
			}
			if (PlayerObject.Local.IsSpectator)
				GameCameraController.Instance.ChangeCamera("Observer");
		};

		StateMachine[EGameState.BatterChange].onEnter = prev =>
		{
			InterfaceManager.ShowPanel(InterfaceManager.Instance.gameAnimator);
			InterfaceManager.ShowPanel(InterfaceManager.Instance.changeBatterUI.animator);
			if (Runner.IsServer)
			{
				GameManager.Instance.ForwardGameProgress();
			}
		};
		
		StateMachine[EGameState.BatterChange].onExit = next=>
		{
			InterfaceManager.HidePanel(InterfaceManager.Instance.changeBatterUI.animator);
		};

		StateMachine[EGameState.BeforeHit].onEnter = prev =>
		{
			if (!GameManager.IsOnline)
				GameManager.Instance.ForwardGameProgress();
			InterfaceManager.ShowPanel(InterfaceManager.Instance.gameAnimator);
			InterfaceManager.ShowPanel(InterfaceManager.Instance.vsPanel.animator);
			InterfaceManager.ShowPanel(InterfaceManager.Instance.currentBatterUI.animator);
			InterfaceManager.ShowPanel(InterfaceManager.Instance.outCountUI.animator);
			if (PlayerObject.Local.IsSpectator)
				InterfaceManager.ShowPanel(InterfaceManager.Instance.spectatorUI.animator);
			InterfaceManager.Instance.playerUI.SetButtonState(PlayerObject.Local.State); // Stateに応じてボタンの表示・非表示も制御してるンゴ

			if (PlayerObject.Local.State == PlayerObject.EPlayerState.None && !PlayerObject.Local.IsSpectator)
				GameCameraController.Instance.ChangeCamera(PlayerObject.EPlayerState.Batter.ToString());
			else
				GameCameraController.Instance.ChangeCamera(PlayerObject.Local.State.ToString());
		};

		StateMachine[EGameState.BeforeHit].onExit = next =>
		{
			if(PlayerObject.Local.State != PlayerObject.EPlayerState.Defence)
				InterfaceManager.HidePanel(InterfaceManager.Instance.playerUI.animator);
			InterfaceManager.HidePanel(InterfaceManager.Instance.outCountUI.animator);
			InterfaceManager.HidePanel(InterfaceManager.Instance.currentBatterUI.animator);
		};

		StateMachine[EGameState.AfterHit].onEnter = prev =>
		{
			if (ResourcesManager.Instance.CurrentMode == GameManager.EMode.Derby)
			{
				Pitcher.Instance.isDerbyTrace = false;
				GameCameraController.Instance.ChangeCamera("Trace");
				ResourcesManager.Instance.DerbyTraceObj.GetComponent<ShakeBehaviour>().TriggerShake(8f, 2f);
            }
			else if (!PlayerObject.Local.IsSpectator)
				GameCameraController.Instance.ChangeCamera("Hit");
		};
		
		StateMachine[EGameState.AfterHit].onExit = next =>
		{
			InterfaceManager.HidePanel(InterfaceManager.Instance.playerUI.animator);
		};

		StateMachine[EGameState.Judging].onEnter = prev =>
		{
			InterfaceManager.ShowPanel(InterfaceManager.Instance.judgeUI.animator);
		};
		
		StateMachine[EGameState.Judging].onExit = next =>
		{
			InterfaceManager.HidePanel(InterfaceManager.Instance.judgeUI.animator);
		};

		StateMachine[EGameState.DefenceOffenceChange].onEnter = prev =>
		{
			if (!PlayerObject.Local.IsSpectator)
				GameCameraController.Instance.ChangeCamera("Change");
			InterfaceManager.ShowPanel(InterfaceManager.Instance.changeOffenceDefenceAnimator);
		};

		StateMachine[EGameState.DefenceOffenceChange].onExit = next =>
		{
			InterfaceManager.HidePanel(InterfaceManager.Instance.changeOffenceDefenceAnimator);
		};

		StateMachine[EGameState.Interval].onEnter = prev =>
		{
			InterfaceManager.ShowPanel(InterfaceManager.Instance.intervalUI.animator);
			StartCoroutine(InterfaceManager.Instance.intervalUI.SetCurrentPointsRoutine(GameManager.Instance.CurrentWhitePoints, GameManager.Instance.CurrentRedPoints, GameManager.Instance.CurrentBoxCount));
		};

		StateMachine[EGameState.Interval].onExit = next =>
		{
			if(next != EGameState.Postgame)
				InterfaceManager.HidePanel(InterfaceManager.Instance.intervalUI.animator);
			if (Runner.IsServer)
			{
				GameManager.Instance.CurrentWhitePoints = 0;
				GameManager.Instance.CurrentRedPoints = 0;
			}
		};

		StateMachine[EGameState.Postgame].onEnter = prev =>
		{
			SEManager.Instance.Play(SEPath.OGI_GAMESET);
			InterfaceManager.Instance.privateResultUI.SetItem();
			if (GameManager.IsOnline)
				StartCoroutine(InterfaceManager.Instance.intervalUI.SetTotalPointsRoutine(GameManager.Instance.TotalWhitePoints, GameManager.Instance.TotalRedPoints));
			else
			{
				InterfaceManager.ShowPanel(InterfaceManager.Instance.intervalUI.animator);
				StartCoroutine(InterfaceManager.Instance.intervalUI.SetTotalPointsRoutineSingle(GameManager.Instance.TotalWhitePoints));
			}
		};

		// 	// Ensures that FixedUpdateNetwork is called for all proxies.
		Runner.SetIsSimulated(Object, true);

		StateMachine.Update(Current, Previous);
	}

	public override void FixedUpdateNetwork()
	{
		if (Runner.IsServer)
		{
			if (Delay.Expired(Runner))
			{
				Delay = TickTimer.None;
				Server_SetState(DelayedState);
			}
		}
		if (Runner.IsForward)
			StateMachine.Update(Current, Previous);
	}

	public void Server_SetState(EGameState st)
	{
		if (Current == st) return;
		//Debug.Log($"Set State to {st}");
		Previous = Current;
		Current = st;
	}
	
	public void Server_DelaySetState(EGameState newState, float delay)
	{
		Debug.Log($"Delay state change to {newState} for {delay}s");
		Delay = TickTimer.CreateFromSeconds(Runner, delay);
		DelayedState = newState;
	}
}