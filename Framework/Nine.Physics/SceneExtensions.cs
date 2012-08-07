namespace Nine.Physics
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using BEPUphysics;
    
    /// <summary>
    /// Contains extension methods related to physics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SceneExtensions
    {
        /// <summary>
        /// Gets the physics space of the specified scene.
        /// Creates a default space if no drawing context is currently bound to the scene.
        /// </summary>
        public static ISpace GetSpace(this Scene scene)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            ISpace space = null;
            AttachablePropertyServices.TryGetProperty(scene, SpaceProperty, out space);
            if (space == null)
                SetSpace(scene, space = CreateSpace());
            return space;
        }

        /// <summary>
        /// Sets the physics space of the specified scene.
        /// </summary>
        public static void SetSpace(Scene scene, ISpace value)
        {
            if (scene == null)
                throw new ArgumentNullException("scene");

            var currentSpace = GetSpace(scene);
            if (currentSpace != null && currentSpace != value)
                throw new InvalidOperationException();

            if (currentSpace == null)
            {
                if (value != null)
                    BindSpace(scene, value);
                AttachablePropertyServices.SetProperty(scene, SpaceProperty, value);
            }
        }
        private static AttachableMemberIdentifier SpaceProperty = new AttachableMemberIdentifier(typeof(SceneExtensions), "Space");

        /// <summary>
        /// Updates the physics simulation of the scene.
        /// </summary>
        public static void UpdatePhysics(this Scene scene, TimeSpan elapsedTime)
        {
            GetSpace(scene).Update((float)elapsedTime.TotalSeconds);
        }

        /// <summary>
        /// Updates the physics simulation of the scene asnychroniously.
        /// </summary>
        public static void UpdatePhysicsAsync(this Scene scene, TimeSpan elapsedTime)
        {
            GetSpace(scene).Update((float)elapsedTime.TotalSeconds);
        }

        /// <summary>
        /// Creates a new physics space.
        /// </summary>
        private static ISpace CreateSpace()
        {
            var space = new Space();

            //If left unset, the default value is (0,0,0).
            space.ForceUpdater.Gravity = new Vector3(0, -9.81f, 0);

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
            return space;
        }

        /// <summary>
        /// Binds scene added/removed events to the drawing context.
        /// </summary>
        private static void BindSpace(Scene scene, ISpace space)
        {
            scene.AddedToScene += (value) =>
            {
                var spaceObject = value as ISpaceObject;
                //if (spaceObject != null)
                //    spaceObject.OnAdded(space);
            };
            scene.RemovedFromScene += (value) =>
            {
                var sceneObject = value as ISpaceObject;
                //if (sceneObject != null)
                //    sceneObject.OnRemoved(space);
            };
            scene.Traverse<ISpaceObject>(spaceObject =>
            {
                //spaceObject.OnAdded(space);
                return TraverseOptions.Continue;
            });
        }
    }
}