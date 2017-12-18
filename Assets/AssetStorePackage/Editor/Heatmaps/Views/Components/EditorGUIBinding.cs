/// <summary>
/// EditorGUI Components decorated with change, fail and validation handlers
/// </summary>
/// 
/// Decorates EditorGUILayout controls, adding three callback handlers
/// * change -     Required. Calls back whenever the value changes. If a validation handler is included and validation fails,
///                change is not called.
/// * failure -    Optional. Called if validation fails.
/// * validation - Optional. If provided, will prevent calls to change when validation fails. If failure is provided, will call
///                that handler instead.

using System;
using UnityEngine;
using UnityEditor;

namespace UnityAnalytics360VideoHeatmap
{
    public class EditorGUIBinding
    {
        #region FloatField
        public delegate void FloatChangeHandler(float value);
        public delegate void FloatFailureHandler();
        public delegate bool FloatValidationHandler(float value);

        private static void FloatApplyChangeHandlers(float value, FloatChangeHandler change, FloatFailureHandler failure, FloatValidationHandler validate)
        {
            bool validated = (validate == null) ? true : validate(value);
            if (validated)
            {
                change(value);
            }
            else if (!validated && failure != null)
            {
                failure();
            }
        }

        public static float FloatField(float value,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(value, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static float FloatField(float value, GUIStyle style,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static float FloatField(string label, float value,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(label, value, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static float FloatField(string label, float value, GUIStyle style,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(label, value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static float FloatField(GUIContent label, float value,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(label, value, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static float FloatField(GUIContent label, float value, GUIStyle style,
            FloatChangeHandler change, FloatFailureHandler failure = null, FloatValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.FloatField(label, value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                FloatApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        #endregion

        #region Popup
        public delegate void IntChangeHandler(int value);
        public delegate void IntFailureHandler();
        public delegate bool IntValidationHandler(int value);

        private static void IntApplyChangeHandlers(int value, IntChangeHandler change, IntFailureHandler failure, IntValidationHandler validate)
        {
            bool validated = (validate == null) ? true : validate(value);
            if (validated)
            {
                change(value);
            }
            else if (!validated && failure != null)
            {
                failure();
            }
        }

        public static int Popup(int selectedIndex, string[] displayedOptions,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(selectedIndex, displayedOptions, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(int selectedIndex, string[] displayedOptions, GUIStyle style,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(selectedIndex, displayedOptions, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(int selectedIndex, GUIContent[] displayedOptions,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(selectedIndex, displayedOptions, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(int selectedIndex, GUIContent[] displayedOptions, GUIStyle style,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(selectedIndex, displayedOptions, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(string label, int selectedIndex, string[] displayedOptions,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(string label, int selectedIndex, string[] displayedOptions, GUIStyle style,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }
        public static int Popup(GUIContent label, int selectedIndex, GUIContent[] displayedOptions, GUIStyle style,
            IntChangeHandler change, IntFailureHandler failure = null, IntValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            selectedIndex = EditorGUILayout.Popup(label, selectedIndex, displayedOptions, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                IntApplyChangeHandlers(selectedIndex, change, failure, validate);
            }
            return selectedIndex;
        }

        #endregion

        #region TextField
        public delegate void TextFieldChangeHandler(string text);
        public delegate void TextFieldFailureHandler();
        public delegate bool TextFieldValidationHandler(string text);

        private static void TextFieldApplyChangeHandlers(string text, TextFieldChangeHandler change, TextFieldFailureHandler failure, TextFieldValidationHandler validate)
        {
            bool validated = (validate == null) ? true : validate(text);
            if (validated)
            {
                change(text);
            }
            else if (!validated && failure != null)
            {
                failure();
            }
        }

        public static string TextField(string text,
            TextFieldChangeHandler change,
            TextFieldFailureHandler failure = null,
            TextFieldValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(text, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        public static string TextField(string text, GUIStyle style, 
            TextFieldChangeHandler change, 
            TextFieldFailureHandler failure = null, 
            TextFieldValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(text, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        public static string TextField(string label, string text,
            TextFieldChangeHandler change,
            TextFieldFailureHandler failure = null,
            TextFieldValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(label, text, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        public static string TextField(string label, string text, GUIStyle style,
            TextFieldChangeHandler change,
            TextFieldFailureHandler failure = null,
            TextFieldValidationHandler validate = null, 
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(label, text, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        public static string TextField(GUIContent label, string text,
            TextFieldChangeHandler change,
            TextFieldFailureHandler failure = null,
            TextFieldValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(label, text, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        public static string TextField(GUIContent label, string text, GUIStyle style,
            TextFieldChangeHandler change,
            TextFieldFailureHandler failure = null,
            TextFieldValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            text = EditorGUILayout.TextField(label, text, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                TextFieldApplyChangeHandlers(text, change, failure, validate);
            }
            return text;
        }
        #endregion

        #region Toggle
        public delegate void ToggleChangeHandler(bool value);
        public delegate void ToggleFailureHandler();
        public delegate bool ToggleValidationHandler(bool value);

        private static void ToggleApplyChangeHandlers(bool value,
            ToggleChangeHandler change,
            ToggleFailureHandler failure,
            ToggleValidationHandler validate)
        {
            bool validated = (validate == null) ? true : validate(value);
            if (validated)
            {
                change(value);
            }
            else if (!validated && failure != null)
            {
                failure();
            }
        }

        public static bool Toggle(bool value,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft("", value, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static bool Toggle(string label, bool value,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft(label, value, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static bool Toggle(GUIContent label, bool value,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft(label, value, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static bool Toggle(bool value, GUIStyle style,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft("", value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static bool Toggle(string label, bool value, GUIStyle style,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft(label, value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }
        public static bool Toggle(GUIContent label, bool value, GUIStyle style,
            ToggleChangeHandler change,
            ToggleFailureHandler failure = null,
            ToggleValidationHandler validate = null,
            params GUILayoutOption[] options)
        {
            EditorGUI.BeginChangeCheck();
            value = EditorGUILayout.ToggleLeft(label, value, style, options);
            if (EditorGUI.EndChangeCheck())
            {
                ToggleApplyChangeHandlers(value, change, failure, validate);
            }
            return value;
        }

        #endregion
    }
}

