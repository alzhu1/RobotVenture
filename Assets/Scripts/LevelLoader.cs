using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour {
    [SerializeField] private Animator transition = null;
    [SerializeField] private float transitionTime = 1f;

    private int sceneIndex = 0;

    void Awake() {
        sceneIndex = SceneManager.GetActiveScene().buildIndex;
    }

    void Update() {
        // Only allow reloading/going back if in a level
        if (sceneIndex != 0) {
            if (Input.GetKeyDown(KeyCode.R)) {
                ReloadLevel();
            } else if (Input.GetKeyDown(KeyCode.Escape)) {
                LoadSpecificLevel(0);
            }
        }
    }
    
    // Various level loading methods are public for other scripts/buttons

    public void LoadSpecificLevel(int loadIndex) {
        StartCoroutine(LoadLevel(loadIndex));
    }

    public void LoadNextLevel() {
        StartCoroutine(LoadLevel(sceneIndex + 1));
    }

    public void ReloadLevel() {
        StartCoroutine(LoadLevel(sceneIndex));
    }

    IEnumerator LoadLevel(int loadIndex) {
        // Begin transition
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);

        // Null propagation in case GameManager is null
        if (GameManager.instance?.highestCompletedLevel < loadIndex) {
            // -1 because loadIndex is the level to load, previous index was just beaten
            GameManager.instance.highestCompletedLevel = loadIndex - 1;
        }

        SceneManager.LoadScene(loadIndex);
    }

    /* Old async loading experiment below */

    // IEnumerator LoadNextLevel(int loadIndex) {
    //     yield return null;

    //     // int currIndex = SceneManager.GetActiveScene().buildIndex;
    //     // int nextIndex = (currIndex + 1) % 2; // TODO: remove mod

    //     //Begin to load the Scene you specify
    //     AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(loadIndex);
    //     //Don't let the Scene activate until you allow it to
    //     asyncOperation.allowSceneActivation = false;
    //     Debug.Log("Pro :" + asyncOperation.progress);
        
    //     //When the load is still in progress, output the Text and progress bar
    //     while (!asyncOperation.isDone)
    //     {
    //         // Debug.Log("Progress is " + asyncOperation.progress);

    //         // Check if the load has finished
    //         if (asyncOperation.progress >= 0.9f)
    //         {

    //             //Wait to you press the space key to activate the Scene
    //             if (activateScene) {
    //                 //Activate the Scene
    //                 asyncOperation.allowSceneActivation = true;
    //                 activateScene = false;
    //                 loading = false;
    //             }
    //         }

    //         yield return null;
    //     }
    // }
}