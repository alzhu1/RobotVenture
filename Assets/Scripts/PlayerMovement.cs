using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float normalGravity = 3.25f;
    [SerializeField] private PlayerCollision collisionChecker = null;

    [Header("Move Speed Attributes")]
    [SerializeField] private float maxSpeed = 100f;
    [SerializeField] private float maxFallSpeed = 200f;
    [SerializeField] private float acceleration = 2f;
    [SerializeField] private float deceleration = 2f;

    [Header("Jump Attributes")]
    [SerializeField] private float initJumpVelocity = 10f;
    [SerializeField] private float wallJumpVelocity = 15f;
    [SerializeField] private float horizontalWallJumpVelocity = 40f;
    [SerializeField] private float wallJumpDrag = 24f;

    [Header("Dash Attributes")]
    [SerializeField] private GhostTrail ghostTrail = null;
    [SerializeField] private float dashMagnitude = 10f;
    [SerializeField] private float dashDrag = 20f;

    [Header("Drag Attributes")]
    [SerializeField] private float dragReduceRate = 2f;

    [Header("Climb Attributes")]
    [SerializeField] private float wallFallSpeed = 2f;
    [SerializeField] private float wallFallGravity = 0.125f;
    [SerializeField] private float climbSpeed = 5f;
    [SerializeField] private ParticleSystem wallSlideParticles = null;
    [SerializeField] private ClimbFlash climbFlashSystem = null;

    [Header("Enemy-Related Attributes")]
    [SerializeField] private float knockbackMagnitude = 15f;
    [SerializeField] private float knockbackDrag = 5f;
    [SerializeField] private float hitMoveDisabledTime = 1f;
    [SerializeField] private float invincibilityTime = 2f;

    [Header("Debug Utilities")]
    [SerializeField] private bool infiniteAbilities = false;

    // Component-related
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerAbilityCount playerAbilityCount;
    private ParticleSystem.VelocityOverLifetimeModule particleVelocity;
    private CameraFollow cameraFollow;

    // Movement/speed variables
    private float horizontal;
    private float vertical;

    // Solid collision bools (set to bools in PlayerCollision)
    private bool isGrounded = false;
    private bool touchingLeftWall = false;
    private bool touchingRightWall = false;
    private bool touchingWall = false;

    // General movement bools
    private bool canMove = true;
    private bool shouldJump = false;
    private bool shouldDash = false;

    // Climbing-related bools
    private bool facingWall = false;
    private bool canClimb = true;
    private bool shouldSlide = false;
    private bool isClimbing = false;
    private bool wallJumping = false;

    private bool facingLeft = false;

    // Drag coroutine for post-dash
    private IEnumerator reduceDragCoroutine;

    void Start()
    {
        // Init components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerAbilityCount = GetComponent<PlayerAbilityCount>();
        particleVelocity = wallSlideParticles.velocityOverLifetime;

        cameraFollow = FindObjectOfType<CameraFollow>();

        reduceDragCoroutine = ReduceDrag();
    }

    void Update()
    {
        if (!cameraFollow.cameraFollowOn) {
            // Grab input
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");

            // Check for collision data/wall related bools
            GetCollisionChecks();
            SetWallBools();

            // Check if ability was used
            CheckAbilityInput();

            // Handle flipping
            HandleFlip(horizontal);

            // Set animator bools here
            animator.SetFloat("xSpeed", Mathf.Abs(horizontal));
            animator.SetBool("isClimbing", isClimbing);
            animator.SetBool("shouldSlide", shouldSlide);

            // Fall faster if down is held
            if (!isGrounded && vertical < 0) {
                rb.velocity -= Vector2.up * Physics2D.gravity.y * vertical * Time.deltaTime;
            }
        }
    }

    void GetCollisionChecks() {
        // Grabbed straight from collision checker
        isGrounded = collisionChecker.isGrounded;
        touchingLeftWall = collisionChecker.touchingLeftWall;
        touchingRightWall = collisionChecker.touchingRightWall;
        touchingWall = collisionChecker.touchingWall;
    }

    void SetWallBools() {
        // Slide if horizontal movement is towards the wall
        shouldSlide = (touchingLeftWall && horizontal < 0) || (touchingRightWall && horizontal > 0);
        shouldSlide = shouldSlide && !isGrounded;

        // Check if player is facing the wall
        facingWall = (touchingLeftWall && facingLeft) || (touchingRightWall && !facingLeft);
    }

    void CheckAbilityInput() {
        // Check if abilities are usable (overridden if debug flag on)
        bool hasJumps = playerAbilityCount.HasAbility(0);
        bool hasDashes = playerAbilityCount.HasAbility(1);
        bool hasClimbs = playerAbilityCount.HasAbility(2);

        // Jump
        if ((hasJumps || infiniteAbilities) && Input.GetButtonDown("Jump")) {
            shouldJump = true;
            playerAbilityCount.DecrementAbility(0);
        }

        // Dash
        if ((hasDashes || infiniteAbilities) && Input.GetButtonDown("Dash")) {
            shouldDash = true;
            playerAbilityCount.DecrementAbility(1);
        }

        // Climb (only if facing + touching wall)
        if (Input.GetButton("Climb") && canClimb && facingWall) {
            if (!wallJumping && (hasClimbs || infiniteAbilities)) {
                // Do this so only 1 climb is used up
                if (!isClimbing) {
                    playerAbilityCount.DecrementAbility(2);
                    climbFlashSystem.StartFlashTimer();
                }
                isClimbing = true;
            }
        } else {
            // Stop timer if need be
            isClimbing = false;
            climbFlashSystem.StopFlashTimer();
        }
    }

    void HandleFlip(float x) {
        // No flipping if currently climbing
        if (!isClimbing) {
            if ((facingLeft && x > 0) || (!facingLeft && x < 0)) {
                facingLeft = !facingLeft;

                // Flip
                Vector3 xFlip = transform.localScale;
                float xValue = Mathf.Abs(xFlip.x);
                xFlip.x = facingLeft ? -xValue : xValue;
                transform.localScale = xFlip;
            }
        }
    }

    void FixedUpdate() {
        if (isClimbing) {
            HandleClimb();
        } else {
            rb.gravityScale = normalGravity;

            // Move if not climbing
            if (canMove) {
                if (shouldSlide) {
                    HandleSlide();
                } else {
                    HandleMove();
                }

                if (shouldDash) {
                    HandleDash();
                    canMove = false;
                    shouldDash = false;
                }
            }
        }

        // Y cannot exceed max fall speed
        float newY = Mathf.Clamp(rb.velocity.y, -maxFallSpeed, Mathf.Infinity);
        rb.velocity = new Vector2(rb.velocity.x, newY);

        // Set y-speed related floats
        animator.SetFloat("yVelocity", rb.velocity.y);
        animator.SetFloat("ySpeed", Mathf.Abs(rb.velocity.y));
    }

    void HandleClimb() {
        // Vertical movement is based on vertical input, no gravity
        rb.gravityScale = 0;
        rb.velocity = new Vector2(0, vertical * climbSpeed);

        // Play climbing sound if moving
        if (Mathf.Abs(rb.velocity.y) > 0) {
            AudioManager.instance?.Play("Climb");
        }

        // Initiate wall jump here
        if (shouldJump) {
            WallJump();
        }

        // Forceful letGo will disable climbing
        if (climbFlashSystem.letGo) {
            isClimbing = false;
            StartCoroutine(DisableClimb());
        }
    }

    void HandleSlide() {
        // Lower gravity and fall acceleration to simulate wall sliding
        if (rb.velocity.y < -2f) {
            rb.velocity = new Vector2(rb.velocity.x, -wallFallSpeed);
            rb.gravityScale = wallFallGravity;
        }

        // Direct particles away from wall to make "dust"
        particleVelocity.xMultiplier = facingLeft ? 0.5f : -0.5f;
        wallSlideParticles.Play();

        // Play wall slide sound effect
        AudioManager.instance?.Play("Wall Slide");

        // Initiate wall jump
        if (shouldJump) {
            WallJump();
        }
    }

    void HandleMove() {
        // Store current x/y velocities
        float currXVel = rb.velocity.x;
        float currYVel = rb.velocity.y;

        // Calculate change in x velocity
        float currMaxSpeed = horizontal * maxSpeed;
        float xVelOffset = (currMaxSpeed - currXVel) * Time.fixedDeltaTime;

        // Use deceleration if movement stops
        if (Mathf.Abs(horizontal) > 0) {
            xVelOffset *= acceleration;
        } else {
            xVelOffset *= deceleration;
        }

        // Update new x velocity, but clamp to maxSpeed
        currXVel = Mathf.Clamp(currXVel + xVelOffset, -maxSpeed, maxSpeed);

        // Overwrite y velocity if jump is made
        if (shouldJump) {
            AudioManager.instance?.Play("Jump");
            currYVel = initJumpVelocity;
            shouldJump = false;
        }

        // Change velocity
        rb.velocity = new Vector2(currXVel, currYVel);
    }

    void HandleDash() {
        // If facing left, auto dash in the left direction (no input)
        float dashXDir = facingLeft ? -1 : 1;
        Vector2 dashVec = new Vector2(dashXDir, 0);

        // Handle cases with horizontal only/vertical only input
        if (Mathf.Abs(vertical) > 0) {
            dashVec.y = Mathf.Sign(vertical);

            if (Mathf.Abs(horizontal) <= 0) {
                dashVec.x = 0;
            }
        }

        // Dash in direction with new velocitt
        dashVec = dashVec.normalized * dashMagnitude;
        rb.velocity = dashVec;

        // Add linear drag to "slow" the dash
        rb.drag = dashDrag;
        rb.gravityScale = 0;

        // If multiple dashes occur, need to stop the coroutine and start a new one
        StopCoroutine(reduceDragCoroutine);
        reduceDragCoroutine = ReduceDrag();
        StartCoroutine(reduceDragCoroutine);

        // Start drawing ghost trails in a coroutine
        StartCoroutine(ghostTrail.DrawGhostTrail());

        // Shake camera and play sound
        CameraShake.instance.ShakeCamera(5f, 0.1f);
        AudioManager.instance?.Play("Dash");
    }

    IEnumerator ReduceDrag() {
        // Reduce drag over fixed update loop
        while (rb.drag > 0) {
            rb.drag -= dragReduceRate;
            yield return new WaitForFixedUpdate();
        }

        // Reset drag to 0, reset other bools + gravity
        rb.drag = 0;
        canMove = true;
        wallJumping = false;
        rb.gravityScale = normalGravity;
    }

    void WallJump() {
        // Disable movement for duration of wall jump
        wallJumping = true;
        canMove = false;
        isClimbing = false;
        climbFlashSystem.StopFlashTimer();
        shouldJump = false;

        // Must be touching a wall by this point, so just use touchingLeftWall
        Vector2 jumpDir = touchingLeftWall ? Vector2.right : Vector2.left;

        // Set jump direction's velocity and drag of wall jump
        jumpDir *= horizontalWallJumpVelocity;
        jumpDir.y = wallJumpVelocity;
        rb.velocity = jumpDir;
        rb.drag = wallJumpDrag;

        // Start a new drag reduce coroutine
        StopCoroutine(reduceDragCoroutine);
        reduceDragCoroutine = ReduceDrag();
        StartCoroutine(reduceDragCoroutine);

        // Play wall jump sound
        AudioManager.instance?.Play("Wall Jump");
    }

    public void EnemyBounce() {
        // Auto jump if bouncing, play extra sound
        shouldJump = true;
        AudioManager.instance?.Play("Enemy Bounce");
    }

    public IEnumerator HitPlayer(Vector2 knockbackDir) {
        // Ignore player-enemy layer collision and disable movement
        collisionChecker.IgnoreEnemyLayerCollision(true);
        canMove = false;
        isClimbing = false;
        climbFlashSystem.StopFlashTimer();

        // Knock back the player
        rb.velocity = knockbackMagnitude * knockbackDir;
        rb.drag = knockbackDrag;
        yield return new WaitForSeconds(hitMoveDisabledTime);

        // Allow movement after some time
        canMove = true;
        rb.drag = 0f;
        yield return new WaitForSeconds(invincibilityTime);

        // Re-enable collision
        collisionChecker.IgnoreEnemyLayerCollision(false);
    }

    IEnumerator DisableClimb() {
        // Stop climbing for 1 second
        canClimb = false;
        yield return new WaitForSeconds(1f);
        canClimb = true;
    }

    public void ResetValues() {
        // General movement bools
        canMove = true;
        shouldJump = false;
        shouldDash = false;

        // Climbing-related bools
        canClimb = true;
        shouldSlide = false;
        isClimbing = false;
        wallJumping = false;

        facingLeft = false;

        // Reset horizontal input to stop movement
        horizontal = 0f;

        // Reset animator floats
        animator.SetFloat("xSpeed", 0f);
        animator.SetBool("isClimbing", isClimbing);
        animator.SetBool("shouldSlide", shouldSlide);
    }

    public void DisableCameraFollow() {
        cameraFollow.allowCameraFollow = false;
        cameraFollow.cameraFollowOn = false;
    }
}
