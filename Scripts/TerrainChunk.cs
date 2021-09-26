using UnityEngine;
using System.Collections.Generic;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 5;
    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    GameObject[] meshObject;
    GameObject chunk;
    Vector2 sampleCentre;
    Bounds bounds;
    Material material;

    MeshRenderer[] meshRenderer;
    MeshFilter[] meshFilter;
    //MeshCollider[] meshCollider;
    MeshCollider meshCollider;

    //LODInfo[] detailLevels;
    //LODMesh[] lodMeshes;
    LODMesh[] NElodMeshes;
    LODMesh colliderMesh;
    //int colliderLODIndex;

    BiomMap biomMap;
    int biomCount;
    //bool biomMapReceived;

    HeightMap heightMap;
    bool heightMapReceived;
    float minHeight;
    float maxHeight;

    //int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDst;
    float sqrVisibleDstThreshold;

    BiomSettings biomSettings;
    MeshSettings meshSettings;
    Transform viewer;
    //сделать колизию в обьекте террейн чанк общую
    //получать информацию о биомах
    public TerrainChunk(Vector2 coord, BiomSettings biomSettings, MeshSettings meshSettings, float visibleDstThreshold, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.biomSettings = biomSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;
        this.material = material;
        //this.parent = parent;

        sampleCentre = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);
        //Debug.Log(meshSettings.meshWorldSize);
        chunk = new GameObject("Terrain Chunk");
        /*meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();*/
        meshCollider = chunk.AddComponent<MeshCollider>();
        //meshRenderer.material = material;

        chunk.transform.position = new Vector3(position.x, 0, position.y);
        chunk.transform.parent = parent;
        SetVisible(false);

        colliderMesh = new LODMesh();
        colliderMesh.updateCallback += UpdateTerrainChunk;
        colliderMesh.updateCallback += UpdateCollisionMesh;
        /*lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }*/

        maxViewDst = visibleDstThreshold;
        sqrVisibleDstThreshold = visibleDstThreshold * visibleDstThreshold;
        //BiomTexture.GenerateBiomTexture(sideSize, temperatureValues, humidityValues, urbanizationValues);
    }

    //начало
    public void Load()
    {
        //RequestHeightMapReceived(BiomMapGenerator.GenerateBiomtMap(meshSettings.numVertsPerLine, biomSettings, sampleCentre, offset));
        ThreadedDataRequester.RequestData(() => BiomMapGenerator.GenerateBiomtMap(meshSettings.numVertsPerLine, biomSettings, sampleCentre), RequestHeightMapReceived);
    }

    void RequestHeightMapReceived(object biomMapObject)
    {
        this.biomMap = (BiomMap)biomMapObject;
        //biomMapReceived = true;
        biomCount = biomMap.uniqueBiomIndex.Count + biomMap.biomsWithMountain;
        /*if (biomCount == 1)//горы и реки
        {
            minHeight = biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[0].temperatureIndex - 1].humidity[biomMap.uniqueBiomIndex[0].humidityIndex - 1].urbanization[biomMap.uniqueBiomIndex[0].humidityIndex - 1].minHeight;
            maxHeight = biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[0].temperatureIndex - 1].humidity[biomMap.uniqueBiomIndex[0].humidityIndex - 1].urbanization[biomMap.uniqueBiomIndex[0].humidityIndex - 1].maxHeight;
            //Сделать шум для гор и рек если они есть в данном биоме и изменить карту высот по резульатам
            //Для этого нужно - Старый нойзСеттингс, сайдсайз, самплЦентр
            //прибавить к biomCount горы и реки
        }*/
        initializeMeshObjects();
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, biomMap.biomIndex, biomMap.biomStrenght, biomSettings, sampleCentre), OnHeightMapReceived);
    }

    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    public void initializeMeshObjects()
    {
        meshObject = new GameObject[biomCount];
        meshRenderer = new MeshRenderer[biomCount];
        meshFilter = new MeshFilter[biomCount];
        //meshCollider = new MeshCollider[biomCount];
        NElodMeshes = new LODMesh[biomCount];
        for (int i = 0, j = biomMap.uniqueBiomIndex.Count; i < biomMap.uniqueBiomIndex.Count; i++)
        {
            meshObject[i] = new GameObject("fragment " + i);
            meshRenderer[i] = meshObject[i].AddComponent<MeshRenderer>();
            meshFilter[i] = meshObject[i].AddComponent<MeshFilter>();
            //meshCollider[i] = meshObject[i].AddComponent<MeshCollider>();
            //Создать усливие если больше макс высоты, если меньше минимальной высоты, если норм
            //meshRenderer[i].material = biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex - 1].humidity[biomMap.uniqueBiomIndex[i].humidityIndex - 1].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex - 1].material;

            Texture2D texture = new Texture2D(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine);
            texture.SetPixels(biomMap.colourMap);
            texture.Apply();

            Material nlMaterial = new Material(biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].material);
            //nMaterial.SetTexture("_MainTex", biomMap.texture);
            biomSettings.ApplyToMaterialTEST(nlMaterial, biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].layers, biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].biomlayers, texture, biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].shaderProperties);
            //biomSettings.UpdateMeshHeight(nMaterial, heightMap.minValue, heightMap.maxValue);
            meshRenderer[i].material = nlMaterial;
            Vector2 position = coord * meshSettings.meshWorldSize;
            meshObject[i].transform.position = new Vector3(position.x, 0, position.y);
            meshObject[i].transform.parent = chunk.transform;

            NElodMeshes[i] = new LODMesh();
            NElodMeshes[i].updateCallback += UpdateTerrainChunk;

            if (biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].hasMountains)
            {
                meshObject[j] = new GameObject("mountain " + j);
                meshRenderer[j] = meshObject[j].AddComponent<MeshRenderer>();
                meshFilter[j] = meshObject[j].AddComponent<MeshFilter>();
                //biomSettings.ApplyToMaterialTEST
                Material nmMaterial = new Material(biomSettings.bioms.temperature[biomMap.uniqueBiomIndex[i].temperatureIndex].humidity[biomMap.uniqueBiomIndex[i].humidityIndex].urbanization[biomMap.uniqueBiomIndex[i].humidityIndex].mountainMaterial);
                meshRenderer[j].material = nmMaterial;
                meshObject[j].transform.position = new Vector3(position.x, 0, position.y);
                meshObject[j].transform.parent = chunk.transform;

                NElodMeshes[j] = new LODMesh();
                NElodMeshes[j].updateCallback += UpdateTerrainChunk;
                j++;
            }
            //NElodMeshes[i].updateCallback += UpdateCollisionMesh;
        }
    }

    public void UpdateTerrainChunk()
    {
        if (heightMapReceived)
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));//дистанция от границы до игрока (без учета высоты)
            bool wasVisible = IsVisible();
            bool visible = viewerDstFromNearestEdge <= maxViewDst;

            if (!colliderMesh.hasRequestedMesh)
            {
                colliderMesh.RequestMesh(heightMap, meshSettings, biomMap.biomIndex);
            }

            if (visible)
            {
                for (int i = 0; i < biomCount - biomMap.biomsWithMountain; i++)
                {
                    LODMesh NElodMesh = NElodMeshes[i];
                    if (NElodMesh.hasMesh)
                    {
                        //previousLODIndex = lodIndex;
                        meshFilter[i].mesh = NElodMesh.mesh;
                    }
                    else if (!NElodMesh.hasRequestedMesh)
                    {
                        NElodMesh.RequestMesh(heightMap, meshSettings, biomMap.biomIndex, biomMap.uniqueBiomIndex[i]);
                    }
                }


            }

            if (wasVisible != visible)
            {
                SetVisible(visible);
                if (onVisibilityChanged != null)
                {
                    onVisibilityChanged(this, visible);
                }
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

            if (sqrDstFromViewerToEdge < sqrVisibleDstThreshold)
            {
                if (!colliderMesh.hasRequestedMesh)
                {
                    colliderMesh.RequestMesh(heightMap, meshSettings, biomMap.biomIndex);//ИСПРАВИТЬ!!!
                }
                /*for (int i = 0; i < biomCount; i++)
                {
                    if (!NElodMeshes[i].hasRequestedMesh)
                    {
                        NElodMeshes[i].RequestMesh(heightMap, meshSettings, biomMap.biomIndex, biomMap.uniqueBiomIndex[i]);
                    }
                }*/
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold)
            {
                if (colliderMesh.hasMesh)
                {
                    meshCollider.sharedMesh = colliderMesh.mesh;
                    hasSetCollider = true;
                }
                /*for (int i = 0; i < biomCount; i++)
                {
                    if (NElodMeshes[i].hasMesh)
                    {
                        meshCollider[i].sharedMesh = NElodMeshes[i].mesh;
                        hasSetCollider = true;
                    }
                }*/
            }
        }
    }

    public void SetVisible(bool visible)
    {
        chunk.SetActive(visible);
    }

    public bool IsVisible()
    {
        return chunk.activeSelf;
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        public event System.Action updateCallback;
        /*int lod;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }*/

        void OnMeshDataReceived(object meshDataObject)
        {
            mesh = ((DevidedMeshData)meshDataObject).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        void OnnnMeshDataReceived(object meshDataObject)
        {
            mesh = ((MeshData)meshDataObject).CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings, BiomIndex[,] biomMap, BiomIndex biomIndex)
        {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => DividedMeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, biomMap, biomIndex), OnMeshDataReceived);
        }
        public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings, BiomIndex[,] biomMap)
        {
            hasRequestedMesh = true;
            ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings), OnnnMeshDataReceived);
        }
    }
}