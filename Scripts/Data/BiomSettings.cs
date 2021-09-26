using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Linq;

[CreateAssetMenu()]
public class BiomSettings : UpdatableData
{
    const int textureSize = 512;
    const TextureFormat textureFormat = TextureFormat.RGB565;

    //public Layer[] layers;
    public NoiseSettings temperatureNoiseSettings;
    public NoiseSettings humidityNoiseSettings;
    public NoiseSettings urbanizationNoiseSettings;
    //public NoiseSettings landmassNoiseSettings;
    //public NoiseSettings mountainsNoiseSettings;
    public NoiseSettings riversNoiseSettings;
    public float riverWidth = 0.2f;
    public float riverDepth = 0.2f;

    public Texture2D waterTexture;
    public Color waterTint;
    [Range(0, 1)]
    public float waterTintStrenght;
    public float waterLevel = -1f;
    public float waterBlendInterval;
    public float waterTextureScale;

    public Bioms bioms;

    float savedMinHeight;
    float savedMsxHeight;

    public void Preparation()
    {
        temperatureNoiseSettings.LoadSeedOffset();
        humidityNoiseSettings.LoadSeedOffset();
        urbanizationNoiseSettings.LoadSeedOffset();
        //landmassNoiseSettings.LoadSeedOffset();
        //mountainsNoiseSettings.LoadSeedOffset();
        riversNoiseSettings.LoadSeedOffset();
    }

    public void ApplyToMaterial(Material material, Layer[] layers, Layer[] biomBlend, Bioms.Temperature.Humidity.Urbanization biom )
    {
        material.SetInt("layerCount", layers.Length);
        material.SetFloat("waterLevel", waterLevel);

        int lenght = layers.Length + biomBlend.Length + 1;

        Color[] colorArray = new Color[lenght];
        float[] tintStrenghtArray = new float[lenght];
        float[] startHeightArray = new float[lenght - 1];
        float[] blendStrengthArray = new float[lenght];
        float[] textureScaleArray = new float[lenght];
        Texture2D[] texturesArray = new Texture2D[lenght];

        for (int i = 0; i < layers.Length; i++)
        {
            colorArray[i] = layers[i].tint;
            tintStrenghtArray[i] = layers[i].tintStrenght;
            startHeightArray[i] = layers[i].startHeight;
            blendStrengthArray[i] = layers[i].blendStrength;
            textureScaleArray[i] = layers[i].textureScale;
            texturesArray[i] = layers[i].texture;
        }

        for (int i = 0; i < biomBlend.Length; i++)
        {
            colorArray[i + layers.Length] = biomBlend[i].tint;
            tintStrenghtArray[i + layers.Length] = biomBlend[i].tintStrenght;
            startHeightArray[i + layers.Length] = biomBlend[i].startHeight;
            blendStrengthArray[i + layers.Length] = biomBlend[i].blendStrength;
            textureScaleArray[i + layers.Length] = biomBlend[i].textureScale;
            texturesArray[i + layers.Length] = biomBlend[i].texture;
        }

        colorArray[lenght - 1] = waterTint;
        tintStrenghtArray[lenght - 1] = waterTintStrenght;
        blendStrengthArray[lenght - 1] = waterBlendInterval;
        textureScaleArray[lenght - 1] = waterTextureScale;
        texturesArray[lenght - 1] = waterTexture;

        biom.shaderProperties.colorArray = colorArray;//layers.Concat(biomBlend).Select(x => x.tint).ToArray();
        biom.shaderProperties.startHeightArray = startHeightArray;// layers.Concat(biomBlend).Select(x => x.startHeight).ToArray();
        biom.shaderProperties.blendStrengthArray = blendStrengthArray;// layers.Concat(biomBlend).Select(x => x.blendStrength).ToArray();
        biom.shaderProperties.tintStrenghtArray = tintStrenghtArray;//layers.Concat(biomBlend).Select(x => x.tintStrenght).ToArray();
        biom.shaderProperties.textureScaleArray = textureScaleArray;// layers.Concat(biomBlend).Select(x => x.textureScale).ToArray();
        Texture2DArray textures2DArray = GenerateTextureArray(texturesArray);//GenerateTextureArray(layers.Concat(biomBlend).Select(x => x.texture).ToArray());
        material.SetTexture("baseTextures", textures2DArray);

        //material.SetColorArray("baseColours", layers.Select(x => x.tint).ToArray());
        //material.SetFloatArray("baseStartHeights", layers.Select(x => x.startHeight).ToArray());
        //material.SetFloatArray("baseBlends", layers.Select(x => x.blendStrength).ToArray());
        //material.SetFloatArray("baseColourStrength", layers.Select(x => x.tintStrenght).ToArray());
        //material.SetFloatArray("baseTextureScales", layers.Select(x => x.textureScale).ToArray());
        //Texture2DArray texturesArray = GenerateTextureArray(layers.Select(x => x.texture).ToArray());
        //material.SetTexture("baseTextures", texturesArray);

        //UpdateMeshHeight(material, savedMinHeight, savedMsxHeight);
    }

    Texture2DArray GenerateTextureArray(Texture2D[] textures)
    {
        Texture2DArray textureArray = new Texture2DArray(textureSize, textureSize, textures.Length, textureFormat, true);
        for (int i = 0; i < textures.Length; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }
        textureArray.Apply();
        return textureArray;
    }

