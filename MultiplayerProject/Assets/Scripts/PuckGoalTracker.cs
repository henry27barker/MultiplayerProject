using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckGoalTracker : MonoBehaviour
{
    private GameManager gameManager; // Reference to the Game Manager

    void Start()
    {
        // Find the GameManager object by tag and get its GameManager component
        GameObject gmObject = GameObject.FindWithTag("GameManager");
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
