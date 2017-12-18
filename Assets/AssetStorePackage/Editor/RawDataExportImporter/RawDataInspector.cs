
using System;
using UnityEngine;
using UnityEditor;
using UnityAnalytics360VideoHeatmap;
using System.Collections.Generic;
using System.Collections;
using UnityAnalytics;
using System.Linq;
using System.Net;

namespace UnityAnalytics360VideoHeatmap
{
    public class RawDataInspector : EditorWindow
    {
        private static string k_FetchKey = "UnityAnalyticsRawDataGenFetchKey";
        private static string k_Installed = "UnityAnalyticsRawDataGenInstallKey";
        private static string k_StartDate = "UnityAnalyticsRawDataStartDateKey";
        private static string k_EndDate = "UnityAnalyticsRawDataEndDateKey";
        private static string k_DataPathKey = "UnityAnalyticsRawDataGenDataPath";
        private static string k_DeviceCountKey = "UnityAnalyticsRawDataGenDeviceCount";
        private static string k_SessionCountKey = "UnityAnalyticsRawDataGenSessionCount";
        private static string k_EventNamesKey = "UnityAnalyticsRawDataGenEventNames";
        private static string k_CustomEventsKey = "UnityAnalyticsRawDataGenCustomEvents";
        private static string k_EventCountKey = "UnityAnalyticsRawDataGenEventCount";

        private static string k_IncludeTimeKey = "UnityAnalyticsRawDataGenIncludeTime";

        private static string k_IncludeLevelKey = "UnityAnalyticsRawDataGenIncludeLevel";
        private static string k_MinLevel = "UnityAnalyticsRawDataGenMinLevel";
        private static string k_MaxLevel = "UnityAnalyticsRawDataGenMaxLevel";

        private static string k_IncludeFPSKey = "UnityAnalyticsRawDataGenIncludeFPS";
        private static string k_MinFPS = "UnityAnalyticsRawDataGenMinFPS";
        private static string k_MaxFPS = "UnityAnalyticsRawDataGenMaxFPS";

        private static string k_IncludeXKey = "UnityAnalyticsRawDataGenIncludeX";
        private static string k_MinX = "UnityAnalyticsRawDataGenMinX";
        private static string k_MaxX = "UnityAnalyticsRawDataGenMaxX";

        private static string k_IncludeYKey = "UnityAnalyticsRawDataGenIncludeY";
        private static string k_MinY = "UnityAnalyticsRawDataGenMinY";
        private static string k_MaxY = "UnityAnalyticsRawDataGenMaxY";

        private static string k_IncludeZKey = "UnityAnalyticsRawDataGenIncludeZ";
        private static string k_MinZ = "UnityAnalyticsRawDataGenMinZ";
        private static string k_MaxZ = "UnityAnalyticsRawDataGenMaxZ";

        private static string k_RotationKey = "UnityAnalyticsRawDataGenRotation";
        private static string k_MinRX = "UnityAnalyticsRawDataGenMinRX";
        private static string k_MaxRX = "UnityAnalyticsRawDataGenMaxRX";
        private static string k_MinRY = "UnityAnalyticsRawDataGenMinRY";
        private static string k_MaxRY = "UnityAnalyticsRawDataGenMaxRY";
        private static string k_MinRZ = "UnityAnalyticsRawDataGenMinRZ";
        private static string k_MaxRZ = "UnityAnalyticsRawDataGenMaxRZ";

        private static string k_MinDX = "UnityAnalyticsRawDataGenMinDX";
        private static string k_MaxDX = "UnityAnalyticsRawDataGenMaxDX";
        private static string k_MinDY = "UnityAnalyticsRawDataGenMinDY";
        private static string k_MaxDY = "UnityAnalyticsRawDataGenMaxDY";
        private static string k_MinDZ = "UnityAnalyticsRawDataGenMinDZ";
        private static string k_MaxDZ = "UnityAnalyticsRawDataGenMaxDZ";

        private static Color s_BoxColor = new Color(.9f, .9f, .9f);

        private GUIContent m_AddEventContent = new GUIContent("+ Event Name", "Events to be randomly added into the created data.");

