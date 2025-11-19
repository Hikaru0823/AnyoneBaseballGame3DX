using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneController : MonoBehaviour
{
    [SerializeField] private Animator titleIconAnim;
    [SerializeField] private Toggle infToggle;
    void Start()
    {
        titleIconAnim.Play(ResourcesManager.PANEL_IN);
        infToggle.isOn = ES3.Load<bool>(SaveKeys.IsINF, false);
    }

    public void OnStartButtonPushed(int sceneIndex)
    {
        SceneManager.LoadScene(1);
        //StartCoroutine(LoadSceneRoutine(sceneIndex));
    }

    public void SceneLoad(string modestring)
    {
        GameManager.EMode mode = (GameManager.EMode)Enum.Parse(typeof(GameManager.EMode), modestring);
        ES3.Save<GameManager.EMode>(SaveKeys.GameMode, mode);
        SceneManager.LoadScene(1);
    }

    IEnumerator LoadSceneRoutine(int sceneIndex)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneIndex, LoadSceneMode.Single);

        while (!asyncLoad.isDone)
        {
            Debug.Log(asyncLoad.progress);
            yield return null;
        }
    }
    
    public void INFToggleChanged(bool isOn)
    {
        ES3.Save<bool>(SaveKeys.IsINF, isOn);
    }
}
