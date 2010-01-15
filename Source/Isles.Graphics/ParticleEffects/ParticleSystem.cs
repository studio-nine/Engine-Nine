#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
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
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.ParticleEffects
{
    public enum ParticleType
    {
        PointSprite,
        Billboard,
    }


    public interface IParticleEmitter
    {
        ParticleVertex Emit(GameTime time, float lerpAmount);
    }


    public sealed class ParticleSystem : IDisposable
    {
        public float Emission { get; set; }
        public bool IsContinuous { get; set; }
        
        // Alpha blending settings.
        public Blend SourceBlend { get; set; }
        public Blend DestinationBlend { get; set; }

        // Controls how much particles are influenced by the velocity of the object
        // which created them. You can see this in action with the explosion effect,
        // where the flames continue to move in the same direction as the source
        // projectile. The projectile trail particles, on the other hand, set this
        // value very low so they are less affected by the velocity of the projectile.
        public float EmitterVelocitySensitivity { get; set; }

        public TimeSpan Duration { get; set; }
        public ParticleType ParticleType { get; set; }

        public IParticleEmitter ParticleEmitter { get; set; }
        public ParticleEffect ParticleEffect { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }


        float timeLeftOver = 0;

        // An array of particles, treated as a circular queue.
        ParticleVertex[] particles;


        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.
        VertexBuffer vertexBuffer = null;


        // Vertex declaration describes the format of our ParticleVertex structure.
        VertexDeclaration vertexDeclaration = null;


        // The particles array and vertex buffer are treated as a circular queue.
        // Initially, the entire contents of the array are free, because no particles
        // are in use. When a new particle is created, this is allocated from the
        // beginning of the array. If more than one particle is created, these will
        // always be stored in a consecutive block of array elements. Because all
        // particles last for the same amount of time, old particles will always be
        // removed in order from the start of this active particle region, so the
        // active and free regions will never be intermingled. Because the queue is
        // circular, there can be times when the active particle region wraps from the
        // end of the array back to the start. The queue uses modulo arithmetic to
        // handle these cases. For instance with a four entry queue we could have:
        //
        //      0
        //      1 - first active particle
        //      2 
        //      3 - first free particle
        //
        // In this case, particles 1 and 2 are active, while 3 and 4 are free.
        // Using modulo arithmetic we could also have:
        //
        //      0
        //      1 - first free particle
        //      2 
        //      3 - first active particle
        //
        // Here, 3 and 0 are active, while 1 and 2 are free.
        //
        // But wait! The full story is even more complex.
        //
        // When we create a new particle, we add them to our managed particles array.
        // We also need to copy this new data into the GPU vertex buffer, but we don't
        // want to do that straight away, because setting new data into a vertex buffer
        // can be an expensive operation. If we are going to be adding several particles
        // in a single frame, it is faster to initially just store them in our managed
        // array, and then later upload them all to the GPU in one single call. So our
        // queue also needs a region for storing new particles that have been added to
        // the managed array but not yet uploaded to the vertex buffer.
        //
        // Another issue occurs when old particles are retired. The CPU and GPU run
        // asynchronously, so the GPU will often still be busy drawing the previous
        // frame while the CPU is working on the next frame. This can cause a
        // synchronization problem if an old particle is retired, and then immediately
        // overwritten by a new one, because the CPU might try to change the contents
        // of the vertex buffer while the GPU is still busy drawing the old data from
        // it. Normally the graphics driver will take care of this by waiting until
        // the GPU has finished drawing inside the VertexBuffer.SetData call, but we
        // don't want to waste time waiting around every time we try to add a new
        // particle! To avoid this delay, we can specify the SetDataOptions.NoOverwrite
        // flag when we write to the vertex buffer. This basically means "I promise I
        // will never try to overwrite any data that the GPU might still be using, so
        // you can just go ahead and update the buffer straight away". To keep this
        // promise, we must avoid reusing vertices immediately after they are drawn.
        //
        // So in total, our queue contains four different regions:
        //
        // Vertices between firstActiveParticle and firstNewParticle are actively
        // being drawn, and exist in both the managed particles array and the GPU
        // vertex buffer.
        //
        // Vertices between firstNewParticle and firstFreeParticle are newly created,
        // and exist only in the managed particles array. These need to be uploaded
        // to the GPU at the start of the next draw call.
        //
        // Vertices between firstFreeParticle and firstRetiredParticle are free and
        // waiting to be allocated.
        //
        // Vertices between firstRetiredParticle and firstActiveParticle are no longer
        // being drawn, but were drawn recently enough that the GPU could still be
        // using them. These need to be kept around for a few more frames before they
        // can be reallocated.

        int firstActiveParticle;
        int firstNewParticle;
        int firstFreeParticle;
        int firstRetiredParticle;


        // Store the current time, in seconds.
        float currentTime;


        // Count how many times Draw has been called. This is used to know
        // when it is safe to retire old particles back into the free list.
        int drawCounter;

        
        // Shared random number generator.
        static Random random = new Random();


        public ParticleSystem(int maxParticles)
        {
            IsContinuous = true;
            Emission = 100;
            Duration = TimeSpan.FromSeconds(1);
            EmitterVelocitySensitivity = 0;


            // Use additive blending.
            SourceBlend = Blend.SourceAlpha;
            DestinationBlend = Blend.One;


            particles = new ParticleVertex[maxParticles];
        }


        /// <summary>
        /// Updates the particle system
        /// </summary>
        public void Update(GameTime time)
        {
            currentTime += (float)time.ElapsedGameTime.TotalSeconds;

            UpdateEmitter(time);

            RetireActiveParticles();
            FreeRetiredParticles();

            // If we let our timer go on increasing for ever, it would eventually
            // run out of floating point precision, at which point the particles
            // would render incorrectly. An easy way to prevent this is to notice
            // that the time value doesn't matter when no particles are being drawn,
            // so we can reset it back to zero any time the active queue is empty.

            if (firstActiveParticle == firstFreeParticle)
                currentTime = 0;

            if (firstRetiredParticle == firstActiveParticle)
                drawCounter = 0;
        }


        /// <summary>
        /// Draws the particle system.
        /// </summary>
        public void Draw(GraphicsDevice graphics, GameTime gameTime, Matrix view, Matrix projection)
        {
            if (ParticleEffect == null)
                return;

            // If we are drawed for the first time, create vertex buffers.
            if (vertexBuffer == null)
            {
                LoadContent(graphics);
            }

            // If there are any particles waiting in the newly added queue,
            // we'd better upload them to the GPU ready for drawing.
            if (firstNewParticle != firstFreeParticle)
            {
                AddNewParticlesToVertexBuffer();
            }

            // If there are any active particles, draw them now!
            if (firstActiveParticle != firstFreeParticle)
            {
                // Set the particle vertex buffer and vertex declaration.
                GraphicsDevice.VertexDeclaration = vertexDeclaration;
                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, ParticleVertex.SizeInBytes);


                ParticleEffect.View = view;
                ParticleEffect.Projection = projection;
                ParticleEffect.World = Matrix.Identity;
                ParticleEffect.ParticleType = ParticleType;

                BeginParticleRenderStates(GraphicsDevice.RenderState);
                {
                    ParticleEffect.Begin(GraphicsDevice, gameTime);
                    {
                        if (firstActiveParticle < firstFreeParticle)
                        {
                            // If the active particles are all in one consecutive range,
                            // we can draw them all in a single call.
                            GraphicsDevice.DrawPrimitives(PrimitiveType.PointList,
                                                  firstActiveParticle,
                                                  firstFreeParticle - firstActiveParticle);
                        }
                        else
                        {
                            // If the active particle range wraps past the end of the queue
                            // back to the start, we must split them over two draw calls.
                            GraphicsDevice.DrawPrimitives(PrimitiveType.PointList,
                                                  firstActiveParticle,
                                                  particles.Length - firstActiveParticle);

                            if (firstFreeParticle > 0)
                            {
                                GraphicsDevice.DrawPrimitives(PrimitiveType.PointList,
                                                      0,
                                                      firstFreeParticle);
                            }
                        }
                    }
                    ParticleEffect.End();
                }
                EndParticleRenderStates(GraphicsDevice.RenderState);
            }

            drawCounter++;
        }


        void LoadContent(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;


            vertexDeclaration = new VertexDeclaration(GraphicsDevice, ParticleVertex.VertexElements);

            // Create a dynamic vertex buffer.
            BufferUsage usage = BufferUsage.WriteOnly | BufferUsage.Points;

            int size = ParticleVertex.SizeInBytes * particles.Length;

            vertexBuffer = new VertexBuffer(GraphicsDevice, size, usage);

            // Initialize the vertex buffer contents. This is necessary in order
            // to correctly restore any existing particles after a lost device.
            vertexBuffer.SetData(particles);
        }


        void UpdateEmitter(GameTime time)
        {
            if (ParticleEmitter == null)
                return;

            // Work out how much time has passed since the previous update.
            float elapsedTime = (float)time.ElapsedGameTime.TotalSeconds;
            float timeBetweenParticles = 1.0f / Emission;

            if (elapsedTime > 0)
            {
                // If we had any time left over that we didn't use during the
                // previous update, add that to the current elapsed time.
                float timeToSpend = timeLeftOver + elapsedTime;

                // Counter for looping over the time interval.
                float currentTime = -timeLeftOver;

                // Create particles as long as we have a big enough time interval.
                while (timeToSpend > timeBetweenParticles)
                {
                    currentTime += timeBetweenParticles;
                    timeToSpend -= timeBetweenParticles;

                    // Work out the optimal position for this particle. This will produce
                    // evenly spaced particles regardless of the object speed, particle
                    // creation frequency, or game update rate.
                    float mu = currentTime / elapsedTime;

                    AddParticle(ParticleEmitter.Emit(time, mu));
                }

                // Store any time we didn't use, so it can be part of the next update.
                timeLeftOver = timeToSpend;
            }
        }


        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            float particleDuration = (float)Duration.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                float particleAge = currentTime - particles[firstActiveParticle].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle++;

                if (firstActiveParticle >= particles.Length)
                    firstActiveParticle = 0;
            }
        }


        /// <summary>
        /// Helper for checking when retired particles have been kept around long
        /// enough that we can be sure the GPU is no longer using them. It moves
        /// old particles from the retired area of the queue to the free section.
        /// </summary>
        void FreeRetiredParticles()
        {
            while (firstRetiredParticle != firstActiveParticle)
            {
                // Has this particle been unused long enough that
                // the GPU is sure to be finished with it?
                int age = drawCounter - (int)particles[firstRetiredParticle].Time;

                // The GPU is never supposed to get more than 2 frames behind the CPU.
                // We add 1 to that, just to be safe in case of buggy drivers that
                // might bend the rules and let the GPU get further behind.
                if (age < 3)
                    break;

                // Move the particle from the retired to the free queue.
                firstRetiredParticle++;

                if (firstRetiredParticle >= particles.Length)
                    firstRetiredParticle = 0;
            }
        }


        /// <summary>
        /// Helper for uploading new particles from our managed
        /// array to the GPU vertex buffer.
        /// </summary>
        void AddNewParticlesToVertexBuffer()
        {
            int stride = ParticleVertex.SizeInBytes;

            if (firstNewParticle < firstFreeParticle)
            {
                // If the new particles are all in one consecutive range,
                // we can upload them all in a single call.
                vertexBuffer.SetData(firstNewParticle * stride, particles,
                                     firstNewParticle,
                                     firstFreeParticle - firstNewParticle,
                                     stride);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride, particles,
                                     firstNewParticle,
                                     particles.Length - firstNewParticle,
                                     stride);

                if (firstFreeParticle > 0)
                {
                    vertexBuffer.SetData(0, particles,
                                         0, firstFreeParticle,
                                         stride);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }


        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        void AddParticle(ParticleVertex vertex)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 1;

            if (nextFreeParticle >= particles.Length)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;

            // Fill in the particle vertex structure.
            vertex.Time = currentTime;

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            vertex.Random = new Color((byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255));

            vertex.Velocity = vertex.Velocity * EmitterVelocitySensitivity;

            particles[firstFreeParticle] = vertex;

            firstFreeParticle = nextFreeParticle;
        }


        /// <summary>
        /// Helper for setting the renderstates used to draw particles.
        /// </summary>
        void BeginParticleRenderStates(RenderState renderState)
        {
            // Enable point sprites.
            renderState.PointSpriteEnable = true;
            renderState.PointSizeMax = 256;

            // Set the alpha blend mode.
            renderState.AlphaBlendEnable = true;
            renderState.AlphaBlendOperation = BlendFunction.Add;
            renderState.SourceBlend = SourceBlend;
            renderState.DestinationBlend = DestinationBlend;

            // Set the alpha test mode.
            renderState.AlphaTestEnable = true;
            renderState.AlphaFunction = CompareFunction.Greater;
            renderState.ReferenceAlpha = 0;

            // Enable the depth buffer (so particles will not be visible through
            // solid objects like the ground plane), but disable depth writes
            // (so particles will not obscure other particles).
            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
        }


        /// <summary>
        /// Helper for setting the renderstates used to draw particles.
        /// </summary>
        void EndParticleRenderStates(RenderState renderState)
        {
            renderState.PointSpriteEnable = false;
            renderState.AlphaBlendEnable = false;
            renderState.SourceBlend = Blend.SourceAlpha;
            renderState.DestinationBlend = Blend.InverseSourceAlpha;
            renderState.AlphaTestEnable = false;
            renderState.DepthBufferWriteEnable = true;
        }


        public void Dispose()
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
            if (vertexDeclaration != null)
                vertexDeclaration.Dispose();
        }
    }
}
