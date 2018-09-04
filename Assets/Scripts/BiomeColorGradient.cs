using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a custom gradient that evaluated the colors for the texture of the noise maps
/// </summary>
[System.Serializable]
public class BiomeColorGradient
{

    public enum colorBlendMode { linear, discrete }
    public colorBlendMode blendMode;
    public bool randomizeNewLayerColors;

    [SerializeField]
    List<TerrainLayer> layers = new List<TerrainLayer>();

    public BiomeColorGradient()
    {
        //Adding some initial layers
        addLayer(Color.white, 0);
        addLayer(Color.black, 1);
    }

    /// <summary>
    /// Returns a color based off a noise map value of 0 to 1
    /// </summary>
	public Color Evaluate(float value)
    {

        //Getting the layers to the left and the right of the map value
        TerrainLayer leftLayer = layers[0];
        TerrainLayer rightLayer = layers[layers.Count - 1];

        for (int i = 0; i < layers.Count - 1; i++)
        {
            if (layers[i].upperBound <= value)
            {
                leftLayer = layers[i];
            }

            if (layers[i].upperBound >= value)
            {
                rightLayer = layers[i];
                break;
            }
        }
        if (blendMode == colorBlendMode.linear)
        {
            float relativeValue = Mathf.InverseLerp(leftLayer.upperBound, rightLayer.upperBound, value);
            return Color.Lerp(leftLayer.Color, rightLayer.Color, relativeValue);
        }
        return rightLayer.Color;

    }

    //For Editor GUI
    public Texture2D GetTexture(int width)
    {
        Texture2D texture = new Texture2D(width, 1);
        Color[] colors = new Color[width];

        for (int i = 0; i < width; i++)
        {
            colors[i] = Evaluate((float)i / (width - 1));
        }

        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }


    public TerrainLayer getlayer(int index)
    {
        return layers[index];
    }

    public int numberOfLayers
    {
        get
        {
            return layers.Count;
        }
    }

    public int addLayer(Color color, float upperBound)
    {
        TerrainLayer layer = new TerrainLayer(upperBound, color);

        //Adding it in a way that the list stays in ascending order of upperBounds
        for (int i = 0; i < layers.Count; i++)
        {
            if (layer.upperBound < layers[i].upperBound)
            {
                layers.Insert(i, layer);
                return i;
            }
        }

        //Incase this new layer actually has the highest upperBound
        layers.Add(layer);
        return layers.Count - 1;
    }


    public void removeLayer(int index)
    {
        if (layers.Count >= 2)
            layers.RemoveAt(index);
    }

    public int updateLayerBound(int index, float newBound)
    {
        //We can't just change the value directly, the array has be in order
        Color color = layers[index].Color;
        removeLayer(index);
        return addLayer(color, newBound);
    }

    public void updateLayerColor(int index, Color color)
    {
        layers[index] = new TerrainLayer(layers[index].upperBound, color);
    }

    //Forces a gradient to remove all it's layers. Risky if not called in the right places
    public void forceClear()
    {
        layers.Clear();
    }

    //Called by the BiomeColorGradientEditorWindow to keep a stack of undoStates
    public static BiomeColorGradient Clone(BiomeColorGradient gradient)
    {
        BiomeColorGradient clone = new BiomeColorGradient();
        clone.mimic(gradient);
        return clone;
    }

    //Copies all the properties of one gradient onto another
    public void mimic(BiomeColorGradient gradient)
    {
        forceClear();
        blendMode = gradient.blendMode;
        randomizeNewLayerColors = gradient.randomizeNewLayerColors;

        for (int i = 0; i < gradient.numberOfLayers; i++)
        {
            addLayer(gradient.getlayer(i).Color, gradient.getlayer(i).upperBound);
        }

    }

    //These act as keys for the biome gradient
    [System.Serializable]
    public struct TerrainLayer
    {
        [SerializeField] float heightUpperBound; //0 to 1
        [SerializeField] Color color;

        public TerrainLayer(float heightUpperBound, Color color)
        {
            this.heightUpperBound = heightUpperBound;
            this.color = color;
        }

        public Color Color
        {
            get
            {
                return color;
            }
        }

        public float upperBound
        {
            get
            {
                return heightUpperBound;
            }
        }
    }
}
