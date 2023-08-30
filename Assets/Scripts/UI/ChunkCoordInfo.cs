using UnityEngine;
using TMPro;

public class ChunkCoordInfo : MonoBehaviour
{
    public void UpdateChunkCoordInfo(Vector2Int chunkCoord)
    {
        this.GetComponent<TextMeshProUGUI>().text = $"Chunk: {chunkCoord.x}, {chunkCoord.y}";
    }
}
