using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    private Rigidbody rb;

    [SerializeField]
    private float moveSpeed = 5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveInput = Input.GetAxis("Vertical");

        if (IsServer && IsLocalPlayer){
            // Apply movement locally for instant feedback
            Move(moveInput);
        }
        else if(IsClient && IsLocalPlayer){
            // Send input to the server for validation
            SubmitMoveServerRpc(moveInput);
        }
    }

    private void Move(float input){
        Vector3 move = new Vector3(0, 0, input) * moveSpeed;
        rb.velocity = move;
    }

    [ServerRpc]
    private void SubmitMoveServerRpc(float input)
    {
        // Apply validated movement on the server
        Move(input);
    }
}
