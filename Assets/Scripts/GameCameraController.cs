using System.Collections;
using System.Collections.Generic;
using KanKikuchi.AudioManager;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class GameCameraController : MonoBehaviour
{
    public static GameCameraController Instance;
    [Header("Resources")]
    public CinemachineBrain brain;
    public CinemachineCamera[] cameras;
    public CinemachineCamera[] introCameras;
    public CinemachineCamera DefenceCamera;
    public CinemachineCamera TraceCamera;
    public PlayableDirector startMovie;

    private System.Action<PlayableDirector> registeredEvent;
    private string prevCamera;
    private string currentCamera;

    void Awake()
    {
        Instance = this;
    }
    public CinemachineCamera ChangeCamera(string cameraName)
    {
        if (cameraName == "Batter")
        {
            if (GameManager.IsOnline)
                cameraName += "_" + ResourcesManager.Instance.CurrentMode.ToString().Replace("Online_", "");
            else
                cameraName += "_" + ResourcesManager.Instance.CurrentMode.ToString();
        }

        Debug.Log($"ChangeCamera: {cameraName}");
        bool isFound = false;
        foreach (var camera in cameras)
            if (camera.name == cameraName) isFound = true;
        if (!isFound) return null;
        prevCamera = currentCamera;
        CinemachineCamera returnCamera = null;
        foreach (var camera in cameras)
        {
            camera.Priority.Value = 0;
            if (camera.name == cameraName)
            {
                camera.Priority.Value = 1;
                returnCamera = camera;
            }
        }
        currentCamera = cameraName;
        return returnCamera;
    }

    public IEnumerator IntroRoutine()
    {
        SEManager.Instance.Play(SEPath.SILEN_FADE);
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.Linear, 3f);
        introCameras[0].Priority.Value = 1;
        yield return new WaitForSeconds(4);
        introCameras[0].Priority.Value = 0;
        introCameras[1].Priority.Value = 1;
        yield return new WaitForSeconds(3);
        SEManager.Instance.Stop(SEPath.SILEN);
        introCameras[1].Priority.Value = 0;
        introCameras[2].Priority.Value = 1;
        yield return new WaitForSeconds(4);
        brain.DefaultBlend = new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut, 1f);
    }

    public IEnumerator ChangeRoutine(float duration = 3f)
    {
        ChangeCamera("Change");
        yield return new WaitForSeconds(duration);
        ChangeCamera(prevCamera);
    }

    public void MovieStart(System.Action<PlayableDirector> stoppedEvent)
    {
        startMovie.stopped -= registeredEvent;
        registeredEvent = stoppedEvent;
        startMovie.stopped += stoppedEvent;
        startMovie.Play();
    }

    public void OnDisable()
    {
        if (registeredEvent != null)
            startMovie.stopped -= registeredEvent;
    }

    public void SetFollowCamera(Transform followTarget, string cameraName)
    {
        var camera = ChangeCamera(cameraName);
        camera.Follow = followTarget;
        camera.LookAt = followTarget;
    }

    public void SetTraceCamera(Transform followTarget)
    {
        TraceCamera.Follow = followTarget;
        TraceCamera.LookAt = followTarget;
    }
}
