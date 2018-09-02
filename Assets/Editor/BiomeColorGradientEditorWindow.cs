using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class BiomeColorGradientEditorWindow : EditorWindow {

    BiomeColorGradient biomeGradient;
    Rect gradientPreviewRect;
    
    const int marginSize = 10;

    Rect[] keyRects;
    const float layerKeyWidth = 10;
    const float layerKeyHeight = 20;

    int selectedKeyIndex;
    bool mouseIsOnLayerKey;

    bool needsRepaint;

    public void setGradient(BiomeColorGradient grad)
    {
        biomeGradient = grad;
    }

    void OnEnable()
    {
        titleContent.text = "Biome Editor Window";
    }

    void OnGUI()
    {
        Draw();
        HandleInput();
    }


    void Draw()
    {
        //Drawing the gradient bar
         gradientPreviewRect = new Rect(marginSize, marginSize, position.width - marginSize * 2, 25);
        GUI.DrawTexture(gradientPreviewRect, biomeGradient.GetTexture((int)gradientPreviewRect.width));

        //Drawing the current layer keys as small rects
        keyRects = new Rect[biomeGradient.numberOfLayers];

        for (int i = 0; i < biomeGradient.numberOfLayers; i++)
        {
            BiomeColorGradient.TerrainLayer layer = biomeGradient.getlayer(i);
            Rect layerKeyRect = new Rect(
                gradientPreviewRect.x + gradientPreviewRect.width * layer.upperBound - layerKeyWidth / 2,
                gradientPreviewRect.yMax + marginSize,
                layerKeyWidth,
                layerKeyHeight);

            //Drawing a border for the selected key (just a larger rect behind it
            if (selectedKeyIndex == i)
            {
                EditorGUI.DrawRect(new Rect(layerKeyRect.x - 2, layerKeyRect.y - 2, layerKeyWidth + 4, layerKeyHeight + 4), Color.black);
            }

            EditorGUI.DrawRect(layerKeyRect, layer.Color);
            keyRects[i] = layerKeyRect;
        }

        //Drawing the settings pane
        Rect settingsRect = new Rect(marginSize, keyRects[0].yMax + marginSize, position.width - marginSize * 2, position.height);

        //GUILayout.BeginArea makes it easy to draw anything between it and EndAre
        GUILayout.BeginArea(settingsRect);

        EditorGUI.BeginChangeCheck(); //Listens for changes in fields between here and endChangeCheck
        Color newLayerColor = EditorGUILayout.ColorField(biomeGradient.getlayer(selectedKeyIndex).Color);
        if (EditorGUI.EndChangeCheck()) biomeGradient.updateLayerColor(selectedKeyIndex, newLayerColor);

        EditorGUI.BeginChangeCheck();
        float newBound = EditorGUILayout.FloatField(biomeGradient.getlayer(selectedKeyIndex).upperBound);
        if (EditorGUI.EndChangeCheck())
        {
            Mathf.Clamp01(newBound);
            selectedKeyIndex = biomeGradient.updateLayerBound(selectedKeyIndex, newBound);
        }

        biomeGradient.blendMode = (BiomeColorGradient.colorBlendMode)EditorGUILayout.EnumPopup("Blend Mode", biomeGradient.blendMode);
        biomeGradient.randomizeNewLayerColors = EditorGUILayout.Toggle("Randomize New Layers", biomeGradient.randomizeNewLayerColors);

        GUILayout.Label("Backspace to delete layers.\nUse arrow buttons to select layers, or click their keys.\nClick empty space to dreate a new layer.");

        GUILayout.EndArea();
    }


    void HandleInput()
    {
        Event guiEvent = Event.current;  //Tracks events like mouse clicks

        //Listening for clicks
        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0) //Left click
        {

            //Checking if an existing key has been clicked
            for (int i = 0; i < keyRects.Length; i++)
            {
                if (keyRects[i].Contains(guiEvent.mousePosition))
                {
                    mouseIsOnLayerKey = true;
                    selectedKeyIndex = i;
                    needsRepaint = true;
                    break;
                }
            }

            //Creating and auto-selecting a new key then
            if (!mouseIsOnLayerKey)
            {
                float newLayerUpperBound = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
                Color newLayerColor = (biomeGradient.randomizeNewLayerColors) ? Random.ColorHSV() : biomeGradient.Evaluate(newLayerUpperBound);
                selectedKeyIndex = biomeGradient.addLayer(newLayerColor, newLayerUpperBound);
                mouseIsOnLayerKey = true;
                needsRepaint = true;
            }
        }

        //A Selected key needs to be dragged
        if (mouseIsOnLayerKey && guiEvent.type == EventType.MouseDrag && guiEvent.button == 0)
        {
            float newLayerUpperBound = Mathf.InverseLerp(gradientPreviewRect.x, gradientPreviewRect.xMax, guiEvent.mousePosition.x);
            selectedKeyIndex = biomeGradient.updateLayerBound(selectedKeyIndex, newLayerUpperBound);
            // selectedKeyIndex is here incase the index of the layer chnged due to it's new bound
            needsRepaint = true;
        }

        if (guiEvent.type == EventType.MouseUp && guiEvent.button == 0) mouseIsOnLayerKey = false;

        //Delete a key on backspace
        if (guiEvent.keyCode == KeyCode.Backspace && guiEvent.type == EventType.KeyDown)
        {
            biomeGradient.removeLayer(selectedKeyIndex);

            //Selecting the next available key...
            if (selectedKeyIndex == 1 || selectedKeyIndex == 0)
            {
                selectedKeyIndex = 0;
            }
            else
            {
                selectedKeyIndex--;
            }
            needsRepaint = true;
        }

        if (guiEvent.keyCode == KeyCode.RightArrow && guiEvent.type == EventType.KeyDown)
        {
            selectedKeyIndex++;
            if (selectedKeyIndex >= biomeGradient.numberOfLayers) selectedKeyIndex = 0;
            needsRepaint = true;
        }

        if (guiEvent.keyCode == KeyCode.LeftArrow && guiEvent.type == EventType.KeyDown)
        {
            selectedKeyIndex--;
            if (selectedKeyIndex < 0) selectedKeyIndex = biomeGradient.numberOfLayers -1;
            needsRepaint = true;
        }




        if (needsRepaint) Repaint();
        needsRepaint = false;
    }
}
