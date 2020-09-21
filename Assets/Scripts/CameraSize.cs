using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

/*
  This class fixes aspect ratio to the bounds of a collider

  Kinda messy, ultimately decided to target fixed aspect ratios instead of all.
*/


public class CameraSize : MonoBehaviour
{
    [SerializeField] private Collider2D levelBound = null;

    private CinemachineVirtualCamera vCam;

    void Awake() {
        vCam = GetComponent<CinemachineVirtualCamera>();
    }

    // Start is called before the first frame update
    void Start()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = (float)levelBound.bounds.size.x / levelBound.bounds.size.y;

        float difference = (screenRatio >= targetRatio) ? 1 : targetRatio / screenRatio;

        vCam.m_Lens.OrthographicSize = levelBound.bounds.size.y / 2 * difference;
    }

    // Update is called once per frame
    void Update()
    {
        float screenRatio = (float)Screen.width / Screen.height;
        float targetRatio = (float)levelBound.bounds.size.x / levelBound.bounds.size.y;

        float difference = (screenRatio >= targetRatio) ? 1 : targetRatio / screenRatio;

        vCam.m_Lens.OrthographicSize = levelBound.bounds.size.y / 2 * difference;
    }
}
