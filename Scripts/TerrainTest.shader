 Shader "Custom/Terrain2"
{
    Properties
    {
        baseTextures("Texture2DArray display name", 2DArray) = "" {}
        _colourTextures("Texture", 2D) = "white" {}
        layerCount("layerCount", int) = 0.0
        minHeight("minHeight", float) = 0.0
        maxHeight("maxHeight", float) = 0.0
        waterLevel("waterLevel", float) = -1.0
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

        const static int maxLayerCount = 16;//729 = 27*27
        const static float epsilon = 1E-4;

        int layerCount;
        float3 baseColours[maxLayerCount];//Скобки массива здесь указываются после имени переменной
        float baseStartHeights[maxLayerCount];
        float baseBlends[maxLayerCount];
        float baseColourStrength[maxLayerCount];
        float baseTextureScales[maxLayerCount];

        float minHeight;
        float maxHeight;
        float waterLevel;

        sampler2D _colourTextures;
        UNITY_DECLARE_TEX2DARRAY(baseTextures);

        struct Input
        {
            float3 worldPos;
            float3 worldNormal;
            float2 uv_colourTextures;
        };

        float inverseLerp(float a, float b, float value) {
            return saturate ((value - a) / (b - a)); //saturate - зажимает число от 0 до 1
        }

        float3 triplanar(float3 worldPos, float scale, float3 blendAxes, int textureIndex) {
            float3 scaledWorldPos = worldPos / scale;

            float3 xProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.y, scaledWorldPos.z, textureIndex)) * blendAxes.x;
            float3 yProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.z, textureIndex)) * blendAxes.y;
            float3 zProjection = UNITY_SAMPLE_TEX2DARRAY(baseTextures, float3(scaledWorldPos.x, scaledWorldPos.y, textureIndex)) * blendAxes.z;
            return xProjection + yProjection + zProjection;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);//Функции должны быть объявнлены раньше чем вызваны!//текущщая точка высоты от 0 до 1
            float3 blendAxes = abs(IN.worldNormal);
            blendAxes /= blendAxes.x + blendAxes.y + blendAxes.z;
            float3 alb;
            for (int i = 0; i < layerCount; i++) {
                //float drawStrength = saturate(sign(heightPercent - baseStartHeights[i]));// sign - Возвращает - 1, если x меньше нуля; 0, если х равен нулю; и 1, если х больше нуля.//Если текущая точка меньше перебираемого цвета то 0, больше - 1
                //https://habr.com/ru/post/342906/
                float drawStrength = inverseLerp(-baseBlends[i]/2 - epsilon, baseBlends[i]/2, heightPercent - baseStartHeights[i]);//Расписать E16 15:30//смешивается высший элемент с низшим

                float3 baseColour = baseColours[i] * baseColourStrength[i];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[i], blendAxes, i) * (1 - baseColourStrength[i]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }

            float3 colorNoise = tex2D(_colourTextures, IN.uv_colourTextures).rgb; //IN.uv_colourTextures

            //Temperature+
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount] / 2, baseBlends[layerCount] / 2 + epsilon, baseStartHeights[layerCount] - colorNoise.x);

                float3 baseColour = baseColours[layerCount] * baseColourStrength[layerCount];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount], blendAxes, layerCount) * (1 - baseColourStrength[layerCount]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
            //Temperature-
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount + 1] / 2 - epsilon, baseBlends[layerCount + 1] / 2, colorNoise.x - baseStartHeights[layerCount + 1]);

                float3 baseColour = baseColours[layerCount + 1] * baseColourStrength[layerCount + 1];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 1], blendAxes, layerCount + 1) * (1 - baseColourStrength[layerCount + 1]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
            //Humidity+
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount + 2] / 2, baseBlends[layerCount + 2] / 2 + epsilon, baseStartHeights[layerCount + 2] - colorNoise.y);

                float3 baseColour = baseColours[layerCount + 2] * baseColourStrength[layerCount + 2];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 2], blendAxes, layerCount + 2) * (1 - baseColourStrength[layerCount + 2]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
            //Humidity-
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount + 3] / 2 - epsilon, baseBlends[layerCount + 3] / 2, colorNoise.y - baseStartHeights[layerCount + 3]);

                float3 baseColour = baseColours[layerCount + 3] * baseColourStrength[layerCount + 3];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 3], blendAxes, layerCount + 3) * (1 - baseColourStrength[layerCount + 3]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
            //Urbanization+
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount + 4] / 2, baseBlends[layerCount + 4] / 2 + epsilon, baseStartHeights[layerCount + 4] - colorNoise.z);

                float3 baseColour = baseColours[layerCount + 4] * baseColourStrength[layerCount + 4];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 4], blendAxes, layerCount + 4) * (1 - baseColourStrength[layerCount + 4]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }
            //Urbanization-
            {
                float drawStrength = inverseLerp(-baseBlends[layerCount + 5] / 2 - epsilon, baseBlends[layerCount + 5] / 2, colorNoise.z - baseStartHeights[layerCount + 5]);

                float3 baseColour = baseColours[layerCount + 5] * baseColourStrength[layerCount + 5];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 5], blendAxes, layerCount + 5) * (1 - baseColourStrength[layerCount + 5]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }

            {
                float drawStrength = inverseLerp(waterLevel + baseBlends[layerCount + 6] + epsilon, waterLevel - baseBlends[layerCount + 6], IN.worldPos.y);

                float3 baseColour = baseColours[layerCount + 6] * baseColourStrength[layerCount + 6];
                float3 textureColour = triplanar(IN.worldPos, baseTextureScales[layerCount + 6], blendAxes, layerCount + 6) * (1 - baseColourStrength[layerCount + 6]);

                alb = alb * (1 - drawStrength) + (baseColour + textureColour) * drawStrength;
            }

            o.Albedo = alb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
