using UnityEngine;

public class SpectatorUI : MonoBehaviour
{
    public Animator animator;

    public void OnButtonClicked(string name)
    {
        GameCameraController.Instance.ChangeCamera(name);
    }
}
