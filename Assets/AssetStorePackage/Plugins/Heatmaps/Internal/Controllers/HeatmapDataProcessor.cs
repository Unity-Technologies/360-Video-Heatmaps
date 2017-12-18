/// <summary>
/// Manages the loading and processing of Heatmap data
/// </summary>
/// 
/// Heatmap data is loaded from GZip or text files and stored in the
/// HeatmapViewModel as a CustomRawData[]. From there, it is aggregated
/// into a HeatPoint[] before being sent to the renderer.
/// 
/// This class manages all that loading and processing, working out the
/// minimum work required to dynamically update the map.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityAnalytics;
using UnityEditor;
using System.Collections;
using System.Linq;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapDataProcessor
    {
        const string k_DataPathKey = "UnityAnalyticsHeatmapDataPathKey";
        const string k_SpaceKey = "UnityAnalyticsHeatmapAggregationSpace";
        const string k_KeyToTime = "UnityAnalyticsHeatmapAggregationTime";
        const string k_RotationKey = "UnityAnalyticsHeatmapAggregationRotation";
        const string k_SmoothSpaceKey = "UnityAnalyticsHeatmapAggregationAggregateSpace";
        const string k_SmoothTimeKey = "UnityAnalyticsHeatmapAggregationAggregateTime";
        const string k_SmoothRotationKey = "UnityAnalyticsHeatmapAggregationAggregateRotation";
        const string k_StartTimeKey = "UnityAnalyticsHeatmapAggregateStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapAggregateEndTime";


        const string k_SeparateSessionKey = "UnityAnalyticsHeatmapAggregationAggregateSessionIDs";
        const string k_SeparateDebugKey = "UnityAnalyticsHeatmapAggregationAggregateDebug";
        const string k_SeparatePlatformKey = "UnityAnalyticsHeatmapAggregationAggregatePlatform";
        const string k_SeparateCustomKey = "UnityAnalyticsHeatmapAggregationAggregateCustom";

        const string k_ArbitraryFieldsKey = "UnityAnalyticsHeatmapAggregationArbitraryFields";

        const float k_DefaultSpace = 10f;
        const float k_DefaultTime = 10f;
        const float k_DefaultRotation = 15f;


        public const int SMOOTH_VALUE = 0;
        public const int SMOOTH_NONE = 1;
        public const int SMOOTH_UNION = 2;

        public HeatmapViewModel m_ViewModel;
        public HeatmapAggregator m_Aggregator;

        bool m_DateChangeHasOccurred = true;

        private string _rawDataPath = "";
        public string m_RawDataPath
        {
            get{
                return _rawDataPath;
            }
            set{
                _rawDataPath = value;
                m_Aggregator.SetDataPath(_rawDataPath);
                EditorPrefs.SetString(k_DataPathKey, value);
            }
        }
        [SerializeField]
        string _startDate = "";
        public string m_StartDate
        {
            get {
                return _startDate;
            }
            set {
                string oldDate = _startDate;
                _startDate = value;
                if (_startDate != oldDate)
                {
                    EditorPrefs.SetString(Application.cloudProjectId + k_StartTimeKey, _startDate);
                    m_DateChangeHasOccurred = true;
                }
            }
        }
        [SerializeField]
        string _endDate = "";
        public string m_EndDate
        {
            get {
                return _endDate;
            }
            set {
                string oldDate = _endDate;
                _endDate = value;
                if (_endDate != oldDate)
                {
                    EditorPrefs.SetString(Application.cloudProjectId + k_EndTimeKey, _endDate);
                    m_DateChangeHasOccurred = true;
                }
            }
        }

        bool _separateSessions = false;
        public bool m_SeparateSessions{
            get{
                return _separateSessions;
            }
            set{
                _separateSessions = value;
                EditorPrefs.SetBool(k_SeparateSessionKey, _separateSessions);
            }
        }
        bool _separatePlatform;
        public bool m_SeparatePlatform
        {
            get {
                return _separatePlatform;
            }
            set {
                _separatePlatform = value;
                EditorPrefs.SetBool(k_SeparateSessionKey, _separatePlatform);
            }
        }
        bool _separateDebug;
        public bool m_SeparateDebug
        {
            get {
                return _separateDebug;
            }
            set {
                _separateDebug = value;
                EditorPrefs.SetBool(k_SeparateDebugKey, _separateDebug);
            }
        }
        bool _separateCustomField;
        public bool m_SeparateCustomField
        {
            get {
                return _separateCustomField;
            }
            set {
                _separateCustomField = value;
                EditorPrefs.SetBool(k_SeparateCustomKey, _separateCustomField);
            }
        }

        List<string> _separationFields = new List<string>();
        public List<string> m_SeparationFields
        {
            get {
                return _separationFields;
            }
            set {
                _separationFields = value;
                string currentArbitraryFieldsString = string.Join("|", _separationFields.ToArray());
                EditorPrefs.SetString(k_ArbitraryFieldsKey, currentArbitraryFieldsString);
            }
        }

        public delegate void AggregationHandler(string jsonPath);
        public delegate void PointHandler(HeatPoint[] heatData);


        static public AggregationMethod[] m_RemapOptionIds = new AggregationMethod[]{
            AggregationMethod.Increment,
            AggregationMethod.Cumulative,
            AggregationMethod.Average,
            AggregationMethod.Min,
            AggregationMethod.Max,
            AggregationMethod.First,
            AggregationMethod.Last,
            AggregationMethod.Percentile
        };

        public HeatmapDataProcessor()
        {
            m_Aggregator = new HeatmapAggregator(m_RawDataPath);
            HeatmapViewModel.StartEndDateUpdated += HeatmapViewModel_StartEndDateUpdated;
        }

        void HeatmapViewModel_StartEndDateUpdated(string startDate, string endDate)
        {
            m_StartDate = startDate;
            m_EndDate = endDate;
            RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
            ProcessAggregation();
        }

        public void RestoreSettings()
        {
            // Restore cached paths
            m_RawDataPath = EditorPrefs.GetString(k_DataPathKey);

            // Set dates based on today (should this be cached?)
            // Yes, yes it should.
            m_EndDate = EditorPrefs.HasKey(Application.cloudProjectId+k_EndTimeKey) ? EditorPrefs.GetString(Application.cloudProjectId+k_EndTimeKey) : String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow);
            m_StartDate = EditorPrefs.HasKey(Application.cloudProjectId + k_StartTimeKey) ? EditorPrefs.GetString(Application.cloudProjectId + k_StartTimeKey) : String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow.Subtract(new TimeSpan(5, 0, 0, 0)));

            // Restore list of arbitrary separation fields
            string loadedArbitraryFields = EditorPrefs.GetString(k_ArbitraryFieldsKey);
            string[] arbitraryFieldsList;
            if (string.IsNullOrEmpty(loadedArbitraryFields))
            {
                arbitraryFieldsList = new string[]{ };
            }
            else
            {
                arbitraryFieldsList = loadedArbitraryFields.Split('|');
            }
            m_SeparationFields = new List<string>(arbitraryFieldsList);
        }

        /// <summary>
        /// Fetch the files within the currently specified date range.
        /// </summary>
        public void Fetch()
        {
            RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
            ProcessAggregation();
        }

        void ProcessAggregation()
        {
            //TODO: Where settings for aggregation go
            DateTime start, end;
            try
            {
                start = DateTime.Parse(m_StartDate).ToUniversalTime();
            }
            catch
            {
                start = DateTime.Parse("2000-01-01").ToUniversalTime();
            }
            try
            {
                end = DateTime.Parse(m_EndDate).ToUniversalTime().Add(new TimeSpan(24,0,0));
            }
            catch
            {
                end = DateTime.UtcNow;
            }

            if (m_DateChangeHasOccurred || RawDataClient.GetInstance().m_ManifestInvalidated ||
                HeatmapViewModel.m_RawDataFileList == null || HeatmapViewModel.m_RawDataFileList.Count == 0)
            {
                RawDataClient.GetInstance().m_DataPath = m_RawDataPath;
                HeatmapViewModel.m_RawDataFileList = RawDataClient.GetInstance().GetFiles(
                    new UnityAnalyticsEventType[]{ UnityAnalyticsEventType.custom }, start, end);
                m_DateChangeHasOccurred = false;
                if (HeatmapViewModel.m_RawDataFileList.Count == 0)
                {
                    return;
                }
            }
            m_Aggregator.Process(HeatmapViewModel.m_RawDataFileList, start, end);
        }
    }
}

