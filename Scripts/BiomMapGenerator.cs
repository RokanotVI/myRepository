using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiomMapGenerator
{
    public static BiomMap GenerateBiomtMap(int sideSize, BiomSettings settings, Vector2 sampleCentre)
    {
        float[,] temperatureValues = Noise.GenerateNoiseMap(sideSize, settings.temperatureNoiseSettings, sampleCentre);//(sideSize-1) * 4
        float[,] humidityValues = Noise.GenerateNoiseMap(sideSize, settings.humidityNoiseSettings, sampleCentre);
        float[,] urbanizationValues = Noise.GenerateNoiseMap(sideSize, settings.urbanizationNoiseSettings, sampleCentre);

        int biomsWithMountain = 0;
        List<BiomIndex> uniqueBiomIndex = new List<BiomIndex>();
        BiomIndex[,] biomIndex = new BiomIndex[sideSize, sideSize];
        float[,] biomStrenght = new float[sideSize, sideSize];

        for (int i = 0; i < sideSize; i ++)
        {
            for (int j = 0; j < sideSize; j ++)
            {
                int indexT = 0, indexH = 0, indexU = 0;

                foreach(BiomSettings.Bioms.Temperature temperature in settings.bioms.temperature)
                {
                    if (temperatureValues[i, j] > temperature.strenght && settings.bioms.temperature.Length - 1 > indexT)
                    {
                        indexT++;
                        continue;
                    }
                    foreach (BiomSettings.Bioms.Temperature.Humidity humidity in temperature.humidity)
                    {
                        if (humidityValues[i, j] > humidity.strenght && temperature.humidity.Length - 1 > indexH)
                        {
                            indexH++;
                            continue;
                        }
                        foreach (BiomSettings.Bioms.Temperature.Humidity.Urbanization urbanization in humidity.urbanization)
                        {
                            if (urbanizationValues[i, j] > urbanization.strenght && humidity.urbanization.Length - 1 > indexU)
                            {
                                indexU++;
                                continue;
                            }
                            break;
                        }
                        break;
                    }
                    break;
                }
                /*if (indexU < 1)
                {
                    Debug.Log($"i,j - {i},{j}         indexT {indexT}          indexH {indexH}            indexU {indexU}");
                }*/

                {//делает границы более выраженными
                    float centreT = settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * 0.5f + settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureStart;
                    float centreH = settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * 0.5f + settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityStart;
                    float centreU = settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * 0.5f + settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationStart;
                    float normalizedT = temperatureValues[i, j] - (centreT);
                    float normalizedH = humidityValues[i, j] - (centreH);
                    float normalizedU = urbanizationValues[i, j] - (centreU);
                    float contrastT = (normalizedT * normalizedT * normalizedT * normalizedT * normalizedT * 16 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval) / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval);
                    float contrastH = (normalizedH * normalizedH * normalizedH * normalizedH * normalizedH * 16 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval) / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval);
                    float contrastU = (normalizedU * normalizedU * normalizedU * normalizedU * normalizedU * 16 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval) / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval);
                    temperatureValues[i, j] = contrastT + centreT;
                    humidityValues[i, j] = contrastH + centreH;
                    urbanizationValues[i, j] = contrastU + centreU;
                    //y=\frac{\left(16\cdot0.6\right)\left(x-0.5\right)^5}{0.6^5}+0.3
                }

                //if (i % settings.colourMapMultyplier == 0 && j % settings.colourMapMultyplier == 0)
                {
                    //int flattedI = i / settings.colourMapMultyplier;
                    //int flattedJ = j / settings.colourMapMultyplier;
                    biomIndex[i, j] = new BiomIndex(indexT, indexH, indexU);
                    //Debug.Log($"i,j {i/4},{j/4}     T:{indexT}    H:{indexH}   U:{indexU}");

                    //ФОРМУЛА y = (x * максразмер - x * x) * (4 / (максразмер * максразмер))
                    //где x текущая величина
                    float normalizedT = temperatureValues[i, j] - settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureStart;
                    float normalizedH = humidityValues[i, j] - settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityStart;
                    float normalizedU = urbanizationValues[i, j] - settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationStart;

                    biomStrenght[i, j] = Mathf.Clamp01((normalizedT * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval - normalizedT * normalizedT) * (4 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].biomeScalingImmunity / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].temperatureInterval)))
                        * Mathf.Clamp01((normalizedH * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval - normalizedH * normalizedH) * (4 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].biomeScalingImmunity / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].humidityInterval)))
                        * Mathf.Clamp01((normalizedU * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval - normalizedU * normalizedU) * (4 * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].biomeScalingImmunity / (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval * settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].urbanizationInterval)));

                    bool unique = true;
                    if (uniqueBiomIndex.Count != 0)
                    {
                        foreach (BiomIndex index in uniqueBiomIndex)
                        {
                            if (index.Equals(biomIndex[i, j]))
                            {
                                unique = false;
                            }
                        }
                    }
                    if (unique)
                    {
                        uniqueBiomIndex.Add(new BiomIndex(indexT, indexH, indexU));
                        if (settings.bioms.temperature[indexT].humidity[indexH].urbanization[indexU].hasMountains)
                        {
                            biomsWithMountain++;
                        }
                    }
                }
            }
        }

        Color[] colourMap = new Color[sideSize * sideSize];
        for (int y = 0; y < sideSize; y++)
        {
            for (int x = 0; x < sideSize; x++)
            {
                colourMap[y * sideSize + x] = new Color(temperatureValues[x, y], humidityValues[x, y], urbanizationValues[x, y]);
            }
        }

        return new BiomMap(biomIndex, uniqueBiomIndex, biomStrenght, colourMap, biomsWithMountain);
    }
}

public struct BiomMap
{
    public readonly BiomIndex[,] biomIndex;
    public readonly List<BiomIndex> uniqueBiomIndex;
    public readonly float[,] biomStrenght;
    //public readonly float[,] temperatureValues;
    //public readonly float[,] humidityValues;
    //public readonly float[,] urbanizationValues;
    public readonly Color[] colourMap;
    public readonly int biomsWithMountain;

    public BiomMap(BiomIndex[,] biomIndex, List<BiomIndex> uniqueBiomIndex, float[,] biomStrenght, Color[] colourMap, int biomsWithMountain)
    {
        this.biomIndex = biomIndex;
        this.uniqueBiomIndex = uniqueBiomIndex;
        //this.temperatureValues = temperatureValues;
        //this.humidityValues = humidityValues;
        //this.urbanizationValues = urbanizationValues;
        this.biomStrenght = biomStrenght;
        this.colourMap = colourMap;
        this.biomsWithMountain = biomsWithMountain;
    }
}

public struct BiomIndex
{
    public int temperatureIndex;
    public int humidityIndex;
    public int urbanizationIndex;
    public int relief;

    public BiomIndex(int t, int h, int u)
    {
        temperatureIndex = t;
        humidityIndex = h;
        urbanizationIndex = u;
        relief = 0;
    }

    public bool Equals(BiomIndex biomIndex)
    {
        return temperatureIndex == biomIndex.temperatureIndex && humidityIndex == biomIndex.humidityIndex && urbanizationIndex == biomIndex.urbanizationIndex;
    }
}
