using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PuckGoalTracker : NetworkBehaviour
{
    private GameManager gameManager; // Reference to the Game Manager

    [SerializeField] private float speedIncreaseFactor = 1.1f; // 10% speed increase per hit
    [SerializeField] private float maxSpeed = 15f; // Set a reasonable max speed
    [SerializeField] private AudioSource collisionSound; // Audio source for collision sounds
    [SerializeField] private AudioSource goalSound; // Audio source for goal sounds

    private Rigidbody rb;

    private void Start()
    {
        if (!IsServer) return; // Ensure only the server controls the puck

        // Find the GameManager object by tag and get its GameManager component
        GameObject gmObject = GameObject.FindWithTag("GameManager");
        rb = GetComponent<Rigidbody>();

        if (gmObject != null)
        {
            gameManager = gmObject.GetComponent<GameManager>();
        }
        else
        {
            Debug.LogError("GameManager not found! Make sure it has the 'GameManager' tag.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return; // Only the server should modify physics

        if (collision.gameObject.CompareTag("Paddle"))
        {
            IncreaseSpeed();
        }

        // Play collision sound
        PlayCollisionSoundClientRpc();
    }

    private void IncreaseSpeed()
    {
        if (rb == null) return;

        Vector3 newVelocity = rb.velocity * speedIncreaseFactor;

        // Limit the speed to prevent infinite acceleration
        if (newVelocity.magnitude > maxSpeed)
        {
            newVelocity = newVelocity.normalized * maxSpeed;
        }

        rb.velocity = newVelocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || gameManager == null) return; // Ensure only the server processes goal scoring

        if (other.CompareTag("Goal1"))
        {
            gameManager.ScoreGoalServerRpc(1);
            PlayGoalSoundClientRpc(); // Play goal sound on all clients
        }
        else if (other.CompareTag("Goal2"))
        {
            gameManager.ScoreGoalServerRpc(2);
            PlayGoalSoundClientRpc(); // Play goal sound on all clients
        }
    }

    [ClientRpc]
    private void PlayCollisionSoundClientRpc()
    {
        if (collisionSound != null)
        {
            collisionSound.volume = Random.Range(0.8f, 1f);
            collisionSound.pitch = Random.Range(0.8f, 1f);
            collisionSound.PlayOneShot(collisionSound.clip);
        }
    }

    [ClientRpc]
    private void PlayGoalSoundClientRpc()
    {
        if (goalSound != null)
        {
            goalSound.Play();
        }
    }
}
