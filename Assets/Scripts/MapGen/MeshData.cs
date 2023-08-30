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
        //mesh.RecalculateNormals();
        mesh.normals = CalculateNormals();
        return mesh;
    }

    // Calculate our own normals because Unity's RecalculateNormals() function does not work well with vertex seams
    private Vector3[] CalculateNormals()
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];

        // Calculate the normals of each triangle
        int triangleCount = triangles.Length / 3;
        for (int i = 0; i < triangleCount; i++)
        {
            // Get the indices of the vertices of the triangle
            int normalTriangleIndex = i * 3; // Start at i * 3 because we have 3 vertices per triangle
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            // Get the vertices of the triangle
            Vector3 triangleVertexA = vertices[vertexIndexA];
            Vector3 triangleVertexB = vertices[vertexIndexB];
            Vector3 triangleVertexC = vertices[vertexIndexC];

            // Calculate the normal of the triangle
            Vector3 triangleNormal = TriangleNormalFromVertices(triangleVertexA, triangleVertexB, triangleVertexC);

            // Set the normals of the triangle
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        // Normalize the normals
        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    private Vector3 TriangleNormalFromVertices(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC)
    {
        Vector3 triangleNormal = Vector3.zero;

        // Get the vectors AB and AC of the triangle
        Vector3 vectorAB = vertexB - vertexA;
        Vector3 vectorAC = vertexC - vertexA;

        // Calculate the cross product of AB and AC to get the normal of the triangle
        triangleNormal = Vector3.Cross(vectorAB, vectorAC);

        // Normalize the normal vector
        triangleNormal.Normalize();

        return triangleNormal;
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
