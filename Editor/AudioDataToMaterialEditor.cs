using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace UntoldGarden.AudioAnalysis
{
    [CustomEditor(typeof(AudioDataToMaterial))]
    [CanEditMultipleObjects]
    public class AudioDataToMaterialEditor : Editor
    {
        //SerializedProperty _analyzer;
        SerializedProperty _bindRMS;
        SerializedProperty _bindTexture;
        SerializedProperty _materials;
        private void OnEnable()
        {
            //_analyzer = serializedObject.FindProperty("_analyzer");
            _materials = serializedObject.FindProperty("_materials");
        }

        public override bool RequiresConstantRepaint()
        {
            // Keep updated while playing.
            return EditorApplication.isPlaying && targets.Length == 1;
        }
        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            AudioDataToMaterial dataBinder = (AudioDataToMaterial)target;

            serializedObject.Update();

            //EditorGUILayout.PropertyField(_analyzer);
            EditorGUILayout.PropertyField(_materials);


            dataBinder.bindWaveformTexture = EditorGUILayout.BeginToggleGroup("Bind Waveform Texture", dataBinder.bindWaveformTexture);
            if (dataBinder.bindWaveformTexture)
            {
                dataBinder.waveformTextureName = EditorGUILayout.TextField("Waveform Texture Name", dataBinder.waveformTextureName);
            }
            EditorGUILayout.EndToggleGroup();

            dataBinder.bindSpectrumTexture = EditorGUILayout.BeginToggleGroup("Bind Spectrum Texture", dataBinder.bindSpectrumTexture);
            if (dataBinder.bindSpectrumTexture)
            {
                dataBinder.spectrumTextureName = EditorGUILayout.TextField("Spectrum Texture Name", dataBinder.spectrumTextureName);
            }
            EditorGUILayout.EndToggleGroup();

            if (dataBinder.bindSpectrumTexture || dataBinder.bindWaveformTexture)
            {
                dataBinder.bindTextureWidth = EditorGUILayout.BeginToggleGroup("Bind Texture Width", dataBinder.bindTextureWidth);
                if (dataBinder.bindTextureWidth)
                {
                    dataBinder.textureWidthName = EditorGUILayout.TextField("Texture Width Name", dataBinder.textureWidthName);
                }
                EditorGUILayout.EndToggleGroup();
            }

            dataBinder.bindRMS = EditorGUILayout.BeginToggleGroup("Bind RMS", dataBinder.bindRMS);
            if (dataBinder.bindRMS)
            {
                dataBinder.rmsName = EditorGUILayout.TextField("RMS Value Name", dataBinder.rmsName);
            }
            EditorGUILayout.EndToggleGroup();

            dataBinder.bindDbValue = EditorGUILayout.BeginToggleGroup("Bind DB Value", dataBinder.bindDbValue);
            if (dataBinder.bindDbValue)
            {
                dataBinder.dbName = EditorGUILayout.TextField("DB Value Name", dataBinder.dbName);
            }
            EditorGUILayout.EndToggleGroup();

            dataBinder.bindPitch = EditorGUILayout.BeginToggleGroup("Bind Pitch", dataBinder.bindPitch);
            if (dataBinder.bindPitch)
            {
                dataBinder.pitchName = EditorGUILayout.TextField("Pitch Value Name", dataBinder.pitchName);
            }
            EditorGUILayout.EndToggleGroup();

            serializedObject.ApplyModifiedProperties();

        }
    }
}
