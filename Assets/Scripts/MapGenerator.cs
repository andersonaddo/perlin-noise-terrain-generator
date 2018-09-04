using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

/// <summary>
/// The main class for map generation. Gets help from a bunch of helper classes, though
/// </summary>
public class MapGenerator : MonoBehaviour {

    public EditorDrawMode editorDrawMode;

    public Noise.NormalizeMode editorNormalizeMode;

    //This is preset because of the Level of Detail system.
    //That system only works for factors of size-1, and 240 has a lot of factors while staying in Unity's limit of 255^2 vertices per mesh
    [HideInInspector] public const int mapChunkSize = 241;

    [Tooltip("Levels of detail allow for dynamic polygon optimization")]
    [Range(0, 6)]public int editorMeshLevelOfDetail; //Factors of 240: 1, 2, 4, 6, 8, 10, 12. meshLevelOfDetail will be multiplied by two later on

    public int mapSeed;

    public Biome biome; 

    [Tooltip("Play with this and you'll see how this works. This offsets the entire map in runtime too (not live, however)")]
    public Vector2 editorMapOffet;

    [Tooltip("Update the Editor map every time you change an inspector value")]
    public bool autoUpdate;

    public enum EditorDrawMode { raw, color, mesh}

    Queue<GeneratedMapThreadInfo<MapData>> mapDataQueue = new Queue<GeneratedMapThreadInfo<MapData>>();
    Queue<GeneratedMapThreadInfo<MeshData>> meshDataQueue = new Queue<GeneratedMapThreadInfo<MeshData>>();


    void OnValidate()
    {
        GetComponent<mapDisplayer>().texturePane.gameObject.SetActive(editorDrawMode != EditorDrawMode.mesh);
        GetComponent<mapDisplayer>().meshFilter.gameObject.SetActive(editorDrawMode == EditorDrawMode.mesh);
    }

    void Start()
    {
        Camera.main.backgroundColor = biome.cameraBackgroundColor;

        //Removing all my editor planes and meshes
        GetComponent<mapDisplayer>().texturePane.gameObject.SetActive(false);
        GetComponent<mapDisplayer>().meshFilter.gameObject.SetActive(false);
    }

    void Update()
    {
        if (mapDataQueue.Count > 0)
        {
            for (int i = 0; i < mapDataQueue.Count; i++)
            {
                GeneratedMapThreadInfo<MapData> data = mapDataQueue.Dequeue();
                data.callback(data.data);
            }               
        }

        if (meshDataQueue.Count > 0)
        {
            for (int i = 0; i < meshDataQueue.Count; i++)
            {
                GeneratedMapThreadInfo<MeshData> data = meshDataQueue.Dequeue();
                data.callback(data.data);
            }
        }
    }

    MapData GenerateMapData(Vector2 center, Noise.NormalizeMode normalizeMode)
    {
        float[,] map = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, mapSeed, center + editorMapOffet, biome.noiseScale, biome.octaves, biome.persistence, biome.lacunarity, normalizeMode);

        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        //Going through the colormap array and assigning colors based off the selected biome
        for (int y = 0; y < mapChunkSize; y++)
            for (int x = 0; x < mapChunkSize; x++)
            {
                colorMap[mapChunkSize * y + x] = biome.layers.Evaluate(map[x, y]);
            }

        return new MapData(map, colorMap);
    }



    //Called by endlessTerrain Script. Generates MapsData struct on separate thread
    public void GenerateMapDataAsync(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataGenerationThreadLogic(center, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataGenerationThreadLogic(Vector2 center, Action<MapData> callback)
    {
        MapData data = GenerateMapData(center, Noise.NormalizeMode.global);
        lock(mapDataQueue) //To prevent multiple thread from accessing the wueue at the same time. Queue's aren't thread safe!
        {
        mapDataQueue.Enqueue(new GeneratedMapThreadInfo<MapData>(callback, data));
        }
        //We're not doing the callbacks straight in these threads because some runtime things are only meant to be done inthe main thread
    }




    public void GenerateMeshDataAsync(MapData mapData, Action<MeshData> callback, int levelOfDetail)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataGenerationThreadLogic(mapData, callback, levelOfDetail);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataGenerationThreadLogic(MapData mapData, Action<MeshData> callback, int levelOfDetail)
    {
        MeshData meshData = MapMeshGenerator.GenerateMesh(mapData.noiseMap, levelOfDetail ,biome.heightMultiplierCurve, biome.heightMultiplier);
        lock (mapDataQueue) 
        {
            meshDataQueue.Enqueue(new GeneratedMapThreadInfo<MeshData>(callback, meshData));
        }
    }



    //Called by MapGenerator's custom editor script
    public void DrawMapInEditor()
    {
        MapData mapData = GenerateMapData (Vector2.zero, editorNormalizeMode);
        if (editorDrawMode == EditorDrawMode.raw) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateRawTexture(mapData.noiseMap));
        else if (editorDrawMode == EditorDrawMode.color) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateColorTexture(mapData.noiseMap, mapData.colorMap));
        else GetComponent<mapDisplayer>().DrawMesh(MapMeshGenerator.GenerateMesh(mapData.noiseMap, editorMeshLevelOfDetail, biome.heightMultiplierCurve, biome.heightMultiplier), TextureGenerator.GenerateColorTexture(mapData.noiseMap, mapData.colorMap));
        Camera.main.backgroundColor = biome.cameraBackgroundColor;
    }

    /// <summary>
    /// These are the results of map and mesh generation threads. The callbalks
    /// will be called with `data` as their parameters from a queue on Unity's main thread
    /// </summary>
    struct GeneratedMapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T data;

        public GeneratedMapThreadInfo(Action<T> callback, T data)
        {
            this.callback = callback;
            this.data = data;
        }
    }
}

/// <summary>
/// Simple struct that holds the noise map and color map of a terrain chunk
/// </summary>
public struct MapData
{
    public readonly float [,] noiseMap;
    public readonly Color[] colorMap;

    public MapData(float[,] noiseMap, Color[] colorMap)
    {
        this.noiseMap = noiseMap;
        this.colorMap = colorMap;
    }
}
