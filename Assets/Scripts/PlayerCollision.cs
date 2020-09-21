using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    // Ground Check
    public bool isGrounded;

    // Wall Check
    public bool touchingLeftWall;
    public bool touchingRightWall;
    public bool touchingWall;

    [Header("Checker Transforms")]
    [SerializeField] private Transform groundCheck = null;
    [SerializeField] private Transform leftWallCheck = null;
    [SerializeField] private Transform rightWallCheck = null;
    [SerializeField] private LayerMask solidLayer = 9;

    private Animator animator;
    private Vector3 topLeftGround;
    private Vector3 bottomRightGround;

    private int playerLayer;
    private int enemyLayer;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Get layer ints
        playerLayer = LayerMask.NameToLayer("Player");
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // Always enable enemy collisions at start
        IgnoreEnemyLayerCollision(false);
    }

    void Update()
    {
        topLeftGround = groundCheck.transform.position + new Vector3(-0.4f, 0, 0);
        bottomRightGround = groundCheck.transform.position + new Vector3(0.4f, 0.05f, 0);

        // Check if player is grounded
        // isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.05f, solidLayer);
        isGrounded = Physics2D.OverlapArea(topLeftGround, bottomRightGround, solidLayer);
        animator.SetBool("isGrounded", isGrounded);

        // Check for wall touching
        touchingLeftWall = Physics2D.OverlapCircle(leftWallCheck.position, 0.05f, solidLayer);
        touchingRightWall = Physics2D.OverlapCircle(rightWallCheck.position, 0.05f, solidLayer);
        touchingWall = touchingLeftWall || touchingRightWall;

        // Local scale is negative if facing left, should flip bools
        if (transform.localScale.x < 0) {
            bool tempFace = touchingLeftWall;
            touchingLeftWall = touchingRightWall;
            touchingRightWall = tempFace;
        }
    }

    public void IgnoreEnemyLayerCollision(bool shouldIgnore) {
        Physics2D.IgnoreLayerCollision(playerLayer, enemyLayer, shouldIgnore);
    }
}
