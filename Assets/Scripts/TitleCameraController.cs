using UnityEngine;
using Unity.Cinemachine;

public class TitleCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera titleCamera;
    [SerializeField] private CinemachineCamera playerEditCamera;
    [SerializeField] private Transform targetObject; // カメラが周回するターゲット
    [SerializeField] private float orbitDistance = 3f; // ターゲットからの距離
    [SerializeField] private float orbitSpeed = 30f; // 周回速度（度/秒）
    [SerializeField] private float orbitHeight = 1.5f; // ターゲットからの高さ

    [SerializeField] CinemachineCamera testCamera;

    private float currentAngle = 0f;

    private void Update()
    {
        
        // // TitleCameraがアクティブな場合のみ周回
        // if (testCamera.Priority > playerEditCamera.Priority && targetObject != null)
        // {
        //     UpdateTitleCameraOrbit();
        // }
    }

    private void UpdateTitleCameraOrbit()
    {
        // 角度を更新
        currentAngle += orbitSpeed * Time.deltaTime;
        
        // ラジアンに変換
        float radian = currentAngle * Mathf.Deg2Rad;
        
        // カメラの新しい位置を計算（ターゲットの周りを円形に周回）
        float xOffset = Mathf.Sin(radian) * orbitDistance;
        float zOffset = Mathf.Cos(radian) * orbitDistance;
        
        Vector3 targetPos = targetObject.position;
        Vector3 cameraPos = new Vector3(
            targetPos.x + xOffset,
            targetPos.y + orbitHeight,
            targetPos.z + zOffset
        );
        
        // カメラを配置
        testCamera.transform.position = cameraPos;
        
        // カメラをターゲットに向かせる
        testCamera.transform.LookAt(targetObject.position);
    }

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
