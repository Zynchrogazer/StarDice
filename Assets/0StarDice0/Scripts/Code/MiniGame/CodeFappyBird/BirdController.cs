using UnityEngine;
using TMPro;

public class BirdController : MonoBehaviour
{
    public float jumpForce = 5f;
    public Rigidbody rb;
    public TextMeshProUGUI startText;

    private bool isStarted = false;
    private bool isGameOver = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        Time.timeScale = 0f;
        startText.gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isStarted)
        {
            if (Input.GetKeyDown(KeyCode.Space)) // ← กด Space เพื่อเริ่ม
            {
                StartGame();
            }
            return;
        }

        // บังคับบินเมื่อเกมเริ่ม
        if (!isGameOver && Input.GetKeyDown(KeyCode.Space)) // ← กด Space เพื่อบิน
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void StartGame()
    {
        isStarted = true;
        Time.timeScale = 1f;
        rb.useGravity = true;
        startText.gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isGameOver)
        {
            isGameOver = true;
            Debug.Log("Game Over!");
            Time.timeScale = 0f;
            GameManager.instance.GameOver();

        }
    }

}
