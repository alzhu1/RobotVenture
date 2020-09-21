using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostTrail : MonoBehaviour
{
    // Use these to create ghost images
    [SerializeField] private int ghostTrailLength = 3;
    [SerializeField] private float timeBetweenGhosts = 0.1f;
    [SerializeField] private float timeBetweenFades = 0.1f;
    [SerializeField] private float fadeRate = 0.2f;
    [SerializeField] private Color ghostColor = Color.cyan;
    
    // Ghosts are GameObjects with overwritten shaders for a silhouette appearance
    private Shader shaderGUItext;
    private Color clearGhostColor;

    void Start()
    {
        shaderGUItext = Shader.Find("GUI/Text Shader");
        clearGhostColor = new Color(ghostColor.r, ghostColor.g, ghostColor.b, 0);
    }

    public IEnumerator DrawGhostTrail() {
        int count = 0;
        while (count < ghostTrailLength) {
            // Create dummy game object with sprite renderer
            GameObject trailPart = new GameObject();
            SpriteRenderer trailPartRenderer = trailPart.AddComponent<SpriteRenderer>();

            // Capture current sprite, and overwrite shader/color for ghost appearance
            trailPartRenderer.sprite = GetComponent<SpriteRenderer>().sprite;
            trailPartRenderer.material.shader = shaderGUItext;
            trailPartRenderer.color = ghostColor;

            // Set position/size to current snapshot of player position, and begin fading ghost
            trailPart.transform.position = transform.position;
            trailPart.transform.localScale = transform.localScale;
            StartCoroutine(FadeGhost(trailPartRenderer));

            // Wait before making another ghost
            yield return new WaitForSeconds(timeBetweenGhosts);
            count++;
        }
    }

    IEnumerator FadeGhost(SpriteRenderer spriteRenderer) {
        // Track fade progress
        float currFade = 0f;

        while (spriteRenderer.color.a > 0) {
            // Fade from ghost's start color to invisibility
            yield return new WaitForSeconds(timeBetweenFades);
            spriteRenderer.color = Color.Lerp(ghostColor, clearGhostColor, currFade);
            currFade += fadeRate;
        }

        // When fade finishes, destroy game object
        Destroy(spriteRenderer.gameObject);
    }
}
