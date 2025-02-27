using System.Collections;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject hockeyPuck;
    [SerializeField] private int startTimer = 3;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private TextMeshProUGUI countdownText; // TextMeshPro UI for countdown
    [SerializeField] private AudioSource countdownBeep; // Beep sound on each second

    private float startCounter;
    private Rigidbody puckRb;
    private bool gameStarted = false;

    void Start()
    {
        puckRb = hockeyPuck.GetComponent<Rigidbody>();
        startCounter = startTimer;

        // Start countdown coroutine
        StartCoroutine(StartCountdown());
    }

    private void FixedUpdate()
    {
        if (gameStarted)
        {
            MaintainMinSpeed();
        }
    }

    IEnumerator StartCountdown()
    {
        while (startCounter > 0)
        {
            // Update UI countdown
            countdownText.text = startCounter.ToString();

            // Play countdown beep
            if (countdownBeep != null)
                countdownBeep.Play();

            yield return new WaitForSeconds(1f);
            startCounter--;
        }

        // Hide countdown UI
        countdownText.text = "";

        // Launch the puck
        LaunchPuck();
    }

    private void LaunchPuck()
    {
        Vector3 randomDirection = Random.insideUnitCircle.normalized; // Random 2D direction
        puckRb.velocity = new Vector3(randomDirection.x, 0, randomDirection.y) * launchSpeed;

        gameStarted = true;
    }

    private void MaintainMinSpeed()
    {
        float currentSpeed = puckRb.velocity.magnitude;

        if (currentSpeed < minSpeed)
        {
            puckRb.velocity = puckRb.velocity.normalized * minSpeed;
        }
    }
}
