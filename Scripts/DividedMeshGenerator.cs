using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DividedMeshGenerator : MonoBehaviour
{
    public static DevidedMeshData GenerateTerrainMesh(float[,] heightMap, MeshSettings meshSettings, BiomIndex[,] biomMap, BiomIndex biomIndex)
    {
        //int levelOfDetail = 1;
        //int scipIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        int numVertsPerLine = meshSettings.numVertsPerLine;//размер Chunk'а + необходимые границы для нормалей

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        //DevidedMeshData meshData = new DevidedMeshData(numVertsPerLine/*, scipIncrement*/);

        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshVertexIndex = 0;//было 0
        int outOfMeshVertexIndex = -1;

        int trianglesCount = 0;
        int outOfMeshTrianglesCount = 0;
        //Debug.Log("Текущий биом - t: " + biomIndex.temperatureIndex + "; h: " + +biomIndex.humidityIndex + "; u: " + biomIndex.urbanizationIndex);
        int[,] mapVerticesFlags = new int[numVertsPerLine, numVertsPerLine];
        int[,] mapTrianglesFlags = new int[numVertsPerLine, numVertsPerLine];
        for (int y = 0; y < numVertsPerLine - 1; y++)
        {
            for (int x = 0; x < numVertsPerLine - 1; x++)
            {
                bool isOutOfMeshVertexBorder = y == 0 || y >= numVertsPerLine - 2 || x == 0 || x >= numVertsPerLine - 2;//true если на границе(т.е. за ней)
                //Debug.Log("клетка биом - t: " + biomMap[x, y].temperatureIndex + "; h: " + +biomMap[x, y].humidityIndex + "; u: " + biomMap[x, y].urbanizationIndex);
                if (!biomIndex.Equals(biomMap[x, y]) || isOutOfMeshVertexBorder)//если на границе или не соответствует текущему биому
                {
                    for (int i = y - 1; i <= y + 1; i++)
                    {
                        for (int j = x - 1; j <= x + 1; j++)
                        {
                            if (j < 0 || i < 0)
                                continue;
                            if (biomIndex.Equals(biomMap[j, i]))//помечает что точка пригранична к нашему биому но не входит в него. Отмечаем что оно подходит для нормалей.
                            {
                                mapTrianglesFlags[x, y] = 1;
                                outOfMeshTrianglesCount += 6;
                                goto exit1;//выводим из цикла, т.к. уже знаем для чего нам эта точка.
                            }
                        }
                    }
                }
                else//точка полностью соответствует данному биому и подходит для меша.
                {
                    mapTrianglesFlags[x, y] = 2;
                    trianglesCount += 6;
                }

            exit1://отмечаем заданную точку, и точки справа, снизу, справа-снизу теми же пометками. 2 - точки подходящие для меша. 1 - точки для нормалей.
                if (mapTrianglesFlags[x, y] == 1)
                {
                    for (int i = y; i <= y + 1; i++)
                    {
                        for (int j = x; j <= x + 1; j++)
                        {
                            if (mapVerticesFlags[j, i] != 2)
                            {
                                mapVerticesFlags[j, i] = 1;
                            }
                        }
                    }
                }
                else if (mapTrianglesFlags[x, y] == 2)
                {
                    for (int i = y; i <= y + 1; i++)
                    {
                        for (int j = x; j <= x + 1; j++)
                        {
                            mapVerticesFlags[j, i] = 2;
                        }
                    }
                }
            }
        }

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                if (mapVerticesFlags[x, y] == 1)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;//помечаем граничащие точки отрицательными числами
                    outOfMeshVertexIndex--;
                }
                else if (mapVerticesFlags[x, y] == 2)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;//нумеруем прорисовываемые точки
                    meshVertexIndex++;
                }
            }
        }

        DevidedMeshData meshData = new DevidedMeshData(numVertsPerLine/*, scipIncrement*/, meshVertexIndex, trianglesCount, -outOfMeshVertexIndex - 1, outOfMeshTrianglesCount);

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                //bool isSkippedVertex = x > 2 && x < numVertsPerLine - 3 && y > 2 && y < numVertsPerLine - 3 && ((x - 2) % scipIncrement != 0 || (y - 2) % scipIncrement != 0);//тот же длиный if

                //if (!isSkippedVertex)
                {
                    //bool isOutOfMeshVertex = y == 0 || y == numVertsPerLine - 1 || x == 0 || x == numVertsPerLine - 1;//true - граница необходимая для нормалей
                    //bool isMeshEdgeVertex = (y == 1 || y == numVertsPerLine - 2 || x == 1 || x == numVertsPerLine - 2) && !isOutOfMeshVertex;//true - 2 граница вершин меша
                    //bool isMainVertex = (x - 2) % scipIncrement == 0 && (y - 2) % scipIncrement == 0 && !isOutOfMeshVertex && !isMeshEdgeVertex;//вершины в центре участвующие в генерации меше(детализация)
                    //bool isEdgeConnectionVertex = (y == 2 || y == numVertsPerLine - 3 || x == 2 || x == numVertsPerLine - 3) && !isOutOfMeshVertex && !isMeshEdgeVertex && !isMainVertex;//граница чуть меньше границы вершин меша необходима для коректного соединения мешей с разной детализацией
                    if (mapVerticesFlags[x, y] == 0)
                        continue;
                    int vertexIndex = vertexIndicesMap[x, y];
                    Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                    Vector2 uv = new Vector2(x, y) / (numVertsPerLine - 1);
                    Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                    float height = heightMap[x, y];

                    /*if (isEdgeConnectionVertex)
                    {
                        bool isVertical = x == 2 || x == numVertsPerLine - 3;
                        int dstToMainVertexA = ((isVertical) ? y - 2 : x - 2) % scipIncrement;//нумерует точки границы в зависимости от детализации
                        int dstToMainVertexB = scipIncrement - dstToMainVertexA;//нумерует в обратную сторону
                        float dstPercentFromAToB = dstToMainVertexA / (float)scipIncrement;//сколько до след. точки

                        float heightMainVertexA = heightMap[(isVertical) ? x : x - dstToMainVertexA, (isVertical) ? y - dstToMainVertexA : y];
                        float heightMainVertexB = heightMap[(isVertical) ? x : x + dstToMainVertexB, (isVertical) ? y + dstToMainVertexB : y];
                        //ровняет точки
                        height = heightMainVertexA * (1 - dstPercentFromAToB) + heightMainVertexB * dstPercentFromAToB;
                    }*/

                    meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), uv, vertexIndex);
                    //Debug.Log($"Позиция - {new Vector3(vertexPosition2D.x, height, vertexPosition2D.y)};               UV - {percent};              {vertexIndex}");
                    bool createTriangle = mapTrianglesFlags[x, y] > 0;//x < numVertsPerLine - 1 && y < numVertsPerLine - 1/* && (!isEdgeConnectionVertex || (x != 2 && y != 2))*/;//меньше чем правый и нижний аутофмеш И (не точка соединения ИЛИ (не верхняя левая граница соед))
                    bool outOfMeshTriangle = mapTrianglesFlags[x, y] != 2;
                    if (createTriangle)
                    {
                        //int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? scipIncrement : 1;
                        int a = vertexIndicesMap[x, y];
                        int b = vertexIndicesMap[x + 1, y];
                        int c = vertexIndicesMap[x, y + 1];
                        int d = vertexIndicesMap[x + 1, y + 1];
                        /*if(mapTrianglesFlags[x, y] == 1)
                        Debug.Log(mapTrianglesFlags[x, y] + "    -   " + a +" "+ b +" " + c + " " + d);*/
                        meshData.AddTriangle(a, d, c, outOfMeshTriangle);
                        meshData.AddTriangle(d, a, b, outOfMeshTriangle);
                    }
                }
            }
        }

        meshData.BakedNormals();


        return meshData;
    }


    public static DevidedMeshData GenerateTerrainMeshWithMountain(float[,] heightMap, MeshSettings meshSettings, BiomIndex[,] biomMap, BiomIndex biomIndex, bool mountain, float mountainStart)
    {
        int numVertsPerLine = meshSettings.numVertsPerLine;//размер Chunk'а + необходимые границы для нормалей

        Vector2 topLeft = new Vector2(-1, 1) * meshSettings.meshWorldSize / 2f;

        int[,] vertexIndicesMap = new int[numVertsPerLine, numVertsPerLine];
        int meshVertexIndex = 0;//было 0
        int outOfMeshVertexIndex = -1;

        int trianglesCount = 0;
        int outOfMeshTrianglesCount = 0;
        int[,] mapVerticesFlags = new int[numVertsPerLine, numVertsPerLine];
        int[,] mapTrianglesFlags = new int[numVertsPerLine, numVertsPerLine];
        for (int y = 0; y < numVertsPerLine - 1; y++)
        {
            for (int x = 0; x < numVertsPerLine - 1; x++)
            {
                bool isOutOfMeshVertexBorder = y == 0 || y >= numVertsPerLine - 2 || x == 0 || x >= numVertsPerLine - 2;//true если на границе(т.е. за ней)

                if (mountain)
                {
                    if (!biomIndex.Equals(biomMap[x, y]) || isOutOfMeshVertexBorder || heightMap[x, y] < mountainStart)//если на границе или не соответствует текущему биому
                    {
                        for (int i = y - 1; i <= y + 1; i++)
                        {
                            for (int j = x - 1; j <= x + 1; j++)
                            {
                                if (j < 0 || i < 0)
                                    continue;
                                if (biomIndex.Equals(biomMap[j, i]) && heightMap[x, y] > mountainStart)//помечает что точка пригранична к нашему биому но не входит в него. Отмечаем что оно подходит для нормалей.
                                {
                                    mapTrianglesFlags[x, y] = 1;
                                    outOfMeshTrianglesCount += 6;
                                    goto exit1;//выводим из цикла, т.к. уже знаем для чего нам эта точка.
                                }
                            }
                        }
                    }
                    else//точка полностью соответствует данному биому и подходит для меша.
                    {
                        mapTrianglesFlags[x, y] = 2;
                        trianglesCount += 6;
                    }
                }
                else
                {
                    if (!biomIndex.Equals(biomMap[x, y]) || isOutOfMeshVertexBorder || heightMap[x, y] > mountainStart)//если на границе или не соответствует текущему биому
                    {
                        for (int i = y - 1; i <= y + 1; i++)
                        {
                            for (int j = x - 1; j <= x + 1; j++)
                            {
                                if (j < 0 || i < 0)
                                    continue;
                                if (biomIndex.Equals(biomMap[j, i]) && heightMap[x, y] < mountainStart)//помечает что точка пригранична к нашему биому но не входит в него. Отмечаем что оно подходит для нормалей.
                                {
                                    mapTrianglesFlags[x, y] = 1;
                                    outOfMeshTrianglesCount += 6;
                                    goto exit1;//выводим из цикла, т.к. уже знаем для чего нам эта точка.
                                }
                            }
                        }
                    }
                    else//точка полностью соответствует данному биому и подходит для меша.
                    {
                        mapTrianglesFlags[x, y] = 2;
                        trianglesCount += 6;
                    }
                }

            exit1://отмечаем заданную точку, и точки справа, снизу, справа-снизу теми же пометками. 2 - точки подходящие для меша. 1 - точки для нормалей.
                if (mapTrianglesFlags[x, y] == 1)
                {
                    for (int i = y; i <= y + 1; i++)
                    {
                        for (int j = x; j <= x + 1; j++)
                        {
                            if (mapVerticesFlags[j, i] != 2)
                            {
                                mapVerticesFlags[j, i] = 1;
                            }
                        }
                    }
                }
                else if (mapTrianglesFlags[x, y] == 2)
                {
                    for (int i = y; i <= y + 1; i++)
                    {
                        for (int j = x; j <= x + 1; j++)
                        {
                            mapVerticesFlags[j, i] = 2;
                        }
                    }
                }
            }
        }

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                if (mapVerticesFlags[x, y] == 1)
                {
                    vertexIndicesMap[x, y] = outOfMeshVertexIndex;//помечаем граничащие точки отрицательными числами
                    outOfMeshVertexIndex--;
                }
                else if (mapVerticesFlags[x, y] == 2)
                {
                    vertexIndicesMap[x, y] = meshVertexIndex;//нумеруем прорисовываемые точки
                    meshVertexIndex++;
                }
            }
        }

        DevidedMeshData meshData = new DevidedMeshData(numVertsPerLine, meshVertexIndex, trianglesCount, -outOfMeshVertexIndex - 1, outOfMeshTrianglesCount);

        for (int y = 0; y < numVertsPerLine; y++)
        {
            for (int x = 0; x < numVertsPerLine; x++)
            {
                if (mapVerticesFlags[x, y] == 0)
                    continue;
                int vertexIndex = vertexIndicesMap[x, y];
                Vector2 percent = new Vector2(x - 1, y - 1) / (numVertsPerLine - 3);
                Vector2 uv = new Vector2(x, y) / (numVertsPerLine - 1);
                Vector2 vertexPosition2D = topLeft + new Vector2(percent.x, -percent.y) * meshSettings.meshWorldSize;
                float height = heightMap[x, y];

                meshData.AddVertex(new Vector3(vertexPosition2D.x, height, vertexPosition2D.y), uv, vertexIndex);
                bool createTriangle = mapTrianglesFlags[x, y] > 0;//x < numVertsPerLine - 1 && y < numVertsPerLine - 1/* && (!isEdgeConnectionVertex || (x != 2 && y != 2))*/;//меньше чем правый и нижний аутофмеш И (не точка соединения ИЛИ (не верхняя левая граница соед))
                bool outOfMeshTriangle = mapTrianglesFlags[x, y] != 2;
                if (createTriangle)
                {
                    //int currentIncrement = (isMainVertex && x != numVertsPerLine - 3 && y != numVertsPerLine - 3) ? scipIncrement : 1;
                    int a = vertexIndicesMap[x, y];
                    int b = vertexIndicesMap[x + 1, y];
                    int c = vertexIndicesMap[x, y + 1];
                    int d = vertexIndicesMap[x + 1, y + 1];
                    /*if(mapTrianglesFlags[x, y] == 1)
                    Debug.Log(mapTrianglesFlags[x, y] + "    -   " + a +" "+ b +" " + c + " " + d);*/
                    meshData.AddTriangle(a, d, c, outOfMeshTriangle);
                    meshData.AddTriangle(d, a, b, outOfMeshTriangle);
                }
            }
        }

        meshData.BakedNormals();

        return meshData;
    }
}

