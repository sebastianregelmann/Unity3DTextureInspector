using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;

[CustomEditor(typeof(GameObject))]
public class TextureInspector3D : Editor
{
    private Texture3D texture;
    private Texture2D previewTexture;
    private int zLayer = 0;
    private float sizeFactor = 1;
    private bool showRed = true, showGreen = true, showBlue = true, showAlpha = true;

    private Texture3D lastTexture;
    private int lastZLayer;
    private bool lastShowRed, lastShowGreen, lastShowBlue, lastShowAlpha;

    private void OnEnable()
    {
        EditorApplication.update += UpdateTexture;
    }

    private void OnDisable()
    {
        EditorApplication.update -= UpdateTexture;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Draw normal inspector

        GUILayout.Space(10);
        GUILayout.Label("3D Texture Inspector", EditorStyles.boldLabel);

        // Automatically find Texture3D in attached components
        texture = FindTexture3DInComponents(target as GameObject);

        // Allow manual selection if not found
        texture = (Texture3D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture3D), false);

        if (texture == null) return;

        zLayer = EditorGUILayout.IntSlider("Z Layer", zLayer, 0, texture.depth - 1);
        sizeFactor = EditorGUILayout.Slider("Size Factor", sizeFactor, 0.01f, 5f);

        EditorGUILayout.BeginHorizontal();
        showRed = GUILayout.Toggle(showRed, "R");
        showGreen = GUILayout.Toggle(showGreen, "G");
        showBlue = GUILayout.Toggle(showBlue, "B");
        showAlpha = GUILayout.Toggle(showAlpha, "A");
        EditorGUILayout.EndHorizontal();

        ////if (SettingsChanged())
        //{
        //   
        //}

        previewTexture = ConvertTo2D();
        DisplayTexture(previewTexture);

        //UpdateLastSettings();
    }

    private void DisplayTexture(Texture2D texture)
    {
        if (texture == null) return;

        float maxWidth = EditorGUIUtility.currentViewWidth - 20;
        float aspectRatio = (float)texture.height / texture.width;
        float displayWidth = Mathf.Min(texture.width, maxWidth);
        float displayHeight = displayWidth * aspectRatio;

        Rect texRect = GUILayoutUtility.GetRect(displayWidth * sizeFactor, displayHeight * sizeFactor);
        GUI.DrawTexture(texRect, texture, ScaleMode.ScaleToFit);
    }

    private Texture2D ConvertTo2D()
    {
        Texture2D processedTexture = new Texture2D(texture.width, texture.height);

        for (int x = 0; x < texture.width; x++)
        {
            for (int y = 0; y < texture.height; y++)
            {
                Color pixelColor = texture.GetPixel(x, y, zLayer);

                pixelColor.r = showRed ? pixelColor.r : 0;
                pixelColor.g = showGreen ? pixelColor.g : 0;
                pixelColor.b = showBlue ? pixelColor.b : 0;
                pixelColor.a = showAlpha ? pixelColor.a : 0;

                processedTexture.SetPixel(x, y, pixelColor);
            }
        }

        processedTexture.Apply();
        return processedTexture;
    }

    private bool SettingsChanged()
    {
        return lastTexture != texture || lastZLayer != zLayer ||
               lastShowRed != showRed || lastShowGreen != showGreen ||
               lastShowBlue != showBlue || lastShowAlpha != showAlpha;
    }

    private void UpdateLastSettings()
    {
        lastTexture = texture;
        lastZLayer = zLayer;
        lastShowRed = showRed;
        lastShowGreen = showGreen;
        lastShowBlue = showBlue;
        lastShowAlpha = showAlpha;
    }

    private void UpdateTexture()
    {
        if (texture != null && texture != lastTexture)
        {
            previewTexture = ConvertTo2D();
            UpdateLastSettings();
            Repaint();
        }
    }

    private Texture3D FindTexture3DInComponents(GameObject obj)
    {
        if (obj == null) return null;

        var components = obj.GetComponents<MonoBehaviour>();

        foreach (var comp in components)
        {
            var fields = comp.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var field in fields)
            {
                if (field.FieldType == typeof(Texture3D))
                {
                    return (Texture3D)field.GetValue(comp);
                }
            }
        }

        return null;
    }
}
