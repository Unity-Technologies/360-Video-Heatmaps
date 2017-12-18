using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityAnalytics360VideoHeatmap
{
    public class FlattenedHeatmapView : EditorWindow
    {
        RenderTexture _composite;

        [MenuItem("Window/Unity Analytics/360 Video Heatmaps/Flattened Heatmap View")]
        static void ShowPreviewWindow()
        {
            EditorWindow.GetWindow(typeof(FlattenedHeatmapView));
        }

        void OnGUI()
        {
            if (_composite == null)
            {
                var go = GameObject.Find("UnityAnalytics__Heatmap");
                if (go)
                {
                    var heatmap = go.GetComponent<Hotspots>();
                    if (heatmap)
                    {
                        _composite = heatmap.composite;
                    }
                }
            }

            float x = 0;
            float y = 0;
            float w = position.width;
            float h = 0;

            if (_composite != null)
            {
                h = _composite.height * w / _composite.width;
                EditorGUI.DrawPreviewTexture(new Rect(x, y, w, h), _composite);
            }

            y += h;
        }

        void OnInspectorUpdate()
        {
            Repaint();
        }
    }
}
