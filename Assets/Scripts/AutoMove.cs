using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoMove : MonoBehaviour
{
    // Time between actions
    [SerializeField] private float jumpWaitTime = 2f;
    [SerializeField] private float dashWaitTime = 3f;
    [SerializeField] private float moveWaitTime = 0.5f;

    // Velocity for actions
    [SerializeField] private float upwardVelocity = 1f;
    [SerializeField] private float dashVelocity = 20f;
    [SerializeField] private float moveVelocity = 10f;

    // Components
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerCollision collision;
    private GhostTrail ghostTrail;

    private bool actionInProgress = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        collision = GetComponent<PlayerCollision>();
        ghostTrail = GetComponent<GhostTrail>();
    }

    void Update() {
        if (!actionInProgress) {
            // Move is a bit weird, only limit to jump and dash
            int actionType = Random.Range(0, 2);

            IEnumerator action;
            switch (actionType) {
                case 0:
                    action = StartJump();
                    break;

                case 1:
                    action = StartJumpDash();
                    break;
                
                case 2:
                    action = StartMove();
                    break;

                default:
                    action = StartJump();
                    break;
            }
            StartCoroutine(action);
            actionInProgress = true;
        }
    }

    void FixedUpdate()
    {
        animator.SetFloat("xSpeed", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetBool("isGrounded", collision.isGrounded);
    }

    IEnumerator StartJump() {
        // Jump, then wait until time is up
        rb.velocity = new Vector2(0, upwardVelocity);
        yield return new WaitForSeconds(jumpWaitTime);
        actionInProgress = false;
    }

    void DashUp() {
        Vector2 dashVec = new Vector2(0, dashVelocity);
        rb.velocity = dashVec;

        // Use higher gravity to emulate gameplay gravity + drag
        rb.gravityScale = 5;

        // Start drawing ghost trails in a coroutine
        StartCoroutine(ghostTrail.DrawGhostTrail());
    }

    IEnumerator StartJumpDash() {
        // Jump first, then wait for air time
        rb.velocity = new Vector2(0, upwardVelocity);
        yield return new WaitForSeconds(0.3f);

        // Dash, then return to normal gravity after time
        DashUp();
        yield return new WaitForSeconds(dashWaitTime);
        rb.gravityScale = 3.25f;
        actionInProgress = false;
    }

    void Flip() {
        Vector3 xFlip = transform.localScale;
        xFlip.x *= -1;
        transform.localScale = xFlip;
    }

    IEnumerator StartMove() {
        // Move right for moveWaitTime
        rb.velocity = new Vector2(moveVelocity, 0);
        yield return new WaitForSeconds(moveWaitTime);

        // Move left for moveWaitTime
        rb.velocity = new Vector2(-moveVelocity, 0);
        Flip();
        yield return new WaitForSeconds(moveWaitTime);

        // Stop moving and wait to reset action
        rb.velocity = Vector2.zero;
        Flip();
        yield return new WaitForSeconds(2 * moveWaitTime);
        actionInProgress = false;
    }
}
