using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private PlayerButton playerButton;
    [SerializeField] private Transform batterUITransform;
    private bool isSavedPos = true;
    private bool isSavedScale = true;
    Vector3 savedPosition;
    Vector3 savedScale;
    Vector3 initialPosition;
    Vector3 initialScale;

    void Awake()
    {
        savedPosition = ES3.Load<Vector3>(SaveKeys.PlayerButtonPositionLocal, defaultValue: default);
        isSavedPos = savedPosition != default;
        savedScale = ES3.Load<Vector3>(SaveKeys.PlayerButtonScaleLocal, defaultValue: default);
        isSavedScale = savedScale != default;
        initialScale = new Vector3(1, 1, 1);
        batterUITransform.localScale = isSavedScale ? savedScale : initialScale;
    }

    private readonly Dictionary<GameManager.EMode, Vector3> BatterUIPosByMode = new Dictionary<GameManager.EMode, Vector3>()
    {
        { GameManager.EMode.BarrierFree, new Vector3(0, 280, 0) },
        { GameManager.EMode.Normal, new Vector3(-420, -200, 0) },
        { GameManager.EMode.Duo, new Vector3(0, -3.25f, 10.3f) },
        { GameManager.EMode.Online_BarrierFree, new Vector3(0, 280, 0) },
        { GameManager.EMode.Evaluation, new Vector3(0, 150, 0) },
        { GameManager.EMode.Derby,new Vector3(0, -10000, 0) },
        { GameManager.EMode.Universal,new Vector3(0, 280, 0) },
        {GameManager.EMode.Online_Universal,new Vector3(0, 280, 0)}
    };

    private void ChangePlayerButtonPosition(PlayerObject.EPlayerState playerState)
    {
        switch (playerState)
        {
            case PlayerObject.EPlayerState.Batter:
                InterfaceManager.ShowPanel(animator);
                initialPosition = BatterUIPosByMode[ResourcesManager.Instance.CurrentMode];
                if(!isSavedPos)
                {
                    batterUITransform.localPosition = initialPosition;
                    isSavedPos = true;
                }
                batterUITransform.localPosition = !batterUITransform.localPosition.Equals(initialPosition) ? savedPosition : initialPosition;
                break;
            case PlayerObject.EPlayerState.Defence:
                InterfaceManager.ShowPanel(animator);
                initialPosition = new Vector3(0, -200, 0); // Example position for Defence
                if(!isSavedPos)
                {
                    batterUITransform.localPosition = initialPosition;
                    isSavedPos = true;
                }
                batterUITransform.localPosition = !batterUITransform.localPosition.Equals(initialPosition) ? savedPosition : initialPosition; // Example position for Defence
                break;
            case PlayerObject.EPlayerState.None:
                InterfaceManager.ShowPanel(animator);
                break;
        }
    }

    public void SetButtonState(PlayerObject.EPlayerState playerState)
    {
        playerButton.SetState(playerState);
        ChangePlayerButtonPosition(playerState);
    }
    
    void OnDisable()
    {
        if(!batterUITransform.localPosition.Equals(initialPosition))
            ES3.Save<Vector3>(SaveKeys.PlayerButtonPositionLocal, batterUITransform.localPosition);
        if(!batterUITransform.localScale.Equals(initialScale))
            ES3.Save<Vector3>(SaveKeys.PlayerButtonScaleLocal, batterUITransform.localScale);
    }
}
