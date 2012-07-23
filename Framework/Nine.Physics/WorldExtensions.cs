namespace Nine.Physics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using BEPUphysics;
    using BEPUphysics.Collidables;
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.CollisionTests.CollisionAlgorithms;
    using BEPUphysics.Constraints;
    using BEPUphysics.DataStructures;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.MathExtensions;
    using BEPUphysics.PositionUpdating;
    using BEPUphysics.Settings;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Extends <see cref="World"/> to be capable of moving objects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class WorldExtensions
    {
        const string PhysicsTypeProperty = "PhysicsComponentContent.PhysicsType";
        const string CollisionMeshProperty = "PhysicsComponentContent.CollisionMesh";
        const string StaticFrictionProperty = "PhysicsComponentContent.StaticFriction";
        const string DynamicFrictionProperty = "PhysicsComponentContent.DynamicFriction";
        const string RestitutionProperty = "PhysicsComponentContent.Restitution";
        const string MassProperty = "PhysicsComponentContent.Mass";

        /// <summary>
        /// Creates the navigation info for this world.
        /// </summary>
        public static ISpace CreatePhysics(this World world)
        {
            return CreatePhysics(world, false);
        }

        /// <summary>
        /// Creates the navigation info for this world.
        /// </summary>
        public static ISpace CreatePhysics(this World world, bool enablePhysicsDebugging)
        {
            if (world.GetService<ISpace>() != null)
            {
                DestroyPhysics(world);
            }

            // Create space.
            var space = new Space();
            space.ForceUpdater.Gravity = new Vector3(0, 0, -9.81f); //If left unset, the default value is (0,0,0).

            // This section lets the engine know that it can make use of multithreaded systems
            // by adding threads to its thread pool.
#if XBOX
            // Note that not all four available hardware threads are used.
            // Currently, BEPUphysics will allocate an equal amount of work to each thread on the xbox360.
            // If two threads are put on one core, it will bottleneck the engine and run significantly slower than using 3 hardware threads.

            space.ThreadManager.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 1 }); }, null);
            space.ThreadManager.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 3 }); }, null);
            space.ThreadManager.AddThread(delegate { Thread.CurrentThread.SetProcessorAffinity(new[] { 5 }); }, null);
#else
            if (Environment.ProcessorCount > 1)
            {
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    space.ThreadManager.AddThread();
                }
            }
