using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Biome", menuName = "Perlin Maps/Biome", order = 1)]
public class Biome : ScriptableObject {

    [Range(0.7f, 100)] public float noiseScale;


    [Tooltip("The number of octaves (ie runs of the perlin calculation) for any map value. Think of them as levels of detail.")]
    public int octaves;

    [Tooltip("The rate of change of reduction of gradualness (like, how jagged an octive is) per ocatve. Higher lacunarity means more detail per octave")]
    [Range(1, 10)]
    public float lacunarity;

    [Tooltip("The rate of change of reduction influence of each octave. Lower persistence means lower infuence per octave.")]
    [Range(0.1f, 1)]
    public float persistence;

    public TerrainLayer[] layers;
}

[System.Serializable]
public struct TerrainLayer
{
    public string name;
    public float heightUpperBound;
    public Color color;
}
