using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraFollow : MonoBehaviour
{
    // Bools are used in PlayerMovement
    public bool allowCameraFollow = true;
    public bool cameraFollowOn = false;

    // Camera movement related
    [SerializeField] private CinemachineVirtualCamera vCam = null;
    [SerializeField] private float scrollSpeed = 10f;
    [SerializeField] private float vCamSize = 7f;

    // Arrow related (index order is up, right, down, left)
    [SerializeField] private SpriteRenderer[] arrows = null;
    [SerializeField] private float fadeRate = 0.1f;
    [SerializeField] private float timeBetweenFade = 0.05f;
    [SerializeField] private Color highlight;

    // Base color will change opacity, clear should be constant (readonly)
    private Color baseColor = new Color(1, 1, 1, 0);
    private readonly Color clear = new Color(1, 1, 1, 0);

    private bool inTransition = false;

    private Camera cam;
    private PlayerMovement player;

    private IEnumerator showArrowsCoroutine;

    // Camera mode bounds
    private float maxX, minX, maxY, minY;

    void Awake()
    {
        // Programmatically find camera and player
        cam = FindObjectOfType<Camera>();
        player = FindObjectOfType<PlayerMovement>();

        // Set init values
        vCam.m_Lens.OrthographicSize = vCamSize;
        maxX = maxY = -Mathf.Infinity;
        minX = minY = Mathf.Infinity;
    }

    void Update()
    {
        // Camera mode transition only allowed if currently playing or no transition
        if (allowCameraFollow && !inTransition) {
            if (Input.GetKeyDown(KeyCode.C)) {
                StartCoroutine(CameraTransition());
            }

            if (cameraFollowOn) {
                // Fix camera position to bounds of confiner
                BoundObjectPosition();

                // Check input
                float horizontal = Input.GetAxisRaw("Horizontal") * Time.deltaTime * scrollSpeed;
                float vertical = Input.GetAxisRaw("Vertical") * Time.deltaTime * scrollSpeed;

                // Update arrow color
                UpdateArrowColors(horizontal, vertical);

                // Move cameera
                Vector3 moveBy = new Vector3(horizontal, vertical, 0);
                Vector3 newPos = transform.position + moveBy;

                if (newPos.x < minX && minX < Mathf.Infinity) newPos.x = minX;
                if (newPos.x > maxX && maxX > -Mathf.Infinity) newPos.x = maxX;
                if (newPos.y < minY && minY < Mathf.Infinity) newPos.y = minY;
                if (newPos.y > maxY && maxY > -Mathf.Infinity) newPos.y = maxY;

                transform.position = newPos;

            }
        }
    }

    void BoundObjectPosition() {
        // Use position of camera/vCam up to 3 decimals
        float currX = Mathf.Round(transform.position.x * 1000f) / 1000f;
        float currY = Mathf.Round(transform.position.y * 1000f) / 1000f;

        float camX = Mathf.Round(cam.transform.position.x * 1000f) / 1000f;
        float camY = Mathf.Round(cam.transform.position.y * 1000f) / 1000f;

        // If vCam outside of horizontal bounds, set min and max accordingly
        minX = currX < camX ? camX : minX;
        maxX = currX > camX ? camX : maxX;

        // If vCam outside of vertical bounds, set min and max accordingly
        minY = currY < camY ? camY : minY;
        maxY = currY > camY ? camY : maxY;

        // Enable/disable horizontal arrows

        // If current position is within bounds or max/min hasn't been set, show arrow
        arrows[0].enabled = (currY < maxY || maxY <= -Mathf.Infinity);
        arrows[1].enabled = (currX < maxX || maxX <= -Mathf.Infinity);
        arrows[2].enabled = (currY > minY || minY >= Mathf.Infinity);
        arrows[3].enabled = (currX > minX || minX >= Mathf.Infinity);

        // Fix camera position to within bounds
        transform.position = new Vector3(currX, currY, 0);
    }

    void UpdateArrowColors(float horizontal, float vertical) {
        // If direction is pressed in, highlight the arrow
        arrows[0].color = vertical > 0 ? highlight : baseColor;
        arrows[1].color = horizontal > 0 ? highlight : baseColor;
        arrows[2].color = vertical < 0 ? highlight : baseColor;
        arrows[3].color = horizontal < 0 ? highlight : baseColor;
    }

    IEnumerator CameraTransition() {
        // Toggle bools
        inTransition = true;
        cameraFollowOn = !cameraFollowOn;

        // Update camera priority and position
        if (cameraFollowOn) {
            transform.position = cam.transform.position;
            vCam.m_Priority = 20;
            player.ResetValues();
        } else {
            vCam.m_Priority = 0;
        }

        // Toggle showing arrows
        if (showArrowsCoroutine != null) {
            StopCoroutine(showArrowsCoroutine);
        }
        showArrowsCoroutine = ShowArrows(cameraFollowOn);
        StartCoroutine(showArrowsCoroutine);

        // Wait for camera transition to end before turning bool off
        yield return new WaitForSeconds(0.2f);
        inTransition = false;
    }

    IEnumerator ShowArrows(bool shouldShow) {
        // Track fade progress
        float currFade = 0f;

        while (currFade <= 1f) {
            // Transition arrow opacity to visible or not depending on shouldShow
            foreach (SpriteRenderer arrow in arrows) {
                Color target = shouldShow ? Color.white : clear;
                arrow.color = Color.Lerp(arrow.color, target, currFade);

                highlight.a = arrow.color.a;
                baseColor.a = arrow.color.a;
            }
            currFade += fadeRate;
            yield return new WaitForSeconds(timeBetweenFade);
        }
    }
}
