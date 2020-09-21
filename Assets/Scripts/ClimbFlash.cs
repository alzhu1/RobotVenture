using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbFlash : MonoBehaviour
{
    // Signals when to forcefully stop climbing
    public bool letGo = false;

    // Flashing values
    [SerializeField] private float timeUntilFlash = 3f;
    [SerializeField] private float flashDuration = 2f;
    [SerializeField] private float flashRate = 0.25f;

    private SpriteRenderer sr;
    private IEnumerator flashDuringClimbCoroutine;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    public void StartFlashTimer() {
        // Reset bool and begin timer
        letGo = false;
        flashDuringClimbCoroutine = FlashDuringClimb();
        StartCoroutine(flashDuringClimbCoroutine);
    }

    public void StopFlashTimer() {
        // If flash timer is ongoing, stop it, and reset color
        if (flashDuringClimbCoroutine != null) {
            StopCoroutine(flashDuringClimbCoroutine);
        }
        sr.color = Color.white;
    }

    IEnumerator FlashDuringClimb() {
        // Wait predesignated time until flashing begins
        yield return new WaitForSeconds(timeUntilFlash);

        // Switch between red and white color until time is up
        bool isRed = false;
        float flashTime = 0f;
        while (flashTime < flashDuration) {
            sr.color = isRed ? Color.white : Color.red;
            yield return new WaitForSeconds(flashRate);
            flashTime += flashRate;
            isRed = !isRed;
        }

        // Force player to stop climbing
        sr.color = Color.white;
        letGo = true;
    }
}
