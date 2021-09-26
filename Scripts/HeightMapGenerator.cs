using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HeightMapGenerator
{
    public static HeightMap GenerateHeightMap(int sideSize, BiomIndex[,] biomIndex, float[,] biomStrenght, BiomSettings biomsSettings, Vector2 sampleCentre)
    {
        float[,] values = new float[sideSize, sideSize];//Noise.GenerateNoiseMap(sideSize, biomsSettings.landmassNoiseSettings, sampleCentre);
        //float[,] mountains = Noise.GenerateNoiseMap(sideSize, biomsSettings.mountainsNoiseSettings, sampleCentre);
        float[,] rivers = Noise.GenerateNoiseMap(sideSize, biomsSettings.riversNoiseSettings, sampleCentre);

        //ФОРМУЛА y = (x * максразмер - x * x) * (4 / максразмер)
        //AnimationCurve heightCurve_threadsafe = new AnimationCurve(settings.heightCurve.keys);//для потока

        //float minValue = float.MaxValue;
        //float maxValue = float.MinValue;

        for (int i = 0; i < sideSize; i++)
        {
            for (int j = 0; j < sideSize; j++)
            {
                values[i, j] = Noise.GenerateNoise(sideSize, biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].landmassNoiseSettings, sampleCentre, i, j);
                //biomMap.biomIndex[j, i].temperatureIndex;
                AnimationCurve heightCurve_threadsafe = biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].heightCurve;
                values[i, j] *= heightCurve_threadsafe.Evaluate(values[i, j]) * biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].heightMultiplier;
                values[i, j] *= biomStrenght[i, j];

                if (biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].hasLakes)
                {
                    float lake = Noise.GenerateNoise(sideSize, biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].lakesNoiseSettings, sampleCentre, i, j);
                    if (lake > biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].lakeThreshold)
                    {
                        float newRelief = (lake - biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].lakeThreshold)
                            / (1f - biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].lakeThreshold);
                        newRelief *= biomStrenght[i, j];
                        newRelief *= biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].lakeMultiplier;
                        values[i, j] -= newRelief;
                    }
                }

                {
                    float displacedRelief = rivers[i, j] - 0.5f * (1f - biomsSettings.riverWidth);
                    float newRelief = -biomsSettings.riverDepth * Mathf.Clamp01(displacedRelief * biomsSettings.riverWidth - displacedRelief * displacedRelief);
                    values[i, j] += newRelief;
                }

                if (biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].hasMountains)
                {
                    float mountain = Noise.GenerateNoise(sideSize, biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainsNoiseSettings, sampleCentre, i, j);
                    if (mountain > biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainThreshold)
                    {
                        float newRelief = (mountain - biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainThreshold)
                            / (1f - biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainThreshold);
                        newRelief *= biomStrenght[i, j];
                        AnimationCurve mountainCurve_threadsafe = biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainCurve;
                        newRelief *= mountainCurve_threadsafe.Evaluate(newRelief) * biomsSettings.bioms.temperature[biomIndex[i, j].temperatureIndex].humidity[biomIndex[i, j].humidityIndex].urbanization[biomIndex[i, j].urbanizationIndex].mountainMultiplier;
                        values[i, j] += newRelief;
                    }
                }
                /*if (values[i, j] > maxValue)
                {
                    maxValue = values[i, j];
                }
                if (values[i, j] < minValue)
                {
                    minValue = values[i, j];
                }*/
            }
        }

        return new HeightMap(values/*, minValue, maxValue*/);
    }
}

public struct HeightMap
{
    public readonly float[,] values;
    /*public readonly float minValue;
    public readonly float maxValue;*/

    public HeightMap(float[,] values/*, float minValue, float maxValue*/)
    {
        this.values = values;
        /*this.minValue = minValue;
        this.maxValue = maxValue;*/
    }
}



/*List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height];
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();
            tiles.Add(tile);

            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y==tile.tileY||x==tile.tileX))
                    {
                        if(mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;*/
