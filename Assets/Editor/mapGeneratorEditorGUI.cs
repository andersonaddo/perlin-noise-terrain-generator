using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class mapGeneratorEditorGUI : Editor {


    // We'll cache the biome editor here
    private Editor cachedBiomeEditor;

    /* using this boolean to keep track if whether cachedEditor has already been assigned too.
      We're required to call Editor.CreateEditor() from OnInspectorGUI(), which is called often, 
      but we only really need to call Editor.CreateEditor() once. */
    private bool cachedEditorNeedsRefresh = true;

    bool showBiomeEditor;

    public void OnEnable()
    {
        // Resetting cachedEditor, and marking it to be reassigned
        cachedBiomeEditor = null;
        cachedEditorNeedsRefresh = true;
    }


    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;

        //Checking if we need to get our biome Editor. Calling Editor.CreateEditor() if needed
        if (cachedEditorNeedsRefresh) cachedBiomeEditor = CreateEditor(mapGenerator.biome);

        //DrawDefaultInspector draws the editor and returns true if an ispector value has changed
        if (DrawDefaultInspector() && mapGenerator.autoUpdate) mapGenerator.DrawMapInEditor();

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider); //Adding a horizontal line

        showBiomeEditor = EditorGUILayout.Foldout(showBiomeEditor, "Biome Variables");

        if (showBiomeEditor)
        {
            if (cachedBiomeEditor.DrawDefaultInspector() && mapGenerator.autoUpdate)
                mapGenerator.DrawMapInEditor();
        }

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);


        //If this button is clicked...
        if (GUILayout.Button("Generate Map")) mapGenerator.DrawMapInEditor(); 
    }
}
