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

    public enum EditorDrawMode { raw, color}

    void OnValidate()
    {
        if (mapHeight < 1) mapHeight = 1;
        if (mapWidth < 1) mapWidth = 1;
    }

    public void generateMap()
    {
        float[,] map = Noise.GenerateNoiseMap(mapWidth, mapHeight, mapSeed, mapOffet, biome.noiseScale, biome.octaves, biome.persistence, biome.lacunarity);

        if (drawMode == EditorDrawMode.raw)
            GetComponent<mapDisplayer>().RenderMap(map);
        else
        {
            Color[] colorMap = new Color[mapHeight * mapWidth];

            //Going through the colormap array and assigning colors based off the selected biome
            for (int y = 0; y < mapHeight; y++)
                for (int x = 0; x < mapWidth; x++)
                {
                    float value = map[x, y];
                    foreach(TerrainLayer layer in biome.layers)
                    {
                        if (value <= layer.heightUpperBound)
                        {
                            colorMap[mapWidth * y + x] = layer.color;
                            break;
                        }
                    }
                }
            GetComponent<mapDisplayer>().RenderMap(colorMap, mapWidth, mapHeight);
        }
    }
}
