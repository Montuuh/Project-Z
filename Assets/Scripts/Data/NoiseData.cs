using UnityEngine;

[CreateAssetMenu()]
public class NoiseData : UpdatableData
{
    public MapGenerator.MapNormalizeMode normalizeMode;

    [Range(1, 10)] public int octaves;
    [Range(0.1f, 1f)] public float persistance;
    [Range(1f, 10f)] public float lacunarity;

    [Range(5f, 100f)] public float scale;

    [Range(0, 100000)] public int seed;
    public Vector2 offset;
}
