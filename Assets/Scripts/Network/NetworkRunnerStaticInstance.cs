using Fusion;
using UnityEngine;

public class NetworkRunnerStaticInstance : SimulationBehaviour
{
    public static NetworkRunner Instance;
    void Awake()
    {
        Instance = GetComponent<NetworkRunner>();
    }

    public void tartGame()
    {

    }
}
