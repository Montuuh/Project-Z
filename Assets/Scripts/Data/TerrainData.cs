using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 1.5f;
    [Range(0.5f, 50f)] public float heightMultiplier;
    public AnimationCurve heightCurve;
}
