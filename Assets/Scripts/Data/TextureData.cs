using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdatableData
{
    float savedMinHeight;
    float savedMaxHeight;
    
    public Color[] baseColors;
    [Range(0, 1)] public float[] baseStartHeights;

    public void ApplyToMaterial(Material material)
    {
        Debug.Log("Setting: " + material.name + " baseColorCount " + baseColors.Length + " baseStartHeightsCount " + baseStartHeights.Length);
        material.SetInt("baseColorCount", baseColors.Length);
        material.SetColorArray("baseColors", baseColors);
        material.SetFloatArray("baseStartHeights", baseStartHeights);

        UpdateMeshHeights(material, savedMinHeight, savedMaxHeight);
    }

    public void UpdateMeshHeights(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMaxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }
}
