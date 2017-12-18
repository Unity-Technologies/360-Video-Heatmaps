using System;
using UnityEditor;
using UnityEngine;
using System.Linq;

namespace UnityAnalytics360VideoHeatmap
{
    public class AnalyticsSmootherControl
    {
        private static GUIContent[] s_SmootherOptionsContent;
       
        private static Texture2D darkSkinUnionIcon = Resources.Load("unity_analytics_heatmaps_union_dark") as Texture2D;
        private static Texture2D darkSkinNumberIcon = Resources.Load("unity_analytics_heatmaps_number_dark") as Texture2D;
        private static Texture2D darkSkinNoneIcon = Resources.Load("unity_analytics_heatmaps_none_dark") as Texture2D;

        private static Texture2D lightSkinUnionIcon = Resources.Load("unity_analytics_heatmaps_union_light") as Texture2D;
        private static Texture2D lightSkinNumberIcon = Resources.Load("unity_analytics_heatmaps_number_light") as Texture2D;
        private static Texture2D lightSkinNoneIcon = Resources.Load("unity_analytics_heatmaps_none_light") as Texture2D;

        public delegate void ChangeHandler(int toggler, float value);

        public static void SmootherControl (int toggler, float value, 
            string label, string tooltip,  ChangeHandler change,
            int endIndex = -1)
        {
            if (s_SmootherOptionsContent == null)
            {
                var unionIcon = lightSkinUnionIcon;
                var smoothIcon = lightSkinNumberIcon;
                var noneIcon = lightSkinNoneIcon;
                if (EditorGUIUtility.isProSkin)
                {
                    unionIcon = darkSkinUnionIcon;
                    smoothIcon = darkSkinNumberIcon;
                    noneIcon = darkSkinNoneIcon;
                }

                s_SmootherOptionsContent = new GUIContent[] {
                    new GUIContent(smoothIcon, "Smooth to value"),
                    new GUIContent(noneIcon, "No smoothing"),
                    new GUIContent(unionIcon, "Unify all")
                };
            }

            using (new EditorGUILayout.VerticalScope())
            {
                var options = endIndex == -1 ? s_SmootherOptionsContent : 
                    s_SmootherOptionsContent.Take(endIndex).ToArray();

                EditorGUI.BeginChangeCheck();
                int oldToggle = toggler;
                toggler = GUILayout.Toolbar(toggler, options, GUILayout.MaxWidth(100));

                float lw = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 50;
                float fw = EditorGUIUtility.fieldWidth;
                EditorGUIUtility.fieldWidth = 20;
                EditorGUI.BeginDisabledGroup(toggler != HeatmapDataProcessor.SMOOTH_VALUE);
                value = EditorGUILayout.FloatField(new GUIContent(label, tooltip), value);
                value = Mathf.Max(0, value);
                EditorGUI.EndDisabledGroup();
                EditorGUIUtility.labelWidth = lw;
                EditorGUIUtility.fieldWidth = fw;

                if (EditorGUI.EndChangeCheck() || oldToggle != toggler)
                {
                    change(toggler, value);
                }
            }
        }
    }
}

