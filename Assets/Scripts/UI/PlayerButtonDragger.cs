using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerButtonDragger : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public bool isDragging = false;
    [SerializeField] private Transform target;
    [SerializeField] private RectTransform canvasRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        target.localPosition += new Vector3(eventData.delta.x, eventData.delta.y, 0);
        ClampToCanvas();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        ES3.Save<Vector3>(SaveKeys.PlayerButtonPositionLocal, target.localPosition);
    }

    void ClampToCanvas()
    {
        Vector2 pos = target.localPosition;

        // Canvas の半サイズ
        Vector2 canvasHalf = canvasRect.rect.size * 0.5f;
        // Image の半サイズ
        Vector2 rect = target.GetComponent<RectTransform>().rect.size * target.localScale.x;

        // Clamp（Image が完全に画面内に収まるように）
        pos.x = Mathf.Clamp(pos.x, -canvasHalf.x, canvasHalf.x - rect.x);
        pos.y = Mathf.Clamp(pos.y, -canvasHalf.y, canvasHalf.y - rect.y);

        target.localPosition = pos;
    }
}
