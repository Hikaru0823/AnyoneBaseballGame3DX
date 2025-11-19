using System;
using System.Collections.Generic;
using ES3Types;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyAssignManager : MonoBehaviour
{
    [SerializeField] private KeyAssignData[] assignData;
    [SerializeField] private InputActionAsset asset;
    [SerializeField] private GameObject keyAssignContentPrefab;
    private bool isWaitingForKey = false;
    private IDisposable op;

    void Start()
    {
        if(FindFirstObjectByType<KeyAssignContent>() == null)
            Instantiate(keyAssignContentPrefab, transform);
        KeyAssignContent.Instance.Map = asset.FindActionMap("Player");
        foreach (var data in assignData)
        {
            if (KeyAssignContent.Instance.Map.FindAction(data.ActionName) == null)
                KeyAssignContent.Instance.Map.AddAction(data.ActionName, InputActionType.Button);
            string binding = ES3.Load<string>(SaveKeys.Binding + data.ActionName, defaultValue: "");
            string currentBinding = string.IsNullOrEmpty(binding) ? data.DefaultBinding : binding;
            data.CurrentBinding = currentBinding;
            KeyAssignContent.Instance.Map[data.ActionName].AddBinding(currentBinding);
            data.KeyText.text = InputControlPath.ToHumanReadableString(currentBinding, InputControlPath.HumanReadableStringOptions.OmitDevice);
            data.AssignButton.onClick.AddListener(() =>
            {
                StartRebind(data);
            });
        }

        KeyAssignContent.Instance.Map.Enable();
    }

    public void StartRebind(KeyAssignData data)
    {
        if (isWaitingForKey) return;
        isWaitingForKey = true;

        KeyAssignContent.Instance.Map.Disable();
        var action = KeyAssignContent.Instance.Map[data.ActionName];

        // 既存の進行中オペレーションがあれば破棄
        op?.Dispose();

        // RebindingOperation を構築
        op = action.PerformInteractiveRebinding()
            // Esc でキャンセル
            .WithCancelingThrough("<Keyboard>/escape")
            // 候補を見つけた瞬間に呼ばれる（競合チェックなどに便利）
            .OnPotentialMatch(o =>
            {
                var selected = o.selectedControl;
                // 必要ならここで「そのキーは使用中です」等を判定→ o.Cancel()
            })
            // 実際にバインディングへ反映する直前（文字列の整形など）
            .OnApplyBinding((o, newPath) =>
            {
                KeyAssignContent.Instance.Map[data.ActionName].ApplyBindingOverride(newPath, path: data.CurrentBinding);
                data.KeyText.text = InputControlPath.ToHumanReadableString(newPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                ES3.Save(SaveKeys.Binding + data.ActionName, newPath);
            })
            // 完了時
            .OnComplete(o =>
            {
                o.Dispose();
                op = null;
                isWaitingForKey = false;
                KeyAssignContent.Instance.Map.Enable();
            })
            // キャンセル時（Esc 等）
            .OnCancel(o =>
            {
                o.Dispose();
                op = null;
                isWaitingForKey = false;
                KeyAssignContent.Instance.Map.Enable();
            })
            .Start();
    }
}

[Serializable]
public class KeyAssignData
{
    public string ActionName;
    public string DefaultBinding;
    public string CurrentBinding;
    public TMP_Text KeyText;
    public Button AssignButton;
}
