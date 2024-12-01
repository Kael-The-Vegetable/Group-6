using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class target : MonoBehaviour
{
    // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Renderer.OnBecameVisible.html

    public Rigidbody body;
    public PlayerMovement playerMovement;

    // Disable this script when the GameObject moves out of the camera's view
    void OnBecameInvisible()
    {
        if (body != null && playerMovement != null)
        {
            playerMovement.RemoveRigidBody(body);
        }
    }

    // Enable this script when the GameObject moves into the camera's view
    void OnBecameVisible()
    {
        if (body != null && playerMovement != null)
        {
            playerMovement.AddRigidBody(body);
        }
    }
}
