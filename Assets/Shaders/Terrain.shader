Shader "Custom/Terrain"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows
        

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        const static int maxBaseColors = 8;

        int baseColorCount;
        float3 baseColors[maxBaseColors];
        float baseStartHeights[maxBaseColors];

        float3 colorFirst;
        float3 colorSecond;

        float minHeight;
        float maxHeight;

        struct Input
        {
            float3 worldPos;
        };

        float inverseLerp(float a, float b, float value)
        {
            return saturate((value - a) / (b - a));
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float height = IN.worldPos.y;

            ////////////////////////////////////////////////////////////
            // Depending on the height of the terrain, do a gradient from black to white
            // float heightPercentage = inverseLerp(minHeight, maxHeight, height); 
            // float3 color = lerp(float3(0, 0, 0), float3(1, 1, 1), heightPercentage); 
            // o.Albedo = color; 
            ////////////////////////////////////////////////////////////

            ////////////////////////////////////////////////////////////
            // Depending on the height of the terrain, do a gradient from colorFirst to colorSecond
            // float heightPercentage = inverseLerp(minHeight, maxHeight, height); 
            // float3 color = lerp(colorFirst, colorSecond, heightPercentage);
            // o.Albedo = color;
            ////////////////////////////////////////////////////////////

            //////////////////////////////////////////////////////////// 
            // Depending on the height of the terrain, select a color from the baseColors array
            // float heightPercentage = inverseLerp(minHeight, maxHeight, height);
            // if (heightPercentage <= 0)
            // {
                //     heightPercentage = 0.0001;
            // }
            // for (int i = 0; i < baseColorCount; i++)
            // {
                //     float drawStrength = saturate(sign(heightPercentage - baseStartHeights[i]));
                //     o.Albedo = o.Albedo * (1 - drawStrength) + baseColors[i] * drawStrength;
            // }
            //////////////////////////////////////////////////////////// 

            float heightPercentage = inverseLerp(minHeight, maxHeight, height);

            float3 finalColor = float3(0, 0, 0); // Initialize to black

            for (int i = 0; i < baseColorCount - 1; i++)
            {
                float lowerBound = baseStartHeights[i];
                float upperBound = baseStartHeights[i + 1];

                if (heightPercentage >= lowerBound && heightPercentage <= upperBound)
                {
                    float localHeightPercentage = inverseLerp(lowerBound, upperBound, heightPercentage);
                    finalColor = lerp(baseColors[i], baseColors[i + 1], localHeightPercentage);
                    break;
                }
            }

            o.Albedo = finalColor;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
