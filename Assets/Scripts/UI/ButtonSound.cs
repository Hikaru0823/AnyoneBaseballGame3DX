using KanKikuchi.AudioManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        SEManager.Instance.Play(SEPath.CLICK);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SEManager.Instance.Play(SEPath.HOVER);
    }
}
