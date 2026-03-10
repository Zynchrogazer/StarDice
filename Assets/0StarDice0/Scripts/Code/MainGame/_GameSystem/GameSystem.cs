using UnityEngine;

public class GameSystem : MonoBehaviour
{
    private void Awake()
    {
        // Scene-local system: intentionally no global singleton.
        // Object lifecycle should follow scene load/unload only.
    }
}
