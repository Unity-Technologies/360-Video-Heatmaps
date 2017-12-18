using UnityEngine;
using System.Collections;

namespace UnityAnalytics360VideoHeatmap
{
    public class HeatmapClipNameList : MonoBehaviour
    {
        public delegate void ChangeHandler(string[] array);

        public static void ListGroup(string[] array, ChangeHandler change)
        {
            if (array == null)
            {
                return;
            }

            change(array);
        }
    }
}
