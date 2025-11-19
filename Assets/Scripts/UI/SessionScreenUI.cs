using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Fusion;
using System.Threading.Tasks;
using System.Linq;
using Helpers.Linq;
using System.Security.Cryptography;

/// <summary>
/// セッション画面のUI管理クラス
/// プレイヤー一覧、ゲーム設定、色設定などを管理
/// </summary>
public class SessionScreenUI : MonoBehaviour
{
    [Header("UI References")]
    public Transform[] whitePlayerItemHolder;              // プレイヤーアイテムの親オブジェクト
    public Transform[] redPlayerItemHolder;
	public TMP_Text spectatorCountText;      // 観戦者数表示用テキスト
    public TMP_Text allPlayerCountText;

	[Header("Session Info UI")]
    public TMP_Text sessionNameText;                // セッション名のテキスト表示

    [Header("Game Control")]
    public Button ReadyButton;                  // ゲーム開始ボタン

    // プレイヤー参照とUIアイテムの対応辞書
    public Dictionary<PlayerRef, PlayerSessionItemUI> playerItems = new Dictionary<PlayerRef, PlayerSessionItemUI>();

    // セッション更新中フラグ（重複更新防止）
    bool isUpdatingSession = false;

    /// <summary>
    /// イベント購読とセッション設定の更新
    /// </summary>
    public void AddSubscriptions()
    {
        // イベント購読の重複を防ぐため、一度解除してから購読
        PlayerRegistry.OnPlayerJoined -= PlayerJoined;
        PlayerRegistry.OnPlayerLeft -= PlayerLeft;
        PlayerRegistry.OnPlayerJoined += PlayerJoined;
        PlayerRegistry.OnPlayerLeft += PlayerLeft;

        // セッション設定を更新し、スライダーをデフォルト値に設定
        UpdateSessionConfig();
    }

    /// <summary>
    /// UI無効化時の処理
    /// イベント購読解除とプレイヤーアイテム辞書のクリア
    /// </summary>
    private void OnDisable()
    {
        playerItems.Clear();

        PlayerRegistry.OnPlayerJoined -= PlayerJoined;
        PlayerRegistry.OnPlayerLeft -= PlayerLeft;
    }

    /// <summary>
    /// セッション設定の更新を開始
    /// 重複実行を防ぐためのフラグチェックあり
    /// </summary>
    public void UpdateSessionConfig()
    {
        if (!isUpdatingSession && gameObject.activeInHierarchy)
        {
            isUpdatingSession = true;
            StartCoroutine(UpdateSessionConfigRoutine());
        }
    }

