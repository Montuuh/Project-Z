using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode
    {
        NoiseMap,
        ColorMap,
        Mesh
    };
    public DrawMode drawMode;

    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    [Range(1, 8)]
    public int octaves;
    [Range(0, 1)]
    public float persistance;
    [Range((float)0.5, 8)]
    public float lacunarity;
    public int seed;
    public Vector2 offset;
    
    public bool autoUpdate;

    public TerrainType[] regions;

    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, octaves, persistance, lacunarity, seed, offset) ;

        Color[] colourMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentHeight <= regions[i].height)
                    {
                        colourMap[mapWidth * y + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay mapDisplay = FindObjectOfType<MapDisplay>();
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
                if (!mapDisplay.textureRenderer.gameObject.activeSelf)
                    mapDisplay.textureRenderer.gameObject.SetActive(true);
                if (mapDisplay.meshRenderer.gameObject.activeSelf)
                    mapDisplay.meshRenderer.gameObject.SetActive(false);

                mapDisplay.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
                break;
            case DrawMode.ColorMap:
                if (!mapDisplay.textureRenderer.gameObject.activeSelf)
                    mapDisplay.textureRenderer.gameObject.SetActive(true);
                if (mapDisplay.meshRenderer.gameObject.activeSelf)
                    mapDisplay.meshRenderer.gameObject.SetActive(false);

                mapDisplay.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                break;
            case DrawMode.Mesh:
                if (mapDisplay.textureRenderer.gameObject.activeSelf)
                    mapDisplay.textureRenderer.gameObject.SetActive(false);
                if (!mapDisplay.meshRenderer.gameObject.activeSelf)
                    mapDisplay.meshRenderer.gameObject.SetActive(true);

                mapDisplay.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap), TextureGenerator.TextureFromColourMap(colourMap, mapWidth, mapHeight));
                break;
        }
    }

    private void OnValidate()
    {
        if (mapWidth < 1)
        {
            mapWidth = 1;
        }
        if (mapHeight < 1)
        {
            mapHeight = 1;
        }
        if (noiseScale < 0.5)
        {
            noiseScale = 0.5f;
        }
    }
}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}