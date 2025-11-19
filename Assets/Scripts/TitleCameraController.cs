using UnityEngine;
using Unity.Cinemachine;

public class TitleCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera titleCamera;
    [SerializeField] private CinemachineCamera playerEditCamera;

    public void SetCameraToTitle()
    {
        titleCamera.Priority = 1;
        playerEditCamera.Priority = 0;
    }

    public void SetCameraToPlayerEdit()
    {
        titleCamera.Priority = 0;
        playerEditCamera.Priority = 1;
    }
}
