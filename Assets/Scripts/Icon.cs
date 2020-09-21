using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Icon : MonoBehaviour
{
    // Create enum for differing icon types
    public enum IconType {
        Jump = 0,
        Dash = 1,
        Climb = 2
    }
    [SerializeField] private IconType iconType = IconType.Jump;
    [SerializeField] private float iconRespawnTime = 1.5f;
    [SerializeField] private Sprite collectedSprite = null;
    [SerializeField] private bool respawnable = true;

    // Ability count is tied to icons
    private PlayerAbilityCount playerAbilityCount;

    // Sprite and collider
    private SpriteRenderer spriteRenderer;
    private Sprite iconSprite;    
    private Collider2D iconCollider;

    // Used for hovering
    private float initY;
    private float yMagnitude = 0.25f;
    private float timer = 0f; 

    void Start()
    {
        playerAbilityCount = FindObjectOfType<PlayerAbilityCount>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        iconSprite = spriteRenderer.sprite;
        iconCollider = GetComponent<Collider2D>();

        initY = transform.position.y;
    }

    void Update()
    {
        // Timer used for a icon hover effect, y position bobs up and down
        timer += Time.deltaTime;
        if (timer > 2 * Mathf.PI) { timer -= 2 * Mathf.PI; }
        float newY = Mathf.Sin(timer) * yMagnitude + initY;
        transform.position = new Vector2(transform.position.x, newY);
    }

    void OnTriggerEnter2D(Collider2D collider) {
        // Collect the icon as player
        if (collider.CompareTag("Player")) {
            playerAbilityCount.IncrementAbility((int)iconType);
            AudioManager.instance?.Play("Icon");

            // Destroy if not respawnable
            if (respawnable) {
                StartCoroutine(DisableCollider());
            } else {
                Destroy(gameObject);
            }
        }
    }

    IEnumerator DisableCollider() {
        // Disable collider/change sprite to prevent recollection
        iconCollider.enabled = false;
        spriteRenderer.sprite = collectedSprite;

        // Depending on icon type, use different color
        switch (iconType) {
            case IconType.Jump:
                spriteRenderer.color = Color.green;
                break;
            
            case IconType.Dash:
                spriteRenderer.color = Color.blue;
                break;

            case IconType.Climb:
                spriteRenderer.color = Color.red;
                break;
        }

        // Wait for icon to repawn
        yield return new WaitForSeconds(iconRespawnTime);

        // Reset icon to default state
        iconCollider.enabled = true;
        spriteRenderer.sprite = iconSprite;
        spriteRenderer.color = Color.white;
    }
}
