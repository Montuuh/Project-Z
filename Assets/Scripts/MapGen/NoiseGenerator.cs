using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        if (scale <= 0)
            scale = 0.01f;
        if (mapWidth <= 0)
            mapWidth = 1;
        if (mapHeight <= 0)
            mapHeight = 1;

        float[,] noiseMap = new float[mapWidth, mapHeight];

        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = x / scale;
                float sampleZ = z / scale;

                float perlin = Mathf.PerlinNoise(sampleX, sampleZ);
                noiseMap[x, z] = perlin;
            }
        }

        return noiseMap;
    }
}