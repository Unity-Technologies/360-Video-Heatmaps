/// <summary>
/// Inspector for the Aggregation portion of the Heatmapper.
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityAnalytics;
using System.Linq;

namespace UnityAnalytics360VideoHeatmap
{
    public class AggregationInspector
    {
        const string k_UseCustomDataPathKey = "UnityAnalyticsHeatmapUsePersistentDataPathKey";

        string m_DataPath = "./";

        HeatmapDataProcessor m_Processor;
        private GUIContent m_DataPathContent = new GUIContent("Input Path", "Where to retrieve data (defaults to Application.persistentDataPath");
        private GUIContent m_DatesContent = new GUIContent("Dates", "ISO-8601 datetimes (YYYY-MM-DD)");

        string m_StartDate = "";
        string m_EndDate = "";
        bool m_ValidDates = true;

        GUIStyle m_ValidDateStyle;
        GUIStyle m_InvalidDateStyle;

        const int k_FetchInitValue = 30;

        public AggregationInspector(HeatmapDataProcessor processor)
        {
            m_Processor = processor;
        }

        public static AggregationInspector Init(HeatmapDataProcessor processor)
        {
            return new AggregationInspector(processor);
        }

        public void OnEnable()
        {
            // Restore cached paths
            m_DataPath = m_Processor.m_RawDataPath;
            m_EndDate = m_Processor.m_EndDate;
            m_StartDate = m_Processor.m_StartDate;
        }

        public void OnGUI()
        {
            if (m_ValidDateStyle == null)
            {
                m_ValidDateStyle = new GUIStyle("button");
                m_InvalidDateStyle = new GUIStyle("button");
                m_InvalidDateStyle.normal.textColor = Color.red;
            }

            using (new GUILayout.VerticalScope())
            {
                EditorGUILayout.LabelField("Data", EditorStyles.boldLabel, GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(m_DataPathContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
                    m_DataPath = Application.dataPath;
                    UseCustomDataPathChange(true);
                    EditorGUILayout.SelectableLabel(m_DataPath + "/RawDataFolder", EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));
                }

                using (new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(m_DatesContent, GUILayout.Width(EditorGUIUtility.labelWidth - 4));
         
                    GUIStyle dateFieldStyle = m_ValidDates ? m_ValidDateStyle : m_InvalidDateStyle;
                    dateFieldStyle.padding = new RectOffset(0, 0, ((int)EditorGUIUtility.singleLineHeight / 3), 0);
                    m_StartDate = AnalyticsDatePicker.DatePicker(m_StartDate, dateFieldStyle, StartDateChange, DateFailure, DateValidationStart);
                    m_EndDate = AnalyticsDatePicker.DatePicker(m_EndDate, dateFieldStyle, EndDateChange, DateFailure, DateValidationEnd);
                }

                EditorGUILayout.Space();
            }
        }

        void UseCustomDataPathChange(bool value)
        {
            EditorPrefs.SetBool(k_UseCustomDataPathKey, value);
            DataPathChange(m_DataPath);
        }

        void DataPathChange(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                m_DataPath = Application.persistentDataPath;
            }
            m_Processor.m_RawDataPath = value;
        }

        void StartDateChange(string value)
        {
            m_ValidDates = true;
            m_Processor.m_StartDate = value;
            HeatmapViewModel.startDate = value;
        }

        void EndDateChange(string value)
        {
            m_ValidDates = true;
            m_Processor.m_EndDate = value;
            HeatmapViewModel.endDate = value;
        }

        void DateFailure()
        {
            m_ValidDates = false;
        }

        bool DateValidationStart(string value)
        {
            return DateValidation(value, m_EndDate);
        }

        bool DateValidationEnd(string value)
        {
            return DateValidation(m_StartDate, value);
        }

        bool DateValidation(string start, string end)
        {
            DateTime startDate;
            DateTime endDate;
            try
            {
                startDate = DateTime.Parse(start);
                endDate = DateTime.Parse(end);
            }
            catch
            {
                return false;
            }
            // Midnight tonight
            var today = DateTime.Today.AddDays(1).ToUniversalTime();
            return startDate < endDate && endDate <= today;
        }
    }
}