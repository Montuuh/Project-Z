using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteChunks : MonoBehaviour
{
    private Transform chunkHolder;
    private static float maxViewDst = 128 * 2;
    private static int numChunks = 1;
    private int chunkSize;
    private int chunksVisibleInViewDst;

    public Transform viewer;
    private static Vector2 viewerPosition;

    Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();

    // Getters and Setters

    // Unity Methods
    void Start()
    {
        // Set the chunk size to the size of the map
        chunkSize = MapGenerator.Instance.GetChunkSize();

        // Set the number of chunks visible in the view distance
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        // Set the chunk holder to the parent of the chunks
        chunkHolder = this.transform;
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    // Methods
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

        // Loop through all the chunks in the renderable distance
        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
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
                    // Debug.Log("Chunk " + viewedChunkCoord + " added");
                }
            }
        }
    }


    // Classes
    public class Chunk
    {
        public GameObject meshObject;
        public Vector2 position;
        public Vector2Int coord;
        public Bounds bounds;
        public int chunkSize;
        public int numChunk;
        public int currentLOD;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        public Chunk(Vector2Int coord, int size, Transform parent, int chunkSize)
        {
            this.numChunk = InfiniteChunks.numChunks;
            this.coord = coord;

            this.position = coord * size;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            this.chunkSize = chunkSize;
            this.bounds = new Bounds(position, Vector2.one * size);

            meshObject = new GameObject("Chunk " + numChunk + ": " + this.coord.x + ", " + this.coord.y);
            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();


            // Material and texture
            // Texture2D texture = TextureHelper.ColorMapToTexture(TextureHelper.GetColorMapFromNoiseMap(noiseMap, mapGenerator.terrainTypes), chunkSize + 1);
            // instanceMaterial.mainTexture = texture;
            // instanceMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
            // instanceMaterial.mainTexture.filterMode = FilterMode.Point;

            Material instanceMaterial = new Material(MapGenerator.Instance.terrainMaterial);
            meshRenderer.material = instanceMaterial;

            SetVisible(false);

            InfiniteChunks.numChunks++;

            // Threading
            MapGenerator.Instance.RequestMapData(OnMapDataReceived, currentLOD);
        }

        public void UpdateChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            int lod = Mathf.Min(2, Mathf.FloorToInt(viewerDistanceFromNearestEdge / (maxViewDst / 3f))); // Change the divisor to control LOD transition distance
            if (currentLOD != lod)
            {
                currentLOD = lod;
                MapGenerator.Instance.RequestMapData(OnMapDataReceived, currentLOD);
            }
            bool visible = viewerDistanceFromNearestEdge <= maxViewDst;
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

        // Threading
        private void OnMapDataReceived(MapData mapData)
        {
            Debug.Log("Map data received. Coord = " + coord.x + ", " + coord.y + ", Size = " + chunkSize + ", LOD = " + mapData.lodIndex + ", Thread = " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            // Set the mesh
            MeshData meshData = mapData.meshData;
            meshFilter.sharedMesh = meshData.ToMesh();

            // Set the collider
            meshCollider.sharedMesh = meshFilter.sharedMesh;
        }
    }
}