#endif

            world.Services.AddService(typeof(ISpace), space);
            world.Services.AddService(typeof(Space), space);

            // TODO: Update order matters.
            world.Updating += new EventHandler<TimeEventArgs>(world_Updating);

            PhysicsDebugger debugger = null;
            if (enablePhysicsDebugging)
                debugger = EnablePhysicsDebugging(world);

            // Create physics object from the scene.
            CreatePhysicsFromWorld(space, world, debugger);

            DisplayEntities(space, debugger);

            GC.Collect();
            return space;
        }

        static void world_Updating(object sender, TimeEventArgs e)
        {
            var space = ((World)sender).GetService<ISpace>();
            space.Update((float)e.ElapsedTime.TotalSeconds);
        }

        /// <summary>
        /// Enables debug visualization and manipulation of this physics world.
        /// </summary>
        private static PhysicsDebugger EnablePhysicsDebugging(this World world)
        {
            if (world.GetService<PhysicsDebugger>() == null)
            {
                var graphicsDevice = world.GetService<GraphicsDevice>();
                var space = world.GetService<Space>();

                if (graphicsDevice != null && space != null)
                {
                    var debugger = new PhysicsDebugger(graphicsDevice, space);
                    world.Services.AddService(typeof(PhysicsDebugger), debugger);
                    world.Drawing += new EventHandler<TimeEventArgs>(world_Drawing);
                    return debugger;
                }
            }
            return null;
        }

        private static void DisplayEntities(Space space, PhysicsDebugger debugger)
        {
#if WINDOWS
            if (debugger == null)
                return;

            foreach (Entity e in space.Entities)
            {
                if ((string)e.Tag != "noDisplayObject")
                {
                    debugger.ModelDrawer.Add(e);
                }
                else //Remove the now unnecessary tag.
                    e.Tag = null;
            }
            for (int i = 0; i < space.Solver.SolverUpdateables.Count; i++)
            {
                //Add the solver updateable and match up the activity setting.
                var objectAdded = debugger.ConstraintDrawer.Add(space.Solver.SolverUpdateables[i]);
                if (objectAdded != null)
                    objectAdded.IsDrawing = space.Solver.SolverUpdateables[i].IsActive;
            }
#endif
        }

        static void world_Drawing(object sender, TimeEventArgs e)
        {
            var debugger = ((World)sender).GetService<PhysicsDebugger>();
            var scene = ((World)sender).GetService<Scene>();

            if (debugger != null && scene != null)
                debugger.Draw(scene.Camera.View, scene.Camera.Projection);
        }

        /// <summary>
        /// Destroies the navigation info of this world.
        /// </summary>
        public static void DestroyPhysics(this World world)
        {
            var space = world.GetService<Space>();
            if (space != null)
            {
                if (world.GetService<PhysicsDebugger>() != null)
                {
                    world.Drawing -= new EventHandler<TimeEventArgs>(world_Drawing);
                    world.Services.RemoveService(typeof(PhysicsDebugger));
                }

                world.Updating -= new EventHandler<TimeEventArgs>(world_Updating);

                ApplyDefaultSettings(space);
                space.Dispose();

                world.Services.RemoveService(typeof(Space));
                world.Services.RemoveService(typeof(ISpace));                
            }
        }

        /// <summary>
        /// Applies the default settings to the space.
        /// These values are what the engine starts with; they don't have to be applied unless you just want to get back to the defaults.
        /// This doesn't cover every single tunable field in the entire engine, just the main ones that this helper class is messing with.
        /// </summary>
        /// <param name="space">Space to configure.</param>
        private static void ApplyDefaultSettings(Space space)
        {
            MotionSettings.ConserveAngularMomentum = false;
            MotionSettings.DefaultPositionUpdateMode = PositionUpdateMode.Discrete;
            MotionSettings.UseRk4AngularIntegration = false;
            SolverSettings.DefaultMinimumIterations = 1;
            space.Solver.IterationLimit = 10;
            GeneralConvexPairTester.UseSimplexCaching = false;
            MotionSettings.UseExtraExpansionForContinuousBoundingBoxes = false;
        }
        
        private static void CreatePhysicsFromWorld(Space space, World world, PhysicsDebugger debugger)
        {
            var scene = world.GetService<Scene>();
            if (scene == null)
                return;
                
            string physicsType;
            float staticFriction;
            float dynamicFriction;
            float restitution;

            foreach (var worldObject in world.WorldObjects)
            {
                var physicsComponent = worldObject.Find<PhysicsComponent>();
                if (physicsComponent != null)
                {
                    physicsComponent.CreatePhysicsEntity();
                    continue;
                }

                var displayObject = worldObject.Find<DrawingGroup>();
                if (displayObject != null)
                {
                    foreach (var drawable in displayObject.Children)
                    {
                        var model = drawable as Nine.Graphics.ObjectModel.Model;
                        if (model != null && 
                            UtilityExtensions.TryGetAttachedProperty(model.Tag, PhysicsTypeProperty, out physicsType))
                        {
                            if (physicsType == "None")
                                continue;
                            
                            float mass;
                            string collisionMesh;
                            if (!UtilityExtensions.TryGetAttachedProperty(model.Tag, CollisionMeshProperty, out collisionMesh))
                                collisionMesh = "Collision";
                            if (!UtilityExtensions.TryGetAttachedProperty(model.Tag, MassProperty, out mass))
                                mass = 1;

                            physicsComponent = CreateModel(space, model, physicsType, collisionMesh, mass, debugger);
                            
                            if (UtilityExtensions.TryGetAttachedProperty(model.Tag, StaticFrictionProperty, out staticFriction))
                                physicsComponent.Entity.Material.StaticFriction = staticFriction;
                            if (UtilityExtensions.TryGetAttachedProperty(model.Tag, DynamicFrictionProperty, out dynamicFriction))
                                physicsComponent.Entity.Material.KineticFriction = dynamicFriction;
                            if (UtilityExtensions.TryGetAttachedProperty(model.Tag, RestitutionProperty, out restitution))
                                physicsComponent.Entity.Material.Bounciness = restitution;

                            worldObject.Components.Add(physicsComponent);
                            break;
                        }

                        var surface = drawable as Surface;
                        if (surface != null &&
                            UtilityExtensions.TryGetAttachedProperty(surface.Tag, PhysicsTypeProperty, out physicsType))
                        {
                            if (physicsType == "None")
                                continue;
                            if (physicsType != "Static")
                                throw new NotSupportedException("Physics type not supported: " + physicsType);

                            var terrain = CreateTerrain(space, surface, debugger);

                            if (UtilityExtensions.TryGetAttachedProperty(surface.Tag, StaticFrictionProperty, out staticFriction))
                                terrain.Material.StaticFriction = staticFriction;
                            if (UtilityExtensions.TryGetAttachedProperty(surface.Tag, DynamicFrictionProperty, out dynamicFriction))
                                terrain.Material.KineticFriction = dynamicFriction;
                            if (UtilityExtensions.TryGetAttachedProperty(surface.Tag, RestitutionProperty, out restitution))
                                terrain.Material.Bounciness = restitution;
                            break;
                        }
                    }
                }
            }
        }

        private static Terrain CreateTerrain(Space space, Surface surface, PhysicsDebugger debugger)
        {
            int xLength = surface.Heightmap.Width + 1;
            int yLength = surface.Heightmap.Height + 1;

            var heights = new float[xLength, yLength];
            for (int x = 0; x < xLength; x++)
            {
                for (int y = 0; y < yLength; y++)
                {
                    heights[x, y] = surface.Heightmap.GetHeight(x, yLength - 1 - y);
                }
            }

            //Create the terrain.
            var terrain = new Terrain(heights, new AffineTransform(
                    new Vector3(surface.Step, 1, surface.Step), 
                    Quaternion.CreateFromYawPitchRoll(0, MathHelper.PiOver2, 0), 
                    surface.AbsolutePosition + Vector3.UnitY * surface.Size.Y));

            space.Add(terrain);

#if WINDOWS
            if (debugger != null)
            {
                // If the terrain is larger than 256x256, ushort is not enough for indices,
                // the debug view is not correct in this case. This does not affect physics simulation.
                debugger.ModelDrawer.Add(terrain);
            }
#endif
            return terrain;
        }

        private static PhysicsComponent CreateModel(Space space, Nine.Graphics.ObjectModel.Model model, string physicsType, string collisionMesh, float mass, PhysicsDebugger debugger)
        {
            Vector3[] vertices;
            int[] indices;
            Vector3 scale;
            Vector3 position;
            Quaternion rotation;
            PhysicsComponent result = null;

            if (!model.AbsoluteTransform.Decompose(out scale, out rotation, out position))
                throw new InvalidOperationException();

            // Hide collision mesh
            if (!string.IsNullOrEmpty(collisionMesh))
            {
                foreach (var part in model.ModelMeshes.Where(part => part.Name == collisionMesh))
                    part.Visible = false;
            }

            GetVerticesAndIndicesFromModel(model.Source, collisionMesh, out vertices, out indices);
            var transform = new AffineTransform(scale, rotation, position);
            if (physicsType == "Dynamic")
            {
                var mesh = new MobileMesh(vertices, indices, transform, MobileMeshSolidity.Counterclockwise, mass);
                result = new PhysicsComponent(mesh);

                // Entities in Bepu is centered, so need to adjust the graphical transform according.
                var offset = -(mesh.Position - position);
                if (offset.LengthSquared() > 1E-8)
                    result.TransformBias = Matrix.CreateTranslation(offset);

                space.Add(mesh);
            }
            else if (physicsType == "Kinematic")
            {
                var mesh = new MobileMesh(vertices, indices, transform, MobileMeshSolidity.Counterclockwise);
                result = new PhysicsComponent(mesh);

                var offset = -(mesh.Position - position);
                if (offset.LengthSquared() > 1E-8)
                    result.TransformBias = Matrix.CreateTranslation(offset);

                space.Add(mesh);
            }
            else if (physicsType == "Static")
            {
                // TODO: Use instanced mesh
                var mesh = new StaticMesh(vertices, indices, transform);
                space.Add(mesh);
            }
            else
            {
                throw new NotSupportedException("Physics type not supported: " + physicsType);
            }

            return result;
        }

        private static void GetVerticesAndIndicesFromModel(Microsoft.Xna.Framework.Graphics.Model collisionModel, string collisionMesh, out Vector3[] vertices, out int[] indices)
        {
            if (string.IsNullOrEmpty(collisionMesh) || !collisionModel.Meshes.Any(mesh => mesh.Name == collisionMesh))
            {
                TriangleMesh.GetVerticesAndIndicesFromModel(collisionModel, out vertices, out indices);
                return;
            }

            var verticesList = new List<Vector3>();
            var indicesList = new List<int>();
            var transforms = new Matrix[collisionModel.Bones.Count];
            collisionModel.CopyAbsoluteBoneTransformsTo(transforms);

            Matrix transform;
            foreach (Microsoft.Xna.Framework.Graphics.ModelMesh mesh in collisionModel.Meshes)
            {
                if (mesh.Name == collisionMesh)
                {
                    if (mesh.ParentBone != null)
                        transform = transforms[mesh.ParentBone.Index];
                    else
                        transform = Matrix.Identity;
                    TriangleMesh.AddMesh(mesh, transform, verticesList, indicesList);
                }
            }

            vertices = verticesList.ToArray();
            indices = indicesList.ToArray();
        }
    }
}
