#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion


//=============================================================================
//
// Thanks to Aquatica water rendering engine: http://www.magicindie.com/aquatica.htm
//
//=============================================================================
namespace Isles.Graphics.Landscape
{
    /// <summary>
    /// this class implements the generation of the heightmap using the oceanographic
    /// statistical based model. The algorithm is described in imulating ocean water?
    /// Jerry Tessendorf.
    /// </summary>    
    public sealed class FourierWave
    {
        Complex[] initialWaves;
        Complex[] currentWaves;
        float[] angularFrequencies;
        float[] heightmapF;
        Color[] heightmapC;
        float kwPower = 6.0f;
        float delta = 0;
        float maximalValue = -1.0f;
        
        Random random = new Random();

        public int Resolution { get; set; }
        public Vector2 Wind { get; private set; }
        public float Amplitude { get; private set; }
        public Texture2D Heightmap { get; private set; }
        public SurfaceFormat Format { get; private set; }


        public FourierWave(GraphicsDevice graphics, int resolution)
            : this(graphics, resolution, 1, 1, Vector2.One, 6.0f, SurfaceFormat.Color)
        { }

        public FourierWave(GraphicsDevice graphics, int resolution, int gridResolution,
                       float amplitude, Vector2 wind, float power, SurfaceFormat format)
        {
            if (graphics == null)
                throw new ArgumentNullException();

            if (!Fourier.IsPowerOf2(resolution) || !Fourier.IsPowerOf2(gridResolution))
                throw new ArgumentException("Resolution must be power of 2");

            if (format != SurfaceFormat.Color && format != SurfaceFormat.Single)
                throw new NotSupportedException();


            Wind = wind;
            Amplitude = amplitude;
            Format = format;
            kwPower = power;
            Resolution = resolution;


            Heightmap = new Texture2D(graphics, resolution, resolution, 0, TextureUsage.AutoGenerateMipMap, format);
        

            initialWaves = new Complex[resolution * resolution];
            currentWaves = new Complex[resolution * resolution];
            angularFrequencies = new float[resolution * resolution];

            if (format == SurfaceFormat.Single)
                heightmapF = new float[resolution * resolution];
            else
                heightmapC = new Color[resolution * resolution];
            
            int i = 0;
            Vector2 wave;

            //the following loop fills initialWaves and angularFrquencies with data
            for (int u = 0; u < resolution; u++)
            {
                wave.X = (-0.5f * (float)resolution + u) * (2.0f * (float)Math.PI / (float)gridResolution);

                for (int v = 0; v < resolution; v++)
                {
                    wave.Y = (-0.5f * (float)resolution + v) * (2.0f * (float)Math.PI / (float)gridResolution);

                    float temp = (float)Math.Sqrt(0.5f * GetPhillipsSpectrum(wave, Wind, kwPower));

                    Complex c;

                    c.Real = GetGaussianRandomFloat() * temp;
                    c.Imaginary = GetGaussianRandomFloat() * temp;

                    initialWaves[i] = c;

                    temp = 9.81f * wave.Length();

                    angularFrequencies[i] = (float)Math.Sqrt(temp);

                    i++;
                }
            }

            //fills the current Waves with dara at time 0
            Update(new GameTime());
        }

        private float GetPhillipsSpectrum(Vector2 waveVector, Vector2 wind, float kwPower)
        {
            //compute the length of the vector
            float k = waveVector.Length();

            if (k < 0.1f)
                return 0;			// to avoid division by 0
            else
            {
                float windVelocity = wind.Length();

                float l = (float)(Math.Pow(windVelocity, 2.0f) / 9.81f);
                float dot = Vector2.Dot(waveVector, wind);

                return (float)(Amplitude * (Math.Exp(-1 / Math.Pow(k * l, 2)) / (Math.Pow(k, 2) * Math.Pow(k, 2))) *
                    Math.Pow(-dot / (k * windVelocity), kwPower));
            }
        }

