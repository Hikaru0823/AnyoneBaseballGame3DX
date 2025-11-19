using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Resource")]
    public Animator buttonAnim;
    [SerializeField] private Image[] stateImages;
    [SerializeField] private Image bgImage;
    [SerializeField] private CanvasGroup group;
    public static PlayerButton Instance { get; private set; }
	private void Awake()
	{
		if (Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
	}

    public void OnPointerEnter(PointerEventData eventData)
    {
        // if (ES3.Load<SceneController.GameMode>("GameMode") == SceneController.GameMode.Online)
        //     NetworkGameManager.Instance.isInput = true;
        // else
        //     GameManager.Instance.OnInput();
        OnInput();
        buttonAnim.Play("MouseIn");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        buttonAnim.Play("MouseOut");
    }

    public void OnInput()
    {
        if (PlayerObject.Local == null || PlayerObject.Local.Controller == null) return;
        PlayerObject.Local.Rpc_OnInput();
    }

    public void SetState(PlayerObject.EPlayerState state)
    {
        foreach (var image in stateImages)
            image.enabled = false;
        switch (state)
        {
            // case PlayerObject.EPlayerState.Pitcher:
            //     stateImages[0].enabled = true;
            //     bgImage.color = Color.white;
            //     break;
            case PlayerObject.EPlayerState.Batter:
                stateImages[1].enabled = true;
                group.alpha = 1f;
                bgImage.color = new Color(1f, 0.5f, 0.5f); // Orange color
                break;
            case PlayerObject.EPlayerState.Defence:
                stateImages[2].enabled = true;
                group.alpha = 0.5f;
                bgImage.color = new Color(0.5f, 0.5f, 1f); // Blue color
                break;
            case PlayerObject.EPlayerState.None:
                group.alpha = 0f;
                break;
        }
    }
}
