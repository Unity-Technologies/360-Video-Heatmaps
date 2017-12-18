using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UnityAnalytics360VideoHeatmap
{
    public class AnalyticsTextFieldList
    {
        private static GUIContent s_AddFieldContent = new GUIContent("+", "Add field");
        private static GUILayoutOption s_ButtonWidth = GUILayout.MaxWidth(20f);


        public delegate void TextFieldListChangeHandler(List<string>  list);

        public static List<string> TextFieldList (List<string> m_ArbitraryFields, TextFieldListChangeHandler change)
        {
            
            if (m_ArbitraryFields.Count == 0)
            {
                m_ArbitraryFields.Add("Field name");
            }

            EditorGUI.BeginChangeCheck();

            for (var a = 0; a < m_ArbitraryFields.Count; a++)
            {
                using (new GUILayout.HorizontalScope())
                {
                    m_ArbitraryFields[a] = EditorGUILayout.TextField(m_ArbitraryFields[a]);

                    EditorGUI.BeginDisabledGroup(m_ArbitraryFields.Count == 1);
                    if (GUILayout.Button("-", s_ButtonWidth))
                    {
                        m_ArbitraryFields.RemoveAt(a);
                        break;
                    }   
                    EditorGUI.EndDisabledGroup();

                    if (a == m_ArbitraryFields.Count-1 && GUILayout.Button(s_AddFieldContent, s_ButtonWidth))
                    {
                        m_ArbitraryFields.Add("Field name");
                    }
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                change(m_ArbitraryFields);
            }

            return m_ArbitraryFields;
        }
    }
}

