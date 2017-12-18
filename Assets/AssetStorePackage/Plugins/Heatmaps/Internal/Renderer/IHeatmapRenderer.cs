/// <summary>
/// Interface for a Heat Map renderer
/// </summary>
/// If you choose to create your own custom renderer, we
/// recommend abiding by this interface.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityAnalytics360VideoHeatmap
{
    public interface IHeatmapRenderer
    {
        /// <summary>
        /// Sets the heatmap data
        /// </summary>
        /// <param name="data">An array of HeatPoints defining the map and its density.</param>
        /// <param name="maxDensity">Density value considered to be 100%.</param>
        void UpdatePointData(Dictionary<string, HeatPoint[]> data, float maxDensity);

        /// <summary>
        /// Updates the time limits.
        /// </summary>
        /// Allows the user to limit the display of data by time within the game.
        /// <param name="startTime">Start time.</param>
        /// <param name="endTime">End time.</param>
        void UpdateTimeLimits(float startTime, float endTime);

        /// <summary>
        /// Gating value to prevent the renderer from rendering.
        /// </summary>
        /// <value><c>true</c> if allow render; otherwise, <c>false</c>.</value>
        bool allowRender{ get; set; }

        /// <summary>
        /// The number of points currently displayed.
        /// </summary>
        /// <value>Count of currently displayed points</value>
        int currentPoints{ get; }

        /// <summary>
        /// The number of points in the current dataset.
        /// </summary>
        /// <value>Count of all points in the current set</value>
        int totalPoints{ get; }
    }
}
