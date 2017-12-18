/// <summary>
/// Handles aggregation of raw data into heatmap data.
/// </summary>
/// 
/// There are three related fields in Processing that need to be understood:
/// smoothOn, aggregateOn, and groupOn.
/// 
/// smoothOn provides a list of parameters that will get smoothed, and by how much.
/// If, for example, "x" is in the list, and the "x" value is 1, then every x between
/// -.5 and .5 will be "smoothed to 0.
/// smoothOn is always a subset of aggregateOn.
/// 
/// aggregateOn provides a list of parameters to meld into a single point if all the
/// values are the same after smoothing. For example, if aggregateOn contains the list
/// "x", "y", "z" and "t", then every time we encounter a point where (after smoothing)
/// x = 25, y = 15, z = 5, and t = 22, we will consider that to be the exact same point,
/// and aggregation can occur (so the density increases).
/// 
/// groupOn allows us to create lists out of the results. A list will ALWAYS be created
/// out of unique event names, but if groupOn contains "userID" and "platform", then
/// unique lists will also be created from these fields.
/// groupOn is always a subset of aggregateOn.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityAnalytics;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapAggregator
    {
        public bool m_Verbose = false;

        string[] headerKeys = new string[]{
            "name",
            "submit_time",
            "custom_params",
            "userid",
            "sessionid",
            "platform",
            "debug_device"
        };

        string m_DataPath = "";

        //Dictionary of a Dictionary of HeatmapsSeparators
        Dictionary<string, object> separatorsToEvents = new Dictionary<string, object>();
        //Dictionary of a Dictionary of a list of Heatpoints
        Dictionary<string, object> heatpointsToEvents = new Dictionary<string, object>();

        public delegate void UpdateSeparatorsHandler(Dictionary<string, object> separatorsToEvents);
        public static event UpdateSeparatorsHandler UpdateSeparators;

        public delegate void UpdateEventDataHandler(Dictionary<string, object> heatpointsToEvents);
        public static event UpdateEventDataHandler UpdateEventData;

        public delegate void UpdateEventNamesHandler(List<string> eventNames);
        public static event UpdateEventNamesHandler UpdateEventNames;

        public HeatmapAggregator(string dataPath)
        {
            SetDataPath(dataPath);
        }

        public static bool IsEventHandlerRegistered_EventData(UpdateEventDataHandler prospectiveHandler)
        {
            if (UpdateEventData != null)
            {
                foreach (UpdateEventDataHandler existingHandler in UpdateEventData.GetInvocationList())
                {
                    if (existingHandler == prospectiveHandler)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsEventHandlerRegistered_SepData(UpdateSeparatorsHandler prospectiveHandler)
        {
            if (UpdateSeparators != null)
            {
                foreach (UpdateSeparatorsHandler existingHandler in UpdateSeparators.GetInvocationList())
                {
                    if (existingHandler == prospectiveHandler)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool IsEventHandlerRegistered_EventNameData(UpdateEventNamesHandler prospectiveHandler)
        {
            if (UpdateEventNames != null)
            {
                foreach (UpdateEventNamesHandler existingHandler in UpdateEventNames.GetInvocationList())
                {
                    if (existingHandler == prospectiveHandler)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Sets the data path.
        /// </summary>
        /// <param name="dataPath">The location on the host machine from which to retrieve data.</param>
        public void SetDataPath(string dataPath)
        {
            m_DataPath = dataPath;
        }

        /// <summary>
        /// Process the specified inputFiles, using the other specified parameters.
        /// </summary>
        /// <param name="inputFiles">A list of one or more raw data text files.</param>
        /// <param name="startDate">Any timestamp prior to this ISO 8601 date will be trimmed.</param>
        /// <param name="endDate">Any timestamp after to this ISO 8601 date will be trimmed.</param>
        /// <param name="aggregateOn">A list of properties on which to specify point uniqueness.</param>
        /// <param name="smoothOn">A dictionary of properties that are smoothable, along with their smoothing values. <b>Must be a subset of aggregateOn.</b></param>
        /// <param name="groupOn">A list of properties on which to group resulting lists (supports arbitrary data, plus 'eventName', 'userID', 'sessionID', 'platform', and 'debug').</param>
        /// <param name="remapDensityToField">If not blank, remaps density (aka color) to the value of the field.</param>
        /// <param name="aggregationMethod">Determines the calculation with which multiple points aggregate (default is Increment).</param>
        /// <param name="percentile">For use with the AggregationMethod.Percentile, a value between 0-1 indicating the percentile to use.</param>
        public void Process(List<string> inputFiles, DateTime startDate, DateTime endDate)
        {
            var headers = GetHeaders();
            if (headers["name"] == -1 || headers["submit_time"] == -1 || headers["custom_params"] == -1)
            {
                //Debug.LogWarning ("No headers found. The likeliest cause of this is that you have no custom_headers.gz file. Perhaps you need to download a raw data job?");
            }
            else
            {
                LoadStream(headers, inputFiles, startDate, endDate);
            }
        }

        internal Dictionary<string, int> GetHeaders()
        {
            var retv = new Dictionary<string, int>();
            string path = System.IO.Path.Combine(m_DataPath, "RawDataFolder");
            path = System.IO.Path.Combine(path, "custom_headers.gz");
            string tsv = IonicGZip.DecompressFile(path);
            tsv = tsv.Replace("\n", "");
            List<string> rowData = new List<string>(tsv.Split('\t'));
            for (var a = 0; a < headerKeys.Length; a++)
            {
                retv.Add(headerKeys[a], rowData.IndexOf(headerKeys[a]));
            }
            return retv;
        }

        private HeatmapSeparators CreateSeparatorForRow(Dictionary<string, int> headers, List<string> rowData)
        {
            HeatmapSeparators retObj = new HeatmapSeparators();

            int nameIndex = headers["name"];
            int paramsIndex = headers["custom_params"];
            int userIdIndex = headers["userid"];
            int sessionIdIndex = headers["sessionid"];
            int platformIndex = headers["platform"];
            int isDebugIndex = headers["debug_device"];

            string userId = rowData[userIdIndex];
            string sessionId = rowData[sessionIdIndex];
            string eventName = rowData[nameIndex];
            string paramsData = rowData[paramsIndex];
            string platform = rowData[platformIndex];
            bool isDebug = bool.Parse(rowData[isDebugIndex]);

            retObj.sessionId = sessionId;
            retObj.userId = userId;
            retObj.platform = platform;
            retObj.eventName = eventName;
            retObj.debugDevice = isDebug;
            retObj.clipName = "";
            var paramsDict = (MiniJSON.Json.Deserialize(paramsData) as Dictionary<string, object>);
            if(paramsDict.ContainsKey("clipName"))
            {
                retObj.clipName = paramsDict["clipName"].ToString();
            }
            return retObj;
        }

        Dictionary<string, string> SanitizeCustomParams (Dictionary<string, string> paramsData) 
        {
            var defaultKeys = new List<string>(){ "x", "y", "z", "t", "rx", "ry", "rz", "dx", "dy", "dz", "z" };
            return paramsData.Where(pair => !defaultKeys.Contains(pair.Key))
                                 .ToDictionary(pair => pair.Key,
                                               pair => pair.Value);
        }

        bool TryUpdateEventNames (string eventName)
        {
            bool retBool = false;
            if (!separatorsToEvents.ContainsKey(eventName))
            {
                separatorsToEvents.Add(eventName, new Dictionary<string, List<HeatmapSeparators>>());
                //A new event name has been added!
                retBool = true;
            }
            if(!heatpointsToEvents.ContainsKey(eventName)) {
                heatpointsToEvents.Add(eventName, new Dictionary<string, List<HeatPoint>>());
                retBool = true;
            }
            return retBool;
        }

        internal void LoadStream(Dictionary<string, int> headers,
            List<string> inputFiles, 
            DateTime startDate, DateTime endDate)
        {
            bool eventNamesUpdated = false;
            bool eventDataUpdated = false;
            bool eventSeparatorsUpdated = false;

            separatorsToEvents = new Dictionary<string, object>();
            heatpointsToEvents = new Dictionary<string, object>();

            foreach (string path in inputFiles)
            {
                if (!System.IO.File.Exists(path))
                {
                    //Debug.LogWarningFormat("File {0} not found.", path);
                    return;
                }

                string tsv = IonicGZip.DecompressFile(path);

                string[] rows = tsv.Split('\n');

                // Define indices
                int nameIndex = headers["name"];

                int submitTimeIndex = headers["submit_time"];
                int paramsIndex = headers["custom_params"];
                int userIdIndex = headers["userid"];
                int sessionIdIndex = headers["sessionid"];
                int platformIndex = headers["platform"];
                int isDebugIndex = headers["debug_device"];

                for (int a = 0; a < rows.Length; a++)
                {
                    List<string> rowData = new List<string>(rows[a].Split('\t'));
                    if (rowData.Count < 6)
                    {
                        // Re-enable this log if you want to see empty lines
                        ////Debug.Log ("No data in line...skipping");
                        continue;
                    }



                    string userId = rowData[userIdIndex];
                    string sessionId = rowData[sessionIdIndex];
                    string eventName = rowData[nameIndex];
                    string paramsData = rowData[paramsIndex];
                    double unixTimeStamp = double.Parse(rowData[submitTimeIndex]);
                    DateTime rowDate = DateTimeUtils.s_Epoch.AddMilliseconds(unixTimeStamp);


                    string platform = rowData[platformIndex];

                    // Pass on rows outside any date trimming
                    if (rowDate < startDate || rowDate > endDate)
                    {
                        //Debug.Log("passing on a row");
                        continue;
                    }

                    Dictionary<string, object> datum = MiniJSON.Json.Deserialize(paramsData) as Dictionary<string, object>;
                    // If no x/y, this isn't a Heatmap Event. Pass.
                    if (datum == null || (!datum.ContainsKey("x") || !datum.ContainsKey("y")))
                    {
                        // Re-enable this log line if you want to be see events that aren't valid for heatmapping
                        //Debug.Log ("Unable to find x/y in: Skipping...");
                        continue;
                    }

                    if (TryUpdateEventNames(eventName))
                        eventNamesUpdated = true;

                    var heatmapSeparator = CreateSeparatorForRow(headers, rowData);
                    var sepDict = (separatorsToEvents[eventName] as Dictionary<string, List<HeatmapSeparators>>);
                    if (!sepDict.ContainsKey(sessionId))
                    {
                        sepDict.Add(sessionId, new List<HeatmapSeparators>());
                        eventSeparatorsUpdated = true;
                    }

                    if (!sepDict[sessionId].Contains(heatmapSeparator))
                    {
                        sepDict[sessionId].Add(heatmapSeparator);
                        eventSeparatorsUpdated = true;
                    }

                    var heatpointDict = (heatpointsToEvents[eventName] as Dictionary<string, List<HeatPoint>>);
                    if (!heatpointDict.ContainsKey(sessionId))
                    {
                        heatpointDict.Add(sessionId, new List<HeatPoint>());
                        eventDataUpdated = true;
                    }
                    eventDataUpdated = true;

                    //make a heatpoint
                    HeatPoint heatpoint = new HeatPoint();

                    float x = 0, y = 0, z = 0, t = 0, rx = 0, ry = 0, rz = 0;
                    foreach (KeyValuePair<string, object> pointKV in datum)
                    {
                        switch (pointKV.Key)
                        {
                            case "x":
                                x = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "y":
                                y = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "z":
                                z = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "t":
                                t = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "rx":
                                rx = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "ry":
                                ry = (float)Convert.ToDouble(pointKV.Value);
                                break;
                            case "rz":
                                rz = (float)Convert.ToDouble(pointKV.Value);
                                break;
                        }
                    }
                    heatpoint.density = 1;
                    heatpoint.position = new Vector3(x, y, z);
                    heatpoint.rotation = new Vector3(rx, ry, rz);
                    heatpoint.time = t;
                    if (heatpointDict[sessionId].Contains(heatpoint))
                    {
                        var indexOfHeatpoint = heatpointDict[sessionId].IndexOf(heatpoint);
                        var temp = heatpointDict[sessionId].ElementAt(indexOfHeatpoint);
                        heatpointDict[sessionId].RemoveAt(indexOfHeatpoint);
                        temp.density += heatpoint.density;
                        heatpointDict[sessionId].Insert(indexOfHeatpoint, temp);
                        eventDataUpdated = true;
                    }
                    else
                    {
                        heatpointDict[sessionId].Add(heatpoint);
                        eventDataUpdated = true;
                    }
                }
            }

            if(eventNamesUpdated)
            {
                if (UpdateEventNames != null)
                    UpdateEventNames(separatorsToEvents.Keys.ToList());
            }
            if(eventSeparatorsUpdated)
            {
                if (UpdateSeparators != null)
                    UpdateSeparators(separatorsToEvents);
            }
            if(eventDataUpdated)
            {
                if (UpdateEventData != null)
                    UpdateEventData(heatpointsToEvents);
            }
        }
    }

    public struct HeatmapSeparators : IEquatable<HeatmapSeparators> {
        public string sessionId;
        public string userId;
        public string platform;
        public bool debugDevice;
        public string eventName;
        public string clipName;

        public bool Equals(HeatmapSeparators other)
        {
            if (sessionId == other.sessionId 
                && userId == other.userId 
                && platform == other.platform 
                && debugDevice == other.debugDevice 
                && eventName == other.eventName 
                && clipName == other.clipName)
            {
                return true;
            }
            return false;
        }
    }
}
