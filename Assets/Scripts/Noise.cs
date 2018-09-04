using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The static class that handles the genration of raw noise maps
/// </summary>
public static class Noise {

    public enum NormalizeMode { local, global} //The way noise maps are localized depends on whether we're using the endlessTerrain system or not


    /// <summary>
    /// Generates a perlin noise map
    /// </summary>
    /// <param name="mapWidth">The width of the map</param>
    /// <param name="mapHeight">The heigh of the map</param>
    /// <param name="seed">The seed for the random offsets</param>
    /// <param name="mapOffset">The complete offset of the map</param>
    /// <param name="scale">The scale of magnification of the map</param>
    /// <param name="octaves">The number of octaves (ie runs of the perlin calculation) for any map value. Think of them as levels of detail</param>
    /// <param name="persistence">The rate of change of reduction influence of each octave. Should be 1 or less</param>
    /// <param name="lacunarity">The rate of change of reduction of gradualness (like, how jagged an octive is) per octve </param>
    /// <returns>A 2D map of values from 0 to 1</returns>
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, Vector2 mapOffset, float scale, int octaves, float persistence, float lacunarity, NormalizeMode mode, float globalDivisor)
     {
        float[,] map = new float[mapWidth, mapHeight];

        System.Random random = new System.Random(seed);
        Vector2[] octaveOffets = new Vector2[octaves]; //These offsets are applied to ocatves to add variance

        //Calculating the max possible non-clamped value attainable
        float maxPossibleHeight = 0;
        float tempAmplitude = 1;
        for (int i = 0; i < octaves; i++)
        {
            octaveOffets[i] = new Vector2(random.Next(-10000, 10000) + mapOffset.x, random.Next(-10000, 10000) - mapOffset.y);
            maxPossibleHeight += tempAmplitude;
            tempAmplitude *= persistence;
        }

        if (scale <= 0) scale = 0.0001f;
        float minLocalNoiseHeight = float.MaxValue;
        float maxLocalNoiseHeight = float.MinValue;

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int octave = 0; octave < octaves; octave++)
                {
                    float sampleX = ((x + octaveOffets[octave].x) / scale) * frequency ;
                    float sampleY = ((y + octaveOffets[octave].y)/ scale) * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                    perlinValue = perlinValue * 2 - 1; //Changing this value's range from 0-1 to -1-1 to allow some octaves to reduce noiseHeight
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                 map[x, y] = noiseHeight;
                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;
            }
        }

        //Restricting the range back to 0-1
        for (int y = 0; y < mapHeight; y++)
            for (int x = 0; x < mapWidth; x++)           
                if (mode == NormalizeMode.local) map[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight , map[x, y]);
                else
                {
                    //We first have to devide the values by the maximum attainable value
                    //Then we change their range back from -1-1 to 0-1
                    //We should keep in mind though, that there's little chance that a perlin value will get anywhere close to maxPossibleHeight
                    //So we'll divide maxPossibleHeight by a number to reduce the devision effect
                    float normalizedHeight = ((map[x, y] / (maxPossibleHeight / globalDivisor)) + 1) / 2;
                    map[x, y] = Mathf.Clamp01( normalizedHeight); //Clamping in case we a high value stayed out of range after out calculations
                }
        return map;
     }
}
