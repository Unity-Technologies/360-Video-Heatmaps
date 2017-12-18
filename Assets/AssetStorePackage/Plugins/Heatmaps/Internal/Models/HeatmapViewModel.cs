/// <summary>
/// Heatmap view model.
/// </summary>

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Video;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapViewModel
    {
        static HeatmapViewModel _instance;
        public static HeatmapViewModel instance
        {
            get {
                if (_instance == null)
                {
                    _instance = new HeatmapViewModel();
                }
                else
                {
                    Restore();
                }
                return _instance;
            }
        }

        /// <summary>
        /// The list of raw data files
        /// </summary>
        public static List<string> m_RawDataFileList = new List<string>();

        ///<summary>
        /// The list of all the event names
        /// </summary>
        static public List<string> eventNames = new List<string>();

        ///<summary>
        /// A mapping of a session's separators to event names
        /// </summary>
        //Dictionary of a Dictionary of HeatmapsSeparators
        static Dictionary<string, object> separatorsToEvents = new Dictionary<string, object>();
        ///<summary>
        /// A mapping of a session's point data to event names
        /// </summary>
        //Dictionary of a Dictionary of a list of Heatpoints
        static Dictionary<string, object> heatpointsToEvents = new Dictionary<string, object>();

        static Dictionary<string, SeparatorData> selectedSeparators = new Dictionary<string, SeparatorData>();

        static Dictionary<string, HeatPoint[]> currentHeatpointsBySessions = new Dictionary<string, HeatPoint[]>();

        public static float heatpointShowStartTime = 0.0f;
        public static float heatpointShowEndTime = 0.01f;
        [SerializeField]
        private static VideoPlayer _videoPlayer;
        public static VideoPlayer videoPlayer
        {
            get { return _videoPlayer; }
            set
            {
                if (value != null &&_videoPlayer == null || _videoPlayer != value)
                {
                    _videoPlayer = value;
                    if (_videoPlayer.isPrepared == false)
                    {
                        _videoPlayer.Prepare();
                        _videoPlayer.prepareCompleted += _videoPlayer_PrepareCompleted;
                    }
                    else 
                    {
                        _videoPlayer_PrepareCompleted(_videoPlayer);
                    }

                }
            }
        }

        public static int endFrame = 1;

        static string _startDate;
        public static string startDate 
        {
            get {
                return _startDate;
            }
            set {
                if(_startDate != value)
                {
                    _startDate = value;
                    StartEndTimeUpdate();
                }
            }
        }
        static string _endDate;
        public static string endDate
        {
            get
            {
                return _endDate;
            }
            set
            {
                if (_endDate != value)
                {
                    _endDate = value;
                    StartEndTimeUpdate();
                }
            }
        }

        public delegate void UpdateSeparatorsHandler();
        public static event UpdateSeparatorsHandler SeparatorsUpdated;

        public delegate void UpdateEventNamesHandler();
        public static event UpdateEventNamesHandler EventNamesUpdated;

        public delegate void UpdateHeatpointsHandler(Dictionary<string, HeatPoint[]> currentHeatpointsBySessions);
        public static event UpdateHeatpointsHandler HeatpointsUpdated;

        public delegate void UpdateStartEndDateHandler(string startDate, string endDate);
        public static event UpdateStartEndDateHandler StartEndDateUpdated;

        public delegate void UpdateCurrentHeatpointsUpdatedHandler(List<HeatPoint[]> newHeatpoints);
        public static event UpdateCurrentHeatpointsUpdatedHandler CurrentHeatpointsUpdated;

        public delegate void UpdateVideoPlayerHandler(VideoPlayer newPlayer);
        public static event UpdateVideoPlayerHandler VideoPlayerUpdated;

        public delegate void UpdateVideoFinalFrameHandler(int endFrame);
        public static event UpdateVideoFinalFrameHandler FinalFrameUpdated;

        public HeatmapViewModel()
        {
            if(!HeatmapAggregator.IsEventHandlerRegistered_EventData(HeatmapAggregator_UpdateEventDataHandler))
            {
                HeatmapAggregator.UpdateEventData += HeatmapAggregator_UpdateEventDataHandler;
            }
            if (!HeatmapAggregator.IsEventHandlerRegistered_SepData(HeatmapAggregator_UpdateSeparators))
            {
                HeatmapAggregator.UpdateSeparators += HeatmapAggregator_UpdateSeparators;
            }
            if (!HeatmapAggregator.IsEventHandlerRegistered_EventNameData(HeatmapAggregator_UpdateEventNames))
            {
                HeatmapAggregator.UpdateEventNames += HeatmapAggregator_UpdateEventNames;
            }

            //initialize
            //add separators
            InitalizeSeperators();
            //get start date/time
            endDate = EditorPrefs.HasKey(Application.cloudProjectId + k_EndTimeKey) ? EditorPrefs.GetString(Application.cloudProjectId + k_EndTimeKey) : String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow);
            startDate = EditorPrefs.HasKey(Application.cloudProjectId + k_StartTimeKey) ? EditorPrefs.GetString(Application.cloudProjectId + k_StartTimeKey) : String.Format("{0:yyyy-MM-dd}", DateTime.UtcNow.Subtract(new TimeSpan(5, 0, 0, 0)));
        }

        static void Restore ()
        {
            if(videoPlayer.isPrepared == false)
            {
                videoPlayer.Prepare();
                videoPlayer.prepareCompleted += _videoPlayer_PrepareCompleted;
            }
            else{
                _videoPlayer_PrepareCompleted(videoPlayer);
            }
        }

        static void _videoPlayer_PrepareCompleted(VideoPlayer source)
        {
            _videoPlayer.prepareCompleted -= _videoPlayer_PrepareCompleted;
            if (VideoPlayerUpdated != null)
                VideoPlayerUpdated(_videoPlayer);
            
            endFrame = (int)_videoPlayer.frameCount;
            if (FinalFrameUpdated != null)
                FinalFrameUpdated(endFrame);

            _videoPlayer.frame = 0;
        }

        public void ManualOverride ()
        {
            if (StartEndDateUpdated != null)
                StartEndDateUpdated(startDate, endDate);
            InitalizeSeperators();
        }

        void HeatmapAggregator_UpdateSeparators(Dictionary<string, object> newSeparatorsToEvents)
        {
            separatorsToEvents = newSeparatorsToEvents;
            if (SeparatorsUpdated != null)
                SeparatorsUpdated();
        }

        void HeatmapAggregator_UpdateEventNames(List<string> newEventNames)
        {
            eventNames = newEventNames;
            if (EventNamesUpdated != null)
                EventNamesUpdated();
        }

        void HeatmapAggregator_UpdateEventDataHandler(Dictionary<string, object> newHeatpointsToEvents)
        {
            heatpointsToEvents = newHeatpointsToEvents;

            InitalizeClipSeparator();

            if (HeatpointsUpdated != null)
                HeatpointsUpdated(currentHeatpointsBySessions);
        }

        static void StartEndTimeUpdate()
        {
            if (StartEndDateUpdated != null)
                StartEndDateUpdated(startDate, endDate);
        }

        public static void UpdateHeatpointShowTimes(float startTime, float endTime)
        {
            heatpointShowStartTime = startTime;
            heatpointShowEndTime = endTime;

            UpdateCurrentHeatpoints();
        }

        //For 360 video purposes, this is hardcoded in
        public static string[] GetEventNames()
        {
            return new string[] { "Heatmap.PlayerLook" };
        }

        public Dictionary<string, List<HeatmapSeparators>> GetSeparatorsForEvent(string eventName)
        {
            return (separatorsToEvents[eventName] as Dictionary<string, List<HeatmapSeparators>>);
        }

        void InitalizeSeperators()
        {
            SeparatorData sep = new SeparatorData();
            sep.isActive = true;
            sep.selectedValue = "Heatmap.PlayerLook";
            sep.separatorValues = new List<string>();
            sep.separatorValues.Add("Heatmap.PlayerLook");
            sep.separatorKey = "eventName";
            UpdateSeparator("eventName", sep);
            sep = new SeparatorData();
            sep.isActive = true;
            sep.separatorKey = "clipName";
            sep.separatorValues = new List<string>();
            if (separatorsToEvents.ContainsKey("Heatmap.PlayerLook"))
            {
                
                Dictionary<string, List<HeatmapSeparators>> eventSeps = separatorsToEvents["Heatmap.PlayerLook"] as Dictionary<string, List<HeatmapSeparators>>;
                foreach (KeyValuePair<string, List<HeatmapSeparators>> currSepList in eventSeps)
                {
                    sep.separatorValues.AddRange(currSepList.Value.Where(x => !sep.separatorValues.Contains(x.clipName)).Select(x => x.clipName).ToList());
                }
            }
            else
            {
                sep.separatorValues.Add("Unnamed");
            }
            sep.separatorKey = sep.separatorValues[0];
            UpdateSeparator("clipName", sep);
        }

        void InitalizeClipSeparator()
        {
            var sep = new SeparatorData();
            sep.isActive = true;
            sep.separatorKey = "clipName";
            sep.separatorValues = new List<string>();
            if (separatorsToEvents.ContainsKey("Heatmap.PlayerLook"))
            {
                Dictionary<string, List<HeatmapSeparators>> eventSeps = separatorsToEvents["Heatmap.PlayerLook"] as Dictionary<string, List<HeatmapSeparators>>;
                foreach (KeyValuePair<string, List<HeatmapSeparators>> currSepList in eventSeps)
                {
                    sep.separatorValues.AddRange(currSepList.Value.Where(x => !sep.separatorValues.Contains(x.clipName)).Select(x => x.clipName).ToList());
                }
            }
            else
            {
                sep.separatorValues.Add("Unnamed");
            }
            sep.separatorKey = sep.separatorValues[0];
            UpdateSeparator("clipName", sep);
        }

        public static Dictionary<string, SeparatorData> GetSeparators()
        {
            return selectedSeparators;
        }

        public static void UpdateSeparator(string sep, SeparatorData value)
        {
            if (selectedSeparators.ContainsKey(sep))
            {
                selectedSeparators[sep] = value;
            }
            else
            {
                selectedSeparators.Add(sep, value);
            }
            UpdateCurrentHeatpoints();
            //update appropriate settings objects
        }

        static void UpdateCurrentHeatpoints()
        {
            if (selectedSeparators.ContainsKey("eventName") && heatpointsToEvents.ContainsKey(selectedSeparators["eventName"].selectedValue))
            {
                var currEventHeatpoints = heatpointsToEvents[selectedSeparators["eventName"].selectedValue] as Dictionary<string, List<HeatPoint>>;
                var currEventSeparators = separatorsToEvents[selectedSeparators["eventName"].selectedValue] as Dictionary<string, List<HeatmapSeparators>>;
                List<string> visibleSessionIds = currEventSeparators.Where(currEventSepKV =>
                {
                    return currEventSepKV.Value.Any(sessSep =>
                    {
                        return sessSep.clipName == selectedSeparators["clipName"].selectedValue;
                    });
                }).Select(kv => kv.Key).ToList();
                currentHeatpointsBySessions = currEventHeatpoints.Where(sessKV => visibleSessionIds.Contains(sessKV.Key)).Select(sessKV => new { sessionId = sessKV.Key, points = sessKV.Value }).ToDictionary(item => item.sessionId, item =>
                {
                    //sorting hacky spot to put it
                    var array = item.points.ToArray(); Array.Sort(array, delegate (HeatPoint x, HeatPoint y)
                    {
                        return x.time.CompareTo(y.time);
                    }); return array;
                }) as Dictionary<string, HeatPoint[]>;

                if (CurrentHeatpointsUpdated != null)
                    CurrentHeatpointsUpdated(GetCurrentHeatpoints());
            }
        }

        public List<string> GetCurrentSessions()
        {
            return currentHeatpointsBySessions.Keys.ToList();
        }

        static List<HeatPoint[]> GetCurrentHeatpoints()
        {
            List<HeatPoint[]> retList = new List<HeatPoint[]>();
            foreach (HeatPoint[] heatpointArr in currentHeatpointsBySessions.Values.ToList())
            {
                List<HeatPoint> soonToBeRetArray = new List<HeatPoint>();
                bool pointAddedForSession = false;
                for (int a = 0; a < heatpointArr.Length; a++)
                {
                    // FILTER FOR TIME & POSITION
                    var pt = heatpointArr[a];
                    if (FilterPoint(pt))
                    {
                        soonToBeRetArray.Add(pt);
                        pointAddedForSession = true;
                    }
                }
                if (pointAddedForSession == false)
                {
                    soonToBeRetArray.Add(FilterForInterpolation(heatpointArr));
                }
                retList.Add(soonToBeRetArray.ToArray());
            }
            return retList;
        }

        static bool FilterPoint(HeatPoint pt)
        {
            if (pt.time < heatpointShowStartTime || pt.time > heatpointShowEndTime)
            {
                return false;
            }
            else
                return true;
        }

        static HeatPoint FilterForInterpolation(HeatPoint[] pts)
        {
            //iterate through all the points, and take the two with the least difference before/after the bounds of timestamps
            HeatPoint[] closestPoints = new HeatPoint[2];

            if(videoPlayer != null)
            {
                for (int i = 1; i < pts.Length; i++)
                {
                    if (pts[i].time > heatpointShowStartTime)
                    {
                        closestPoints[0] = pts[i - 1];
                        closestPoints[1] = pts[i];
                        break;
                    }
                }

                //special handling for VideoPlayer and film
                int startFrame = (int)(heatpointShowStartTime * (int)videoPlayer.frameCount);

                int dataStartFrame = (int)(closestPoints[0].time * (int)videoPlayer.frameCount);
                int dataEndFrame = (int)(closestPoints[1].time * (int)videoPlayer.frameCount);
                int dataNumFrames = dataEndFrame - dataStartFrame;
                if (dataNumFrames == 0)
                {
                    dataNumFrames = 1;
                }

                float T = ((float)startFrame - (float)dataStartFrame) / (float)dataNumFrames;
                HeatPoint interpolatedPoint = new HeatPoint();

                interpolatedPoint.rotation = Vector3.Slerp(closestPoints[0].rotation, closestPoints[1].rotation, T);
                interpolatedPoint.density = (int)Mathf.Lerp(closestPoints[0].density, closestPoints[1].density, T);
                return interpolatedPoint;
            }
            else 
            {
                return new HeatPoint();
            }
        }

        public static Dictionary<string, HeatPoint[]> GetHeatpointToSessionMapping ()
        {
            return currentHeatpointsBySessions;
        }


        const string k_StartTimeKey = "UnityAnalyticsHeatmapAggregateStartTime";
        const string k_EndTimeKey = "UnityAnalyticsHeatmapAggregateEndTime";
    }



    public struct SeparatorData
    {
        public string separatorKey;
        public bool isActive;
        public List<string> separatorValues;
        public string selectedValue;
    }
}

