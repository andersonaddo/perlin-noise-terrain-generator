using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(BiomeColorGradient))]
public class BiomeColorGradientDrawer : PropertyDrawer {

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Event guiEvent = Event.current; //Tracks inspector events like clicks and re-draws

        BiomeColorGradient biomeGradient = (BiomeColorGradient)fieldInfo.GetValue(property.serializedObject.targetObject);
        float labelWidth = GUI.skin.label.CalcSize(label).x + 5; //THe 5 is just a buffer for looks
        Rect gradientTextureRect = new Rect(position.x + labelWidth, position.y, position.width - labelWidth, position.height);

        if (guiEvent.type == EventType.Repaint)
        {
            GUI.Label(position, label); //Rendering the name of the field

            //We want to draw the biomeGradient gradient in the inspector, but GUI.draw texture is buggy.
            //So, we'll draw the gradent as a label instead
            GUIStyle gradientStyle = new GUIStyle();
            gradientStyle.normal.background = biomeGradient.GetTexture((int)position.width);
            GUI.Label(gradientTextureRect, GUIContent.none, gradientStyle);
        }
        else if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0) //0 is the left mouse button
        {
            if (gradientTextureRect.Contains(guiEvent.mousePosition))
            {
                BiomeColorGradientEditorWindow window = EditorWindow.GetWindow<BiomeColorGradientEditorWindow>();
                window.setGradient(biomeGradient);
            }

        }
    }


}
