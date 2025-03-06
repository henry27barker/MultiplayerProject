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

    [SerializeField] private GameObject hockeyPuckPrefab;
    [SerializeField] private Transform puckSpawnPoint;

    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;

    private int player1Score = 0;
    private int player2Score = 0;

    public void ScoreGoal(int player)
    {
        if (player == 1)
        {
            player1Score++;
        }
        else if (player == 2)
        {
            player2Score++;
        }

        // Update UI
        player1ScoreText.text = player1Score.ToString();
        player2ScoreText.text = player2Score.ToString();

        // Restart game after short delay
        StartCoroutine(ResetGame());
    }

    private IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(2f); // Short delay before reset

        // Destroy current puck
        GameObject existingPuck = GameObject.FindGameObjectWithTag("Puck");
        if (existingPuck != null)
        {
            Destroy(existingPuck);
        }

        // Spawn new puck at center
        GameObject newPuck = Instantiate(hockeyPuckPrefab, puckSpawnPoint.position, Quaternion.identity);
        newPuck.tag = "Puck";
        puckRb = newPuck.GetComponent<Rigidbody>(); // Update Rigidbody reference

        // Wait a moment before launching the puck
        yield return new WaitForSeconds(1f);

        LaunchPuck();
    }

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
        float angle = GetValidLaunchAngle();
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)); // Convert angle to vector

        puckRb.velocity = new Vector3(direction.x, 0, direction.y) * launchSpeed;
        gameStarted = true;
    }

    private float GetValidLaunchAngle()
    {
        float[] validAngles = { 
            Random.Range(30f, 60f),     // Top-right
            Random.Range(120f, 150f),   // Top-left
            Random.Range(210f, 240f),   // Bottom-left
            Random.Range(300f, 330f)    // Bottom-right
        };

        return validAngles[Random.Range(0, validAngles.Length)] * Mathf.Deg2Rad; // Convert to radians
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
