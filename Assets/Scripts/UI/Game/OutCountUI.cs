using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class OutCountUI : MonoBehaviour
{
    public Animator animator;
    [SerializeField] private Image[] outCounts;

    public void SetOutCount(int count)
    {
        for (int i = 0; i < outCounts.Length; i++)
        {
            if (i < count)
            {
                outCounts[i].color = Color.red; // Out count is red
            }
            else
            {
                outCounts[i].color = Color.black; // Remaining counts are white
            }
        }
    }
}