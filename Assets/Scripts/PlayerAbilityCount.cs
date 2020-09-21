using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAbilityCount : MonoBehaviour
{
    // Use different indices to indicate count
    // 0 - jump, 1 - dash, 2 - climb
    public int[] abilityCount;
    private Text[] abilityText;
    private IEnumerator[] flashTextCoroutines;

    void Awake()
    {
        // Get total count of icon types
        int iconCount = Enum.GetNames(typeof(Icon.IconType)).Length;
        if (abilityCount == null) {
            abilityCount = new int[iconCount];
        }

        // Attempt to get all relavent UI for ability count
        abilityText = new Text[iconCount];
        abilityText[0] = GameObject.Find("IconUI/JumpCount")?.GetComponent<Text>();
        abilityText[1] = GameObject.Find("IconUI/DashCount")?.GetComponent<Text>();
        abilityText[2] = GameObject.Find("IconUI/ClimbCount")?.GetComponent<Text>();

        // Assign inital values to text
        for (int type = 0; type < abilityCount.Length; type++) {
            if (abilityText[type] != null) {
                abilityText[type].text = "x " + abilityCount[type];
            }
        }

        // Create array of IEnumerators to track coroutines
        flashTextCoroutines = new IEnumerator[iconCount];
    }

    public void DecrementAbility(int type) {
        if (abilityText[type] != null) {
            abilityCount[type]--;
            abilityText[type].text = "x " + abilityCount[type];
            BeginFlashText(abilityText[type], true, type);
        }
    }

    public void IncrementAbility(int type) {
        if (abilityText[type] != null) {
            abilityCount[type]++;
            abilityText[type].text = "x " + abilityCount[type];
            BeginFlashText(abilityText[type], false, type);
        }

    }

    public bool HasAbility(int type) {
        return abilityCount[type] > 0;
    }

    void BeginFlashText(Text text, bool decremented, int type) {
        // Get coroutine for ability type
        IEnumerator flashTextCoroutine = flashTextCoroutines[type];

        // Stop and start new coroutine, and store it
        if (flashTextCoroutine != null) {
            StopCoroutine(flashTextCoroutine);
        }
        flashTextCoroutine = FlashText(text, decremented);
        StartCoroutine(flashTextCoroutine);
        flashTextCoroutines[type] = flashTextCoroutine;
    }

    IEnumerator FlashText(Text text, bool decremented) {
        // Flash the text a certain color
        text.color = decremented ? Color.red : Color.green;

        // Track fade progress
        float currFade = 0f;
        while (text.color != Color.white) {
            yield return new WaitForSeconds(0.1f);
            text.color = Color.Lerp(text.color, Color.white, currFade);
            currFade += 0.25f;
        }
    }
}
