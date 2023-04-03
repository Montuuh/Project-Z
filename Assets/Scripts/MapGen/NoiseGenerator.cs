using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    // Basic noise map generation (without octaves and seeds)
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, Vector2 offset)
    {
        // Check for invalid values
        if (scale < 0.3f)
            scale = 0.3f;
        if (mapWidth <= 0)
            mapWidth = 1;
        if (mapWidth <= 0)
            mapWidth = 1;

        // Create noise map array
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Generate noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Sample noise map in noise space
                float sampleX = (x + offset.x) / scale;
                float sampleY = (y + offset.y) / scale;

                // Calculate periln noise value at sample point given
                float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);

                // Assign perlin noise value to noise map
                noiseMap[x, y] = perlinVal;
            }
        }

        // Normalize values
        noiseMap = NormalizeFloatMap(noiseMap);

        // Return noise map
        return noiseMap;
    }

    // Noise map generation with octaves, persistance and lacunarity
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        // Check for invalid values
        if (scale <= 0.3f)
            scale = 0.3f;
        if (mapWidth <= 0)
            mapWidth = 1;
        if (mapWidth <= 0)
            mapWidth = 1;

        // Create noise map array
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Generate noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1.0f;
                float frequency = 1.0f;
                float octaveHeight = 0.0f;

                for (int i = 0; i < octaves; i++)
                {
                    // Sample noise map in noise space
                    float sampleX = (x + offset.x) / scale * frequency;
                    float sampleY = (y + offset.y) / scale * frequency;

                    // Calculate periln noise value at sample point given
                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

                    // Add perlin noise value to octave height
                    octaveHeight += perlinVal * amplitude;

                    // Increase amplitude and frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Assign the final octave height to the noise map
                noiseMap[x, y] = octaveHeight;
            }
        }

        // Normalize values
        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                if (noiseMap[x, y] > maxNoiseHeight)
                    maxNoiseHeight = noiseMap[x, y];
                if (noiseMap[x, y] < minNoiseHeight)
                    minNoiseHeight = noiseMap[x, y];
            }
        }
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        // Return noise map
        return noiseMap;
    }

    public static float[,] NormalizeFloatMap(float[,] map)
    {
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Find the minimum and maximum values in the noise map
        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                float value = map[y, x];
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }
        }

        // Normalize the noise map values
        float[,] normalizedMap = new float[map.GetLength(0), map.GetLength(1)];

        for (int y = 0; y < map.GetLength(0); y++)
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                normalizedMap[y, x] = Mathf.InverseLerp(minValue, maxValue, map[y, x]);
            }
        }

        return normalizedMap;
    }
}