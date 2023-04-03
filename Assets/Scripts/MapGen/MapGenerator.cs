using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MapTypeGen
{
    Map2D_perlin,
    Map2D_perlin_without_octaves,
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

public class MapGenerator : MonoBehaviour
{
    public MapTypeGen mapType;

    [Range(1, 100)] public int mapWidth = 10;
    [Range(1, 100)] public int mapHeight = 10;

    [Range(1, 10)] public int octaves = 1;
    [Range(0.1f, 1f)] public float persistance = 0.5f;
    [Range(1f, 10f)] public float lacunarity = 2f;

    [Range(0.3f, 100f)] public float scale = 0.3f;

    public Vector2 offset = new Vector2(0, 0);

    public bool autoUpdate = true;

    public Renderer texture2DRenderer;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;

    [SerializeField]
    public TerrainType[] terrainTypes;

    public void Start()
    {
        GenerateMap();
    }

    public void Update()
    {
        // Animation test
        offset.x += Time.deltaTime * 10;
        GenerateMap();

        // Keyboard input test
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            offset.y += 100;
            GenerateMap();
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            offset.y -= 100;
            GenerateMap();
        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            offset.x -= 100;
            GenerateMap();
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            offset.x += 100;
            GenerateMap();
        }
    }

    public void GenerateMap()
    {
        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin:
                GenerateMap2Dperlin();
                break;
            case MapTypeGen.Map2D_perlin_without_octaves:
                GenerateMap2DperlinWithoutOctaves();
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

    private void GenerateMap2Dperlin()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2DperlinWithoutOctaves()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, offset);
        SetTexture(TextureHelper.NoiseMapToTexture(noiseMap));
    }

    private void GenerateMap2Dtexture()
    {
        float[,] noiseMap = NoiseGenerator.GenerateNoiseMap(mapWidth, mapHeight, scale, octaves, persistance, lacunarity, offset);
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

        SetTexture(TextureHelper.ColorMapToTexture(colorMap, mapWidth, mapHeight));
    }

    private void GenerateMap3Dperlin()
    {
        throw new System.NotImplementedException();
    }

    private void GenerateMap3Dtexture()
    {
        throw new System.NotImplementedException();
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
    }
}

public static class TextureHelper {
    // This function transforms a color map into a texture
    public static Texture2D ColorMapToTexture(Color[] colorMap, int width, int height) {
        Texture2D texture = new Texture2D(width, height);
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    // This function transforms a noise map into a texture
    public static Texture2D NoiseMapToTexture(float[,] noiseMap) {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }

        return ColorMapToTexture(colorMap, width, height);
    }
}