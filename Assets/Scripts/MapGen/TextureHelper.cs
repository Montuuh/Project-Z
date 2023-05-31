using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureHelper
{
    // This function transforms a color map into a texture
    public static Texture2D ColorMapToTexture(Color[] colorMap, int chunkSize)
    {
        Texture2D texture = new Texture2D(chunkSize, chunkSize);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    // This function transforms a noise map into a texture
    public static Texture2D NoiseMapToTexture(float[,] noiseMap)
    {
        int chunkSize = noiseMap.GetLength(0);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                colorMap[y * chunkSize + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return ColorMapToTexture(colorMap, chunkSize);
    }

    // This function transforms a noise map into a basic color map array depending on the terrain types
    public static Color[] GetColorMapFromNoiseMap(float[,] noiseMap, TerrainType[] terrainTypes)
    {
        int chunkSize = noiseMap.GetLength(0);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (currentHeight <= terrainTypes[i].Height)
                    {
                        colorMap[y * chunkSize + x] = terrainTypes[i].Color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }

    // This function transforms a noise map into a basic black and white color map array
    public static Color[] GetColorMapFromNoiseMap(float[,] noiseMap)
    {
        int chunkSize = noiseMap.GetLength(0);

        Color[] colorMap = new Color[chunkSize * chunkSize];
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                colorMap[y * chunkSize + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return colorMap;
    }
}

