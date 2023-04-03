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

        // Return normalized noise map
        return NormalizeFloatMap(noiseMap);
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

        // Center noise map
        float centerX = mapWidth / 2f;
        float centerY = mapHeight / 2f;

        // Octaves offset
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = offset.x;
            float offsetY = offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

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
                    float sampleX = ((x - centerX) / scale * frequency) + octaveOffsets[i].x;
                    float sampleY = ((y - centerY) / scale * frequency) + octaveOffsets[i].y;

                    // Calculate periln noise value at sample point given
                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY);

                    // Add perlin noise value to total octaves height
                    octaveHeight += perlinVal * amplitude;

                    // Update amplitude and frequency for next octave
                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                // Assign the final octaves height to the noise map
                noiseMap[x, y] = octaveHeight;
            }
        }

        // Return normalized noise map
        return NormalizeFloatMap(noiseMap);
    }

    private static float[,] NormalizeFloatMap(float[,] map)
    {
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        // Find the minimum and maximum values in the noise map
        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                float value = map[x, y];
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }
        }

        // Normalize the noise map values
        float[,] normalizedMap = new float[map.GetLength(0), map.GetLength(1)];

        for (int y = 0; y < map.GetLength(1); y++)
        {
            for (int x = 0; x < map.GetLength(0); x++)
            {
                normalizedMap[x, y] = Mathf.InverseLerp(minValue, maxValue, map[x, y]);
            }
        }

        return normalizedMap;
    }
}