using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public MapTypeGen mapType;

    [Range(1, 100)] public int mapWidth = 10;
    [Range(1, 100)] public int mapHeight = 10;

    [Range(1, 10)] public int octaves = 1;
    [Range(0.1f, 1f)] public float persistance = 0.5f;
    [Range(1f, 10f)] public float lacunarity = 2f;

    [Range(0.3f, 100f)] public float scale = 0.3f;

    [Range(0.5f, 50f)]
    public float heightMultiplier = 1f;
    public AnimationCurve heightCurve;
    public Vector2 offset = new Vector2(0, 0);

    public bool autoUpdate = true;

    public Renderer texture2DRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    [SerializeField]
    public TerrainType[] terrainTypes;

    public void Start()
    {
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
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin_without_octaves:
            case MapTypeGen.Map2D_perlin:
            case MapTypeGen.Map2D_texture:
                meshFilter.gameObject.SetActive(false);
                texture2DRenderer.gameObject.SetActive(true);
                break;
            case MapTypeGen.Map3D_perlin:
            case MapTypeGen.Map3D_texture:
                meshFilter.gameObject.SetActive(true);
                texture2DRenderer.gameObject.SetActive(false);
                break;
        }

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

    private void GenerateMap2DperlinWithoutOctaves()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2Dtexture()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
        SetTexture(TextureHelper.ColorMapToTexture(GetColorMapFromNoiseMap(noiseMap), mapWidth, mapHeight));
    }

    private void GenerateMap3Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
        meshFilter.sharedMesh = MeshHelperGenerator.GenerateMesh(noiseMap, heightMultiplier, heightCurve).ToMesh();
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap3Dtexture()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
        meshFilter.sharedMesh = MeshHelperGenerator.GenerateMesh(noiseMap, heightMultiplier, heightCurve).ToMesh();
        SetTexture(TextureHelper.ColorMapToTexture(GetColorMapFromNoiseMap(noiseMap), mapWidth, mapHeight));
    }

    private void InitializeTerrainTypes()
    {
        terrainTypes = new TerrainType[]
        {
            new TerrainType("Water", 0, new Color(0, 0, 1)),
            new TerrainType("Sand", 0.2f, new Color(1, 1, 0)),
            new TerrainType("Grass", 0.5f, new Color(0, 1, 0)),
            new TerrainType("Rock", 0.7f, new Color(0.5f, 0.5f, 0.5f)),
            new TerrainType("Snow", 1, new Color(1, 1, 1))
        };
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

    private Color[] GetColorMapFromNoiseMap(float[,] noiseMap)
    {
        int mapWidth = noiseMap.GetLength(0);
        int mapHeight = noiseMap.GetLength(1);

        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float currentHeight = noiseMap[x, y];
                for (int i = 0; i < terrainTypes.Length; i++)
                {
                    if (currentHeight <= terrainTypes[i].height)
                    {
                        colorMap[y * mapWidth + x] = terrainTypes[i].color;
                        break;
                    }
                }
            }
        }

        return colorMap;
    }
}

public static class TextureHelper
{
    // This function transforms a color map into a texture
    public static Texture2D ColorMapToTexture(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    // This function transforms a noise map into a texture
    public static Texture2D NoiseMapToTexture(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return ColorMapToTexture(colorMap, width, height);
    }
}

public static class MeshHelperGenerator
{
    public static MeshHelper GenerateMesh(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        float centerX = (width - 1) / -2f;
        float centerZ = (height - 1) / 2f;

        MeshHelper meshHelper = new MeshHelper(width, height);

        int vi = 0; // vertex index
        int ti = 0; // triangle index

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                meshHelper.SetVertex(vi, centerX + x, heightCurve.Evaluate(noiseMap[x, y]) * heightMultiplier, centerZ - y);
                meshHelper.SetUV(vi, x / (float)width, y / (float)height);

                if (x < width - 1 && y < height - 1)
                {
                    meshHelper.SetTriangle(ti, vi, vi + width + 1, vi + width);
                    meshHelper.SetTriangle(ti + 3, vi + width + 1, vi, vi + 1);

                    ti += 6;
                }

                vi++;
            }
        }

        return meshHelper;
    }
}

public class MeshHelper
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;
    private int width;
    private int height;

    // Constructor
    public MeshHelper(int width, int height)
    {
        vertices = new Vector3[width * height];
        triangles = new int[(width - 1) * (height - 1) * 6];
        uvs = new Vector2[width * height];
        this.width = width;
        this.height = height;
    }

    // Setters
    public void SetVertex(int i, float x, float y, float z)
    {
        vertices[i] = new Vector3(x, y, z);
    }

    public void SetTriangle(int i, int a, int b, int c)
    {
        triangles[i] = a;
        triangles[i + 1] = b;
        triangles[i + 2] = c;
    }

    public void SetUV(int i, float x, float y)
    {
        uvs[i] = new Vector2(x, y);
    }

    // This function transforms the mesh helper class into a mesh
    public Mesh ToMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}

public enum MapTypeGen
{
    Map2D_perlin_without_octaves,
    Map2D_perlin,
    Map2D_texture,
    Map3D_perlin,
    Map3D_texture
}

[System.Serializable]
public class TerrainType
{
    public string name;
    public float height;
    public Color color;

    public TerrainType(string name, float height, Color color)
    {
        this.name = name;
        this.height = height;
        this.color = color;
    }
}