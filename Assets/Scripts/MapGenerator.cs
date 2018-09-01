using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The main class for map generation. Gets help from a bunch of helper classes, though
/// </summary>
public class MapGenerator : MonoBehaviour {

    public EditorDrawMode drawMode;

    //This is preset because of the Level of Detail system.
    //That system only works for factors of size-1, and 240 has a lot of factors while staying in Unity's limit of 255^2 vertices per mesh
    const int mapChunkSize = 241;

    [Tooltip("Allows for dynamic polygon optimization")]
    [Range(0, 6)]public int meshLevelOfDetail; //Factors of 240: 1, 2, 4, 6, 8, 10, 12. meshLevelOfDetail will be multiplied by two later on

    public int mapSeed;

    public Biome biome; 

    [Tooltip("Play with this and you'll see how this works.")]
    public Vector2 mapOffet;

    [Tooltip("Update the Editor map every time you change an inspector value")]
    public bool autoUpdate;

    public enum EditorDrawMode { raw, color, mesh}

    void OnValidate()
    {
        GetComponent<mapDisplayer>().texturePane.gameObject.SetActive(drawMode != EditorDrawMode.mesh);
        GetComponent<mapDisplayer>().meshFilter.gameObject.SetActive(drawMode == EditorDrawMode.mesh);
    }

    public void generateMap()
    {
        float[,] map = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, mapSeed, mapOffet, biome.noiseScale, biome.octaves, biome.persistence, biome.lacunarity);

        if (drawMode == EditorDrawMode.raw) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateRawTexture(map));
        else if (drawMode == EditorDrawMode.color) GetComponent<mapDisplayer>().DrawTexture(TextureGenerator.GenerateColorTexture(map, biome.layers));
        else GetComponent<mapDisplayer>().DrawMesh(MapMeshGenerator.GenerateMesh(map, meshLevelOfDetail, biome.heightMultiplierCurve, biome.heightMultiplier), TextureGenerator.GenerateColorTexture(map, biome.layers));
    }
}
