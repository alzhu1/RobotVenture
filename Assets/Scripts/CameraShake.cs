using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance = null;

    private CinemachineVirtualCamera vCam;

    // Starting time/intensity
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    void Awake() {
        instance = this;
        vCam = GetComponent<CinemachineVirtualCamera>();
    }

    void Update() {
        if (shakeTimer > 0f) {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin shaker = GetShaker();

            // Even out shake intensity over time
            shaker.m_AmplitudeGain = Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));
        }
    }

    public void ShakeCamera(float intensity, float time) {
        CinemachineBasicMultiChannelPerlin shaker = GetShaker();
        shaker.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimerTotal = shakeTimer = time;
    }

    CinemachineBasicMultiChannelPerlin GetShaker() {
        return vCam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }
}
