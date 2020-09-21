using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GoalFake : MonoBehaviour
{
    [SerializeField] private Tilemap ground = null;
    [SerializeField] private int trapDepth = 10;

    void OnTriggerEnter2D(Collider2D collider) {
        if (collider.CompareTag("Player")) {
            StartTrap();
        }
    }

    void StartTrap() {
        // Start trap at beginning coordinates
        int x = (int)transform.position.x - 1;
        int y = (int)transform.position.y - 1;
        Vector3Int tilePos = new Vector3Int(x, y, 0);

        // Keep trap going until depth is reached
        for (int currY = y; currY > y - trapDepth; currY--) {
            // Set y
            tilePos.y = currY;

            // Delete tiles at x and x+1 positions
            tilePos.x = x;
            ground.SetTile(tilePos, null);
            tilePos.x = x + 1;
            ground.SetTile(tilePos, null);
        }
    }
}
