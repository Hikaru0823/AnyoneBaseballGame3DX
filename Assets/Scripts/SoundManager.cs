using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    public Image stateImage;
    public Sprite[] stateSprites;
    public int stateNum = 0; //0: OK-OK, 1: NO-OK,, 2: OK-NO,, 3: NO-NO
    public float baseBGMVolume = 0.3f;
    public float baseSEVolume = 1f;
    void Start()
    {
        BGMManager.Instance.Play(BGMPath.COMPETE);
        ChangeState(0);
        SceneManager.activeSceneChanged += OnSceneLoades;
    }

    public void OnSceneLoades(Scene preScene, Scene nextScene)
    {
        if(nextScene.buildIndex == 0)
            BGMManager.Instance.Play(BGMPath.COMPETE);
    }

    public void OnButtonClicked()
    {
        stateNum++;
        if(stateNum == 4)
            stateNum = 0;
        ChangeState(stateNum);
    }
    void ChangeState(int num)
    {
        stateImage.sprite = stateSprites[num];
        switch(stateNum)
        {
            case 0:
                BGMManager.Instance.ChangeBaseVolume(baseBGMVolume);
                SEManager.Instance.ChangeBaseVolume(baseSEVolume);
                break;
            case 1:
                BGMManager.Instance.ChangeBaseVolume(0);
                SEManager.Instance.ChangeBaseVolume(baseSEVolume);
                break;
            case 2:
                BGMManager.Instance.ChangeBaseVolume(baseBGMVolume);
                SEManager.Instance.ChangeBaseVolume(0);
                break;
            case 3:
                BGMManager.Instance.ChangeBaseVolume(0);
                SEManager.Instance.ChangeBaseVolume(0);
                break;
        }
    }
}
