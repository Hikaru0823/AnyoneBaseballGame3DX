using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NameInput : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI nameText = default;
    [SerializeField] private NamePanel namePanel = default;
    [SerializeField] private NickName nickName = default;
    [SerializeField] private GameObject NamePanel;
    //private TitleDirector titleDirector = default;

    public void OnNameChanged(string newName)
    {
        nickName.Set_Value(newName);
    }

    public void Init(string currentName)
    {
        this.nameText.text = currentName;
        this.namePanel.Init(currentName);
        NamePanel.SetActive(false);
    }

    public void OnButtonClicked(Button button)
    {
        switch (button.name)
        {
            case "StartButton":

                //this.transform.SetAsLastSibling();
                SetActive_PanelController(true);
                break;

            default:
                break;
        }
    }

    public void SetActive_PanelController(bool is_active)
    {
        NamePanel.SetActive(is_active);
    }
}
