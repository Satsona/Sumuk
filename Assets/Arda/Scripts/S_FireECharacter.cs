using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_FireECharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 15f;
    public float airMoveSpeed = 8f;

    [Header("Jump Settings")]
    public float jumpForce = 15f;
    public float fallMultiplier = 4f;
    public float lowJumpMultiplier = 1.2f;


    [Header("Wall Collision Settings")]
    public float wallSpeedThreshold = 0f;
    public float wallStateDuration = 3f;

    private Rigidbody2D rb;
    private Vector2 movement;

    [Header("Bools")]
    public bool isGrounded = false;
    public bool isOnWall = false;
    public bool hasWallJumped = false; // New variable to track if player wall-jumped
    private float defaultGravityScale;
    private Coroutine wallStateCoroutine;


    [Header("Hurricane Settings")]
    public GameObject hurricanePrefab;
    public float hurricaneGravityScale = -2f;
    private bool isInHurricane = false;
    public float hurricaneSpawnDistance = 2f;
    public float hurricaneLifetime = 3f;
    private bool hasActiveHurricane = false;

    private Vector3 originalScale;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;
        originalScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        movement = new Vector2(horizontalInput, 0f).normalized;

        if (Input.GetKeyDown(KeyCode.Space) && (isGrounded || isOnWall) || Input.GetKeyDown(KeyCode.W) && (isGrounded || isOnWall))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnHurricane();
        }
    }

    void FixedUpdate()
    {
        if (!isOnWall)
        {
            float currentMoveSpeed = isGrounded ? moveSpeed : airMoveSpeed;
            rb.velocity = new Vector2(movement.x * currentMoveSpeed, rb.velocity.y);

            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            }
            else if (!isInHurricane && rb.velocity.y > 0 && !Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.W))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        if (movement.x < 0 && GetFacingDirection() == 1)
        {
            Turn();
        }
        else if (movement.x > 0 && GetFacingDirection() == -1)
        {
            Turn();
        }

        if (isInHurricane)
        {
            rb.gravityScale = hurricaneGravityScale;
        }
    }

    void Jump()
    {
        float wallJumpDirection = GetFacingDirection() * -1; // Push away from the wall

        if (isOnWall && !hasWallJumped) // Wall Jump (only if not already wall-jumped)
        {
            rb.velocity = new Vector2(wallJumpDirection * moveSpeed, jumpForce);
            hasWallJumped = true; // Set the limit
        }
        else if (isGrounded) // Normal Jump
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            hasWallJumped = false; // Reset wall jump status when grounded

        }

        rb.gravityScale = defaultGravityScale;
        isGrounded = false;
        isOnWall = false;
    }

    private IEnumerator EnableWallState()
    {
        isOnWall = true; // Wall state is enabled only if player hasn't wall-jumped
        rb.velocity = Vector2.zero;
        rb.gravityScale = 2f;

        // Wait until the player either stays on the wall for the duration or moves away
        float wallStayTime = 0.0f;

        while (wallStayTime < wallStateDuration)
        {
            if ((movement.x > 0 && GetFacingDirection() == -1) || (movement.x < 0 && GetFacingDirection() == 1))
            {
                // If the player is moving in the opposite direction, exit the wall state
                isOnWall = false;
                rb.gravityScale = defaultGravityScale;
                yield break; // Exit the coroutine
            }

            wallStayTime += Time.deltaTime;
            yield return null;
        }
        // Only reset the wall state if the player has not wall-jumped
        if (!hasWallJumped)
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts[0].normal.y > 0.5f && !isInHurricane)
        {
            isGrounded = true;
            hasWallJumped = false; // Reset wall jump when touching the ground

        }

        if (collision.collider.CompareTag("Wall"))
        {
            if (!hasWallJumped)
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
        // Reset isGrounded when the character leaves the ground
        if (collision.collider.CompareTag("Ground"))
        {
            isGrounded = false;
        }

        if (collision.collider.CompareTag("Wall"))
        {
            isOnWall = false;
            rb.gravityScale = defaultGravityScale;
            if (wallStateCoroutine != null)
            {
                StopCoroutine(wallStateCoroutine);
                wallStateCoroutine = null;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Hurricane"))
        {
            isInHurricane = true;
            isGrounded = false;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Hurricane"))
        {
            isInHurricane = false;
            StartCoroutine(TemporaryGravityBoost());
        }
    }

    private float GetFacingDirection()
    {
        return transform.localScale.x > 0 ? 1f : -1f;
    }



    private IEnumerator TemporaryGravityBoost()
    {
        rb.gravityScale = defaultGravityScale * 2f;
        yield return new WaitForSeconds(2f);
        rb.gravityScale = defaultGravityScale;
    }

    private void SpawnHurricane()
    {
        if (hasActiveHurricane)
        {
            Debug.Log("Cannot spawn a new hurricane until the previous one is gone.");
            return;
        }

        float facingDirection = GetFacingDirection();
        Vector3 spawnPosition = transform.position + new Vector3(facingDirection * hurricaneSpawnDistance, 3f, 0f);
        float checkDistance = 0.1f;
        float maxSpawnDistance = hurricaneSpawnDistance;

        BoxCollider2D hurricaneCollider = hurricanePrefab.GetComponent<BoxCollider2D>();
        float hurricaneWidth = hurricaneCollider ? hurricaneCollider.size.x : 1f;

        RaycastHit2D hit = Physics2D.Linecast(transform.position, spawnPosition, LayerMask.GetMask("Wall", "Ground"));

        if (hit.collider != null)
        {
            Debug.Log("Wall detected in spawn path, adjusting spawn position...");
            float distanceMoved = 0f;
            while (hit.collider != null && distanceMoved < maxSpawnDistance)
            {
                spawnPosition -= new Vector3(facingDirection * checkDistance, 0f, 0f);
                distanceMoved += checkDistance;
                hit = Physics2D.Linecast(transform.position, spawnPosition, LayerMask.GetMask("Wall", "Ground"));
            }
        }

        while (IsCollidingWithWalls(spawnPosition, hurricaneWidth) && maxSpawnDistance > 0)
        {
            spawnPosition -= new Vector3(facingDirection * checkDistance, 0f, 0f);
            maxSpawnDistance -= checkDistance;
        }

        GameObject newHurricane = Instantiate(hurricanePrefab, spawnPosition, Quaternion.identity);
        hasActiveHurricane = true;
        StartCoroutine(TrackHurricaneLifetime(newHurricane));
    }

    private bool IsCollidingWithWalls(Vector3 spawnPosition, float width)
    {
        Vector2 boxSize = new Vector2(width + 0.1f, 0.1f);
        Collider2D[] colliders = Physics2D.OverlapBoxAll(spawnPosition, boxSize, 0f, LayerMask.GetMask("Wall", "Ground"));
        return colliders.Length > 0;
    }

    private IEnumerator TrackHurricaneLifetime(GameObject hurricane)
    {
        yield return new WaitForSeconds(hurricaneLifetime);

        if (hurricane != null)
        {
            Destroy(hurricane);
        }
        hasActiveHurricane = false;
    }

    private void Turn()
    {
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
}

