using System;
using System.Collections;
using Fusion;
using Fusion.Addons.Physics;
using KanKikuchi.AudioManager;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class NetBaseballBat : PlayerController
{

    [Header("Resources")]
    public NetworkRigidbody3D networkRigidbody;
    public HingeJoint hinge;
    public Rigidbody rigidbody;
    public Collider collider;
    public PlayerObject PlayerObj { get; private set; }

    [Header("Settings")]
    public float range = 0.5f; // バットが動く範囲（バリアフリー）
    public float coolTime = 1; // 空振りしたときに再び触れるまでの時間
    private bool isSwing = true;
    private bool isHit = false;

    private Quaternion initRot;
    private Vector3 initPos;
    JointSpring jointSpring;
    private float moveTime; // 
    private bool isNormal = false;
    [Networked]
    public bool isMove { get; private set; } = false;

    JointSpring _js;


    //インジケータ関連
    public Transform arrow;
    private bool isMoveArrow = false;
    float currentAngle;             // 現在の角度
    int direction = 1;              // 回転方向 (+1 or -1)
    public float maxAngle = 45f;
    public float swingSpeed = 60f;

    public override void Spawned()
    {
        isNormal = ResourcesManager.Instance.CurrentMode == GameManager.EMode.Normal;
        isMoveArrow = ResourcesManager.Instance.CurrentMode == GameManager.EMode.Evaluation;
        isMove = ResourcesManager.Instance.CurrentMode == GameManager.EMode.BarrierFree || ResourcesManager.Instance.CurrentMode == GameManager.EMode.Online_BarrierFree;
        arrow = ResourcesManager.Instance.Arrow.transform;

        PlayerObj = PlayerRegistry.GetPlayer(GameManager.Instance.CurrentBatter);
        if (GameManager.IsOnline)
            GameManager.Instance.BatterChanged(PlayerObj);
        PlayerObj.Controller = this;
        if (!Object.HasStateAuthority) return;
        initRot = transform.localRotation;
        initPos = transform.position;
        jointSpring.spring = 60;
        jointSpring.damper = 8;
        jointSpring.targetPosition = 180;
        hinge.connectedAnchor = initPos;
        Init();
        //StartCoroutine("StartRoutine");
        Runner.SetIsSimulated(Object, true);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        PlayerObj.Controller = null;
    }

    public override void OnInput()
    {
        Swing();
    }

    public override void FixedUpdateNetwork()
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        if (isNormal) return;

        if(isMove)
        {
            moveTime += Runner.DeltaTime;
            hinge.connectedAnchor = initPos + Vector3.forward * Mathf.Sin(moveTime) * range;
        }

        if(isMoveArrow)
        {
            currentAngle += direction * swingSpeed * Runner.DeltaTime;

            if (Mathf.Abs(currentAngle) > maxAngle)
            {
                currentAngle = Mathf.Sign(currentAngle) * maxAngle;
                direction *= -1;
            }

            arrow.transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
        }
    }

    public void Swing()
    {
        if (isSwing) return;
        isSwing = true;
        isMove = false;
        isMoveArrow = false;
        hinge.spring = jointSpring;
        hinge.useSpring = true;
        RPC_SwingEvent();
        StopCoroutine(ReloadRoutine());
        StartCoroutine(ReloadRoutine());
    }

    public void Init()
    {
        isHit = false;
        isSwing = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        Quaternion targetRotation = Quaternion.Euler(initRot.eulerAngles + Vector3.up * -1);
        networkRigidbody.Teleport(transform.position, targetRotation);
        collider.isTrigger = false;
    }

    private IEnumerator ReloadRoutine()
    {
        yield return new WaitForSeconds(coolTime);
        hinge.useSpring = false;
        yield return new WaitForSeconds(0.1f);
        if (!isHit)
            Init();
    }

    private IEnumerator StartRoutine()
    {
        yield return new WaitForSeconds(1);
        Init();
    }

    public void Despawn()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        if (collision.gameObject.tag == "Ball")
        {
            collider.isTrigger = true;
            RPC_HitEvent();
            isHit = true;
            if (GameManager.Instance.Runner.IsServer)
                GameManager.State.Server_SetState(GameState.EGameState.AfterHit);
            
            switch (ResourcesManager.Instance.CurrentMode)
            {
                case GameManager.EMode.BarrierFree:
                    break;
                case GameManager.EMode.Normal:
                    var X = Mathf.Abs(transform.rotation.z);
                    collision.gameObject.GetComponent<NetBall>().AddPower(new Vector3(Mathf.Cos((2 * Mathf.PI / 0.7f) * X), 0, Mathf.Sin((2 * Mathf.PI / 0.7f) * X)) * 0.8f, 3f);
                    break;
                case GameManager.EMode.Derby:
                    hinge.useSpring = false;
                    rigidbody.linearVelocity = Vector3.zero;
                    rigidbody.angularVelocity = Vector3.zero;
                    StartCoroutine(DelayHit(collision.gameObject.GetComponent<NetBall>()));
                    break;
                case GameManager.EMode.Evaluation:
                    collision.gameObject.GetComponent<NetBall>().AddPower(-arrow.transform.forward, 2.5f);
                    PlayerObj.EvaScores.Set(GameManager.Instance.CurrentBoxCount - 1, 100 - Math.Abs(currentAngle));
                    break;
                case GameManager.EMode.Online_BarrierFree:
                    break;
                default:
                    break;
            }
        }
    }

    IEnumerator DelayHit(NetBall ball)
    {
        yield return new WaitForSeconds(3f);
        ball.AddPower(new Vector3(0, 1, -1), 20f);
    }

    // void OnTriggerEnter(Collider other)
    // {
    //     if (other.gameObject.tag == "Ball")
    //     {
    //         if (ResourcesManager.Instance.CurrentMode == GameManager.EMode.Derby)
    //         {
    //             RPC_HitEvent();
    //             if (GameManager.Instance.Runner.IsServer)
    //                 GameManager.State.Server_SetState(GameState.EGameState.AfterHit);
    //         }
    //     }
    // }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HitEvent()
    {
        SEManager.Instance.Play(SEPath.OGI_UTTA);
        SEManager.Instance.Play(SEPath.BAT_SE);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SwingEvent()
    {
        SEManager.Instance.Play(SEPath.SWING_BAT);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    public void RPC_SwingReq()
    {
        Swing();
    }
}
