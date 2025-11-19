using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ExitManager : Singleton<ExitManager>
{
    [Header("Resouces")]
    public Button exitButton;
    public Button returnButton;
    public Animator returnButtonAnimation;
    public Animator returnPanelAnimation;
    public Animator exitPanelAnimation;

    private Vector3 initPos;
    string panelFadeIn = "Panel In";
    string panelFadeOut = "Panel Out";

    void Start()
    {
        initPos = returnButton.transform.GetComponent<RectTransform>().localPosition;
        SceneManager.activeSceneChanged += OnSceneLoades;
    }

    public void SetReturnButtonVisible(bool isVisible)
    {
        if (isVisible)
            returnButtonAnimation.Play(panelFadeIn);
        else
            returnButtonAnimation.Play(panelFadeOut);
    }

    public void SetLeftPos()
    {
        returnButton.GetComponent<RectTransform>().localPosition = initPos + Vector3.left * 300;
    }

    public void SetInitPos()
    {
        returnButton.GetComponent<RectTransform>().localPosition = initPos;
    }

    public void OnSceneLoades(Scene preScene, Scene nextScene)
    {
        returnButton.onClick.RemoveAllListeners();
        if (nextScene.buildIndex == 0)
        {
            returnButtonAnimation.Play(panelFadeOut);
        }
        else if (nextScene.buildIndex == 1)
        {
            returnButtonAnimation.Play(panelFadeIn);
            SetLeftPos();
            returnButton.onClick.AddListener(() =>
            {
                if (FindFirstObjectByType<NetworkRunner>().GameMode != GameMode.Single)
                {
                    returnPanelAnimation.Play(panelFadeIn);
                }
                else
                {
                    FindFirstObjectByType<NetworkRunner>().Shutdown();
                    SceneManager.LoadScene(0);
                }
            });
        }
        else if (nextScene.buildIndex == 2)
        {
            SetLeftPos();
            returnButton.onClick.AddListener(() => SceneManager.LoadScene(0));
        }
    }

    public void OnExitButtonClicked()
    {
#if UNITY_WEBGL
        // WebGLの場合は何もしない
        return;
#elif UNITY_EDITOR
        // エディタの場合は再生停止
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルド版の場合はアプリケーション終了
        if (FindFirstObjectByType<NetworkRunner>())
        {
            if (FindFirstObjectByType<NetworkRunner>().GameMode  != GameMode.Single)
            {
                exitPanelAnimation.Play(panelFadeIn);
            }
            else
            {
                Application.Quit();
            }
        }
        else
        {
            Application.Quit();
        }
#endif
    }

    public void OnReturnButtonDecide(bool yes)
    {
        returnPanelAnimation.Play(panelFadeOut);
        if (yes)
        {
            FindFirstObjectByType<NetworkRunner>().Shutdown();
            SceneManager.LoadScene(0);
        }
    }

    public void OnExitButtonDecide(bool yes)
    {
        exitPanelAnimation.Play(panelFadeOut);
        if (yes)
        {
            Application.Quit();
        }
    }
}
