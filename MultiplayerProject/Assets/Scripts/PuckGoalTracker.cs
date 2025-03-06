using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckGoalTracker : MonoBehaviour
{
    private GameManager gameManager; // Reference to the Game Manager

    [SerializeField] private float speedIncreaseFactor = 1.1f; // 10% speed increase per hit
    [SerializeField] private float maxSpeed = 15f; // Set a reasonable max speed

    private Rigidbody rb;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            IncreaseSpeed();
        }
    }

    private void IncreaseSpeed()
    {
        Vector3 newVelocity = rb.velocity * speedIncreaseFactor;

        // Limit the speed to prevent infinite acceleration
        if (newVelocity.magnitude > maxSpeed)
        {
            newVelocity = newVelocity.normalized * maxSpeed;
        }

        rb.velocity = newVelocity;
    }

    void Start()
    {
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

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager == null) return; // Prevent errors if GameManager is not found

        if (other.CompareTag("Goal1"))
        {
            gameManager.ScoreGoal(1);
        }
        else if (other.CompareTag("Goal2"))
        {
            gameManager.ScoreGoal(2);
        }
    }
}
