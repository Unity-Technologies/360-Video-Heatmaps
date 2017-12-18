/// <summary>
/// Heatmapper inspector.
/// </summary>
/// This code drives the Heatmapper inspector
/// The HeatmapDataParser handles loading and parsing the data.
/// The HeatmapRendererInspector speaks to the Renderer to achieve a desired look.

using System.Collections.Generic;
using UnityAnalytics360VideoHeatmap;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using System;

namespace UnityAnalytics360VideoHeatmap
{
    public class Heatmapper : EditorWindow
    {
        [MenuItem("Window/Unity Analytics/360 Video Heatmaps/Heatmapper")]
        static void HeatmapperMenuOption()
        {
            EditorWindow.GetWindow(typeof(Heatmapper));
        }

        static Texture m_HeatmapperIconTexture;

        // Views
        AggregationInspector m_AggregationView;
        HeatmapRendererInspector m_RenderView;

        // Data handler
        HeatmapDataProcessor m_Processor;

        //ViewModel
        HeatmapViewModel viewModel;

        GameObject m_HeatMapInstance;

        Vector2 m_ScrollPosition;

        void OnEnable()
        {
            EnsureHeatmapInstance();
            viewModel = HeatmapViewModel.instance;
            m_Processor = new HeatmapDataProcessor();
            m_Processor.RestoreSettings();
            m_RenderView = HeatmapRendererInspector.Init(this, m_Processor);
            m_AggregationView = AggregationInspector.Init(m_Processor);
            m_AggregationView.OnEnable();
            m_HeatmapperIconTexture = Resources.Load("unity_analytics_heatmaps_heatmapper") as Texture;
            viewModel.ManualOverride();

        }

        void OnFocus()
        {
            SystemProcess();
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
            {
                EnsureHeatmapInstance();
            }

            // Guistyle
            GUIStyle m_boxStyle = new GUIStyle("box");
            m_boxStyle.padding = new RectOffset(6, 6, 8, 8);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField(new GUIContent(" 360 Video Heatmapper", m_HeatmapperIconTexture), EditorStyles.boldLabel);

            using (new EditorGUILayout.VerticalScope(m_boxStyle))
            {
                using (var scroll = new EditorGUILayout.ScrollViewScope(m_ScrollPosition))
                {
                    m_ScrollPosition = scroll.scrollPosition;

                    using (new EditorGUILayout.VerticalScope())
                    {
                        m_AggregationView.OnGUI();
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        //TODO: Clean up
                        EditorGUILayout.LabelField("Video Player", EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                        var temp = EditorGUILayout.ObjectField(HeatmapViewModel.videoPlayer, typeof(VideoPlayer), allowSceneObjects: true) as VideoPlayer;
                        if(temp != null)
                            HeatmapViewModel.videoPlayer = temp;
                    }

                    using (new EditorGUILayout.VerticalScope())
                    {
                        if (m_RenderView != null)
                        {
                            m_RenderView.OnGUI();
                        }
                    }
                }
            }
        }

        void Update()
        {
            Repaint();
        }

        void SystemProcess()
        {
            EnsureHeatmapInstance();
        }

        public void SwapRenderer(Type renderer)
        {
            AttemptReconnectWithHeatmapInstance();
            CreateHeatmapInstance(renderer);
        }

        void EnsureHeatmapInstance()
        {
            AttemptReconnectWithHeatmapInstance();
            if (m_HeatMapInstance == null)
            {
                CreateHeatmapInstance();
            }
            if (m_RenderView != null)
            {
                m_RenderView.SetGameObject(m_HeatMapInstance);
            }
        }

        /// <summary>
        /// Attempts to reconnect with a heatmap instance.
        /// </summary>
        void AttemptReconnectWithHeatmapInstance()
        {
            m_HeatMapInstance = GameObject.Find("UnityAnalytics__Heatmap");
        }

        /// <summary>
        /// Creates the heat map instance.
        /// </summary>
        void CreateHeatmapInstance(bool force = false)
        {
            if (force)
            {
                DestroyHeatmapInstance();
            }
            CreateHeatmapInstance(typeof(VideoRenderer));
        }

        void CreateHeatmapInstance(Type t)
        {
            DestroyHeatmapInstance();
            m_HeatMapInstance = new GameObject();
            m_HeatMapInstance.tag = "EditorOnly";
            m_HeatMapInstance.name = "UnityAnalytics__Heatmap";
            m_HeatMapInstance.AddComponent(t);
            m_HeatMapInstance.GetComponent<IHeatmapRenderer>().allowRender = true;
        }

        void DestroyHeatmapInstance()
        {
            if (m_HeatMapInstance)
            {
                m_HeatMapInstance.transform.parent = null;
                DestroyImmediate(m_HeatMapInstance);
            }
        }
    }
}
