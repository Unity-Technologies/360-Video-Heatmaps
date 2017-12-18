/// <summary>
/// Heat map renderer inspector.
/// </summary>
/// This code manages the portion of the inspector that
/// controls the Heat Map renderer.

using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapRendererInspector
    {
        const string k_Renderer = "UnityAnalyticsHeatmapRenderer";

        const string k_StartTimeKey = "UnityAnalyticsHeatmapStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapEndTime";
        const string k_PlaySpeedKey = "UnityAnalyticsHeatmapPlaySpeed";

        const string k_LowXKey = "UnityAnalyticsHeatmapLowX";
        const string k_HighXKey = "UnityAnalyticsHeatmapHighX";
        const string k_LowYKey = "UnityAnalyticsHeatmapLowY";
        const string k_HighYKey = "UnityAnalyticsHeatmapHighY";
        const string k_LowZKey = "UnityAnalyticsHeatmapLowZ";
        const string k_HighZKey = "UnityAnalyticsHeatmapHighZ";

        const string k_ShowTipsKey = "UnityAnalyticsHeatmapShowRendererTooltips";

        const string k_ClipName = "UnityAnalyticsHeatmapClipName";

        Heatmapper m_Heatmapper;

        int m_MaxTime = -1;

        int m_ClipNameIndex = 0;
        Rect m_ClipDropdownRect;

        int m_QualityIndex = 0;

        float m_minRadius = 1.0f;
        float m_maxRaidus = 50.0f;

        float m_minDecay = 0.0f;
        float m_maxDecay = 1.0f;

        const int k_NoFollow = 0;
        const int k_SceneFollow = 1;
        const int k_MainCameraFollow = 2;

        GameObject m_GameObject;
        VideoRenderer videoRenderer;

        Texture2D darkSkinPlayIcon = Resources.Load("unity_analytics_heatmaps_play_dark") as Texture2D;
        Texture2D darkSkinPauseIcon = Resources.Load("unity_analytics_heatmaps_pause_dark") as Texture2D;
        Texture2D darkSkinRewindIcon = Resources.Load("unity_analytics_heatmaps_rwd_dark") as Texture2D;

        Texture2D lightSkinPlayIcon = Resources.Load("unity_analytics_heatmaps_play_light") as Texture2D;
        Texture2D lightSkinPauseIcon = Resources.Load("unity_analytics_heatmaps_pause_light") as Texture2D;
        Texture2D lightSkinRewindIcon = Resources.Load("unity_analytics_heatmaps_rwd_light") as Texture2D;

        GUIContent m_RestartContent;
        GUIContent m_PlayContent;
        GUIContent m_PauseContent;

        GUIContent m_ClipDropdown = new GUIContent("Video Clip", "Select the video clip Name");

        GUIContent m_MinRadiusContent = new GUIContent("   Radius");
        GUIContent m_DecayRateContent = new GUIContent("   Decay Rate");
        GUIContent m_FrameContent = new GUIContent("   Frame");

        GUIContent m_FunnelContent = new GUIContent("Funnel", "Display the player progression");
        public delegate void FunnelDataChangeHandler(Dictionary<string, HeatPoint[]> points);

        public HeatmapRendererInspector()
        {
            var playIcon = lightSkinPlayIcon;
            var pauseIcon = lightSkinPauseIcon;
            var rwdIcon = lightSkinRewindIcon;
            if (EditorGUIUtility.isProSkin)
            {
                playIcon = darkSkinPlayIcon;
                pauseIcon = darkSkinPauseIcon;
                rwdIcon = darkSkinRewindIcon;
            }

            m_RestartContent = new GUIContent(rwdIcon, "Back to Start");
            m_PlayContent = new GUIContent(playIcon, "Play");
            m_PauseContent = new GUIContent(pauseIcon, "Pause");

            HeatmapViewModel.HeatpointsUpdated += SetLimits;
            HeatmapViewModel.FinalFrameUpdated += HeatmapViewModel_FinalFrameUpdated;
        }

        void HeatmapViewModel_FinalFrameUpdated(int endFrame)
        {
            m_MaxTime = endFrame;
        }

        public static HeatmapRendererInspector Init(Heatmapper heatmapper, HeatmapDataProcessor processor)
        {
            var inspector = new HeatmapRendererInspector();
            inspector.m_Heatmapper = heatmapper;
            return inspector;
        }

        int ConvertNormalizedTimeToFrame (float t)
        {
            if (HeatmapViewModel.videoPlayer != null && HeatmapViewModel.videoPlayer.isPrepared)
                return Convert.ToInt32(t * (float)m_MaxTime);
            else
                return 0;
        }

        public void OnGUI()
        {
            if(m_MaxTime == -1)
            {
                m_MaxTime = HeatmapViewModel.endFrame;
            }

            using (new GUILayout.VerticalScope())
            {
                videoRenderer.videoMaterial = EditorGUILayout.ObjectField("Video Material", videoRenderer.videoMaterial, typeof(Material), true) as Material;

                var separators = HeatmapViewModel.GetSeparators();

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(m_ClipDropdown, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    m_ClipDropdownRect = EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("", EditorStyles.boldLabel);
                    EditorGUILayout.EndVertical();
                    var clipNameSep = separators["clipName"];
                    EditorGUI.BeginChangeCheck();
                    m_ClipNameIndex = EditorGUI.Popup(m_ClipDropdownRect, m_ClipNameIndex, clipNameSep.separatorValues.ToArray());

                    if (EditorGUI.EndChangeCheck())
                    {
                        clipNameSep.selectedValue = clipNameSep.separatorValues[m_ClipNameIndex];
                        HeatmapViewModel.UpdateSeparator("clipName", clipNameSep);
                    }
                }

                EditorGUILayout.LabelField("Render Options", GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                var _player = HeatmapViewModel.videoPlayer;
                //Disable render options if the videoplayer hasn't been assigned
                EditorGUI.BeginDisabledGroup(_player == null);
                using (new EditorGUILayout.VerticalScope())
                {
                    if (videoRenderer != null)
                    {
                        videoRenderer.minRadius = EditorGUILayout.Slider(m_MinRadiusContent, videoRenderer.minRadius, m_minRadius, m_maxRaidus);
                        videoRenderer.maxRadius = videoRenderer.minRadius;
                        videoRenderer.decay = EditorGUILayout.Slider(m_DecayRateContent, videoRenderer.decay, m_minDecay, m_maxDecay);

                        m_QualityIndex = EditorGUILayout.Popup("   Quality", m_QualityIndex, new string[]{"1X", "2X", "5X"});

                        if (_player != null)
                        {
                            int m_HeatMapWidthBase = (int)_player.clip.width / 5;
                            int m_HeatMapHeightBase = (int)_player.clip.height / 5;

                            switch (m_QualityIndex)
                            {
                                case 0:
                                    videoRenderer.heatMapWidth = m_HeatMapWidthBase;
                                    videoRenderer.heatMapHeight = m_HeatMapHeightBase;
                                    break; ;
                                case 1:
                                    videoRenderer.heatMapWidth = m_HeatMapWidthBase * 2;
                                    videoRenderer.heatMapHeight = m_HeatMapHeightBase * 2;
                                    break;
                                case 2:
                                    videoRenderer.heatMapWidth = m_HeatMapWidthBase * 5;
                                    videoRenderer.heatMapHeight = m_HeatMapHeightBase * 5;
                                    break;
                            }  
                        }

                        videoRenderer.numPoints = EditorGUILayout.IntSlider("   Points", videoRenderer.numPoints, 1, 2000);
                        videoRenderer.gradientSize = EditorGUILayout.IntSlider("   Gradient Fidelity", videoRenderer.gradientSize, 1, 256);


                        EditorGUI.BeginDisabledGroup(_player != null && _player.isPrepared == false);
                        if (_player != null)
                        {
                            var oldStartTime = ConvertNormalizedTimeToFrame(HeatmapViewModel.heatpointShowStartTime);
                            var temp = ConvertNormalizedTimeToFrame(HeatmapViewModel.heatpointShowStartTime);

                            if(_player.isPlaying)
                            {
                                EditorGUI.BeginDisabledGroup(true);
                                temp = EditorGUILayout.IntSlider(m_FrameContent, Convert.ToInt32(_player.frame), 1, m_MaxTime);
                                EditorGUI.EndDisabledGroup();
                            }
                            else 
                            {
                                temp = EditorGUILayout.IntSlider(m_FrameContent, (int)temp, 1, m_MaxTime);
                                if (temp != oldStartTime)
                                {
                                    _player.frame = temp;
                                }
                            }
                            if (temp != oldStartTime)
                            {
                                HeatmapViewModel.UpdateHeatpointShowTimes(GetNormalizedTime(temp), GetNormalizedTime(temp + 1));
                                m_Heatmapper.Repaint();
                            }
                        }

                        EditorGUILayout.Space();
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField("", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                            using (new EditorGUILayout.HorizontalScope())
                            {
                                if (GUILayout.Button(m_RestartContent))
                                {
                                    _player.Pause();
                                    _player.frame = 1;
                                    Restart();
                                }
                                GUIContent playButtonContent = _player != null && _player.isPlaying ? m_PauseContent : m_PlayContent;
                                if (GUILayout.Button(playButtonContent))
                                {
                                    if (_player.isPlaying)
                                    { 
                                        _player.Pause();
                                    }
                                    else
                                    {
                                        _player.Play();
                                    }
                                }
                            }
                        }
                        EditorGUI.EndDisabledGroup();
                        EditorGUILayout.Space();
                    }
                }
                EditorGUI.EndDisabledGroup();
                DisplayFunnelGui();

            }
        }

        public void SetLimits(Dictionary<string, HeatPoint[]> points)
        {
            float maxDensity = 0;

            foreach (KeyValuePair<string, HeatPoint[]> entry in points)
            {
                for (int a = 0; a < entry.Value.Length; a++)
                {
                    maxDensity = Mathf.Max(maxDensity, entry.Value[a].density);
                }
            }
        }

        float GetNormalizedTime(float frameNum) 
        {
            return frameNum / (float)m_MaxTime;
        }

        void Restart()
        {
            HeatmapViewModel.UpdateHeatpointShowTimes(GetNormalizedTime(1), GetNormalizedTime(2));
        }

        public void SetGameObject(GameObject go)
        {
            m_GameObject = go;
            if (m_GameObject != null)
            {
                videoRenderer = m_GameObject.GetComponent<VideoRenderer>();
            }
        }

        public void UpdateClipNameDropDown(string[] clipNames)
        {
            EditorGUI.BeginChangeCheck();
            m_ClipNameIndex = EditorGUI.Popup(m_ClipDropdownRect, m_ClipNameIndex, clipNames);

            if (EditorGUI.EndChangeCheck())
            {
                string m_clipName = clipNames[m_ClipNameIndex];

                // do something if clip changed
                PlayerPrefs.SetString(k_ClipName, m_clipName);
            }
        }

        void DisplayFunnelGui()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(m_FunnelContent, EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                using (new EditorGUILayout.VerticalScope())
                {
                    AnimationCurve m_Curve = CreateFunnel();
                    EditorGUI.CurveField(EditorGUILayout.GetControlRect(), m_Curve);
                    var _player = HeatmapViewModel.videoPlayer;
                    if (_player != null && _player.isPrepared)
                    {
                        EditorGUI.BeginDisabledGroup(true);
                        float _StandardTime = _player.frame / (float)_player.frameCount;
                        EditorGUILayout.LabelField(string.Format("Total number of players present: {0}", GetPresentPlayer(m_Curve, _StandardTime)), EditorStyles.label);
                        EditorGUI.EndDisabledGroup();
                    }
                }
            }
            if (HeatmapViewModel.GetHeatpointToSessionMapping() == null || HeatmapViewModel.GetHeatpointToSessionMapping().Count() == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("There is no data currently to display.", MessageType.Warning);
            }
        }

        AnimationCurve CreateFunnel()
        {
            AnimationCurve curve = new AnimationCurve();

            Dictionary<float, int> _CountTable = new Dictionary<float, int>();

            foreach (KeyValuePair<string, HeatPoint[]> kv in HeatmapViewModel.GetHeatpointToSessionMapping())
            {
                for (int i = 0; i < kv.Value.Length; i++)
                {
                    if (_CountTable.ContainsKey(kv.Value[i].time))
                    {
                        _CountTable[kv.Value[i].time] = _CountTable[kv.Value[i].time] + 1;
                    }
                    else
                    {
                        _CountTable.Add(kv.Value[i].time, 1);
                    }
                }
            }

            foreach (float m_time in _CountTable.Keys)
            {
                curve.AddKey(m_time, _CountTable[m_time]);
            }

            return curve;
        }

        int GetPresentPlayer(AnimationCurve curve, float time)
        {
            if (curve.keys.Length == 0)
            {
                return 0;
            }

            if (time > 1f || time < 0f)
            {
                return 0;
            }

            return (int)curve.Evaluate(time);
        }
    }
}
