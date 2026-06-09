using ExitGames.Client.Photon.StructWrapping;
using Fusion;
using UnityEngine;

public class NetBoard : NetworkBehaviour
{
    public NetBall netBall;
    [SerializeField] private float _angularVelocity = 2.5f;

    public override void FixedUpdateNetwork()
    {
        transform.Rotate(Vector3.down * _angularVelocity, Space.World);
    }
    public void Despawn()
    {
        Runner.Despawn(GetComponent<NetworkObject>());
    }
}