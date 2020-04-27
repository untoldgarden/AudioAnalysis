using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UntoldGarden.AudioAnalysis
{
    public class AudioAnalyzerRT : MonoBehaviour
    {
        [SerializeField] AudioSource _audioSource;
        public AudioSource audioSource
        {
            get => _audioSource;
            set => _audioSource = value;
        }


        [SerializeField] FFTWindow _FFTType = FFTWindow.BlackmanHarris;
        public FFTWindow FFTType
        {
            get => _FFTType;
            set => _FFTType = value;
        }


        // Spectrum resolution
        [SerializeField] int _resolution = 512;
        public int resolution
        {
            get => _resolution;
            set => _resolution = ValidateResolution(value);
        }

        public int numSamples;

        // Channel Selection
        [SerializeField, Range(0, 15)] int _channel = 0;
        public int channel
        {
            get => _channel;
            set => _channel = value;
        }
        public float[] _samples;
        public float[] _spectrum;
        public float[] _filteredSpectrum;
        private float _fSample;

        public float RmsValue { get; set; }
        public float DbValue { get; set; }
        public float PitchValue { get; set; }

        float highestLogFreq, frequencyScaleFactor; //multiplier to ensure that the frequencies stretch to the highest record in the array.
        public bool useLogarithmicFrequency = true;
        public bool multiplyByFrequency = true;
        public float frequencyLimitLow = 0.00f;
        public float frequencyLimitHigh = 20000f;


        [SerializeField, Range(0, 1)] float _clipSpectrum = 1.00f;
        public float clipSpectrum
        {
            get => _clipSpectrum;
            set => _clipSpectrum = value;

        }


        private const float RefValue = 0.1f;
        private const float Threshold = 0.02f;
        // Start is called before the first frame update
        void Start()
        {
            numSamples = resolution * 2;

            _samples = new float[numSamples];
            _spectrum = new float[numSamples];
            _filteredSpectrum = new float[resolution];
            //_spectrum = new float[ToNextNearest(Mathf.CeilToInt(resolution / _clipSpectrum))];
            _fSample = AudioSettings.outputSampleRate;
            print(_spectrum.Length);


            highestLogFreq = Mathf.Log(resolution + 1, 2); //gets the highest possible logged frequency, used to calculate which sample of the spectrum to use for a bar
            frequencyScaleFactor = 1.0f / (AudioSettings.outputSampleRate / 2) * numSamples;


        }

        // Update is called once per frame
        void Update()
        {
            AnalyzeSound();
        }

        void AnalyzeSound()
        {
            _audioSource.GetOutputData(_samples, _channel); // fill array with samples
            float sum = 0;
            for (int i = 0; i < numSamples; i++)
            {
                sum += _samples[i] * _samples[i]; // sum squared samples
            }
            RmsValue = Mathf.Sqrt(sum / numSamples); // rms = square root of average
            DbValue = 20 * Mathf.Log10(RmsValue / RefValue); // calculate dB
            if (DbValue < -160) DbValue = -160; // clamp it to -160dB min
                                                // get sound _spectrum


            _audioSource.GetSpectrumData(_spectrum, _channel, FFTWindow.BlackmanHarris);
            for (int i = 0; i < resolution; i++)
            {
                float value;
                float trueSampleIndex;

                //GET SAMPLES
                if (useLogarithmicFrequency)
                {
                    //LOGARITHMIC FREQUENCY SAMPLING

                    //trueSampleIndex = highFrequencyTrim * (highestLogFreq - Mathf.Log(barAmount + 1 - i, 2)) * logFreqMultiplier; //old version

                    trueSampleIndex = Mathf.Lerp(frequencyLimitLow, frequencyLimitHigh, (highestLogFreq - Mathf.Log(resolution + 1 - i, 2)) / highestLogFreq) * frequencyScaleFactor;

                    //'logarithmic frequencies' just means we want to bias to the lower frequencies.
                    //by doing log2(max(i)) - log2(max(i) - i), we get a flipped log graph
                    //(make a graph of log2(64)-log2(64-x) to see what I mean)
                    //this isn't finished though, because that graph doesn't actually map the bar index (x) to the spectrum index (y).
                    //then we divide by highestLogFreq to make the graph to map 0-barAmount on the x axis to 0-1 in the y axis.
                    //we then use this to Lerp between frequency limits, and then an index is calculated.
                    //also 1 gets added to barAmount pretty much everywhere, because without it, the log hits (barAmount-1,max(freq))

                }
                else
                {
                    //LINEAR (SCALED) FREQUENCY SAMPLING 
                    //trueSampleIndex = i * linearSampleStretch; //don't like this anymore

                    trueSampleIndex = Mathf.Lerp(frequencyLimitLow, frequencyLimitHigh, ((float)i) / resolution) * frequencyScaleFactor;
                    //sooooo this one's gotten fancier...
                    //firstly a lerp is used between frequency limits to get the 'desired frequency', then it's divided by the outputSampleRate (/2, who knows why) to get its location in the array, then multiplied by numSamples to get an index instead of a fraction.

                }
                //the true sample is usually a decimal, so we need to lerp between the floor and ceiling of it.

                int sampleIndexFloor = Mathf.FloorToInt(trueSampleIndex);
                sampleIndexFloor = Mathf.Clamp(sampleIndexFloor, 0, _spectrum.Length - 2); //just keeping it within the _spectrum array's range

                value = Mathf.SmoothStep(_spectrum[sampleIndexFloor], _spectrum[sampleIndexFloor + 1], trueSampleIndex - sampleIndexFloor); //smoothly interpolate between the two samples using the true index's decimal.

                //MANIPULATE & APPLY SAMPLES
                if (multiplyByFrequency) //multiplies the amplitude by the true sample index
                {

                    value = value * (trueSampleIndex + 1);

                }

                value = Mathf.Sqrt(value); //compress the amplitude values by sqrt(x)
                _filteredSpectrum[i] = value;
            }


            //CLIP
            //clippedSpectrum = (float[])_spectrum.Clone();
            //int clippedLength = Mathf.FloorToInt(resolution * _clipSpectrum);
            //if (clippedSpectrum.Length != clippedLength)
            //Array.Resize(ref clippedSpectrum, clippedLength);

            float maxV = 0;
            var maxN = 0;
            for (int i = 0; i < _spectrum.Length; i++)
            { // find max 
                if (!(_spectrum[i] > maxV) || !(_spectrum[i] > Threshold))
                    continue;

                maxV = _spectrum[i];
                maxN = i; // maxN is the index of max
            }
            float freqN = maxN; // pass the index to a float variable
            if (maxN > 0 && maxN < _spectrum.Length - 1)
            { // interpolate index using neighbours
                var dL = _spectrum[maxN - 1] / _spectrum[maxN];
                var dR = _spectrum[maxN + 1] / _spectrum[maxN];
                freqN += 0.5f * (dR * dR - dL * dL);
            }
            PitchValue = freqN * (_fSample / 2) / _spectrum.Length; // convert index to frequency
        }

        static int ValidateResolution(int x)
        {
            if (x > 0 && (x & (x - 1)) == 0) return x;
            Debug.LogError("Spectrum resolution must be a power of 2.");
            return 1 << (int)Mathf.Max(1, Mathf.Round(Mathf.Log(x)));
        }
        int ToNearest(int x)
        {
            int next = ToNextNearest(x);
            int prev = next >> 1;
            return next - x < x - prev ? next : prev;
        }
        int ToNextNearest(int x)
        {
            if (x < 0) { return 0; }
            --x;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }


    }
}
