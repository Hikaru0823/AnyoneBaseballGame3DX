using UnityEngine;
using UnityEngine.UI;

public class NetConnectionChecker : MonoBehaviour
{
    [Header("Resouces")]
    public Button targetButton;
    public Image[] netImages; //0:false, 1:true

    void LateUpdate()
    {
        if(Application.internetReachability == NetworkReachability.NotReachable)
        {
            SetState(false);
        }
        else
        {
            SetState(true);
        }
    }

    void SetState(bool isConnect)
    {
        targetButton.interactable = isConnect;
        netImages[0].enabled = !isConnect;
        netImages[1].enabled = isConnect;
    }
}
