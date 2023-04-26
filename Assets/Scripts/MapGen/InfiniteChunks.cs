using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteChunks : MonoBehaviour
{
    private Transform chunkHolder;
    private const int maxRenderableChunkDistance = 2;
    public static int numChunks = 1;
    private int chunkSize;

    public Transform viewer;
    public static Vector2 viewerPosition;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();

    void Start()
    {
        // Set the chunk size to the size of the map
        chunkSize = MapGenerator.Instance.GetChunkSize();

        // Set the chunk holder to the parent of the chunks
        chunkHolder = this.transform;
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
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
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -maxRenderableChunkDistance; yOffset <= maxRenderableChunkDistance; yOffset++)
        {
            for (int xOffset = -maxRenderableChunkDistance; xOffset <= maxRenderableChunkDistance; xOffset++)
            {
                // 2D coordinates of the chunk
                Vector2Int viewedChunkCoord = new Vector2Int(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
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
                    chunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, chunkHolder, chunkSize));
                    Debug.Log("Chunk " + viewedChunkCoord + " added");
                }
            }
        }
    }

    public class Chunk
    {
        public GameObject meshObject;
        public Vector2 position;
        public Vector2Int coord;
        public Bounds bounds;
        public int chunkSize;
        public int numChunk;

        public Chunk(Vector2Int coord, int size, Transform parent, int chunkSize)
        {
            this.numChunk = InfiniteChunks.numChunks;
            InfiniteChunks.numChunks++;
            this.coord = coord;
            this.chunkSize = chunkSize;
            position = this.coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            // meshObject = new GameObject("Chunk " + position.x + ", " + position.y);
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.name = "Chunk " + numChunk + ": " + this.coord.x + ", " + this.coord.y;
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            // meshObject.layer = 9;
            SetVisible(false);
        }

        public void UpdateChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= (maxRenderableChunkDistance * this.chunkSize);
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
