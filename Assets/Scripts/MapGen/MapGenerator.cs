using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapTypeGen
{
    Map2D_perlin,
    Map2D_perlin_without_octaves,
    Map2D_texture,
    Map3D_perlin,
    Map3D_texture
}

public class MapGenerator : MonoBehaviour
{
    public MapTypeGen mapType;

    [Range(1, 100)] public int mapWidth = 10;
    [Range(1, 100)] public int mapHeight = 10;

    [Range(1, 10)] public int octaves = 1;
    [Range(0.1f, 1f)] public float persistance = 0.5f;
    [Range(1f, 10f)] public float lacunarity = 2f;

    [Range(0.3f, 100f)] public float scale = 0.3f;

    public bool autoUpdate = true;

    public Renderer textureRenderer;

    public void GenerateMap()
    {
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin:
                GenerateMap2Dperlin();
                break;
            case MapTypeGen.Map2D_perlin_without_octaves:
                GenerateMap2DperlinWithoutOctaves();
                break;
            case MapTypeGen.Map2D_texture:
                GenerateMap2Dtexture();
                break;
            case MapTypeGen.Map3D_perlin:
                GenerateMap3Dperlin();
                break;
            case MapTypeGen.Map3D_texture:
                GenerateMap3Dtexture();
                break;
            default:
                Debug.LogError("MapTypeGen not implemented");
                throw new System.NotImplementedException();
        }
    }

    private void GenerateMap2Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colorMap[z * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
    }

    private void GenerateMap2DperlinWithoutOctaves()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colorMap[z * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, z]);
            }
        }

        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        texture.SetPixels(colorMap);
        texture.Apply();

        textureRenderer.sharedMaterial.mainTexture = texture;
    }

    private void GenerateMap2Dtexture()
    {
        throw new System.NotImplementedException();
    }

    private void GenerateMap3Dperlin()
    {
        throw new System.NotImplementedException();
    }

    private void GenerateMap3Dtexture()
    {
        throw new System.NotImplementedException();
    }
}
