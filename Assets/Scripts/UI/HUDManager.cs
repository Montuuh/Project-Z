using UnityEngine;

public class HUDManager : MonoBehaviour
{
    private ExtraHUD extraHUD;
    private ChunkCoordInfo chunkCoordInfo;

    private void Awake()
    {
        // Event system
        InfiniteChunks.OnChunkCoordChanged += UpdateChunkCoordInfo;
        InputManager.OnToggleExtraHUD += ToggleExtraHUD;

        extraHUD = GameObject.Find("ExtraHUD").GetComponent<ExtraHUD>();
        chunkCoordInfo = GameObject.Find("ChunkCoordInfo").GetComponent<ChunkCoordInfo>();
    }
    private void OnDestroy()
    {
        InfiniteChunks.OnChunkCoordChanged -= UpdateChunkCoordInfo;
        InputManager.OnToggleExtraHUD -= ToggleExtraHUD;
    }

    private void UpdateChunkCoordInfo(Vector2Int coord)
    {
        if (chunkCoordInfo != null)
        {
            chunkCoordInfo.UpdateChunkCoordInfo(coord);
        }
        else
        {
            Debug.LogError("ChunkCoordInfo is null");
        }
    }

    private void ToggleExtraHUD()
    {
        if (extraHUD != null)
        {
            extraHUD.ToggleExtraHUD();
        }
        else
        {
            Debug.LogError("ExtraHUD is null");
        }
    }
}
