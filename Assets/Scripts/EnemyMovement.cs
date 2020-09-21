using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    // Enum for differing enemy types
    public enum MovementType {
        Walker, WalkerFall, Jumper
    }
    // Init movement variables
    [SerializeField] private MovementType movementType = MovementType.Walker;
    [SerializeField] private bool initFacingLeft = false;
    [SerializeField] private float speed = 10f;

    // Jump variables
    [SerializeField] private float jumpVelocity = 15f;
    [SerializeField] private float defaultJumpTime = 3f;

    // Raycast variables
    [SerializeField] private float offsetInFront = 0.5f;
    [SerializeField] private float groundRayLength = 1f;
    [SerializeField] private float wallRayLength = 0.25f;
    
    // Ground check variables
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private LayerMask solidLayer = 9;
    [SerializeField] private BoxCollider2D mainBodyCollider = null;

    // Components
    private Rigidbody2D rb;

    // Track player once in contact
    private PlayerMovement player;

    // Enemy bools
    private bool facingLeft = false;
    private bool isGrounded = false;
    private bool jumpActivated = false;
    private bool disableGroundCheck = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Set facing left
        if (initFacingLeft) {
            Flip();
        }
        facingLeft = initFacingLeft;
    }

    void Update()
    {
        // Check if enemy touches ground
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.05f, solidLayer);

        if (jumpActivated && isGrounded) {
            rb.velocity = Vector2.zero;
        }
    }

    void FixedUpdate() {
        switch(movementType) {
            case MovementType.Walker:
                WalkerMove(false);
            break;

            case MovementType.WalkerFall:
                WalkerMove(true);
                break;

            case MovementType.Jumper:
                JumperMove();
            break;

            default:
            break;
        }
    }

    void WalkerMove(bool shouldFall) {
        // Set velocity to speed * x direction
        float xVelocity = facingLeft ? -speed : speed;
        Vector2 velocity = new Vector2(xVelocity, rb.velocity.y);

        if (isGrounded) {
            velocity.y = 0;
        }
        rb.velocity = velocity;

        // In "Walker" case, if they shouldn't fall, then a check is needed
        // Check if moving in that direction will make the enemy fall
        if (!shouldFall) {
            // Start with point in front of the enemy
            Vector2 pointInFront = new Vector2(transform.position.x, transform.position.y);
            Vector2 currDir = facingLeft ? Vector2.left : Vector2.right;
            pointInFront += currDir * offsetInFront;

            // TODO: first do a horizontal raycast in the move direction
            // If something is found, disable the vertical ground checker raycast
            // Direction change will come from head on collision
            // Once direction changes, re-enable the vertical raycast

            // First, check if wall is in front of enemy
            // A wall means no ground check is needed
            RaycastHit2D wallCheck = Physics2D.Raycast(pointInFront, currDir, wallRayLength, solidLayer);
            if (wallCheck.collider != null) {
                disableGroundCheck = true;
            }

            if (!disableGroundCheck) {
                // Check if there is no ground in front of enemy
                RaycastHit2D hit = Physics2D.Raycast(pointInFront, Vector2.down, groundRayLength, solidLayer);

                // If so, move in opposite direction
                if (hit.collider == null) {
                    Flip();
                    disableGroundCheck = false;
                }
            }
        }
    }

    void JumperMove() {
        if (jumpActivated) {
            // Set direction if jumping
            if (rb.velocity.x < 0) {
                facingLeft = true;
            } else if (rb.velocity.x > 0) {
                facingLeft = false;
            }
        } else if (isGrounded) {
            // Begin a jump only if grounded
            float jumpSpeed = facingLeft ? -speed : speed;
            rb.velocity = new Vector2(jumpSpeed, jumpVelocity);

            StartCoroutine(JumpWait());
        }
    }

    IEnumerator JumpWait() {
        // Jump should last for a bit of time
        jumpActivated = true;
        yield return new WaitForSeconds(defaultJumpTime);
        jumpActivated = false;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        // Track contact point and center of opposing object
        Collider2D collider = collision.collider;
        Vector3 contactPoint = collision.contacts[0].point;
        Vector3 center = collider.bounds.center;
        Vector3 enemyCenter = mainBodyCollider.bounds.center;

        // Only switch direction if colliding with a non-player
        if (!collider.CompareTag("Player")) { 
            // Ignore this for jumpers
            if (movementType != MovementType.Jumper) {
                bool rightOfCollider = contactPoint.x > enemyCenter.x;

                if (rightOfCollider == !facingLeft) {
                    Flip();
                    disableGroundCheck = false;
                }
            }
        } else {
            // No y velocity change when colliding with player
            rb.velocity = new Vector2(rb.velocity.x, 0);

            // Store player
            if (player == null) {
                player = collision.gameObject.GetComponent<PlayerMovement>();
            }

            // If player collided with enemy, check if enemy should die or if player should get hit
            ContactPoint2D hitPos = collision.contacts[0];

            // Player is hit if they approached enemy from anywhere other than the top
            if (hitPos.normal.y >= 0 || mainBodyCollider.bounds.Contains(center)) {
                // Keep moving, disable player's collider, and call player function to start invincibility timer
                Vector2 knockbackDir = Vector2.up;

                // Compare the center of enemyCenter and center
                if (enemyCenter.x < center.x) {
                    knockbackDir += Vector2.right;
                } else {
                    knockbackDir += Vector2.left;
                }

                StartCoroutine(player.HitPlayer(knockbackDir.normalized));
            } else {
                // Player bounces off enemy
                player.EnemyBounce();
            }
        }
    }

    void Flip() {
        facingLeft = !facingLeft;

        // Flip
        Vector3 xFlip = transform.localScale;
        xFlip.x *= -1;
        transform.localScale = xFlip;

    }

    // void OnDrawGizmos() {
    //     if (p != null) {
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawRay(p, Vector3.down * groundRayLength);
    //         Gizmos.DrawRay(p, d * wallRayLength);

    //         Gizmos.color = Color.green;
    //         Gizmos.DrawSphere(p, 0.05f);
    //     }

    //     if (p1 != null) {
    //         Gizmos.color = Color.green;
    //         Gizmos.DrawSphere(p1, 0.05f);
    //         Gizmos.color = Color.yellow;
    //         Gizmos.DrawSphere(p2, 0.05f);
    //         Gizmos.color = Color.red;
    //         Gizmos.DrawSphere(p3, 0.05f);

    //         Gizmos.color = Color.black;
    //         Gizmos.DrawLine(p3 + new Vector3(0.4f, 0, 0), p3 + new Vector3(-0.4f, 0, 0));
    //         // Gizmos.DrawSphere(p3 + new Vector3(0.4f, 0, 0), 0.05f);
    //         // Gizmos.DrawSphere(p3 + new Vector3(-0.4f, 0, 0), 0.05f);
    //     }
    // }
}
