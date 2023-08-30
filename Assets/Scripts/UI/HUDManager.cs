using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public TMPro.TextMeshProUGUI fpsText;
    public TMPro.TextMeshProUGUI chunkCoord;

    [SerializeField] private float updateInterval = 0.5f;

    private float fpsAccumulator = 0f;
    private int fpsFrames = 0;
    private float fpsTimeLeft = 0f;

    private void Awake()
    {
        // Events
        InfiniteChunks.OnChunkCoordChanged += UpdateChunkCoordText;
    }

    private void Start()
    {
        fpsTimeLeft = updateInterval;
    }

    private void OnDestroy()
    {
        // Events
        InfiniteChunks.OnChunkCoordChanged -= UpdateChunkCoordText;
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

    private void UpdateChunkCoordText(Vector2Int coord)
    {
        chunkCoord.text = $"Chunk: {coord.x}, {coord.y}";
    }
}
