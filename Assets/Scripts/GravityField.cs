using Fusion;
using KanKikuchi.AudioManager;
using NUnit.Framework;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GravityField : PlayerController
{
    // 減衰率（0〜1、値が小さいほど減衰が強い）
    // [Networked]
    // public float scale { get; set; } = 1f;
    public PlayerObject playerObject;
    [SerializeField] private float damping = 0.95f;
    //[SerializeField] private Transform targetTransform; // ターゲットのTransform
    [SerializeField] private RawImage targetImage; // ターゲットのRawImage
    [SerializeField] private TextMeshProUGUI targetText; // ターゲットのTextMeshProUGUI
    [SerializeField] private GameObject particle; // 力を及ぼす範囲の半径

    // スケール変化のパラメータ
    [SerializeField] private float scaleAmplitude = 0.08f;   // 変化幅
    [SerializeField] private float scaleOffset = 0.8f;      // 最小スケール
    // 最大スケール
    [SerializeField] private float scaleMax = 1.8f; // 追加

    private bool isScaling = false; // スケール変化中かどうか
    

    private bool isActive = false;
    private float time = 0f; // 時間計測用

    public override void Spawned()
    {
        playerObject = PlayerRegistry.GetPlayer(Object.InputAuthority);
        if (Object.HasStateAuthority)
        {
            playerObject.Controller = this;
            //Object.RemoveInputAuthority();  
        }
        transform.parent.position = ResourcesManager.Instance.defencePoses[playerObject.TeamIndex].position;
        if (playerObject.IsImageSelected)
            SetParam(GameManager.Instance.PlayerCustomImages[playerObject.Ref], playerObject.Nickname);
        else
            SetParam(ResourcesManager.Instance.CharacterTextureByName[playerObject.CharacterName], playerObject.Nickname);
        //GameManager.Instance.Runner.SetIsSimulated(Object, true);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority) return;
        
        if (isScaling)
        {
            time += Runner.DeltaTime;

            // 0.1秒かけて一定の大きさまで拡大
            float progress = time / 0.1f; // 0.1秒で完了
            progress = Mathf.Clamp01(progress); // 0〜1に制限
            
            Vector3 currentScale = transform.localScale;
            float targetScale = currentScale.x + scaleAmplitude; // 目標サイズ
            targetScale = Mathf.Min(targetScale, scaleMax); // 最大値で制限
            
            float newScale = Mathf.Lerp(currentScale.x, targetScale, progress);
            transform.localScale = new Vector3(newScale, currentScale.y, newScale);
            
            // 0.5秒経過したら停止
            if (progress >= 1.0f)
            {
                isScaling = false;
                time = 0f; // timeをリセット
            }
        }
        else
        {
            // isScalingがfalseの時は徐々に小さく（scaleOffsetに戻す）
            Vector3 currentScale = transform.localScale;
            float currentXZ = currentScale.x;
            
            if (currentXZ > scaleOffset)
            {
                float shrinkSpeed = 0.28f; // 縮小速度
                float newScale = Mathf.MoveTowards(currentXZ, scaleOffset, shrinkSpeed * Runner.DeltaTime);
                transform.localScale = new Vector3(newScale, currentScale.y, newScale);
            }
        }
    }

    // コリジョン内にいるRigidbodyの速度を徐々に減衰させる
    private void OnTriggerStay(Collider other)
    {
        if (!Object.HasStateAuthority) return;
        Rigidbody rb = other.attachedRigidbody;
        if (rb != null)
        {
            rb.linearVelocity *= damping;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Ball")
            SEManager.Instance.Play(SEPath.POWER_DOWN);
    }
    public override void OnInput()
    {
        ScaleUp();

    }

    public void ScaleUp()
    {
        Debug.Log("RPC_OnInput called");
        if (!isScaling) // まだスケーリング中でなければ開始
        {
            isScaling = true;
            time = 0f; // timeをリセット
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasStateAuthority)
    {
        playerObject.Controller = null;
    }

    public void SetParam(Texture2D texture, string name)
    {
        if (playerObject == PlayerObject.Local)
        {
            targetText.color = new Color(0.6f, 0.6f, 1.0f, 1f);
        }
        else
            particle.SetActive(false);
        targetImage.texture = texture;
        targetText.text = name;
    }
}
