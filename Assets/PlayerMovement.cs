using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.InputSystem.HID;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("KeyBinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode hookKey = KeyCode.Mouse0;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    public float horizontalInput;
    public float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Grappel Hook")]
    public RectTransform Sprite;
    public Rigidbody currentTarget;
    public Camera currentCamera;
    public Camera currentCamera2d;
    public List<Rigidbody> targets;
    public float max_distance;
    public float grapelForce;
    public LineRenderer laserLine;
    public GameObject laserOrigin;

    private bool hooking = false;
    private bool letGo = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        ResetJump();
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();

        // handle drag
        if (grounded)
            rb.drag = groundDrag;
        else
            rb.drag = 0;

        // https://u3ds.blogspot.com/2021/12/shooting-laser-raycast-linerenderer.html
        if (currentTarget != null && hooking)
        {
            laserLine.SetWidth(0.1f, 0.1f);
            laserLine.SetPosition(0, laserOrigin.transform.position);
            laserLine.SetPosition(1, currentTarget.transform.position);
        }

    }

    private void FixedUpdate()
    {
        MovePlayer();
        if (FindClosestTarget())
        {
            MoveTarget();
        }
        
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        //when to jump

        if(Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        if (Input.GetKey(hookKey))
        {
            StartHook();
        } else
        {
            EndHook();
        }
    }

    private void MovePlayer()
    {
        //Calculate Movement Direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        //on ground
        if (grounded)
        { 
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        //on air
        else if (!grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f,  rb.velocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    public void AddRigidBody(Rigidbody Body)
    {
        if (!targets.Contains(Body))
        {
            targets.Add(Body);
        }
    }

    public void RemoveRigidBody(Rigidbody Body) { 
        if (targets.Contains(Body)) { 
            targets.Remove(Body); 
        }
    }

    private bool FindClosestTarget()
    {
        // https://docs.unity3d.com/ScriptReference/Camera.WorldToViewportPoint.html

        float largest = max_distance;
        Rigidbody newrb = null;

        for (int target = 0; target < targets.Count; target++) {

            if (targets[target] == null)
            {
                continue;
            }

            Vector3 viewPos = currentCamera.WorldToViewportPoint(targets[target].position);
            if (viewPos.x + viewPos.y < largest)
                newrb = targets[target];
                largest = viewPos.x + viewPos.y;
        }

        if (newrb == null)
        {
            return false;
        }




        currentTarget = newrb;

        return true;
    }

    public void MoveTarget()
    {
        if (currentTarget == null)
        {
            Sprite.transform.position  = Vector3.zero;
        }

        Vector3 screenPosition = currentCamera.WorldToScreenPoint(currentTarget.transform.position);
        Sprite.transform.position = new Vector3(screenPosition.x, screenPosition.y, 10);
    }

    private void StartHook() {
        rb.useGravity = false;
        laserLine.enabled = true;
        hooking = true;
        ApplyForce(currentTarget);
    }

    private void LetGo()
    {
        rb.useGravity = true;

        Vector3 direction = currentTarget.transform.position - transform.position;
        rb.AddForceAtPosition(direction.normalized * grapelForce * 100, currentTarget.transform.position);
    }

    private void EndHook()
    {
        hooking = false;
        laserLine.enabled = false;

        if (letGo) {
            letGo = false;
            LetGo();
        }
    }

    // https://docs.unity3d.com/ScriptReference/Rigidbody.AddForceAtPosition.html
    void ApplyForce(Rigidbody body)
    {
        transform.localPosition = Vector3.MoveTowards(transform.localPosition, currentTarget.transform.position, Time.deltaTime * grapelForce);
        letGo = true;
    }
}
    