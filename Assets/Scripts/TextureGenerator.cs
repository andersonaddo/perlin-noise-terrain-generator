using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture GenerateRawTexture(float[,] noiseMap)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
        Texture2D texture = new Texture2D(width, height);

        Color[] colorMap = new Color[height * width];

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]); //Black = 0, white = 1

        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture GenerateColorTexture(float[,] noiseMap, TerrainLayer[] layers)
    {

        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);

        Color[] colorMap = new Color[height * width];

        //Going through the colormap array and assigning colors based off the selected biome
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float value = noiseMap[x, y];
                foreach (TerrainLayer layer in layers)
                {
                    if (value <= layer.heightUpperBound)
                    {
                        colorMap[width * y + x] = layer.color;
                        break;
                    }
                }
            }

        Texture2D texture = new Texture2D(width, height);

        texture.SetPixels(colorMap);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp; //Prevents The texture edges from wrapping and showing the color from another side of the texture
        texture.Apply();

        return texture;
    }
}
