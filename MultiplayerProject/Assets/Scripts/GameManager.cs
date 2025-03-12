using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro; // Import TextMeshPro namespace

public class GameManager : NetworkBehaviour
{
    [SerializeField] private GameObject hockeyPuckPrefab;
    [SerializeField] private Transform puckSpawnPoint;
    [SerializeField] private int startTimer = 3;
    [SerializeField] private float launchSpeed = 5f;
    [SerializeField] private float minSpeed = 2f;
    [SerializeField] private TextMeshProUGUI countdownText;
    [SerializeField] private AudioSource countdownBeep;
    [SerializeField] private AudioSource goalSound;
    [SerializeField] private AudioSource victorySound;
    [SerializeField] private AudioSource loseSound;
    [SerializeField] private TMP_Text player1ScoreText;
    [SerializeField] private TMP_Text player2ScoreText;
    [SerializeField] private TMP_Text winText;

    private NetworkVariable<int> player1Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> player2Score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> playerCount = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    private int winningScore = 7;
    private float startCounter;
    private bool gameStarted = false;
    private GameObject puckInstance;
    private Rigidbody puckRb;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.OnClientDisconnectCallback += OnClientDisconnected;
        }
        
        winText.text = ""; // Ensure it's empty at the start
        player1Score.OnValueChanged += (oldValue, newValue) => player1ScoreText.text = newValue.ToString();
        player2Score.OnValueChanged += (oldValue, newValue) => player2ScoreText.text = newValue.ToString();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (IsServer)
        {
            playerCount.Value++;
            if (playerCount.Value == 2)
            {
                SpawnPuck();
                StartCoroutine(StartCountdown());
            }
        }

        // Ensure the client gets the current score values when they connect
        UpdateScoresOnClientJoin();
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (IsServer)
        {
            playerCount.Value--;
            gameStarted = false; // Stop the game if a player leaves
        }
    }

    private void UpdateScoresOnClientJoin()
    {
        if (!IsServer) return;

        // Update the score texts for all clients (this is called when a client connects)
        player1ScoreText.text = player1Score.Value.ToString();
        player2ScoreText.text = player2Score.Value.ToString();
    }

    [ServerRpc]
    public void ScoreGoalServerRpc(int player)
    {
        if (player == 1)
        {
            player1Score.Value++;
        }
        else if (player == 2)
        {
            player2Score.Value++;
        }

        // Play goal sound after a short delay
        PlayGoalSoundClientRpc();

        if (player1Score.Value >= winningScore)
        {
            StartCoroutine(WinGame(1));
        }
        else if (player2Score.Value >= winningScore)
        {
            StartCoroutine(WinGame(2));
        }
        else
        {
            StartCoroutine(ResetGame());
        }
    }

    [ClientRpc]
    private void PlayGoalSoundClientRpc()
    {
        StartCoroutine(DelayedGoalSound());
    }

    private IEnumerator DelayedGoalSound()
    {
        yield return new WaitForSeconds(0.5f); // Wait for half a second

        if (goalSound != null)
        {
            goalSound.Play();
        }
    }


    private IEnumerator WinGame(int player)
    {
        // Update win text on all clients
        WinGameClientRpc(player);

        // Play victory and lose sounds properly
        PlayWinLoseSoundClientRpc(player);

        // Wait for the duration of the sounds (~7.5 seconds) before restarting
        yield return new WaitForSeconds(7.5f);

        player1Score.Value = 0;
        player2Score.Value = 0;

        // Clear win text on all clients
        ClearWinTextClientRpc();

        // Restart the game after the delay
        StartCoroutine(ResetGame());
    }

    [ClientRpc]
    private void PlayWinLoseSoundClientRpc(int winner)
    {
        ulong localClientId = NetworkManager.Singleton.LocalClientId; // Get local player ID

        if ((winner == 1 && localClientId == 0) || (winner == 2 && localClientId == 1))
        {
            // The winner hears the victory sound
            if (victorySound != null) victorySound.Play();
        }
        else
        {
            // The loser hears the lose sound
            if (loseSound != null) loseSound.Play();
        }
    }


    private IEnumerator ResetGame()
    {
        yield return new WaitForSeconds(2f);
        ResetPuckServerRpc();
    }

    [ServerRpc]
    private void ResetPuckServerRpc()
    {
        if (puckInstance != null)
        {
            puckRb = puckInstance.GetComponent<Rigidbody>();

            if (puckRb != null)
            {
                puckRb.velocity = Vector3.zero;
                puckRb.angularVelocity = Vector3.zero;

                // Ensure the position update takes effect
                puckRb.MovePosition(puckSpawnPoint.position);
                puckRb.rotation = Quaternion.identity;
                puckRb.Sleep(); // Stops physics calculations temporarily
            }
        }
        else
        {
            SpawnPuck();
        }

        StartCoroutine(StartCountdown());
    }

    private void SpawnPuck()
    {
        if (!IsServer) return;

        if (puckInstance != null)
        {
            Destroy(puckInstance);
        }

        puckInstance = Instantiate(hockeyPuckPrefab, puckSpawnPoint.position, Quaternion.identity);
        puckInstance.GetComponent<NetworkObject>().Spawn();
        puckRb = puckInstance.GetComponent<Rigidbody>();
    }

    private IEnumerator StartCountdown()
    {
        if (playerCount.Value < 2)
        {
            CountdownTextClientRpc("Waiting for players...");
            yield break; // Don't start if there are not enough players
        }

        startCounter = startTimer;

        while (startCounter > 0)
        {
            // Update countdown text on all clients
            CountdownTextClientRpc(startCounter.ToString());

            // Play countdown beep sound on all clients
            CountdownBeepClientRpc(false);

            yield return new WaitForSeconds(1f);
            startCounter--;
        }

        CountdownBeepClientRpc(true);

        // Clear countdown text
        CountdownTextClientRpc("");

        countdownText.text = "";
        LaunchPuckServerRpc();
    }

    // ClientRpc to play the countdown beep on all clients
    [ClientRpc]
    private void CountdownBeepClientRpc(bool isHigherPitch)
    {
        if (countdownBeep != null)
        {
            if(!isHigherPitch){
                countdownBeep.pitch = 1f;
                countdownBeep.Play();
            }
            else{
                countdownBeep.pitch = 1.5f;
                countdownBeep.Play();
            }
        }
    }


    [ServerRpc]
    private void LaunchPuckServerRpc()
    {
        float angle = GetValidLaunchAngle();
        Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        if (puckRb != null)
        {
            puckRb.velocity = new Vector3(direction.x, 0, direction.y) * launchSpeed;
            gameStarted = true;
        }
    }

    private float GetValidLaunchAngle()
    {
        float[] validAngles = { 
            Random.Range(30f, 80f),
            Random.Range(100f, 150f),
            Random.Range(210f, 260f),
            Random.Range(280f, 330f)
        };

        return validAngles[Random.Range(0, validAngles.Length)] * Mathf.Deg2Rad;
    }

    private void FixedUpdate()
    {
        if (gameStarted && puckRb != null)
        {
            MaintainMinSpeed();
        }
    }

    private void MaintainMinSpeed()
    {
        if (puckRb != null && puckRb.velocity.magnitude < minSpeed)
        {
            puckRb.velocity = puckRb.velocity.normalized * minSpeed;
        }
    }

    // ClientRpc to update win text on all clients
    [ClientRpc]
    private void WinGameClientRpc(int player)
    {
        winText.text = $"Player {player} Wins!";
        winText.gameObject.SetActive(true);
    }

    // ClientRpc to clear win text on all clients
    [ClientRpc]
    private void ClearWinTextClientRpc()
    {
        winText.gameObject.SetActive(false);
    }

    // ClientRpc to update countdown text on all clients
    [ClientRpc]
    private void CountdownTextClientRpc(string text)
    {
        countdownText.text = text;
    }
}
