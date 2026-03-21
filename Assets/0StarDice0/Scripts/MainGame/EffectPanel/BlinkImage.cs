using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkImage : MonoBehaviour
{
    public Image imageToBlink;
    public float blinkInterval = 0.5f;

    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            imageToBlink.enabled = !imageToBlink.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
