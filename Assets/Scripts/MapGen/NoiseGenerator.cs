using UnityEngine;

public static class NoiseGenerator
{
    private static int _octaves = 1;
    private static float _persistance = 1.0f;

    // Noise map generation with octaves, persistance and lacunarity
    public static float[,] GenerateNoiseMap(int chunkSize, float scale, int octaves, float persistance, float lacunarity, Vector2 offset, MapGenerator.MapNormalizeMode normalizeMode, int seed)
    {
        _octaves = octaves;
        _persistance = persistance;

        // Create noise map array
        float[,] noiseMap = new float[chunkSize, chunkSize];

        // Center noise map
        float centerX = chunkSize / 2f;
        float centerY = chunkSize / 2f;

        // Seed random offset. 
        // A number of parameter is used to generate the same noise map, so if the are two seeds with same number, the noise map will be the same
        System.Random prng = new System.Random(seed);

        // Octaves offset
        Vector2[] octaveOffsets = new Vector2[octaves];
        for (int i = 0; i < octaves; i++)
        {
            // The random offset is between -100000 and 100000
            float offsetX = offset.x + prng.Next(-100000, 100000);
            float offsetY = -offset.y + prng.Next(-100000, 100000);
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Generate noise map
        for (int y = 0; y < chunkSize; y++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                float amplitude = 1.0f;
                float frequency = 1.0f;
                float octaveHeight = 0.0f;

                for (int i = 0; i < octaves; i++)
                {
                    // Calculate the normalized coordinates in relation to the center of the chunk
                    float sampleX = (x - centerX + octaveOffsets[i].x) / scale * frequency;
                    float sampleY = (y - centerY + octaveOffsets[i].y) / scale * frequency;

                    // Calculate perlin noise value at sample point given
                    // Perlin noise returns a value between 0 and 1, so we multiply by 2 and subtract 1 to get a value between -1 and 1
                    float perlinVal = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;

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
        return NormalizeFloatMap(noiseMap, normalizeMode);
    }

    private static float[,] NormalizeFloatMap(float[,] map, MapGenerator.MapNormalizeMode normalizeMode)
    {
        if (normalizeMode == MapGenerator.MapNormalizeMode.Global)
            return NormalizeFloatMapGlobal(map);
        else
            return NormalizeFloatMapLocal(map);
    }

    private static float[,] NormalizeFloatMapLocal(float[,] map)
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

    private static float[,] NormalizeFloatMapGlobal(float[,] noiseMap)
    {
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);

        float maxPossibleNoiseHeight = 1.0f;
        float minPossibleNoiseHeight = -1.0f;

        // Normalize the noise map
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                // Adjusting the noise value to be between 0 and 1
                noiseMap[x, y] = (noiseMap[x, y] - minPossibleNoiseHeight) / (maxPossibleNoiseHeight - minPossibleNoiseHeight);
            }
        }

        return noiseMap;
    }
}