using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KanKikuchi.AudioManager;

public class SessionItemUI : MonoBehaviour
{
    [SerializeField] private TMP_Text sessionNameLabel;
    [SerializeField] private TMP_Text PlayerCount;
    //[SerializeField] private TMP_Text statusText;
    [SerializeField] private Image statusIcon;
    [SerializeField] private Button[] joinButtons;
    [HideInInspector] public string sessionName = null;

    private bool _isActive = false;

    public void Init(string sessionName, int Players, bool isOpen)
    {
        if (Players > 0)
            _isActive = true;
        foreach (var joinButton in joinButtons)
            joinButton.interactable = isOpen;
        gameObject.SetActive(true);
        sessionNameLabel.text = this.sessionName = sessionName;
        PlayerCount.text = $"{Players}";
        statusIcon.enabled = !isOpen;
    }
    public void Join(bool IsWhite)
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ResourcesManager.Instance.IsSpectator = false;
        ResourcesManager.Instance.isWhite = IsWhite;
        if (_isActive)
            LobbyManager.Instance.TryJoinSession(sessionName);
        else
            LobbyManager.Instance.TryHostSession(sessionName);
    }
    
    public void Join()
    {
        SEManager.Instance.Play(SEPath.CLICK);
        ResourcesManager.Instance.IsSpectator = true;
        if (_isActive)
            LobbyManager.Instance.TryJoinSession(sessionName);
        else
            LobbyManager.Instance.TryHostSession(sessionName);
    }
}
