/// <summary>
/// Heat point.
/// </summary>
/// This struct is used by the rendering system to define each individual
/// heat map datum.

using System;
using UnityEngine;

namespace UnityAnalytics360VideoHeatmap
{
    public struct HeatPoint : IEquatable<HeatPoint>
    {
        public Vector3 position;
        public Vector3 rotation;
        public int density;
        public float time;

        public bool Equals(HeatPoint other)
        {
            if(position == other.position
               && rotation == other.rotation
               && density == other.density
               && time == other.time)
            {
                return true;
            }
            return false;
        }
    }
}
