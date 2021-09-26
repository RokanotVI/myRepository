using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int sideSize, NoiseSettings settings, Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[sideSize, sideSize];
        /*System.Random prng = new System.Random(settings.seed);
        Vector2[] octaveOffsets = new Vector2[settings.octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;
        float frequensy = 1;

        for (int i = 0; i < settings.octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + settings.offset.x + sampleCentre.x;
            float offsetY = prng.Next(-100000, 100000) - settings.offset.y - sampleCentre.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= settings.persistance;
        }*/
        
        /*float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHight = float.MaxValue;*/

        float halfSize = sideSize / 2f;

        for(int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                float amplitude = 1;
                float frequensy = 1;
                float noiseHight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (x - halfSize + settings.octaveOffsets[i].x + sampleCentre.x) / settings.scale * frequensy;
                    float sampleY = (y - halfSize + settings.octaveOffsets[i].y - sampleCentre.y) / settings.scale * frequensy;
                    
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//от - 1 до 1
                    noiseHight += perlinValue * amplitude;//noiseMap[x, y] = perlinValue;
                    amplitude *= settings.persistance;
                    frequensy *= settings.lacunarity;
                }

                /*//узнаем минимальную и максимальную высоту нашей карты
                if (noiseHight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHight;
                }
                if (noiseHight < minLocalNoiseHight)
                {
                    minLocalNoiseHight = noiseHight;
                }*/

               
                noiseMap[x, y] = noiseHight;

                float normalizedHeight = (noiseMap[x, y] + 1) / (settings.maxPossibleHeight * 2f);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        return noiseMap;
    }

    public static float GenerateNoise(int sideSize, NoiseSettings settings, Vector2 sampleCentre, int x, int y)
    {
        float halfSize = sideSize / 2f;

        float amplitude = 1;
        float frequensy = 1;
        float noiseHight = 0;

        for (int i = 0; i < settings.octaves; i++)
        {
            float sampleX = (x - halfSize + settings.octaveOffsets[i].x + sampleCentre.x) / settings.scale * frequensy;
            float sampleY = (y - halfSize + settings.octaveOffsets[i].y - sampleCentre.y) / settings.scale * frequensy;

            float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//от - 1 до 1
            noiseHight += perlinValue * amplitude;//noiseMap[x, y] = perlinValue;
            amplitude *= settings.persistance;
            frequensy *= settings.lacunarity;
        }

        float normalizedHeight = (noiseHight + 1) / (settings.maxPossibleHeight * 2f);
        noiseHight = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);

        return noiseHight;
    }

    public static float[,] GenerateDetailedNoiseMap(int sideSize, int detailedSideSize, NoiseSettings settings, Vector2 sampleCentre)
    {
        float[,] noiseMap = new float[detailedSideSize, detailedSideSize];

        float halfSize = sideSize / 2f;

        for (int y = 0; y < detailedSideSize; y++)
        {
            for (int x = 0; x < detailedSideSize; x++)
            {
                float amplitude = 1;
                float frequensy = 1;
                float noiseHight = 0;

                for (int i = 0; i < settings.octaves; i++)
                {
                    float sampleX = (((float)x / (float)detailedSideSize * (float)sideSize) - halfSize + settings.octaveOffsets[i].x + sampleCentre.x) / settings.scale * frequensy;
                    //Debug.Log($"x:{x}   new:{(float)x / (float)detailedSideSize * (float)sideSize}");
                    float sampleY = (((float)y / (float)detailedSideSize * (float)sideSize) - halfSize + settings.octaveOffsets[i].y - sampleCentre.y) / settings.scale * frequensy;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;//от - 1 до 1
                    noiseHight += perlinValue * amplitude;//noiseMap[x, y] = perlinValue;
                    amplitude *= settings.persistance;
                    frequensy *= settings.lacunarity;
                }

                noiseMap[x, y] = noiseHight;

                float normalizedHeight = (noiseMap[x, y] + 1) / (settings.maxPossibleHeight * 2f);
                noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
            }
        }

        return noiseMap;
    }
}

[System.Serializable]
public class NoiseSettings
{
    public float scale = 50;

    public int octaves = 6;
    [Range(0, 1)]
    public float persistance = 0.6f;
    public float lacunarity = 2;

    public int seed;
    public Vector2 offset;

    [HideInInspector]
    public Vector2[] octaveOffsets;
    [HideInInspector]
    public float maxPossibleHeight;

    public void LoadSeedOffset()
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0;
        float amplitude = 1;

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= persistance;
        }

        this.octaveOffsets = octaveOffsets;
        this.maxPossibleHeight = maxPossibleHeight;
    }

    public void ValidateValues()
    {
        scale = Mathf.Max(scale, 0.01f);
        octaves = Mathf.Max(octaves, 1);
        lacunarity = Mathf.Max(lacunarity, 1);
        persistance = Mathf.Clamp01(persistance);
    }
}
