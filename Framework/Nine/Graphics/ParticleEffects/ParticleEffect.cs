#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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

namespace Nine.Graphics.ParticleEffects
{
    public sealed class ParticleEffect : IDisposable
    {
        #region Properties

        public ParticleEmitter Emitter { get; private set; }

        public ISpatialEmitter SpatialEmitter
        {
            get { return Emitter.SpatialEmitter; }
            set { Emitter.SpatialEmitter = value; }
        }

        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the estimated bounding box of this particle effect.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box = SpatialEmitter.BoundingBox;

                float h = (float)(Math.Abs(HorizontalVelocity.Max) * Duration.Max.TotalSeconds);
                float v = (float)(VerticalVelocity.Max * Duration.Max.TotalSeconds);

                box.Max.X += h;
                box.Max.Y += h;
                box.Min.X -= h;
                box.Min.Y -= h;

                if (v > 0)
                    box.Min.Z -= v;
                else if (v < 0)
                    box.Max.Z += v;

                return box;
            }
        }

        /// <summary>
        /// Gets or sets the texture used by this particle system.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Duration of each particle.
        /// </summary>
        public Range<TimeSpan> Duration { get; set; }

        /// <summary>
        /// Range of values controlling how much X and Y axis velocity to give each particle.
        /// </summary>
        public Range<float> HorizontalVelocity { get; set; }
        
        /// <summary>
        /// Range of values controlling how much Z axis velocity to give each particle.
        /// </summary>
        public Range<float> VerticalVelocity { get; set; }
        
        /// <summary>
        /// Direction and strength of the gravity effect. 
        /// </summary>
        public Vector3 Gravity { get; set; }

        /// <summary>
        /// Controls how the particle velocity will change over their lifetime. If set
        /// to 1, particles will keep going at the same speed as when they were created.
        /// If set to 0, particles will come to a complete stop right before they die.
        /// Values greater than 1 make the particles speed up over time.
        /// </summary>
        public float EndVelocity { get; set; }


        /// <summary>
        /// Range of values controlling the particle start color and alpha. 
        /// </summary>
        public Range<Color> StartColor { get; set; }

        /// <summary>
        /// Range of values controlling the particle end color and alpha. 
        /// </summary>
        public Range<Color> EndColor { get; set; }

        /// <summary>
        /// Range of values controlling how fast the particles rotate.
        /// </summary>
        public Range<float> RotateSpeed { get; set; }

        /// <summary>
        /// Range of values controlling how big the particles are when first created.
        /// </summary>
        public Range<float> StartSize { get; set; }

        /// <summary>
        /// Range of values controlling how big particles become at the end of their life.
        /// </summary>
        public Range<float> EndSize { get; set; }

        #endregion
        
        #region Fields

        // An array of particles, treated as a circular queue.
        ParticleVertex[] particles;


        // A vertex buffer holding our particles. This contains the same data as
        // the particles array, but copied across to where the GPU can access it.
        DynamicVertexBuffer vertexBuffer = null;
               
        // The index buffer is static since all particle system has the same index buffer
        static IndexBuffer indexBuffer = null;

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

        #endregion


        public ParticleEffect(int maxParticles)
        {
            Duration = TimeSpan.FromSeconds(2);

            HorizontalVelocity = new Range<float>(0, 15);
            VerticalVelocity = new Range<float>(-10, 10);

            // Set gravity upside down, so the flames will 'fall' upward.
            Gravity = new Vector3(0, 0, -10);

            StartColor = new Color(255, 255, 255, 255);
            EndColor = new Color(255, 255, 255, 0);

            StartSize = 1;
            EndSize = 3;

            Emitter = new ParticleEmitter();

            particles = new ParticleVertex[maxParticles * 4];
        }


        /// <summary>
        /// Updates the particle system
        /// </summary>
        public void Update(GameTime time)
        {
            currentTime += (float)time.ElapsedGameTime.TotalSeconds;

            if (Emitter != null)
            {
                foreach (Vector3 position in Emitter.Update(time))
                {
                    AddParticle(position);
                }
            }
            
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
        internal void Draw(GraphicsDevice graphics, GameTime gameTime)
        {
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
                GraphicsDevice.SetVertexBuffer(vertexBuffer);
                GraphicsDevice.Indices = indexBuffer;

                if (firstActiveParticle < firstFreeParticle)
                {
                    // If the active particles are all in one consecutive range,
                    // we can draw them all in a single call.
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                          0, 0, 
                                          firstFreeParticle - firstActiveParticle,
                                          firstActiveParticle / 4 * 6,
                                          (firstFreeParticle - firstActiveParticle) / 2);
                }
                else
                {
                    // If the active particle range wraps past the end of the queue
                    // back to the start, we must split them over two draw calls.
                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                          0, 0, 
                                          particles.Length - firstActiveParticle,
                                          firstActiveParticle / 4 * 6,
                                          (particles.Length - firstActiveParticle) / 2);

                    if (firstFreeParticle > 0)
                    {
                        GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList,
                                              0, 0,
                                              firstFreeParticle,
                                              0,
                                              firstFreeParticle / 2);
                    }
                }
            }

            drawCounter++;
        }


        void LoadContent(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
                        
            // Create a dynamic vertex buffer.
            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(ParticleVertex), particles.Length, BufferUsage.WriteOnly);

            // Initialize the vertex buffer contents. This is necessary in order
            // to correctly restore any existing particles after a lost device.
            vertexBuffer.SetData(particles);

            RequestIndexBuffer(GraphicsDevice, particles.Length / 2);
        }

        static void RequestIndexBuffer(GraphicsDevice graphics, int maxPrimitiveCount)
        {
            int indexCount = maxPrimitiveCount * 3;

            if (indexBuffer != null && indexBuffer.IndexCount < indexCount)
            {
                indexBuffer.Dispose();
                indexBuffer = null;
            }

            if (indexBuffer == null)
            {
                indexBuffer = new IndexBuffer(graphics, typeof(ushort), indexCount, BufferUsage.WriteOnly);

                ushort[] indices = new ushort[indexCount];

                int n = 0;

                for (int i = 0; i < maxPrimitiveCount / 2; i++)
                {
                    indices[n++] = (ushort)(i * 4 + 0);
                    indices[n++] = (ushort)(i * 4 + 1);
                    indices[n++] = (ushort)(i * 4 + 2);
                    indices[n++] = (ushort)(i * 4 + 1);
                    indices[n++] = (ushort)(i * 4 + 3);
                    indices[n++] = (ushort)(i * 4 + 2);
                }

                indexBuffer.SetData<ushort>(indices);
            }
        }


        /// <summary>
        /// Helper for checking when active particles have reached the end of
        /// their life. It moves old particles from the active area of the queue
        /// to the retired section.
        /// </summary>
        void RetireActiveParticles()
        {
            float particleDuration = (float)Duration.Max.TotalSeconds;

            while (firstActiveParticle != firstNewParticle)
            {
                // Is this particle old enough to retire?
                float particleAge = currentTime - particles[firstActiveParticle].Time;

                if (particleAge < particleDuration)
                    break;

                // Remember the time at which we retired this particle.
                particles[firstActiveParticle].Time = drawCounter;

                // Move the particle from the active to the retired queue.
                firstActiveParticle += 4;

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
                firstRetiredParticle += 4;

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
                                     stride, SetDataOptions.NoOverwrite);
            }
            else
            {
                // If the new particle range wraps past the end of the queue
                // back to the start, we must split them over two upload calls.
                vertexBuffer.SetData(firstNewParticle * stride, particles,
                                     firstNewParticle,
                                     particles.Length - firstNewParticle,
                                     stride, SetDataOptions.NoOverwrite);

                if (firstFreeParticle > 0)
                {
                    vertexBuffer.SetData(0, particles,
                                         0, firstFreeParticle,
                                         stride, SetDataOptions.NoOverwrite);
                }
            }

            // Move the particles we just uploaded from the new to the active queue.
            firstNewParticle = firstFreeParticle;
        }
        
        /// <summary>
        /// Adds a new particle to the system.
        /// </summary>
        void AddParticle(Vector3 position)
        {
            // Figure out where in the circular queue to allocate the new particle.
            int nextFreeParticle = firstFreeParticle + 4;

            if (nextFreeParticle >= particles.Length)
                nextFreeParticle = 0;

            // If there are no free particles, we just have to give up.
            if (nextFreeParticle == firstRetiredParticle)
                return;


            ParticleVertex vertex = new ParticleVertex();

            vertex.Position = position;

            // Fill in the particle vertex structure.
            vertex.Time = currentTime;

            // Choose four random control values. These will be used by the vertex
            // shader to give each particle a different size, rotation, and color.
            vertex.Random1 = new Color((byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255));
            vertex.Random2 = new Color((byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255),
                                      (byte)random.Next(255));

            float horizontalVelocity = MathHelper.Lerp(HorizontalVelocity.Min,
                                                       HorizontalVelocity.Max,
                                                       (float)random.NextDouble());

            double horizontalAngle = random.NextDouble() * MathHelper.TwoPi;

            vertex.Velocity.X = horizontalVelocity * (float)Math.Cos(horizontalAngle);
            vertex.Velocity.Y = horizontalVelocity * (float)Math.Sin(horizontalAngle);

            // Add in some random amount of vertical velocity.
            vertex.Velocity.Z = MathHelper.Lerp(VerticalVelocity.Min,
                                                VerticalVelocity.Max,
                                                (float)random.NextDouble());

            vertex.TextureCoordinates = Vector2.UnitY;
            particles[firstFreeParticle + 0] = vertex;

            vertex.TextureCoordinates = Vector2.One;
            particles[firstFreeParticle + 1] = vertex;

            vertex.TextureCoordinates = Vector2.Zero;
            particles[firstFreeParticle + 2] = vertex;

            vertex.TextureCoordinates = Vector2.UnitX;
            particles[firstFreeParticle + 3] = vertex;

            firstFreeParticle = nextFreeParticle;
        }

        public void Dispose()
        {
            if (vertexBuffer != null)
                vertexBuffer.Dispose();
        }
    }
}
