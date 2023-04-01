using UnityEngine;
using System.Collections;
using UnityEditor;
using JetBrains.Annotations;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator mapGen = (MapGenerator)target;
        
        if (DrawDefaultInspector()) 
        {
            if (mapGen.autoUpdate)
                Generate(mapGen.mapType);
        }

        if (GUILayout.Button("Generate"))
        {
            Generate(mapGen.mapType);
        }
    }

    private void Generate(MapTypeGen mapType)
    {
        MapGenerator mapGen = (MapGenerator)target;

        switch (mapType)
        {
            case MapTypeGen.Map2D_perlin_BlackWhite:
                mapGen.GenerateMap2DperlinBlackWhite();
                break;
            case MapTypeGen.Map3D_perlin_BlackWhite:
                mapGen.GenerateMap3DPerlinBlackWhite();
                break;
            default:
                // Not yet implemented
                break;
        }
    }
}
