using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This class contains general info about a mesh and has some methods to help with the creation of a mesh
public class MeshData
{
    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uvs;

    // Constructor
    public MeshData(int chunkSize)
    {
        vertices = new Vector3[chunkSize * chunkSize];
        triangles = new int[(chunkSize - 1) * (chunkSize - 1) * 6];
        uvs = new Vector2[chunkSize * chunkSize];
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

public static class MeshDataHelper
{
    public static MeshData GenerateTerrainMesh(float[,] noiseMap, float heightMultiplier, AnimationCurve heightCurve, int lodLevel)
    {
        AnimationCurve heightCurveCopy = new AnimationCurve(heightCurve.keys);

        int chunkSize = noiseMap.GetLength(0);

        // SKIPPING VERTICES LOD SYSTEM
        // LOD 0 = 1 vertex per square (no skipping)
        // LOD 1 = 2 vertices per square (skipping 1 vertex per square)
        // LOD 2 = 4 vertices per square (skipping 3 vertices per square)
        // LOD 3 = 8 vertices per square (skipping 7 vertices per square)
        int lodStep = (int)Mathf.Pow(2, lodLevel);

        // The number of vertices per line of the mesh rendered at LOD level
        int lodVertexCount = (chunkSize - 1) / lodStep + 1;

        // For the mesh to be centered at the origin
        float centerX = (chunkSize - 1) / -2f;
        float centerZ = (chunkSize - 1) / 2f;

        // Initialize mesh helper variables
        MeshData MeshData = new MeshData(lodVertexCount);

        int vi = 0; // vertex index
        int ti = 0; // triangle index

        for (int y = 0; y < chunkSize; y += lodStep)
        {
            for (int x = 0; x < chunkSize; x += lodStep)
            {
                MeshData.SetVertex(vi, centerX + x, heightCurveCopy.Evaluate(noiseMap[x, y]) * heightMultiplier, centerZ - y);
                MeshData.SetUV(vi, x / (float)(chunkSize - 1), y / (float)(chunkSize - 1));

                // We do not want to create triangles on the right and bottom edges of the mesh
                if (x < chunkSize - lodStep && y < chunkSize - lodStep)
                {
                    MeshData.SetTriangle(ti, vi, vi + lodVertexCount + 1, vi + lodVertexCount);
                    MeshData.SetTriangle(ti + 3, vi + lodVertexCount + 1, vi, vi + 1);

                    ti += 6;
                }

                vi++;
            }
        }

        return MeshData;
    }
}
