using System;
using Fusion;
using Fusion.Addons.Physics;
using KanKikuchi.AudioManager;
using UnityEngine;

public class NetBall : NetworkBehaviour
{
    public MeshRenderer meshRenderer;
    public Rigidbody rigidbody;
    public Collider collider;
    Vector3 initPos;

    bool isActive = false;
    bool isEnterJudge = false;
    float stayTime = 0;
    float lifeTime = 0;
    public float lifeLimitTime = 10;
    public float stayLimitTime = 3;
    public float rayDistance = 3f;
    private Collider stayCollider;
    bool isCurve = false;
    bool isSlow = false;
    float slowVelocity  = 0;
    float slowVelocitymax = 3;
    float slowVelocityStep = 0.02f;


    public override void Spawned()
    {
        GameCameraController.Instance.SetTraceCamera(transform);
        switch (ResourcesManager.Instance.CurrentMode)
        {
            case GameManager.EMode.BarrierFree:
                break;
            case GameManager.EMode.Normal:
                meshRenderer.enabled = false;
                StartCoroutine(Pitcher.Instance.ThrewAnimateRoutine());
                break;
            case GameManager.EMode.Derby:
                transform.SetParent(ResourcesManager.Instance.DerbyTraceObj);
                meshRenderer.enabled = false;
                //collider.isTrigger = true;
                Destroy(GetComponent<NetworkRigidbody3D>());
                rigidbody.isKinematic = true;
                StartCoroutine(Pitcher.Instance.ThrewAnimateRoutine());
                break;
            case GameManager.EMode.Evaluation:
                break;
            case GameManager.EMode.Online_BarrierFree:
                break;
            default:
                break;
        }
    }

    void FixedUpdate()
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        if (isActive)
        {
            lifeTime += Time.deltaTime;
            if (lifeTime > lifeLimitTime)
            {
                lifeTime = 0;
                stayTime = 0;
                GameManager.EJudge judge;
                try
                { judge = (GameManager.EJudge)Enum.Parse(typeof(GameManager.EJudge), stayCollider.gameObject.name); }
                catch
                {
                    var obj = CheckBelow();
                    if (obj != null)
                        judge = (GameManager.EJudge)Enum.Parse(typeof(GameManager.EJudge), obj.gameObject.name);
                    else
                        judge = GameManager.EJudge.None;
                }
                RPC_SendJudge(judge);
            }
        }

        if (isEnterJudge)
        {
            lifeTime += Time.deltaTime;
        }

