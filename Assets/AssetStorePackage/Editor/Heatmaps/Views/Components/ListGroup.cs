/// <summary>
/// Visual component for managing a list of popup lists.
/// </summary>

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityAnalytics360VideoHeatmap
{
    public class AnalyticsListGroup
    {

        public delegate void ChangeHandler(List<int> value);


        public static List<int> ListGroup(List<int>value, List<List<string>> lists, ChangeHandler change)
        {
            ////Debug.Log(value.ToString());
            ////Debug.Log(lists.ToString());
            if (lists == null || value == null)
            {
                return null;
            }

            EditorGUI.BeginChangeCheck();

            if (value.Count > 1)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Data Set Options", GUILayout.Width(EditorGUIUtility.labelWidth - 4));

                    using (new EditorGUILayout.VerticalScope())
                    {
                        for (int a = 0; a < value.Count; a++)
                        {
                            ////Debug.Log(value[a]);
                            ////Debug.Log(lists);
                            if (lists[a].Contains("Heatmap.PlayerLook"))
                            {
                                continue;
                            }

                            var listArray = lists[a].ToArray();
                            value[a] = EditorGUILayout.Popup(value[a], listArray);
                        }
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                change(value);
            }
            return value;
        }
    }
}

