using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance;

    public enum MapTypeGen { Map2D_perlin, Map2D_texture, Map3D_perlin, Map3D_texture, Map3D_Infinite }
    public enum MapNormalizeMode { Local, Global }

    public MapTypeGen mapType;
    public MapNormalizeMode normalizeMode;
    public bool autoUpdateEditor;

    [SerializeField] private const int chunkSize = 129;
    [SerializeField] private const int mapSize = chunkSize;
    [Range(0, 3)] public int editorLodIndex = 0;

    [Range(1, 10)] public int octaves = 1;
    [Range(0.1f, 1f)] public float persistance = 0.5f;
    [Range(1f, 10f)] public float lacunarity = 2f;

    [Range(5f, 100f)] public float scale = 5f;
    
    [Range(0, 100000)] public int seed = 0;

    [Range(0.5f, 50f)] public float heightMultiplier = 1f;
    public AnimationCurve heightCurve;
    public Vector2 offset = new Vector2(0, 0);

    public GameObject local2Dmesh;
    public GameObject local3Dmesh;

    [SerializeField] public TerrainType[] terrainTypes;

    private ConcurrentQueue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MapData>>();
    private ConcurrentQueue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MeshData>>();

    // Getters
    // Chunk size returns -1 because the real chunkSize is 129x129 but the effective chunkSize is 128x128
    public int GetChunkSize() { return chunkSize - 1; }
    public int GetMapSize() { return mapSize; }


    // Unity Methods
    public void Awake()
    {
        Instance = this;
    }

    public void Start()
    {

    }

    public void Update()
    {
        // This function checks if there is any map data to be processed
        if (mapDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                // Dequeue the concurrent queue
                bool isDone = mapDataThreadInfoQueue.TryDequeue(out MapThreadInfo<MapData> threadInfo);

                // Set the callback function
                if (isDone)
                    threadInfo.callback(threadInfo.parameter);
            }
        }

        // This function checks if there is any mesh data to be processed
        if (meshDataThreadInfoQueue.Count > 0)
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                // Dequeue the concurrent queue
                bool isDone = meshDataThreadInfoQueue.TryDequeue(out MapThreadInfo<MeshData> threadInfo);

                // Set the callback function
                if (isDone)
                    threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    // Methods
    // This function generates the map data and draws it (only called from editor button)
    public void GenerateMap()
    {
        // This function sets the active map (2D or 3D or Infinite)
        HandleMap();

        // Generate initial map data
        MapData mapData = GenerateMapData(Vector2.zero);

        // Draw map
        DrawMap(mapData);
    }

    private void DrawMap(MapData mapData)
    {
        Texture2D texture2D = TextureHelper.NoiseMapToTexture(mapData.noiseMap);
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin:
                SetTexture(texture2D);
                break;
            case MapTypeGen.Map2D_texture:
                texture2D = TextureHelper.ColorMapToTexture(mapData.colorMap, chunkSize);
                SetTexture(texture2D);
                break;
            case MapTypeGen.Map3D_perlin:
                SetTexture(texture2D);
                local3Dmesh.GetComponent<MeshFilter>().sharedMesh = MeshDataHelper.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, heightCurve, editorLodIndex).ToMesh();
                break;
            case MapTypeGen.Map3D_texture:
                texture2D = TextureHelper.ColorMapToTexture(mapData.colorMap, chunkSize);
                SetTexture(texture2D);
                local3Dmesh.GetComponent<MeshFilter>().sharedMesh = MeshDataHelper.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, heightCurve, editorLodIndex).ToMesh();
                break;
        }
    }

    private void HandleMap()
    {
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin:
            case MapTypeGen.Map2D_texture:
                local2Dmesh.gameObject.SetActive(true);
                local3Dmesh.SetActive(false);
                break;
            case MapTypeGen.Map3D_perlin:
            case MapTypeGen.Map3D_texture:
                local2Dmesh.gameObject.SetActive(false);
                local3Dmesh.SetActive(true);
                break;
            case MapTypeGen.Map3D_Infinite:
                local2Dmesh.gameObject.SetActive(false);
                local3Dmesh.SetActive(false);

#if UNITY_EDITOR
                EditorApplication.isPlaying = true;
#endif
                break;
        }
    }

    private void SetTexture(Texture2D texture)
    {
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        local2Dmesh.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
        local2Dmesh.transform.localScale = new Vector3(texture.width, 1, texture.height);

        local3Dmesh.GetComponent<MeshRenderer>().sharedMaterial.mainTexture = texture;
    }

    private MapData GenerateMapData(Vector2 chunkOffset)
    {
        // Generate noise map
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, octaves, persistance, lacunarity, offset + chunkOffset, normalizeMode, seed);

        // Generate color map from terrain types
        Color[] colorMap = TextureHelper.GetColorMapFromNoiseMap(noiseMap, terrainTypes);

        return new MapData(noiseMap, colorMap);
    }

    // Threading
    public void RequestMapData(Action<MapData> callback, Vector2 chunkOffset)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(callback, chunkOffset);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Action<MapData> callback, Vector2 chunkOffset)
    {
        MapData mapData = GenerateMapData(chunkOffset);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(Action<MeshData> callback, MapData mapData, int lod)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(callback, mapData, lod);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(Action<MeshData> callback, MapData mapData, int lod)
    {
        MeshData meshData = MeshDataHelper.GenerateTerrainMesh(mapData.noiseMap, heightMultiplier, heightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    // Structs & Classes
    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}

[System.Serializable]
public class TerrainType
{
    [SerializeField] private string name;
    [SerializeField] private float height;
    [SerializeField] private Color color;

    public TerrainType(string name, float height, Color color)
    {
        this.name = name;
        this.height = height;
        this.color = color;
    }

    public string Name => name;
    public float Height => height;
    public Color Color => color;
}

[System.Serializable]
public struct MapData
{
    public float[,] noiseMap;
    public Color[] colorMap;
    // public MeshData meshData;
    // public int lodIndex;

    public MapData(float[,] noiseMap, Color[] colorMap/*, MeshData meshData, int lodIndex*/)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
        // this.meshData = meshData;
        // this.lodIndex = lodIndex;
    }
}

