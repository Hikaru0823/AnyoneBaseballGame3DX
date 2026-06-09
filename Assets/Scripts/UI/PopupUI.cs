using System;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PopupUI : MonoBehaviour
{
	public static PopupUI Instance { get; private set; }

	public GameObject ui;
	public TMP_Text title;
	public TMP_Text description;
	public Button okButton;
    public Button noButton;
	public Image icon;
	public Sprite defaultIcon;

	private void Awake()
	{
		if (Instance)
		{
			Destroy(gameObject);
			return;
		}
		Instance = this;
		noButton.onClick.AddListener(CloseButton);
	}

    public static void OnVisible(string title, string description, Sprite icon = null, UnityEngine.Events.UnityAction onClose = null)
    {
        Instance.title.text = title;
        Instance.description.text = description;
        Instance.icon.sprite = icon ?? Instance.defaultIcon;

        Instance.ui.SetActive(true);
        Cursor.lockState = CursorLockMode.None;

        Instance.okButton.onClick.RemoveAllListeners();
        if (onClose != null)
        {
            Instance.okButton.onClick.AddListener(() =>
            {
                onClose.Invoke();
                Instance.CloseButton();
            });
        }
    }

	private void CloseButton()
	{
		ui.SetActive(false);
	}
}