using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator mapGenerator;

    public enum MapTypeGen { Map2D_perlin_without_octaves, Map2D_perlin, Map2D_texture, Map3D_perlin, Map3D_texture }

    public MapTypeGen mapType;

    [SerializeField] public int chunkSize = 129;
    [Range(0, 3)] public int lodIndex = 0;

    [Range(1, 10)] public int octaves = 1;
    [Range(0.1f, 1f)] public float persistance = 0.5f;
    [Range(1f, 10f)] public float lacunarity = 2f;

    [Range(0.3f, 100f)] public float scale = 0.3f;

    [Range(0.5f, 50f)] public float heightMultiplier = 1f;
    public AnimationCurve heightCurve;
    public Vector2 offset = new Vector2(0, 0);

    public Renderer texture2DRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    [SerializeField] public TerrainType[] terrainTypes;

    public void Start()
    {
        mapGenerator = this;
        // GenerateMap();
    }

    public void Update()
    {
        // Animation test
        offset.x += Time.deltaTime;
        GenerateMap();
    }

    public void GenerateMap()
    {
        // This function sets the active map (2D or 3D)
        SetActive();

        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin_without_octaves:
                GenerateMap2DperlinWithoutOctaves();
                break;
            case MapTypeGen.Map2D_perlin:
                GenerateMap2Dperlin();
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

    private void SetActive()
    {
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin_without_octaves:
            case MapTypeGen.Map2D_perlin:
            case MapTypeGen.Map2D_texture:
                SetActiveMap2D();
                break;
            case MapTypeGen.Map3D_perlin:
            case MapTypeGen.Map3D_texture:
                SetActiveMap3D();
                break;
            default:
                Debug.LogError("MapTypeGen not implemented");
                throw new System.NotImplementedException();
        }
    }

    private void SetActiveMap2D()
    {
        texture2DRenderer.gameObject.SetActive(true);
        meshFilter.gameObject.SetActive(false);
    }

    private void SetActiveMap3D()
    {
        texture2DRenderer.gameObject.SetActive(false);
        meshFilter.gameObject.SetActive(true);
    }

    private void GenerateMap2DperlinWithoutOctaves()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, octaves, persistance, lacunarity, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2Dtexture()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, octaves, persistance, lacunarity, offset);
        SetTexture(TextureHelper.ColorMapToTexture(TextureHelper.GetColorMapFromNoiseMap(noiseMap, terrainTypes), chunkSize));
    }

    private void GenerateMap3Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, octaves, persistance, lacunarity, offset);
        meshFilter.sharedMesh = MeshHelperGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve, lodIndex).ToMesh();
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap3Dtexture()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(chunkSize, scale, octaves, persistance, lacunarity, offset);
        meshFilter.sharedMesh = MeshHelperGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, heightCurve, lodIndex).ToMesh();
        SetTexture(TextureHelper.ColorMapToTexture(TextureHelper.GetColorMapFromNoiseMap(noiseMap, terrainTypes), chunkSize));
    }

    private void SetTexture(Texture2D texture)
    {
        texture2DRenderer.sharedMaterial.mainTexture = texture;
        texture2DRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Point;

        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.sharedMaterial.mainTexture.wrapMode = TextureWrapMode.Clamp;
        meshRenderer.sharedMaterial.mainTexture.filterMode = FilterMode.Point;
    }
}

[System.Serializable]
public class TerrainType
{
    [SerializeField] private string name;
    [SerializeField] private float height;
    [SerializeField] private Color color;

    public TerrainType(string name, float height, Color color)
    {
        this.name = name;
        this.height = height;
        this.color = color;
    }

    public string Name => name;
    public float Height => height;
    public Color Color => color;
}