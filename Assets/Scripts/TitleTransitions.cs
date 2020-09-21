using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleTransitions : MonoBehaviour
{
    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void SetAnimatorTrigger(string trigger) {
        animator.SetTrigger(trigger);
    }

    /* Technically below is a lot of writing code, but removes human error in typing in inspector? */

    // public void ToLevelSelect() {
    //     animator.SetTrigger("levelSelect");
    // }

    // public void ToHowToPlay() {
    //     animator.SetTrigger("howToPlay");
    // }

    // public void ToCredits() {
    //     animator.SetTrigger("credits");
    // }

    // public void ToTitleFromLevels() {
    //     animator.SetTrigger("titleFromLevels");
    // }

    // public void ToTitleFromHowToPlay() {
    //     animator.SetTrigger("titleFromHowToPlay");
    // }

    // public void ToTitleFromCredits() {
    //     animator.SetTrigger("titleFromCredits");
    // }
}
