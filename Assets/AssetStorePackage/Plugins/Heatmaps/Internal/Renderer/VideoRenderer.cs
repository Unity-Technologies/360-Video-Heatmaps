using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Analytics;
using UnityAnalytics360VideoHeatmap;

[RequireComponent(typeof(Hotspots))]
[ExecuteInEditMode]
public class VideoRenderer : MonoBehaviour, IHeatmapRenderer {
    Dictionary<string, HeatPoint[]> m_Data;

    float m_StartTime = 0f;
    float m_EndTime = 1.0f;

    [Range(1, 50)]
    public float minRadius = 35;
    [Range(1, 50)]
    public float maxRadius;

    [Range(0, 1)]
    public float decay;

    Hotspots hotspots = null;

    // Default, but allow user to define
    // governs fidelity of heatmap render - needs to be same asepct ratio as video
    public int heatMapWidth
    {
        get { return hotspots.heatMapWidth; }
        set { hotspots.heatMapWidth = value; }
    }
    public int heatMapHeight
    {
        get { return hotspots.heatMapHeight; }
        set { hotspots.heatMapHeight = value; }
    }

    //max number of points that can be displayed
    public int numPoints
    {
        get { return hotspots.numPoints; }
        set { hotspots.numPoints = value; }
    }

    //can be private
    //public Texture2D gradient;
    //number of colors in gradient - more is smoother, less you get more banding
    public int gradientSize
    {
        get { return hotspots.gradientSize; }
        set { hotspots.gradientSize = value; }
    }

    //dev needs to set to be same as one in video
    public Material videoMaterial
    {
        get { return hotspots.videoMaterial; }
        set 
        {
            if(hotspots.videoMaterial == null || hotspots.videoMaterial != value)
            {
                hotspots.videoMaterial = value;
                hotspots.Init(true);
            }
        }
    }

    // can be exposed a la heatmap UI
    public Gradient _gradient 
    {
        get { return hotspots._gradient; }
        set { hotspots._gradient = value; }
    }

    void OnEnable() {
        allowRender = true;
        hotspots = gameObject.GetComponent<Hotspots>();
        HeatmapViewModel.CurrentHeatpointsUpdated += RenderHeatmap;
    }

    /// <summary>
    /// Sets the heatmap data
    /// </summary>
    /// <param name="data">An array of HeatPoints defining the map and its density.</param>
    /// <param name="maxDensity">Density value considered to be 100%.</param>
    public void UpdatePointData(Dictionary<string, HeatPoint[]> data, float maxDensity)
    {
        m_Data = data;
    }

    /// <summary>
    /// Updates the time limits.
    /// </summary>
    /// Allows the user to limit the display of data by time within the game.
    /// <param name="startTime">Start time.</param>
    /// <param name="endTime">End time.</param>
    public void UpdateTimeLimits(float startTime, float endTime)
    {
        if (m_StartTime != startTime || m_EndTime != endTime)
        {
            m_StartTime = startTime;
            m_EndTime = endTime;
        }
    }

    /// <summary>
    /// Renders the heat map.
    /// </summary>
    void RenderHeatmap(List<HeatPoint[]> listOfHeatpoints) 
    {
        List<HeatPoint> otherPoints = new List<HeatPoint>();

        foreach(HeatPoint[] heatpointArr in listOfHeatpoints)
        {
            for (int a = 0; a < heatpointArr.Length; a++)
            {
                // FILTER FOR TIME & POSITION
                var pt = heatpointArr[a];
                otherPoints.Add(pt);
            }
        }

        HeatPoint[] filteredData = otherPoints.ToArray();

        hotspots.minRadius = minRadius;
        hotspots.maxRadius = maxRadius;
        hotspots.decay = decay;
        hotspots.SetPoints(filteredData);

        totalPoints = listOfHeatpoints.Count;
    }

    int GetTotalPoints ()
    {
        int retTotal = 0;

        foreach (KeyValuePair<string, HeatPoint[]> entry in m_Data) 
        {
            retTotal += entry.Value.Length;
        }

        return retTotal;
    }

    bool FilterPoint(HeatPoint pt)
    {
        if (pt.time < m_StartTime || pt.time > m_EndTime)
        {
            return false;
        }
        else
            return true;
    }

    /// <summary>
    /// Gating value to prevent the renderer from rendering.
    /// </summary>
    /// <value><c>true</c> if allow render; otherwise, <c>false</c>.</value>
    public bool allowRender { get; set; }

    /// <summary>
    /// The number of points currently displayed.
    /// </summary>
    /// <value>Count of currently displayed points</value>
    public int currentPoints { get; set; }

    /// <summary>
    /// The number of points in the current dataset.
    /// </summary>
    /// <value>Count of all points in the current set</value>
    public int totalPoints { get; set; }

    bool hasData()
    {
        return m_Data != null && m_Data.Count > 0;
    }
}
