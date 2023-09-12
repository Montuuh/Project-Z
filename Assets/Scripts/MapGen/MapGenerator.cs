using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

public class MapGenerator : MonoBehaviour
{
    // Singleton pattern
    public static MapGenerator Instance;
    
    // Enums

    #region Inspector Variables
    public enum MapTypeGen { perlinNoise, mesh }
    public enum MapNormalizeMode { Local, Global }
    public TerrainData terrainData;
    public NoiseData noiseData;
    public TextureData textureData;

    public Material terrainMaterial;

    public MapTypeGen mapType;
    public bool autoUpdateEditor;
    [Range(0, 3)] public int editorLodIndex = 0;
    public GameObject local2Dmesh;
    public GameObject local3Dmesh;
    #endregion
    
    #region Hide Variables
    private const int chunkSize = 129;
    private const int mapSize = chunkSize;
    #endregion

    private ConcurrentQueue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MapData>>();
    private ConcurrentQueue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new ConcurrentQueue<MapThreadInfo<MeshData>>();

    // Getters
    // Chunk size returns -1 because the real chunkSize is 129x129 but the effective chunkSize is 128x128
    public int GetChunkSize() { return chunkSize - 1; }
    public int GetMapSize() { return mapSize; }


    // Unity Methods
    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Debug.LogError("More than one MapGenerator in scene!");
            Destroy(this);
        }

        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);
    }

    private void OnValidate()
    {
        if (noiseData != null)
        {
            noiseData.OnValuesUpdated -= OnValuesUpdated;
            noiseData.OnValuesUpdated += OnValuesUpdated;
        }
        if (terrainData != null)
        {
            terrainData.OnValuesUpdated -= OnValuesUpdated;
            terrainData.OnValuesUpdated += OnValuesUpdated;
        }
        if (textureData != null)
        {
            textureData.OnValuesUpdated -= OnTextureValuesUpdated;
            textureData.OnValuesUpdated += OnTextureValuesUpdated;
        }
    }

    private void OnValuesUpdated()
    {
        if (!Application.isPlaying)
        {
            GenerateMap();
        }
    }

    private void OnTextureValuesUpdated()
    {
        textureData.ApplyToMaterial(terrainMaterial);
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
        textureData.UpdateMeshHeights(terrainMaterial, terrainData.minHeight, terrainData.maxHeight);

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
            case MapTypeGen.perlinNoise:
                SetTexture(texture2D);
                break;
            case MapTypeGen.mesh:
                local3Dmesh.GetComponent<MeshFilter>().sharedMesh = MeshDataHelper.GenerateTerrainMesh(mapData.noiseMap, terrainData.heightMultiplier, terrainData.heightCurve, editorLodIndex).ToMesh();
                local3Dmesh.transform.localScale = Vector3.one * terrainData.uniformScale;
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
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, noiseData.scale, noiseData.octaves, noiseData.persistance, noiseData.lacunarity, noiseData.offset + chunkOffset, noiseData.normalizeMode, noiseData.seed);

        return new MapData(noiseMap);
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

        // Using concurrent queues, lock is not needed because it is thread safe
        mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
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
        MeshData meshData = MeshDataHelper.GenerateTerrainMesh(mapData.noiseMap, terrainData.heightMultiplier, terrainData.heightCurve, lod);

        // Using concurrent queues, lock is not needed because it is thread safe
        meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
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
public struct MapData
{
    public float[,] noiseMap;

    public MapData(float[,] noiseMap)
    {
        this.noiseMap = noiseMap;
    }
}