    /// <summary>
    /// セッション設定更新のコルーチン
    /// セッション情報の取得を待機し、UI要素を更新
    /// </summary>
    IEnumerator UpdateSessionConfigRoutine()
    {
        // セッション情報が利用可能になるまで待機
        if (!(GameManager.Instance?.Runner?.SessionInfo == true))
        {
            // セッション情報がない間はUI要素を無効化
            // privateCheck.interactable = collisionCheck.interactable =
            //     maxShotsSetting.interactable = holeTimeSetting.interactable =
            //     courseLengthSetting.interactable = false;

            yield return new WaitUntil(() => GameManager.Instance?.Runner?.SessionInfo == true);
        }

        // サーバーの場合、既存プレイヤーのUIアイテムを作成
        if (GameManager.Instance.Runner.IsServer)
            PlayerRegistry.ForEach(p =>
            {
                if (!playerItems.ContainsKey(p.Ref))
                    CreatePlayerItem(p.Ref);
            }, true);

        // セッション名をUIに反映
        sessionNameText.text = GameManager.Instance.Runner.SessionInfo.Name;

        isUpdatingSession = false;
    }
    /// <summary>
    /// プレイヤー参加時のイベントハンドラー
    /// </summary>
    /// <param name="runner">ネットワークランナー</param>
    /// <param name="player">参加したプレイヤー</param>
    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        CreatePlayerItem(player);
    }

    /// <summary>
    /// プレイヤーのUIアイテムを作成
    /// </summary>
    /// <param name="pRef">プレイヤー参照</param>
    private void CreatePlayerItem(PlayerRef pRef)
    {
        // 既に存在しない場合のみ作成
        if (!playerItems.ContainsKey(pRef))
        {
            // ネットワークオブジェクトをスポーン可能な場合
            if (LobbyManager.Instance.Runner.CanSpawn)
            {
                // プレイヤーセッションアイテムをスポーン
                PlayerSessionItemUI item = LobbyManager.Instance.Runner.Spawn(
                    prefab: ResourcesManager.Instance.playerSessionItemUI,
                    inputAuthority: pRef);
                if (!playerItems.ContainsKey(pRef))
                    playerItems.Add(pRef, item);
                Debug.Log(playerItems.Count + " items in dictionary");
            }
        }
        else
        {
            Debug.LogWarning($"{pRef} already in dictionary");
        }
    }

    /// <summary>
    /// プレイヤー退出時のイベントハンドラー
    /// </summary>
    /// <param name="runner">ネットワークランナー</param>
    /// <param name="player">退出したプレイヤー</param>
    public void PlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Player {player} left the session.");
        // プレイヤーのUIアイテムが存在する場合
        if (playerItems.TryGetValue(player, out PlayerSessionItemUI item) && runner.IsServer)
        {
            if (item)
            {
                Debug.Log($"Removing {nameof(PlayerSessionItemUI)} for {player}");
                // ネットワークオブジェクトを破棄
                runner.Despawn(item.Object);
            }
            else
            {
                Debug.Log($"{nameof(PlayerSessionItemUI)} for {player} was null.");
            }
            // 辞書から削除
            playerItems.Remove(player);
        }
        else
        {
            Debug.LogWarning($"{player} not found");
        }
    }

    /// <summary>
    /// プレイヤーの準備状態を更新
    /// </summary>
    /// <param name="isReady">準備状態</param>
    public void UpdatePlayerReadyState(bool isReady)
    {
        PlayerObject.Local.Rpc_SetReadyState(isReady);
    }

    /// <summary>
    /// 全プレイヤーUIで画像がロードされたかどうかをチェック
    /// </summary>
    /// <returns>全てのプレイヤーの画像がロード完了した場合true</returns>
    public bool AllImageLoadDone()
    {
        // プレイヤーが存在しない場合はfalseを返す
        if (playerItems.Count == 0)
        {
            Debug.Log("なんで-？");
            return false;
        }
        
        // 全てのPlayerSessionItemUIのisImageLoadedがtrueかチェック
        foreach (var kvp in playerItems)
        {
            Debug.Log($"Checking player {kvp.Key} image loaded: {kvp.Value.usernameText.text} - {kvp.Value.isImageLoaded}");
            PlayerSessionItemUI item = kvp.Value;
            
            // アイテムがnullまたはisImageLoadedがfalseの場合はfalseを返す
            if (item == null || !item.isImageLoaded)
            {
                Debug.Log("なんで？");
                return false;
            }
        }
        
        // 全てのプレイヤーの画像がロード完了
        return true;
    }

    #region Game Control（ゲーム制御関連メソッド）

    /// <summary>
    /// 観戦モードの切り替え
    /// </summary>
    public void ToggleSpectate()
    {
        PlayerObject.Local.Rpc_ToggleSpectate();
    }

    /// <summary>
    /// セッションから退出
    /// </summary>
    public void Leave()
    {
        StartCoroutine(LeaveRoutine());
    }

    /// <summary>
    /// セッション退出処理のコルーチン
    /// ネットワークランナーをシャットダウンし、初期画面に戻る
    /// </summary>
    IEnumerator LeaveRoutine()
    {
        // ネットワークランナーのシャットダウンを開始
        Task task = LobbyManager.Instance.Runner.Shutdown();
        
        // シャットダウン完了まで待機
        while (!task.IsCompleted)
        {
            yield return null;
        }
        
        // 初期画面に戻る
        UIScreen.BackToInitial();
    }

    #endregion
}
