using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public float visibleDstThreshold;//+

    public MeshSettings meshSettings;
    //public HeightMapSettings heightMapSettings;
    public TextureData textureSettings;
    public BiomSettings biomSettings; //+

    public Transform viewer;
    public Material mapMaterial;

    Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    float meshWorldSize;
    int chunksVisibleInViewDst;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();//Словарь ключ-значение;
    List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private void Awake()
    {
        //textureSettings.ApplyToMaterial(mapMaterial);
        //textureSettings.UpdateMeshHeight(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);//отправляем данные материалу
        biomSettings.Preparation();
        float temperatureMin = 0, temperatureMax;
        foreach (BiomSettings.Bioms.Temperature temperature in biomSettings.bioms.temperature)
        {
            float humidityMin = 0, humidityMax;
            temperatureMax = temperature.strenght;
            foreach (BiomSettings.Bioms.Temperature.Humidity humidity in temperature.humidity)
            {
                float urbanizationMin = 0;
                humidityMax = humidity.strenght;
                foreach (BiomSettings.Bioms.Temperature.Humidity.Urbanization urbanization in humidity.urbanization)
                {
                    biomSettings.ApplyToMaterial(urbanization.material, urbanization.layers, urbanization.biomlayers, urbanization);
                    biomSettings.UpdateMeshHeight(urbanization.material, urbanization.minHeight, urbanization.maxHeight);
                    biomSettings.UpdateBiomBoadrs(urbanization, temperatureMin, temperatureMax, humidityMin, humidityMax, urbanizationMin);
                    biomSettings.UpdeteSeedOffset(urbanization);
                    urbanizationMin = urbanization.strenght;
                }
                humidityMin = humidityMax;
            }
            temperatureMin = temperatureMax;
        }
    }

    void Start()
    {
        //создать цикл для материалов
        //textureSettings.ApplyToMaterial(mapMaterial);
        //textureSettings.UpdateMeshHeight(mapMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);//отправляем данные материалу

        /*foreach (BiomSettings.Bioms.Temperature temperature in biomSettings.bioms.temperature)
        {
            foreach (BiomSettings.Bioms.Temperature.Humidity humidity in temperature.humidity)
            {
                foreach (BiomSettings.Bioms.Temperature.Humidity.Urbanization urbanization in humidity.urbanization)
                {
                    biomSettings.ApplyToMaterial(urbanization.material, urbanization.layers);
                    biomSettings.UpdateMeshHeight(urbanization.material, urbanization.minHeight, urbanization.maxHeight);
                }
            }
        }//переместить в Awake?*/

        

        //float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        //chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);
        chunksVisibleInViewDst = Mathf.RoundToInt(visibleDstThreshold / meshWorldSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPosition != viewerPositionOld)
        {
            foreach(TerrainChunk chunk in visibleTerrainChunks)
            {
                chunk.UpdateCollisionMesh();
            }
        }

        if ((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
    }

    void UpdateVisibleChunks()
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)//перебираем все куски террейна(записаны только видимые)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)//Цикл для определения координат клеток вокруг нашего персонажа
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (!alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    if (terrainChunkDictionary.ContainsKey(viewedChunkCoord)) //проверяем записана ли у нас данная координата на терейне. Если да то показываем её, если нет то создаем новую.
                    {
                        terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    }
                    else
                    {
                        //Debug.Log(viewedChunkCoord);
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, biomSettings, meshSettings, visibleDstThreshold, transform, viewer, mapMaterial);
                        terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                        newChunk.onVisibilityChanged += OnTerrainChunkVisibilityChanged;
                        newChunk.Load();
                    }
                }
            }
        }
    }

    void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}
