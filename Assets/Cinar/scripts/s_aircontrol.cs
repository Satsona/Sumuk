using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_aircontrol: MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Adjust the movement speed of the character.")]
    public float moveSpeed = 5f; // Movement speed adjustable in the Inspector
    [Tooltip("Adjust the movement speed of the character while in the air.")]
    public float airMoveSpeed = 2.5f; // Reduced movement speed when in the air

    [Header("Jump Settings")]
    [Tooltip("Adjust the jump force of the character.")]
    public float jumpForce = 10f; // Jump force adjustable in the Inspector
    [Tooltip("Adjust the gravity scale when falling.")]
    public float fallMultiplier = 2.5f; // Multiplier for faster falling
    [Tooltip("Adjust the gravity scale when ascending.")]
    public float lowJumpMultiplier = 2f; // Multiplier for slower ascent

    [Header("Wall Collision Settings")]
    [Tooltip("Minimum movement speed required to enable wall state.")]
    public float wallSpeedThreshold = 2f; // Minimum speed to trigger wall collision
    [Tooltip("Duration for which isOnWall remains true after activation.")]
    public float wallStateDuration = 3f; // Duration for wall state

    private Rigidbody2D rb;
    private Vector2 movement;
    private bool isGrounded = true; // Track if the character is on the ground
    private bool isOnWall = false; // Track if the character is colliding with a wall
    private float defaultGravityScale; // Store the default gravity scale
    private Coroutine wallStateCoroutine; // Reference to the wall state coroutine

    void Start()
    {
        // Get the Rigidbody2D component attached to the character
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale; // Save the default gravity scale
    }

    void Update()
    {
        // Get input for horizontal movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        // Calculate the movement vector
        movement = new Vector2(horizontalInput, 0f).normalized;

        // Check for jump input and if the character is grounded or on a wall
        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isOnWall))
        {
            Jump();
        }
    }

    void FixedUpdate()
    {
        if (!isOnWall)
        {
            // Determine the effective movement speed based on whether the character is grounded
            float currentMoveSpeed = isGrounded ? moveSpeed : airMoveSpeed;

            // Apply the movement to the Rigidbody2D
            rb.velocity = new Vector2(movement.x * currentMoveSpeed, rb.velocity.y);

            // Adjust gravity for smoother jumping and falling
            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        else
        {
            // Stop movement and falling when on a wall
            rb.velocity = Vector2.zero;
        }
    }

    void Jump()
    {
        // Apply an upward force to the Rigidbody2D
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        rb.gravityScale = defaultGravityScale; // Reset gravity scale
        isGrounded = false; // Character is no longer on the ground
        isOnWall = false;  // Reset wall state after jumping
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the character collides with the ground
        if (collision.contacts[0].normal.y > 0.5f)
        {
            isGrounded = true;
        }

        // Check if the character collides with a wall
        if (collision.collider.CompareTag("Wall"))
        {
            // Enable wall state only if the character's horizontal velocity exceeds the threshold
            if (Mathf.Abs(rb.velocity.x) >= wallSpeedThreshold)
            {
                if (wallStateCoroutine != null)
                {
                    StopCoroutine(wallStateCoroutine);
                }
                wallStateCoroutine = StartCoroutine(EnableWallState());
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        // Reset wall state and gravity when leaving the wall
        if (collision.collider.CompareTag("Wall"))
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale; // Restore gravity scale
            if (wallStateCoroutine != null)
            {
                StopCoroutine(wallStateCoroutine);
                wallStateCoroutine = null;
            }
        }
    }

    private IEnumerator EnableWallState()
    {
        isOnWall = true;
        rb.velocity = Vector2.zero; // Stop all movement and falling
        rb.gravityScale = 0f; // Disable gravity when on a wall

        yield return new WaitForSeconds(wallStateDuration);

        isOnWall = false;
        rb.gravityScale = defaultGravityScale; // Restore gravity scale
    }
}