        /// <summary>
        /// returns a Gaussian random number with mean 0 and standard deviation 1,
        /// using Box - muller transform
        /// </summary>
        /// <returns></returns>
        private float GetGaussianRandomFloat()
        {
            float x1, x2, w, y1;

            do
            {
                x1 = (float)(2.0f * random.NextDouble() - 1.0f);
                x2 = (float)(2.0f * random.NextDouble() - 1.0f);
                w = x1 * x1 + x2 * x2;

            } while (w >= 1.0f);

            w = (float)(Math.Sqrt((-2.0f * Math.Log(w)) / w));
            y1 = x1 * w;

            return y1;
        }


        /// <summary>
        /// Update the FFT wave with delta time in milliseconds.
        /// </summary>
        public void Update(GameTime time)
        {
            delta += (float)(time.ElapsedGameTime.TotalSeconds);

            int i = 0;

            for (int u = 0; u < Resolution; u++)
            {
                for (int v = 0; v < Resolution; v++)
                {
                    Complex positive_h0 = initialWaves[u * (Resolution) + v];
                    Complex negative_h0 = initialWaves[(Resolution - 1 - u) * (Resolution) + (Resolution - 1 - v)];

                    float wt = angularFrequencies[u * (Resolution) + v] * delta;

                    float coswt_pos = (float)Math.Cos(wt);
                    float sinwt_pos = (float)Math.Sin(wt);
                    float coswt_neg = (float)Math.Cos(-wt);
                    float sinwt_neg = (float)Math.Sin(-wt);

                    Complex c;

                    c.Real =
                        positive_h0.Real * coswt_pos - positive_h0.Imaginary * sinwt_pos + negative_h0.Real * coswt_neg - (-negative_h0.Imaginary) * sinwt_neg;
                    c.Imaginary =
                        positive_h0.Real * sinwt_pos + positive_h0.Imaginary * coswt_pos + negative_h0.Real * sinwt_neg + (-negative_h0.Imaginary) * coswt_neg;

                    currentWaves[i++] = c;
                }
            }

            Fourier.FFT2(currentWaves, Resolution, Resolution, FourierDirection.Backward);


            for (int x = 0; x < Resolution; x++)
            {
                for (int y = 0; y < Resolution; y++)
                {
                    if (((x + y) & 0x1) == 1)
                        currentWaves[x * Resolution + y] *= 1;
                    else
                        currentWaves[x * Resolution + y] *= -1;
                }
            }

            // Normalize result
            float min = float.MaxValue;
            float max = float.MinValue;

            float currentMax = 0;

            for (i = 0; i < Resolution * Resolution; i++)
            {
                if (min > currentWaves[i].Real)
                    min = currentWaves[i].Real;
                if (max < currentWaves[i].Real)
                    max = currentWaves[i].Real;
            }

            min = Math.Abs(min);
            max = Math.Abs(max);

            if (min > max)
                currentMax = min;
            else
                currentMax = max;

            if (currentMax > maximalValue)
                maximalValue = currentMax;

            //to avoide division by zero
            float scaleCoef = maximalValue + 0.000001f;


            i = 0;
            
            // Update heightmap texture
            for (int u = 0; u < Resolution; u++)
            {
                for (int v = 0; v < Resolution; v++)
                {
                    float value = currentWaves[i].Real;

                    value = (value + scaleCoef) / (scaleCoef * 2);

                    if (value < 0) value = 0.0f;
                    if (value > 1.0) value = 1.0f;

                    if (Format == SurfaceFormat.Single)
                        heightmapF[i] = value;
                    else
                        heightmapC[i] = new Color(value, value, value, 1.0f);
                    i++;
                }
            }

            // Fill heightmap texture data
            if (Format == SurfaceFormat.Single)
                Heightmap.SetData<float>(heightmapF);
            else
                Heightmap.SetData<Color>(heightmapC);
        }
    }
}
