using System.Collections;
using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.UI;

public class Pitcher : MonoBehaviour
{
    [SerializeField] private Canvas gazeCanvas;
    [SerializeField] private Image gaze;
    public float DerbyTraceTime = 6f; // Speed of the Derby trace movement
    public static Pitcher Instance { get; private set; }
    public Animator animator;
    bool isGazeStarted = false;
    public bool isDerbyTrace = false;
    float gazeTime;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    void FixedUpdate()
    {
        if (isGazeStarted)
        {
            gaze.fillAmount += Time.deltaTime / gazeTime;
            if (gaze.fillAmount >= 1f)
            {
                isGazeStarted = false;
            }
        }

        if (isDerbyTrace)
        {
            ResourcesManager.Instance.DerbyTraceObj.position += Vector3.forward * Time.deltaTime / (DerbyTraceTime / 2f);
        }
    }

    public IEnumerator ThrewAnimateRoutine()
    {
        gazeCanvas.enabled = true;
        gaze.fillAmount = 0;
        float duration = Random.Range(1.5f, 3.5f);
        gazeTime = duration + 1.3f;
        isGazeStarted = true;
        yield return new WaitForSeconds(duration);
        animator.SetBool("isThrow", true);
    }

    public void ThrewBall()
    {
        isGazeStarted = false;
        gazeCanvas.enabled = false;
        animator.SetBool("isThrow", false);
        if (GameManager.Instance.netBall != null)
        {
            SEManager.Instance.Play(SEPath.THREW_BALL);
            SEManager.Instance.Play(SEPath.OGI_THREW);
            if (ResourcesManager.Instance.CurrentMode == GameManager.EMode.Derby)
            {
                InterfaceManager.HidePanel(InterfaceManager.Instance.vsPanel.animator);
                InterfaceManager.HidePanel(InterfaceManager.Instance.outCountUI.animator);
                InterfaceManager.HidePanel(InterfaceManager.Instance.currentBatterUI.animator);
                GameCameraController.Instance.SetFollowCamera(ResourcesManager.Instance.DerbyTraceObj, "Batter");
                GameManager.Instance.netBall.meshRenderer.enabled = true;
                isDerbyTrace = true;
                return;
            }
            GameManager.Instance.netBall.AddPower(Vector3.forward, 2f);
        }
    }
}
