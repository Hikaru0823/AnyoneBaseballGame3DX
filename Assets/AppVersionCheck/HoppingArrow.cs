using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoppingArrow : MonoBehaviour
{
    RectTransform rt;

    private void Start()
    {
        rt = GetComponent<RectTransform>();
        StartCoroutine(Hopping_Coroutine());
    }

    IEnumerator Hopping_Coroutine()
    {
        var pos = rt.transform.localPosition;
        var anim = new AnimationCurve(new Keyframe(0, 20), new Keyframe(1, -20));
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            rt.transform.localPosition = new Vector3(pos.x + anim.Evaluate(time), pos.y, pos.z);
            if (time > 1)
                time = 0;
            yield return null;
        }

    }
}
