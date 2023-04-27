using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI fpsText;

    [SerializeField] private float updateInterval = 0.5f;

    private float fpsAccumulator = 0f;
    private int fpsFrames = 0;
    private float fpsTimeLeft = 0f;

    private void Start()
    {
        fpsTimeLeft = updateInterval;
    }

    void Update()
    {
        fpsTimeLeft -= Time.deltaTime;
        fpsAccumulator += Time.timeScale / Time.deltaTime;
        fpsFrames++;

        if (fpsTimeLeft <= 0f)
        {
            float currentFPS = fpsAccumulator / fpsFrames;
            fpsText.text = $"FPS: {currentFPS:0.}";
            fpsTimeLeft = updateInterval;
            fpsAccumulator = 0f;
            fpsFrames = 0;
        }
    }
}
