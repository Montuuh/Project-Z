using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// enum with 2D and 3D maptype generator
public enum MapTypeGen
{
    Map2D_perlin_BlackWhite,
    Map3D_perlin_BlackWhite
}

public class MapGenerator : MonoBehaviour
{
    public MapTypeGen mapType;
    public int mapWidth = 30;
    public int mapHeight = 30;
    public float scale = 0.3f;

    public bool autoUpdate = true;

    // This function generates a 3D mesh with perlin black/white texture
    public void GenerateMap3DPerlinBlackWhite()
    {
        if (scale <= 0)
            scale = 0.01f;
        
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

        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[(mapWidth + 1) * (mapHeight + 1)];
        for (int i = 0, z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float y = Mathf.PerlinNoise(x, z) * 2f;
                vertices[i] = new Vector3(x, y, z);
                i++;
            }
        }
        mesh.vertices = vertices;

        int[] triangles = new int[mapWidth * mapHeight * 6];
        int vert = 0;
        int tris = 0;
        for (int z = 0; z < mapHeight; z++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + mapWidth + 1;
                triangles[tris + 2] = vert + 1;
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + mapWidth + 1;
                triangles[tris + 5] = vert + mapWidth + 2;

                vert++;
                tris += 6;
            }
        }
        mesh.triangles = triangles;

        //// Set normals
        //Vector3[] normals = new Vector3[vertices.Length];
        //for (int n = 0; n < normals.Length; n++)
        //    normals[n] = Vector3.up;
        //mesh.normals = normals;

        //// Set UVs
        //Vector2[] uvs = new Vector2[vertices.Length];
        //for (int v = 0, z = 0; z <= mapHeight; z++)
        //{
        //    for (int x = 0; x <= mapWidth; x++, v++)
        //    {
        //        uvs[v] = new Vector2((float)x / mapWidth, (float)z / mapHeight);
        //    }
        //}
        //mesh.uv = uvs;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        // Set mesh
        GetComponent<MeshFilter>().mesh = mesh;

        // Perlin noise color (black & white)
        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colorMap[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        // Texture setter
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.mainTexture = texture;
        //meshRenderer.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }

    // This function generates a 2D mesh with perlin black/white texture
    public void GenerateMap2DperlinBlackWhite()
    {
        if (scale <= 0)
            scale = 0.01f;

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

        // Perlin noise (black & white)
        Texture2D texture = new Texture2D(mapWidth, mapHeight);
        Color[] colorMap = new Color[mapWidth * mapHeight];
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                colorMap[y * mapWidth + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        texture.SetPixels(colorMap);
        texture.Apply();

        // Texture setter
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.sharedMaterial.mainTexture = texture;
        meshRenderer.transform.localScale = new Vector3(mapWidth, 1, mapHeight);
    }
}