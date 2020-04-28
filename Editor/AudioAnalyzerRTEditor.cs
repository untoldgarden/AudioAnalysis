using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UntoldGarden.AudioAnalysis
{
    [CustomEditor(typeof(AudioAnalyzerRT))]
    [CanEditMultipleObjects]
    public class AudioAnalyzerRTEditor : Editor
    {
        SerializedProperty _channel;
        SerializedProperty _resolution;
        SerializedProperty _audioSource;
        SerializedProperty _FFTType;
        SerializedProperty _clipSpectrum;

        static GUIContent[] _resolutionLabels = {
            new GUIContent("128"), new GUIContent("256"),
            new GUIContent("512"), new GUIContent("1024"),
            new GUIContent("2048")
        };
        static int[] _resolutionOptions = { 128, 256, 512, 1024, 2048 };

        private void OnEnable()
        {
            _channel = serializedObject.FindProperty("_channel");
            _resolution = serializedObject.FindProperty("_resolution");
            _audioSource = serializedObject.FindProperty("_audioSource");
            _FFTType = serializedObject.FindProperty("_FFTType");
            _clipSpectrum = serializedObject.FindProperty("_clipSpectrum");
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            AudioAnalyzerRT analyzer = (AudioAnalyzerRT)target;

            serializedObject.Update();

            EditorGUILayout.PropertyField(_audioSource);
            // Channel selection
            EditorGUILayout.PropertyField(_channel);


            // Spectrum resolution (disabled during play mode)
            using (new EditorGUI.DisabledScope(EditorApplication.isPlaying))
            {
                EditorGUILayout.IntPopup
                  (_resolution, _resolutionLabels, _resolutionOptions);
                EditorGUILayout.Slider(_clipSpectrum, 0, 1);
            }

            //analyzer.src = EditorGUILayout.ObjectField()
            EditorGUILayout.PropertyField(_FFTType);

            serializedObject.ApplyModifiedProperties();


        }
    }
}