using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace UntoldGarden.AudioAnalysis
{

    [RequireComponent(typeof(AudioAnalyzerRT))]
    public class AudioDataToMaterial : MonoBehaviour
    {
        AudioAnalyzerRT analyzer;

        private RenderTexture _waveformRenderTexture;
        private Texture2D _waveformTexture;
        private RenderTexture _spectrumRenderTexture;
        private Texture2D _spectrumTexture;

        [SerializeField] List<Material> _materials;
        public List<Material> materials
        {
            get => _materials;
            set => _materials = value;
        }

        public bool bindRMS = false;
        public bool bindWaveformTexture = false;
        public bool bindSpectrumTexture = false;

        public bool bindDbValue = false;
        public bool bindPitch = false;
        public bool bindTextureWidth = false;

        public string rmsName = "RMS";
        public string dbName = "Db";
        public string pitchName = "Pitch";
        public string waveformTextureName = "LeftWaveform";
        public string textureWidthName = "TextureWidth";
        public string spectrumTextureName = "LeftSpectrum";





        // Start is called before the first frame update
        void Start()
        {
            analyzer = GetComponent<AudioAnalyzerRT>();
        }

        // Update is called once per frame
        void Update()
        {
            if (analyzer != null)
            {
                if (bindSpectrumTexture)
                {



                    // Refresh the temporary texture when the analyzer.resolution was changed.
                    if (_spectrumTexture != null && _spectrumTexture.width != analyzer.resolution)
                    {
                        Destroy(_spectrumTexture);
                        _spectrumTexture = null;
                    }

                    // Lazy initialization of the temporary texture
                    if (_spectrumTexture == null)
                        _spectrumTexture = new Texture2D(analyzer._filteredSpectrum.Length, 1,
                                                 TextureFormat.RFloat, false)
                        { wrapMode = TextureWrapMode.Clamp };

                    byte[] byteArray = new byte[analyzer._filteredSpectrum.Length * 4];
                    Buffer.BlockCopy(analyzer._filteredSpectrum, 0, byteArray, 0, byteArray.Length);
                    byte[] trimmedArray = new byte[byteArray.Length];

                    // Texture update
                    _spectrumTexture.LoadRawTextureData(byteArray);

                    _spectrumTexture.Apply();

                    // Update the external render texture.
                    if (_spectrumRenderTexture != null)
                        Graphics.CopyTexture(_spectrumTexture, _spectrumRenderTexture);

                }
                if (bindWaveformTexture)
                {



                    // Refresh the temporary texture when the analyzer.resolution was changed.
                    if (_waveformTexture != null && _waveformTexture.width != analyzer.resolution)
                    {
                        Destroy(_waveformTexture);
                        _waveformTexture = null;
                    }

                    // Lazy initialization of the temporary texture
                    if (_waveformTexture == null)
                        _waveformTexture = new Texture2D(analyzer.resolution, 1,
                                                 TextureFormat.RFloat, false)
                        { wrapMode = TextureWrapMode.Clamp };
                    byte[] byteArray = new byte[analyzer._samples.Length * 4];
                    Buffer.BlockCopy(analyzer._samples, 0, byteArray, 0, byteArray.Length);

                    // Texture update
                    _waveformTexture.LoadRawTextureData(byteArray);

                    _waveformTexture.Apply();

                    // Update the external render texture.
                    if (_waveformRenderTexture != null)
                        Graphics.CopyTexture(_waveformTexture, _waveformRenderTexture);

                }
                foreach (Material m in materials)
                {
                    if (bindSpectrumTexture)
                        m.SetTexture(spectrumTextureName, _spectrumTexture);
                    if (bindWaveformTexture)
                        m.SetTexture(waveformTextureName, _waveformTexture);
                    if (bindRMS)
                        m.SetFloat(rmsName, analyzer.RmsValue);
                    if (bindDbValue)
                        m.SetFloat(dbName, analyzer.DbValue);
                    if (bindPitch)
                        m.SetFloat(pitchName, analyzer.PitchValue);
                }
            }
            else Debug.LogError("No Analyzer Reference!");
        }
    }
}