        private GUIContent m_UpidContent = new GUIContent("UPID", "Copy the Unity Project ID from Services > Settings or the 'Editing Project' page of your project dashboard");
        private GUIContent m_SecretKeyContent = new GUIContent("API Key", "Copy the key from the 'Editing Project' page of your project dashboard");
        private GUIContent m_StartDateContent = new GUIContent("Start Date (YYYY-MM-DD)", "Start date as ISO-8601 datetime");
        private GUIContent m_EndDateContent = new GUIContent("End Date (YYYY-MM-DD)", "End date as ISO-8601 datetime");

        private GUIContent m_ContinueJobContent = new GUIContent(">", "Continue job");
        private GUIContent m_DownloadJobContent = new GUIContent("Download", "Download job files");
        private GUIContent m_GetJobsContent = new GUIContent("Get Jobs", "Load the manifest of job files");
        private GUIContent m_DownloadAllContent = new GUIContent("Download All", "Download the files for all jobs");

        private GUIContent m_FailedContent;
        private GUIContent m_CompleteContent;
        private GUIContent m_RunningContent;

        private static int defaultEventCount = 100;
        private static int defaultDeviceCount = 5;
        private static int defaultSessionCount = 10;
        private static float defaultMinAngle = 0f;
        private static float defaultMaxAngle = 360f;
        private static float defaultMinSpace = -100f;
        private static float defaultMaxSpace = 100f;
        private static int defaultRotational = 0;
        private static int defaultMinLevel = 1;
        private static int defaultMaxLevel = 99;
        private static float defaultMinFPS = 1f;
        private static float defaultMaxFPS = 99f;

        public const string headers = "ts\tappid\ttype\tuserid\tsessionid\tremote_ip\tplatform\tsdk_ver\tdebug_device\tuser_agent\tsubmit_time\tname\tcustom_params\n";

        [MenuItem("Window/Unity Analytics/360 Video Heatmaps/Raw Data")]
        static void RawDataInspectorMenuOption()
        {
            EditorWindow.GetWindow(typeof(RawDataInspector));
        }

        string m_DataPath = "";
        bool m_ValidManifest = false;

        int m_EventCount = defaultEventCount;
        int m_DeviceCount = defaultDeviceCount;
        int m_SessionCount = defaultSessionCount;

        string m_AppId = "";
        string m_SecretKey = "";
        string m_StartDate = "";
        string m_EndDate = "";

        List<RawDataReport> m_Jobs = null;
        bool[] m_JobFoldouts;

        int m_DataSource = 0;
        static int FETCH = 0;
        List<string> m_EventNames = new List<string> { };

        bool m_IncludeTime = true;

        bool m_IncludeX = true;
        float m_MinX = defaultMinSpace;
        float m_MaxX = defaultMaxSpace;

        bool m_IncludeY = true;
        float m_MinY = defaultMinSpace;
        float m_MaxY = defaultMaxSpace;

        bool m_IncludeZ = true;
        float m_MinZ = defaultMinSpace;
        float m_MaxZ = defaultMaxSpace;

        // Flag for rotation vs destination
        int m_Rotational = defaultRotational;

        float m_MinRX = defaultMinAngle;
        float m_MaxRX = defaultMaxAngle;
        float m_MinRY = defaultMinAngle;
        float m_MaxRY = defaultMaxAngle;
        float m_MinRZ = defaultMinAngle;
        float m_MaxRZ = defaultMaxAngle;

        float m_MinDX = defaultMinSpace;
        float m_MaxDX = defaultMaxSpace;
        float m_MinDY = defaultMinSpace;
        float m_MaxDY = defaultMaxSpace;
        float m_MinDZ = defaultMinSpace;
        float m_MaxDZ = defaultMaxSpace;

        bool m_IncludeLevel = false;
        int m_MinLevel = defaultMinLevel;
        int m_MaxLevel = defaultMaxLevel;

        bool m_IncludeFPS = false;
        float m_MinFPS = defaultMinFPS;
        float m_MaxFPS = defaultMaxFPS;

