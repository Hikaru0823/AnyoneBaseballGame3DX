using UnityEngine;
using UnityEngine.InputSystem;

public class KeyAssignContent : MonoBehaviour
{
    public static KeyAssignContent Instance { get; private set; }
    public InputActionMap Map;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
