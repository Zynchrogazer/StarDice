using UnityEngine;
using TMPro;
using System.Collections;

public class BlinkTMPText : MonoBehaviour
{
    public TextMeshProUGUI tmpText;
    public float blinkInterval = 0.5f;

    void Start()
    {
        StartCoroutine(Blink());
    }

    IEnumerator Blink()
    {
        while (true)
        {
            tmpText.enabled = !tmpText.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }
}
