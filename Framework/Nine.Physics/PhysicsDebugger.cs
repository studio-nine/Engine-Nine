namespace Nine.Physics
{
    using System;
    using BEPUphysics;
    using BEPUphysics.Entities;
    using BEPUphysicsDrawer.Lines;
    using BEPUphysicsDrawer.Models;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;


    class PhysicsDebugger
    {
#if WINDOWS
        public GraphicsDevice GraphicsDevice;
        public Space Space;

        public ModelDrawer ModelDrawer;
        public LineDrawer ConstraintDrawer;
        public ContactDrawer ContactDrawer;
        public BoundingBoxDrawer BoundingBoxDrawer;
        public SimulationIslandDrawer SimulationIslandDrawer;
        public BasicEffect LineDrawer;

        //Display Booleans        
        private bool displayEntities = true;
        private bool displayUI = true;
        private bool displayConstraints = true;
        private bool displayMenu;
        private bool displayContacts;
        private bool displayBoundingBoxes;
        private bool displaySimulationIslands;
#endif

        public PhysicsDebugger(GraphicsDevice graphics, Space space)
        {
#if WINDOWS
            this.GraphicsDevice = graphics;
            this.Space = space;

            LineDrawer = new BasicEffect(graphics);

            //if (graphics.GraphicsProfile == GraphicsProfile.HiDef)
            //    ModelDrawer = new InstancedModelDrawer(graphics);
            //else
                ModelDrawer = new BruteModelDrawer(graphics);

            ConstraintDrawer = new LineDrawer(graphics);
            ContactDrawer = new ContactDrawer(graphics);
            BoundingBoxDrawer = new BoundingBoxDrawer(graphics);
            SimulationIslandDrawer = new SimulationIslandDrawer(graphics);
            
            //Clear out any old rendering stuff.
            ModelDrawer.Clear();
            ConstraintDrawer.Clear();

            foreach (Entity e in space.Entities)
            {
                if ((string)e.Tag != "noDisplayObject")
                {
                    ModelDrawer.Add(e);
                }
                else //Remove the now unnecessary tag.
                    e.Tag = null;
            }
            for (int i = 0; i < space.Solver.SolverUpdateables.Count; i++)
            {
                //Add the solver updateable and match up the activity setting.
                LineDisplayObjectBase objectAdded = ConstraintDrawer.Add(space.Solver.SolverUpdateables[i]);
                if (objectAdded != null)
                    objectAdded.IsDrawing = space.Solver.SolverUpdateables[i].IsActive;
            }
            
            GC.Collect();
#endif
        }

        public void Draw(Matrix view, Matrix projection)
        {
#if WINDOWS
            var keyboardState = Keyboard.GetState();
            if (!keyboardState.IsKeyDown(Keys.F2) && !keyboardState.IsKeyDown(Keys.F3))
                return;

            if (keyboardState.IsKeyDown(Keys.F2))
                GraphicsDevice.Clear(new Color(.41f, .41f, .45f, 1));

            if (displayEntities)
            {
                ModelDrawer.IsWireframe = keyboardState.IsKeyDown(Keys.LeftShift);
                ModelDrawer.Update();
                ModelDrawer.Draw(view, projection);
            }
            
            if (displayConstraints)
            {
                ConstraintDrawer.Update();
                ConstraintDrawer.Draw(view, projection);
            }

            LineDrawer.LightingEnabled = false;
            LineDrawer.VertexColorEnabled = true;
            LineDrawer.World = Matrix.Identity;
            LineDrawer.View = view;
            LineDrawer.Projection = projection;

            if (displayContacts)
                ContactDrawer.Draw(LineDrawer, Space);

            if (displayBoundingBoxes)
                BoundingBoxDrawer.Draw(LineDrawer, Space);

            if (displaySimulationIslands)
                SimulationIslandDrawer.Draw(LineDrawer, Space);
#endif
        }
    }
}