        Vector2 m_ScrollPosition;

        RawDataClient m_RawDataClient;

        void OnEnable()
        {
            if (m_RawDataClient == null)
            {
                Texture2D failedIcon = Resources.Load("unity_analytics_heatmaps_failed") as Texture2D;
                Texture2D completeIcon = Resources.Load("unity_analytics_heatmaps_success") as Texture2D;
                Texture2D runningIcon = Resources.Load("unity_analytics_heatmaps_running") as Texture2D;

                m_RawDataClient = RawDataClient.GetInstance();
                if (m_DataSource == FETCH && !m_ValidManifest)
                {
                    m_RawDataClient.GetJobs(GetJobsCompletionHandler);
                    m_ValidManifest = true;
                }
                titleContent = new GUIContent("Raw Data");

                m_FailedContent = new GUIContent(failedIcon, "Status: Failed");
                m_CompleteContent = new GUIContent(completeIcon, "Status: Completed");
                m_RunningContent = new GUIContent(runningIcon, "Status: Running");
                return;
            }
        }

        void OnFocus()
        {
            if (EditorPrefs.GetBool(k_Installed))
            {
                RestoreValues();
            }
            else
            {
                SetInitValues();
            }
        }

        void OnGUI()
        {
            EditorPrefs.SetString(k_DataPathKey, m_DataPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Reset"))
                {
                    if (EditorUtility.DisplayDialog("Resetting to factory defaults", "Are you sure?", "Reset", "Cancel"))
                    {
                        SetInitValues();
                    }
                }
                if (GUILayout.Button("Open Folder"))
                {
                    EditorUtility.RevealInFinder(m_DataPath + "/RawDataFolder");
                }
            }

            //output path
            m_DataPath = Application.dataPath;

            using (new GUILayout.VerticalScope("box"))
            {
                EditorGUILayout.LabelField("Output path", EditorStyles.boldLabel);
                EditorGUILayout.SelectableLabel(m_DataPath + "/RawDataFolder");
            }

            if (m_DataSource == FETCH)
            {
                OnGUIFetchView();
            }
        }

