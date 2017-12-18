using System;
using UnityEngine;

namespace UnityAnalytics360VideoHeatmap
{
    public class GradientUtils
    {
        public static Color PickGradientColor(Gradient gradient, float percent)
        {
            return (gradient == null) ? Color.magenta : gradient.Evaluate(percent);
        }

        public static bool CompareGradients(Gradient gradient, Gradient otherGradient)
        {
            if (gradient == null || otherGradient == null)
            {
                return false;
            }

            // Compare the lengths before checking actual colors and alpha components
            if (gradient.colorKeys.Length != otherGradient.colorKeys.Length || gradient.alphaKeys.Length != otherGradient.alphaKeys.Length) {
                return false;
            }

            // Compare all the colors
            for (int a = 0; a < gradient.colorKeys.Length; a++)
            {
                // Test if the color and alpha is the same
                GradientColorKey key = gradient.colorKeys[a];
                GradientColorKey otherKey = otherGradient.colorKeys[a];
                if (key.color != otherKey.color || key.time != otherKey.time) {
                    return false;
                }
            }

            // Compare all the alphas
            for (int a = 0; a < gradient.alphaKeys.Length; a++)
            {
                // Test if the color and alpha is the same
                GradientAlphaKey key = gradient.alphaKeys[a];
                GradientAlphaKey otherKey = otherGradient.alphaKeys[a];
                if (key.alpha != otherKey.alpha || key.time != otherKey.time)
                {
                    return false;
                }
            }

            // They're the same
            return true;
        }
    }
}

