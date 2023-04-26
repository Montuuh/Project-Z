using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteChunks : MonoBehaviour
{
    private Transform chunkHolder;

    public const int maxViewDistance = 100;
    public Transform viewer;
    public static Vector2 viewerPosition;
    int chunkSize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();

    void Start()
    {
        // Set the chunk size to the size of the map. It is -1 because the map is 129x129 but the chunks should be 128x128 
        chunkSize = MapGenerator.mapGenerator.chunkSize - 1;

        // Set the number of chunks visible in the view distance
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        // Set the chunk holder to the parent of the chunks
        chunkHolder = this.transform;
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / chunkSize;
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        // Set all the chunks visible last update to invisible
        for (int i = 0; i < chunksVisibleLastUpdate.Count; i++)
        {
            chunksVisibleLastUpdate[i].SetVisible(false);
        }
        chunksVisibleLastUpdate.Clear();

        // Transform the viewer position to the chunk position
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y);

        for (int yOffset = -chunksVisibleInViewDistance; yOffset <= chunksVisibleInViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDistance; xOffset <= chunksVisibleInViewDistance; xOffset++)
            {
                // 2D coordinates of the chunk
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // If the chunk is already in the dictionary, update it
                if (chunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    chunkDictionary[viewedChunkCoord].UpdateChunk();

                    // If the chunk is visible, add it to the list of visible chunks
                    if (chunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        chunksVisibleLastUpdate.Add(chunkDictionary[viewedChunkCoord]);
                    }
                }
                else 
                {
                    // If the chunk is not in the dictionary, add it
                    chunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, chunkHolder));
                }
            }
        }
    }

    public class Chunk
    {
        public GameObject meshObject;
        public Vector2 position;
        public Bounds bounds;

        public Chunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            bounds = new Bounds(position, Vector2.one * size);

            meshObject = new GameObject("Chunk " + position.x + ", " + position.y);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size;
            meshObject.transform.parent = parent;
            meshObject.layer = 9;
        }

        public void UpdateChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
