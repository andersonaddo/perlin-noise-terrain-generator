using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main class for map generation. Gets help from a bunch of helper classes, though
/// </summary>
public class MapGenerator : MonoBehaviour {

    public EditorDrawMode drawMode;
    public int mapHeight, mapWidth;
    public int mapSeed;

    public Biome biome; 

    [Tooltip("Play with this and you'll see how this works.")]
    public Vector2 mapOffet;

    [Tooltip("Update the Editor map every time you change an inspector value")]
    public bool autoUpdate;

    public enum EditorDrawMode { raw, color, mesh}

    void OnValidate()
    {
        if (mapHeight < 1) mapHeight = 1;
        if (mapWidth < 1) mapWidth = 1;

        GetComponent<mapDisplayer>().texturePane.gameObject.SetActive(drawMode != EditorDrawMode.mesh);
        GetComponent<mapDisplayer>().meshFilter.gameObject.SetActive(drawMode == EditorDrawMode.mesh);
    }

    public void generateMap()
    {
        float[,] map = Noise.GenerateNoiseMap(mapWidth, mapHeight, mapSeed, mapOffet, biome.noiseScale, biome.octaves, biome.persistence, biome.lacunarity);

        if (drawMode == EditorDrawMode.raw) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateRawTexture(map));
        else if (drawMode == EditorDrawMode.color) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateColorTexture(map, biome.layers));
        else GetComponent<mapDisplayer>().DrawMesh(MapMeshGenerator.GenerateMesh(map, biome.heightMultiplierCurve), TextureGenerator.GenerateColorTexture(map, biome.layers));
    }
}