    public void ApplyToMaterialTEST(Material material, Layer[] layers, Layer[] biomBlend, Texture2D colourTexture, ShaderProp prop)//отправлять данные о шуме
    {
        //material.SetInt("layerCount", layers.Length);

        //material.SetColorArray("baseColours", layers.Concat(biomBlend).Select(x => x.tint).ToArray());
        //material.SetFloatArray("baseStartHeights", layers.Concat(biomBlend).Select(x => x.startHeight).ToArray());
        //material.SetFloatArray("baseBlends", layers.Concat(biomBlend).Select(x => x.blendStrength).ToArray());
        //material.SetFloatArray("baseColourStrength", layers.Concat(biomBlend).Select(x => x.tintStrenght).ToArray());
        //material.SetFloatArray("baseTextureScales", layers.Concat(biomBlend).Select(x => x.textureScale).ToArray());
        //Texture2DArray texturesArray = GenerateTextureArray(layers.Concat(biomBlend).Select(x => x.texture).ToArray());
        //material.SetTexture("baseTextures", texturesArray);

        material.SetColorArray("baseColours", prop.colorArray);
        material.SetFloatArray("baseStartHeights", prop.startHeightArray);
        material.SetFloatArray("baseBlends", prop.blendStrengthArray);
        material.SetFloatArray("baseColourStrength", prop.tintStrenghtArray);
        material.SetFloatArray("baseTextureScales", prop.textureScaleArray);

        material.SetTexture("_colourTextures", colourTexture);

        //UpdateMeshHeight(material, savedMinHeight, savedMsxHeight);
    }

    public void UpdateMeshHeight(Material material, float minHeight, float maxHeight)
    {
        savedMinHeight = minHeight;
        savedMsxHeight = maxHeight;

        material.SetFloat("minHeight", minHeight);
        material.SetFloat("maxHeight", maxHeight);
    }

    public void UpdateBiomBoadrs(Bioms.Temperature.Humidity.Urbanization biom, float minT, float maxT, float minH, float maxH, float minU)
    {
        biom.temperatureStart = minT;
        biom.temperatureInterval = maxT - minT;
        biom.humidityStart = minH;
        biom.humidityInterval = maxH - minH;
        biom.urbanizationStart = minU;
        biom.urbanizationInterval = biom.strenght - minU;
    }

    public void UpdeteSeedOffset(Bioms.Temperature.Humidity.Urbanization biom)
    {
        biom.mountainsNoiseSettings.LoadSeedOffset();
        biom.lakesNoiseSettings.LoadSeedOffset();
        biom.landmassNoiseSettings.LoadSeedOffset();
    }

    [System.Serializable]
    public class Bioms
    {
        //ноиз сеттинг
        public Temperature[] temperature;
        [System.Serializable]
        public class Temperature
        {
#if UNITY_EDITOR
            public string Name;
#endif
            [Range(0, 1)]
            public float strenght;
            public Humidity[] humidity;
            [System.Serializable]
            public class Humidity
            {
#if UNITY_EDITOR
                public string Name;
#endif
                [Range(0, 1)]
                public float strenght;
                public Urbanization[] urbanization;
                [System.Serializable]
                public class Urbanization
                {
#if UNITY_EDITOR
                    public string Name;
#endif
                    [Range(0, 1)]
                    public float strenght;

                    public NoiseSettings landmassNoiseSettings;
                    public float heightMultiplier;
                    public AnimationCurve heightCurve;

                    public Material material;
                    public Layer[] layers;
                    public Layer[] biomlayers;

                    public bool hasMountains;
                    public NoiseSettings mountainsNoiseSettings;
                    public float mountainMultiplier = 50f;
                    public AnimationCurve mountainCurve;
                    [Range(0, 1)]
                    public float mountainThreshold = 0.5f;
                    public float biomeScalingImmunity = 1;

                    public Material mountainMaterial;
                    public Layer[] mountainLayers;

                    public bool hasLakes;
                    public NoiseSettings lakesNoiseSettings;
                    public float lakeMultiplier = 50f;
                    [Range(0, 1)]
                    public float lakeThreshold = 0.5f;

                    [HideInInspector]
                    public float temperatureStart, temperatureInterval, humidityStart, humidityInterval, urbanizationStart, urbanizationInterval;
                    [HideInInspector]
                    public ShaderProp shaderProperties;

                    public float minHeight
                    {
                        get
                        {
                            return heightMultiplier * heightCurve.Evaluate(0);
                        }
                    }

                    public float maxHeight
                    {
                        get
                        {
                            return heightMultiplier * heightCurve.Evaluate(1);
                        }
                    }

                    public float mountainMinHeight
                    {
                        get
                        {
                            return mountainMultiplier * mountainCurve.Evaluate(0) + maxHeight;
                        }
                    }

                    public float mountainMaxHeight
                    {
                        get
                        {
                            return mountainMultiplier * mountainCurve.Evaluate(1) + maxHeight;
                        }
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class Layer
    {
        public Texture2D texture;
        public Color tint;
        [Range(0, 1)]
        public float tintStrenght;
        [Range(0, 1)]
        public float startHeight;
        [Range(0, 1)]
        public float blendStrength;
        public float textureScale;
    }

    [System.Serializable]
    public class ShaderProp
    {
        public Color[] colorArray;
        public float[] tintStrenghtArray;
        public float[] startHeightArray;
        public float[] blendStrengthArray;
        public float[] textureScaleArray;
    }
}