        if (isCurve)
            {
                rigidbody.linearVelocity += Vector3.left * 0.01f;
            }
        if(isSlow)
        {
            slowVelocity += slowVelocityStep;
            if(slowVelocity > slowVelocitymax)
                isSlow  = false;
            rigidbody.linearVelocity += Vector3.back * slowVelocityStep;
        }
    }
    // public void ChangePichMode(GameBall.PitchMode mode)
    // {
    //     Debug.Log(mode.ToString());
    //     switch(mode)
    //     {
    //         case GameBall.PitchMode.Straight:
    //             rigidbody.AddForce(Vector3.forward * 2.5f, ForceMode.Impulse);
    //             break;
    //         case GameBall.PitchMode.Curve:
    //             rigidbody.AddForce(Vector3.forward * 2.5f, ForceMode.Impulse);
    //             isCurve  = true;
    //             break;
    //         case GameBall.PitchMode.Slow:
    //             rigidbody.AddForce(Vector3.forward * 2.5f, ForceMode.Impulse);
    //             isSlow  =true;
    //             break;
    //     }
    //     RPC_ThrewEvent();
    // }
    public void AddPower(Vector3 vector, float power)
    {
        meshRenderer.enabled = true;
        rigidbody.isKinematic = false;
        rigidbody.linearVelocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.AddForce(vector * power, ForceMode.Impulse);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        RPC_HitEvent();
        if (!isActive && collision.gameObject.tag == "BaseBallBat")
        {
            isActive = true;
            // if(false)
            // {    
            //     rigidbody.AddForce(rigidbody.linearVelocity * 1.6f + Vector3.up * 2.2f, ForceMode.Impulse);
            //     RPC_HomeRunEvent();
            // }
        }

        if(collision.gameObject.name == "HomeRun")
        {
            rigidbody.isKinematic = true;
        }

        if(collision.gameObject.tag == "Catcher" && !isActive)
        {
            lifeTime = 0;
            stayTime = 0;
            var judge = GameManager.EJudge.Strike;
            RPC_SendJudge(judge);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        if (other.gameObject.tag == "Judge")
        {
            stayCollider = other;
            isEnterJudge = true;
            stayTime = 0;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        if (other.gameObject.tag == "Judge")
        {
            stayCollider = null;
            isEnterJudge = false;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!GameManager.Instance.Runner.IsServer) return;
        stayCollider = other;
    }

    Collider CheckBelow()
    {
        RaycastHit hit;
        Vector3 rayOrigin = transform.position; // ボールの位置
        Vector3 rayDirection = Vector3.down; // 真下に向ける

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            // レイを可視化（エディタ上で確認用）
            Debug.DrawRay(rayOrigin, rayDirection * rayDistance, Color.red, 2f);
            return hit.collider;
        }
        else
        {
            return null;
        }
    }

    public void Despawn()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_SendJudge(GameManager.EJudge judge)
    {
        if (judge == GameManager.EJudge.None) return;
        isActive = false;
        InterfaceManager.Instance.judgeUI.SetJudge(judge);
        int point = 0;
        switch (judge)
        {
            case GameManager.EJudge.HomeRun:
                point = 4;
                SEManager.Instance.Play(SEPath.OGI_HOMERUN);
                SEManager.Instance.Play(SEPath.BASEBALL_CHEER4);
                break;
            case GameManager.EJudge.BH3:
                point = 3;
                SEManager.Instance.Play(SEPath.OGI_3BH);
                break;
            case GameManager.EJudge.BH2:
                point = 2;
                SEManager.Instance.Play(SEPath.OGI_2BH);
                break;
            case GameManager.EJudge.Hit:
                point = 1;
                SEManager.Instance.Play(SEPath.OGI_HIT);
                break;
            case GameManager.EJudge.Strike:
                point = 0;
                SEManager.Instance.Play(SEPath.OGI_STRIKE);
                break;
            case GameManager.EJudge.Faul:
                point = 0;
                SEManager.Instance.Play(SEPath.OGI_FAUL);
                break;
            case GameManager.EJudge.Out:
                point = 0;
                SEManager.Instance.Play(SEPath.OGI_OUT);
                break;
            case GameManager.EJudge.Ball:
                point = 0;
                SEManager.Instance.Play(SEPath.OGI_BALL);
                break;
        }
        if (Runner.IsServer)
        {
            GameManager.Instance.UpdateGameParamater(point, judge);
        }
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HitEvent()
    {
        SEManager.Instance.Play(SEPath.HIT_BALL);
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_HomeRunEvent()
    {
        GameCameraController.Instance.SetFollowCamera(this.transform, "HomeRun");
    }

    // [Rpc(RpcSources.All, RpcTargets.StateAuthority, HostMode = RpcHostMode.SourceIsHostPlayer)]
    // public void RPC_ThrewReq(GameBall.PitchMode mode)
    // {
    //     Debug.Log("aaaaa");
    //     ChangePichMode(mode);
    // }

    // [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    // public void RPC_ThrewEvent()
    // {
    //     if (NetworkGameManager.Instance.localPlObj.state == PlayerRegistry.PlayerState.Pitcher)
    //     {
    //         GameUIManager.Instance.VisibleNetPitch(false);
    //         NetworkGameManager.Instance.canInput = false; // 投球後は入力不可
    //     }
    //     SEManager.Instance.Play(SEPath.OGI_THREW);
    //     SEManager.Instance.Play(SEPath.THREW_BALL);
    // }
}