public class DevidedMeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bakedNormals;

    Vector3[] outOfMeshVertices;
    int[] outOfMeshTriangles;

    int triangleIndex;
    int outOfMeshTriangleIndex;
    public DevidedMeshData(int numVertsPerLine/*, int skipIncrement*/, int numVerts, int numTriangles, int numOutOfMeshVerts, int numOutOfMeshTriangles)
    {
        //считаем кол-во точек каждого типа
        //int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;//(53-2)*4-4 = 200
        //int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
        //int numMainVerticesPerLine = (numVertsPerLine - 5)/* / skipIncrement*/ + 1;//49
        //int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;//49*49=2401

        vertices = new Vector3[numVerts/*numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices*/];
        uvs = new Vector2[vertices.Length];

        //int numMeshEdgeTriangles = (numVertsPerLine - 4) * 8;//((numVertsPerLine - 3) * 4 - 4) * 2
        //int numMaintTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
        triangles = new int[numTriangles/*(numMeshEdgeTriangles + numMaintTriangles) * 3*/];

        outOfMeshVertices = new Vector3[numOutOfMeshVerts/*numVertsPerLine * 4 - 4*/];//-4 вычет углов
        outOfMeshTriangles = new int[numOutOfMeshTriangles/*24 * (numVertsPerLine - 2)*/];//упрощенное - ((numVertsPerLine - 1) * 4 - 4) * 2 * 3
        //Debug.Log("Отчет: Старое vertices = " + (numMeshEdgeVertices + numMainVertices) + "; Новое vertices = " + numVerts + ";");
        //Debug.Log("Отчет: Старое triangles = " + ((numMeshEdgeTriangles + numMaintTriangles) * 3) + "; Новое triangles = " + numTriangles + ";");
        //Debug.Log("Отчет: Старое outOfMeshVertices = " + (numVertsPerLine * 4 - 4) + "; Новое outOfMeshVertices = " + numOutOfMeshVerts + ";");
        //Debug.Log("Отчет: Старое outOfMeshTriangles = " + (24 * (numVertsPerLine - 2)) + "; Новое outOfMeshTriangles = " + numOutOfMeshTriangles + ";");
    }

    public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
    {
        if (vertexIndex < 0)
        {
            outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
        }
        else
        {
            vertices[vertexIndex] = vertexPosition;
            uvs[vertexIndex] = uv;
        }
    }

    public void AddTriangle(int a, int b, int c, bool outOfMeshTriangle)
    {
        if (outOfMeshTriangle/*a < 0 || b < 0 || c < 0*/)
        {
            //Debug.Log(outOfMeshTriangleIndex);
            outOfMeshTriangles[outOfMeshTriangleIndex] = a;
            outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
            outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
            outOfMeshTriangleIndex += 3;
        }
        else
        {
            //Debug.Log(a+" "+b+" "+ c);
            triangles[triangleIndex] = a;
            triangles[triangleIndex + 1] = b;
            triangles[triangleIndex + 2] = c;
            triangleIndex += 3;
        }
    }

    Vector3[] CalculateNormals()//основанно на пронумировке точек, т.е. вне меша отрицательные, рисуемые положительные
    {
        Vector3[] vertexNormals = new Vector3[vertices.Length];//кол-во обычных точек
        int triangleCount = triangles.Length / 3;//получаем кол-во треугольников, делим на три т.к. в треугольнике 3 точки
        for (int i = 0; i < triangleCount; i++)
        {
            int normalTriangleIndex = i * 3;//получаем номера точек треугольника
            int vertexIndexA = triangles[normalTriangleIndex];
            int vertexIndexB = triangles[normalTriangleIndex + 1];
            int vertexIndexC = triangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);//возвращает точку D где отрезок AD перпендикулярен AB и AC
            vertexNormals[vertexIndexA] += triangleNormal;
            vertexNormals[vertexIndexB] += triangleNormal;
            vertexNormals[vertexIndexC] += triangleNormal;
        }

        int borderTriangleCount = outOfMeshTriangles.Length / 3;//получаем кол-во треугольников вне меша
        for (int i = 0; i < borderTriangleCount; i++)
        {
            int normalTriangleIndex = i * 3;//получаем номера точек треугольника
            int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
            int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
            int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

            Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
            if (vertexIndexA >= 0)
            {
                vertexNormals[vertexIndexA] += triangleNormal;
            }
            if (vertexIndexB >= 0)
            {
                vertexNormals[vertexIndexB] += triangleNormal;
            }
            if (vertexIndexC >= 0)
            {
                vertexNormals[vertexIndexC] += triangleNormal;
            }
        }

        for (int i = 0; i < vertexNormals.Length; i++)
        {
            vertexNormals[i].Normalize();
        }

        return vertexNormals;
    }

    Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
    {
        Vector3 pointA = (indexA < 0) ? outOfMeshVertices[-indexA - 1] : vertices[indexA];//получаем точки треугольника
        Vector3 pointB = (indexB < 0) ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
        Vector3 pointC = (indexC < 0) ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

        Vector3 sideAB = pointB - pointA;//получаем отрезки треугольника
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;//получаем отрезок перпендикулярный этим отрезкам
    }

    public void BakedNormals()
    {
        bakedNormals = CalculateNormals();
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        //mesh.RecalculateNormals();
        mesh.normals = bakedNormals;
        return mesh;
    }
}