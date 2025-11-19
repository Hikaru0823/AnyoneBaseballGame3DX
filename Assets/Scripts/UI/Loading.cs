using UnityEngine;
using UnityEngine.UIElements;

public class Loading : Singleton<Loading>
{
    [SerializeField] private Canvas loadingCanvas;

    public void SetVisible(bool isVisible)
    {
        loadingCanvas.enabled = isVisible;
    }
}
