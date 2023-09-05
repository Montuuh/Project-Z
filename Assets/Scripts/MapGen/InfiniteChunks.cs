using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteChunks : MonoBehaviour
{
    public LODDistance[] lodDistanceLevels;
    private static float maxViewDst;

    private Transform chunkHolder;
    private int chunkSize;
    private int chunksVisibleInViewDst;
    private static int numChunks = 1;

    public Transform viewer;
    private static Vector2 viewerPosition;
    private static Vector2 viewerPositionOld;
    private static Vector2Int viewerChunkCoord;
    private const float sqrViewerMoveThresholdForChunkUpdate = 25f;

    private const float chunkScale = 1.0f;

    private Dictionary<Vector2, Chunk> chunkDictionary = new Dictionary<Vector2, Chunk>();
    private static List<Chunk> chunksVisibleLastUpdate = new List<Chunk>();
    
    public Material material;

    // Events
    public static event System.Action<Vector2Int> OnChunkCoordChanged;

    // Getters and Setters

    // Unity Methods
    private void Awake()
    {
        
    }

    private void Start()
    {
        // Set the chunk size to the size of the map
        chunkSize = MapGenerator.Instance.GetChunkSize();

        // Set the max view distance to the max view distance of the LODs
        maxViewDst = lodDistanceLevels[lodDistanceLevels.Length - 1].distance;

        // Set the number of chunks visible in the view distance
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / chunkSize);

        // Set the chunk holder to the parent of the chunks
        chunkHolder = this.transform;

        // Initial Chunk coordinates
        Vector2Int currentChunkCoord = GetCurrentChunkCoord();
        OnChunkCoordChanged?.Invoke(currentChunkCoord);

        UpdateVisibleChunks();
    }

    private void Update()
    {
        // We have to divide by the scale because the viewer position is in world coordinates and the chunk position is in chunk coordinates
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / chunkScale;

        // If the viewer has moved more than the threshold, update the chunks
        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

        // ChunkCoord management
        Vector2Int currentChunkCoord = GetCurrentChunkCoord();
        if (currentChunkCoord != viewerChunkCoord)
        {
            viewerChunkCoord = currentChunkCoord;
            OnChunkCoordChanged?.Invoke(viewerChunkCoord);
        }
    }

    // Methods
    private void UpdateVisibleChunks()
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
                }
                else
                {
                    // If the chunk is not in the dictionary, add it
                    chunkDictionary.Add(viewedChunkCoord, new Chunk(viewedChunkCoord, chunkSize, chunkHolder, lodDistanceLevels, material));
                }
            }
        }
    }

    private Vector2Int GetCurrentChunkCoord()
    {
        return new Vector2Int(Mathf.RoundToInt(viewerPosition.x / chunkSize), Mathf.RoundToInt(viewerPosition.y / chunkSize));
    }


    // Classes
    public class Chunk
    {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;
        private Vector2Int coord;
        private int chunkSize;
        private int numChunk;

        private MeshRenderer meshRenderer;
        private MeshFilter meshFilter;
        private MeshCollider meshCollider;

        private LODMesh[] lodMeshes;
        private LODDistance[] lodDistanceLevels;

        private MapData mapData;
        private bool mapDataReceived;
        private int previousLOD;

        public Chunk(Vector2Int coord, int chunkSize, Transform parent, LODDistance[] lodDistanceLevels, Material material)
        {
            this.numChunk = InfiniteChunks.numChunks;
            InfiniteChunks.numChunks++;

            this.coord = coord;
            this.position = coord * chunkSize;
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            this.lodDistanceLevels = lodDistanceLevels;
            this.previousLOD = -1;
            this.mapDataReceived = false;


            this.chunkSize = chunkSize;
            this.bounds = new Bounds(position, Vector2.one * chunkSize);

            meshObject = new GameObject("Chunk " + numChunk + ": " + this.coord.x + ", " + this.coord.y);
            meshObject.transform.position = positionV3 * chunkScale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * chunkScale;

            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshCollider = meshObject.AddComponent<MeshCollider>();

            // Material and texture
            meshRenderer.material = material;

            // LOD meshes
            lodMeshes = new LODMesh[lodDistanceLevels.Length];
            for (int i = 0; i < lodDistanceLevels.Length; i++)
            {
                lodMeshes[i] = new LODMesh(lodDistanceLevels[i].lod, UpdateChunk);
            }

            // Threading
            MapGenerator.Instance.RequestMapData(OnMapDataReceived, position);
        }

        public void UpdateChunk()
        {
            if (mapDataReceived) {
				float viewerDstFromNearestEdge = Mathf.Sqrt (bounds.SqrDistance (viewerPosition));
				bool visible = viewerDstFromNearestEdge <= maxViewDst;

				if (visible) {
					int lodIndex = 0;

					for (int i = 0; i < lodDistanceLevels.Length - 1; i++) {
						if (viewerDstFromNearestEdge > lodDistanceLevels [i].distance) {
							lodIndex = i + 1;
						} else {
							break;
						}
					}

					if (lodIndex != previousLOD) {
						LODMesh lodMesh = lodMeshes [lodIndex];
						if (lodMesh.hasMesh) {
							previousLOD = lodIndex;
							meshFilter.mesh = lodMesh.mesh;
                            meshCollider.sharedMesh = lodMesh.mesh;
                        } else if (!lodMesh.hasRequestedMesh) {
							lodMesh.RequestMesh (mapData);
						}
					}

                    chunksVisibleLastUpdate.Add (this);
				}

				SetVisible (visible);
			}
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
            Debug.Log("Map data received. Coord = " + coord.x + ", " + coord.y + ", Thread = " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            this.mapData = mapData;
            mapDataReceived = true;

            // Set the texture
            Texture2D texture = TextureHelper.ColorMapToTexture(mapData.colorMap, MapGenerator.Instance.GetMapSize());
            meshRenderer.material.mainTexture = texture;

            UpdateChunk();
        }
    }

    private class LODMesh
    {

        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        public int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.ToMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            MapGenerator.Instance.RequestMeshData(OnMeshDataReceived, mapData, lod);
        }

    }

    [System.Serializable]
    public struct LODDistance
    {
        public int lod;
        public float distance;
    }
}
