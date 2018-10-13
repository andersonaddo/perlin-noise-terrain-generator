using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[CreateAssetMenu(fileName = "New Biome", menuName = "Perlin Maps/Biome", order = 1)]
public class Biome : ScriptableObject {

    [Range(0.7f, 100)] public float noiseScale;
    public float globalNormalizerDivisor;


    [Tooltip("The number of octaves (ie runs of the perlin calculation) for any map value. Think of them as levels of detail.")]
    public int octaves;

    [Tooltip("The rate of change of reduction of gradualness (like, how jagged an octive is) per ocatve. Higher lacunarity means more detail per octave")]
    [Range(1, 10)]
    public float lacunarity;

    [Tooltip("The rate of change of reduction influence of each octave. Lower persistence means lower infuence per octave.")]
    [Range(0.1f, 1)]
    public float persistence;

    [Tooltip("This curve maps the map values (0-1) to real mesh heights.")]
    public AnimationCurve heightMultiplierCurve;

    [Tooltip("Multiplies the output of the heightMultiplierCurve")]
    public float heightMultiplier;

    public biomeAmbienceData ambienceData;

    public BiomeColorGradient layers;

    [Tooltip("What is the chance that an artifact will be placed on any vertix on the mesh?")]
    [Range(0, 1)] public float artifactChancePerVertix;

    public List<BiomeArtifact> artifacts;
}

[System.Serializable]
public struct TerrainLayer
{
    public float heightUpperBound;
    public Color color;
}

/// <summary>
/// Holds all the secondary aethetic data for a biome (like post processing)
/// </summary>
[System.Serializable]
public class biomeAmbienceData
{
    public Color cameraBackgroundColor;
    public PostProcessingProfile postProcessingProfile;
    public Color directionalLightColor;
    public float directionalLightIntensity;
}
