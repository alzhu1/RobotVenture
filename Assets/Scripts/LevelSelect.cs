using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    [SerializeField] private int levelIndex = 0;
    private LevelLoader levelLoader;

    void Start()
    {
        levelLoader = FindObjectOfType<LevelLoader>();

        // levelIndex - 1  represents the prerequesite level to beat
        if (levelIndex - 1 > GameManager.instance?.highestCompletedLevel) {
            GetComponent<Button>().interactable = false;
        }
    }

    public void LoadLevel() {
        levelLoader.LoadSpecificLevel(levelIndex);
    }
}
