using UnityEngine;
using UnityEngine.UI;

public class MemoryButton : MonoBehaviour
{
    public int index;
    public MemoryGameManager gameManager;

    public void OnClick()
    {
        gameManager.OnButtonPressed(index);
    }
}
