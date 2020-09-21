using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathPlane : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            // Disable movement, restart level
            collider.GetComponent<PlayerMovement>().enabled = false;
            LevelLoader loader = FindObjectOfType<LevelLoader>();
            loader.ReloadLevel();
        }
    }
}
