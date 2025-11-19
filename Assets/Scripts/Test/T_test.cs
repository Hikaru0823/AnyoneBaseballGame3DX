using UnityEngine;

public class T_test : MonoBehaviour
{
    private bool isLoop = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isLoop)
            {
                isLoop = false;
                Debug.Log("Stopping loop...");
                StopCoroutine(Loop());
            }
            else
            {
                isLoop = true;
                Debug.Log("Starting loop...");
                StartCoroutine(Loop());
            }
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            StartCoroutine(Loop());
        }
    }

    System.Collections.IEnumerator Loop()
    {
        while (isLoop)
        {
            Debug.Log("Looping...");
            yield return new WaitForSeconds(1f);
        }
    }
}