        void OnGUIFetchView()
        {
            if (Event.current.type == EventType.Layout)
            {
                m_RawDataClient.TestFilesAreLocal(m_Jobs);
            }

            using (new GUILayout.VerticalScope("box"))
            {
                string oldKey = m_SecretKey;
                m_AppId = EditorGUILayout.TextField(m_UpidContent, m_AppId);
                RestoreAppId();
                m_SecretKey = EditorGUILayout.TextField(m_SecretKeyContent, m_SecretKey);

                m_RawDataClient.m_DataPath = m_DataPath;
                m_RawDataClient.m_AppId = m_AppId;
                m_RawDataClient.m_SecretKey = m_SecretKey;

                if (oldKey != m_SecretKey && !string.IsNullOrEmpty(m_SecretKey))
                {
                    EditorPrefs.SetString(k_FetchKey, m_SecretKey);
                }
            }

            using (new GUILayout.VerticalScope("box"))
            {
                GUILayout.Label("New Job", EditorStyles.boldLabel);

                var oldStartDate = m_StartDate;
                var oldEndDate = m_EndDate;
                m_StartDate = EditorGUILayout.TextField(m_StartDateContent, m_StartDate);
                m_EndDate = EditorGUILayout.TextField(m_EndDateContent, m_EndDate);
                if (oldStartDate != m_StartDate || oldEndDate != m_EndDate)
                {
                    EditorPrefs.SetString(k_StartDate, m_StartDate);
                    EditorPrefs.SetString(k_EndDate, m_EndDate);
                }
                if (GUILayout.Button("Create Job"))
                {
                    RawDataReport report = null;
                    try
                    {
                        DateTime startDate = DateTime.Parse(m_StartDate).ToUniversalTime();
                        DateTime endDate = DateTime.Parse(m_EndDate).ToUniversalTime();
                        report = m_RawDataClient.CreateJob("custom", startDate, endDate);
                    }
                    catch (Exception ex)
                    {
                        string exText = "Unknown exception.";
                        if (ex is FormatException)
                        {
                            exText = "Date formats appear to be incorrect. Start and End Dates must be ISO-8601 format (YYYY-MM-DD).";
                        }
                        else if (ex is WebException)
                        {
                            WebException webEx = ex as WebException;
                            exText = webEx.Message;
                        }
                        EditorUtility.DisplayDialog("Can't create job", exText, "OK");
                    }
                    if (m_Jobs == null)
                    {
                        m_Jobs = new List<RawDataReport>();
                    }
                    if (report != null)
                    {
                        m_Jobs.Add(report);
                    }
                    m_JobFoldouts = m_Jobs.Select(fb => false).ToArray();
                }
            }

            using (new GUILayout.VerticalScope("box"))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(m_GetJobsContent))
                    {
                        m_RawDataClient.GetJobs(GetJobsCompletionHandler);
                    }
                    if (GUILayout.Button(m_DownloadAllContent))
                    {
                        m_RawDataClient.DownloadAll(m_Jobs);
                    }
                }
                m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);
                if (m_Jobs != null)
                {
                    for (int a = m_Jobs.Count - 1; a > -1; a--)
                    {
                        var job = m_Jobs[a];
                        string start = String.Format("{0:yyyy-MM-dd}", job.request.startDate);
                        string end = String.Format("{0:yyyy-MM-dd}", job.request.endDate);
                        string shortStart = String.Format("{0:MM-dd}", job.request.startDate);
                        string shortEnd = String.Format("{0:MM-dd}", job.request.endDate);
                        string created = String.Format("{0:yyyy-MM-dd hh:mm:ss}", job.createdAt);
                        string type = job.request.dataset;

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            float windowWidth = EditorGUIUtility.currentViewWidth;
                            float foldoutWidth = windowWidth * .5f;
                            float downloadButtonWidth = 75f;
                            float continueButtonWidth = 25f;
                            float downloadedWidth = downloadButtonWidth;
                            float statusWidth = 20f;

                            GUIContent foldoutContent = new GUIContent(type + ": " + shortStart + " to " + shortEnd, start + " — " + end + "\n" + job.id);
                            Rect pos = GUILayoutUtility.GetRect(foldoutContent, "foldout");
                            Rect rect = new Rect(pos.x, pos.y, foldoutWidth, 20f);
                            m_JobFoldouts[a] = EditorGUI.Foldout(
                                rect,
                                m_JobFoldouts[a],
                                foldoutContent,
                                true
                                );

                            var statusContent = m_CompleteContent;
                            switch (job.status)
                            {
                                case RawDataReport.Failed:
                                    statusContent = m_FailedContent;
                                    break;
                                case RawDataReport.Running:
                                    statusContent = m_RunningContent;
                                    break;
                            }
                            GUILayout.Label(statusContent, GUILayout.Width(statusWidth));

                            if (job.status == RawDataReport.Completed)
                            {
                                if (job.isLocal)
                                {
                                    GUILayout.Label("Downloaded", GUILayout.Width(downloadedWidth));
                                }
                                else if (job.result != null && job.result.size == 0)
                                {
                                    GUILayout.Label("No Data", GUILayout.Width(downloadedWidth));
                                }
                                else if (GUILayout.Button(m_DownloadJobContent, GUILayout.Width(downloadButtonWidth)))
                                {
                                    m_RawDataClient.Download(job);
                                }
                                if (GUILayout.Button(m_ContinueJobContent, GUILayout.Width(continueButtonWidth)))
                                {
                                    RawDataReport report = m_RawDataClient.ContinueFromJob(job);
                                    m_Jobs.Add(report);
                                    m_JobFoldouts = m_Jobs.Select(fb => false).ToArray();
                                }
                            }
                        }
                        if (m_JobFoldouts[a])
                        {
                            Color defaultColor = GUI.color;
                            GUI.backgroundColor = s_BoxColor;
                            using (new GUILayout.VerticalScope("box"))
                            {
                                GUILayout.Label("ID: " + job.id);
                                GUILayout.Label("Created: " + created);
                                GUILayout.Label("Duration: " + (job.duration / 1000) + " seconds");
                                if (job.result != null)
                                {
                                    GUILayout.Label("# Events: " + job.result.eventCount);
                                    GUILayout.Label("# Bytes: " + job.result.size);
                                    GUILayout.Label("# Files: " + job.result.fileList.Count);
                                    GUILayout.Label("Partial day: " + job.result.intraDay);
                                }
                            }
                            GUI.backgroundColor = defaultColor;
                        }
                    }

                    if (m_Jobs.Count == 0)
                    {
                        GUILayout.Label("No jobs found", EditorStyles.boldLabel);
                    }
                }
                else
                {
                    GUILayout.Label("No data yet", EditorStyles.boldLabel);
                }
                GUILayout.Space(10f);
                EditorGUILayout.EndScrollView();
            }



            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Purge"))
                {
                    PurgeData();
                }
                if (GUILayout.Button("Dashboard"))
                {
                    Application.OpenURL(m_RawDataClient.DashboardPath);
                }
                if (GUILayout.Button("Project Config"))
                {
                    Application.OpenURL(m_RawDataClient.ConfigPath);
                }
            }
        }

        private void GetJobsCompletionHandler(bool success, List<RawDataReport> list, string reason = "")
        {
            m_Jobs = list;
            m_JobFoldouts = m_Jobs.Select(fb => false).ToArray();
        }

        void CreateHeadersFile()
        {
            SaveFile(headers, "custom_headers.gz", true);
        }

        void CreateManifestFile(List<RawDataReport> list)
        {
            var manifest = m_RawDataClient.GenerateManifest(list);
            SaveFile(manifest, "manifest.json", false);
        }

        bool IncludeSet(ref bool value, string label, string key, bool force = false)
        {
            string tooltip = force ? label + " must be included" : null;
            var content = new GUIContent(label, tooltip);
            EditorGUI.BeginDisabledGroup(force);
            value = EditorGUILayout.Toggle(content, value);
            EditorGUI.EndDisabledGroup();
            EditorPrefs.SetBool(key, value);
            return value;
        }

        void DrawFloatRange(ref float min, ref float max, string minKey, string maxKey)
        {
            float oldMin = min;
            min = EditorGUILayout.FloatField(min);
            if (oldMin != min)
            {
                EditorPrefs.SetFloat(minKey, min);
            }
            float oldMax = max;
            max = EditorGUILayout.FloatField(max);
            if (oldMax != max)
            {
                EditorPrefs.SetFloat(maxKey, max);
            }
        }

        void DrawIntRange(ref int min, ref int max, string minKey, string maxKey)
        {
            int oldMin = min;
            min = EditorGUILayout.IntField(min);
            if (oldMin != min)
            {
                EditorPrefs.SetInt(minKey, min);
            }
            int oldMax = max;
            max = EditorGUILayout.IntField(max);
            if (oldMax != max)
            {
                EditorPrefs.SetInt(maxKey, max);
            }
        }

        int SaveCustomFile(string data, double firstDate)
        {
            return SaveFile(data, firstDate + "_custom.gz", true);
        }

        int SaveFile(string data, string fileName, bool compress)
        {
            string savePath = System.IO.Path.Combine(GetSavePath(), "RawDataFolder");
            // Create the save path if necessary
            if (!System.IO.Directory.Exists(savePath))
            {
                System.IO.Directory.CreateDirectory(savePath);
            }
            string outputFileName = fileName;
            string path = System.IO.Path.Combine(savePath, outputFileName);
            int size = 0;
            if (compress)
            {
                IonicGZip.CompressAndSave(path, data);
            }
            else
            {
                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(path))
                {
                    file.Write(data);
                }
            }
            System.IO.FileInfo fileInfo = new System.IO.FileInfo(path);
            size = (int)fileInfo.Length;
            return size;
        }

        string GetSavePath()
        {
            return m_DataPath;
        }

        public void PurgeData()
        {
            if (EditorUtility.DisplayDialog("Destroy local data?", "You are about to delete your local heatmaps data cache, meaning you'll have to reload from the server (or regenerate from this tool). Are you sure?", "Purge", "Cancel"))
            {
                string savePath = System.IO.Path.Combine(GetSavePath(), "RawDataFolder");
                if (System.IO.Directory.Exists(savePath))
                {
                    System.IO.Directory.Delete(savePath, true);
                }
            }
        }

        protected void SetInitValues()
        {
            m_DataPath = "";
            m_IncludeTime = true;
            m_IncludeX = m_IncludeY = m_IncludeZ = true;
            m_IncludeLevel = m_IncludeFPS = false;
            m_Rotational = defaultRotational;
            m_MinX = m_MinY = m_MinZ = m_MinDX = m_MinDY = m_MinDZ = defaultMinSpace;
            m_MaxX = m_MaxY = m_MaxZ = defaultMaxSpace;
            m_MinRX = m_MinRY = m_MinRZ = defaultMinAngle;
            m_MaxRX = m_MaxRY = m_MaxRZ = defaultMaxAngle;
            m_MinLevel = defaultMinLevel;
            m_MaxLevel = defaultMaxLevel;
            m_MinFPS = defaultMinFPS;
            m_MaxFPS = defaultMaxFPS;
            string[] eventsList = new string[] { "PlayerPosition" };
            m_EventNames = new List<string>(eventsList);

            EditorPrefs.SetFloat(k_MinX, m_MinX);
            EditorPrefs.SetFloat(k_MinY, m_MinY);
            EditorPrefs.SetFloat(k_MinZ, m_MinZ);
            EditorPrefs.SetFloat(k_MinDX, m_MinDX);
            EditorPrefs.SetFloat(k_MinDY, m_MinDY);
            EditorPrefs.SetFloat(k_MinDZ, m_MinDZ);
            EditorPrefs.SetFloat(k_MaxX, m_MaxX);
            EditorPrefs.SetFloat(k_MaxY, m_MaxY);
            EditorPrefs.SetFloat(k_MaxZ, m_MaxZ);
            EditorPrefs.SetFloat(k_MaxDX, m_MaxDX);
            EditorPrefs.SetFloat(k_MaxDY, m_MaxDY);
            EditorPrefs.SetFloat(k_MaxDZ, m_MaxDZ);

            EditorPrefs.SetFloat(k_MinRX, m_MinRX);
            EditorPrefs.SetFloat(k_MinRY, m_MinRY);
            EditorPrefs.SetFloat(k_MinRZ, m_MinRZ);
            EditorPrefs.SetFloat(k_MaxRX, m_MaxRX);
            EditorPrefs.SetFloat(k_MaxRY, m_MaxRY);
            EditorPrefs.SetFloat(k_MaxRZ, m_MaxRZ);

            EditorPrefs.SetInt(k_MinLevel, m_MinLevel);
            EditorPrefs.SetInt(k_MaxLevel, m_MaxLevel);
            EditorPrefs.SetFloat(k_MinFPS, m_MinFPS);
            EditorPrefs.SetFloat(k_MaxFPS, m_MaxFPS);
            EditorPrefs.SetString(k_EventNamesKey, eventsList[0]);
            EditorPrefs.SetString(k_CustomEventsKey, eventsList[0]);
            EditorPrefs.SetInt(k_DeviceCountKey, m_DeviceCount);
            EditorPrefs.SetInt(k_SessionCountKey, m_SessionCount);
            EditorPrefs.SetBool(k_Installed, true);
        }

        protected void RestoreAppId()
        {
            if (string.IsNullOrEmpty(m_AppId) && !string.IsNullOrEmpty(Application.cloudProjectId))
            {
                m_AppId = Application.cloudProjectId;
            }
        }

        protected void RestoreValues()
        {
            RestoreAppId();

            m_SecretKey = EditorPrefs.GetString(k_FetchKey, m_SecretKey);
            m_StartDate = EditorPrefs.GetString(k_StartDate, m_StartDate);
            m_EndDate = EditorPrefs.GetString(k_EndDate, m_EndDate);
            m_IncludeTime = EditorPrefs.GetBool(k_IncludeTimeKey, m_IncludeTime);
            m_IncludeX = EditorPrefs.GetBool(k_IncludeXKey, m_IncludeX);
            m_MinX = EditorPrefs.GetFloat(k_MinX, m_MinX);
            m_MaxX = EditorPrefs.GetFloat(k_MaxX, m_MaxX);
            m_IncludeY = EditorPrefs.GetBool(k_IncludeYKey, m_IncludeY);
            m_MinY = EditorPrefs.GetFloat(k_MinY, m_MinY);
            m_MaxY = EditorPrefs.GetFloat(k_MaxY, m_MaxY);
            m_IncludeZ = EditorPrefs.GetBool(k_IncludeZKey, m_IncludeZ);
            m_MinZ = EditorPrefs.GetFloat(k_MinZ, m_MinZ);
            m_MaxZ = EditorPrefs.GetFloat(k_MaxZ, m_MaxZ);

            m_Rotational = EditorPrefs.GetInt(k_RotationKey, m_Rotational);
            m_MinRX = EditorPrefs.GetFloat(k_MinRX, m_MinRX);
            m_MaxRX = EditorPrefs.GetFloat(k_MaxRX, m_MaxRX);
            m_MinRY = EditorPrefs.GetFloat(k_MinRY, m_MinRY);
            m_MaxRY = EditorPrefs.GetFloat(k_MaxRY, m_MaxRY);
            m_MinRZ = EditorPrefs.GetFloat(k_MinRZ, m_MinRZ);
            m_MaxRZ = EditorPrefs.GetFloat(k_MaxRZ, m_MaxRZ);

            m_MinDX = EditorPrefs.GetFloat(k_MinDX, m_MinDX);
            m_MaxDX = EditorPrefs.GetFloat(k_MaxDX, m_MaxDX);
            m_MinDY = EditorPrefs.GetFloat(k_MinDY, m_MinDY);
            m_MaxDY = EditorPrefs.GetFloat(k_MaxDY, m_MaxDY);
            m_MinDZ = EditorPrefs.GetFloat(k_MinDZ, m_MinDZ);
            m_MaxDZ = EditorPrefs.GetFloat(k_MaxDZ, m_MaxDZ);

            m_IncludeLevel = EditorPrefs.GetBool(k_IncludeLevelKey, m_IncludeLevel);
            m_MinLevel = EditorPrefs.GetInt(k_MinLevel, m_MinLevel);
            m_MaxLevel = EditorPrefs.GetInt(k_MaxLevel, m_MaxLevel);

            m_IncludeFPS = EditorPrefs.GetBool(k_IncludeFPSKey, m_IncludeFPS);
            m_MinFPS = EditorPrefs.GetFloat(k_MinFPS, m_MinFPS);
            m_MaxFPS = EditorPrefs.GetFloat(k_MaxFPS, m_MaxFPS);

            m_EventCount = EditorPrefs.GetInt(k_EventCountKey, m_EventCount);
            string loadedEventNames = EditorPrefs.GetString(k_EventNamesKey);
            string[] eventNamesList;
            if (string.IsNullOrEmpty(loadedEventNames))
            {
                eventNamesList = new string[] { };
            }
            else
            {
                eventNamesList = loadedEventNames.Split('|');
            }
            m_EventNames = new List<string>(eventNamesList);
        }

        void ViewEventNames()
        {
            string oldEventsString = string.Join("|", m_EventNames.ToArray());
            if (GUILayout.Button(m_AddEventContent))
            {
                m_EventNames.Add("Event name");
            }
            for (var a = 0; a < m_EventNames.Count; a++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("-", GUILayout.MaxWidth(20f)))
                    {
                        m_EventNames.RemoveAt(a);
                        break;
                    }
                    m_EventNames[a] = EditorGUILayout.TextField(m_EventNames[a]);
                }
            }
            string currentEventsString = string.Join("|", m_EventNames.ToArray());

            if (oldEventsString != currentEventsString)
            {
                EditorPrefs.SetString(k_EventNamesKey, currentEventsString);
            }
        }
    }
}

