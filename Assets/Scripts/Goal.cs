using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    [SerializeField] private LevelLoader loader = null;

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            // Disable camera follow mode and movement
            PlayerMovement playerMovement = collider.GetComponent<PlayerMovement>();
            playerMovement.DisableCameraFollow();
            playerMovement.enabled = false;

            // Add drag to slow player down
            collider.GetComponent<Rigidbody2D>().drag = 5;

            StartCoroutine(InitiateVictoryPose(collider.gameObject));
        }
    }

    IEnumerator InitiateVictoryPose(GameObject player) {
        // Wait for player to be grounded first
        PlayerCollision collisionChecker = player.GetComponent<PlayerCollision>();
        int count = 0;
        while (!collisionChecker.isGrounded && count < 20) {
            yield return new WaitForSeconds(0.25f);
            count++;
        }

        // Set trigger and try loading level
        player.GetComponent<Animator>().SetTrigger("hasWon");
        StartCoroutine(LoadNextLevel(player));

    }

    IEnumerator LoadNextLevel(GameObject player) {
        // Use long load time in case player dies during victory
        yield return new WaitForSeconds(2.0f);
        AudioManager.instance?.Play("Victory");
        loader.LoadNextLevel();
    }

}
