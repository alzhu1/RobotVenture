using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Using Brackeys' AudioManager as template

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance = null;

    [SerializeField] private Sound[] sounds = null;
    private Dictionary<string, Sound> soundMap;

    private bool muted = false;

    void Awake() {
        if (instance == null) {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
            return;
        }

        soundMap = new Dictionary<string, Sound>();

        // Create Sound object for each clip
        foreach (Sound s in sounds) {
            s.source = gameObject.AddComponent<AudioSource>();
            s.source.clip = s.clip;
            s.source.volume = s.volume;
            s.source.pitch = s.pitch;

            // Loop doesn't work in WebGL
            s.source.loop = s.loop;
        }
    }

    void Start() {
        StartCoroutine(PlayIntro());
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.M)) {
            ToggleMute();
        }
    }

    void ToggleMute() {
        muted = !muted;
        foreach (Sound s in sounds) {
            s.source.volume = muted ? 0 : s.volume;
        }
    }

    // Do this because WebGL looping audio doesn't work
    IEnumerator PlayIntro() {
        Sound intro = Array.Find(sounds, sound => sound.name == "Intro");
        Sound theme = Array.Find(sounds, sound => sound.name == "Theme");

        // Play intro, then theme, then wait combined length
        intro.source.Play();
        theme.source.PlayDelayed(intro.clip.length);
        yield return new WaitForSeconds(intro.clip.length + theme.clip.length);

        // Loop theme forever
        while (true) {
            theme.source.Play();
            yield return new WaitForSeconds(theme.clip.length);
        }
    }

    public void Play(string name) {
        Sound s;

        // Look in map first
        if (!soundMap.ContainsKey(name)) {
            s = Array.Find(sounds, sound => sound.name == name);
            if (s == null) {
                Debug.LogWarning(name + " not found");
                return;
            }

            soundMap.Add(name, s);
        } else {
            s = soundMap[name];
        }

        // Prevent repetitious playing
        if (!s.source.isPlaying) {
            s.source.Play();
        }
    }
}

[Serializable]
public class Sound {
    public string name;
    public AudioClip clip;

    [Range(0f, 1f)]
    public float volume;
    [Range(.1f, 3f)]
    public float pitch;

    public bool loop;

    [HideInInspector]
    public AudioSource source;
} 