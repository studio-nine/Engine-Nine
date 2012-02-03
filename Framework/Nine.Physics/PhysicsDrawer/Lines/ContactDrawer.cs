using BEPUphysics;
using BEPUphysics.DataStructures;
using BEPUphysics.NarrowPhaseSystems.Pairs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BEPUphysicsDrawer.Lines
{
    /// <summary>
    /// Renders contact points.
    /// </summary>
    class ContactDrawer
    {
        GraphicsDevice graphics;
        public ContactDrawer(GraphicsDevice graphics)
        {
            this.graphics = graphics;
        }
        
        RawList<VertexPositionColor> contactLines = new RawList<VertexPositionColor>();

        public void Draw(Effect effect, Space space)
        {
            contactLines.Clear();
            int contactCount = 0;
            foreach (var pair in space.NarrowPhase.Pairs)
            {
                var pairHandler = pair as CollidablePairHandler;
                if (pairHandler != null)
                {
                    foreach (ContactInformation information in pairHandler.Contacts)
                    {
                        contactCount++;
                        contactLines.Add(new VertexPositionColor(information.Contact.Position, Color.White));
                        contactLines.Add(new VertexPositionColor(information.Contact.Position + information.Contact.Normal * information.Contact.PenetrationDepth, Color.Red));
                        contactLines.Add(new VertexPositionColor(information.Contact.Position + information.Contact.Normal * information.Contact.PenetrationDepth, Color.White));
                        contactLines.Add(new VertexPositionColor(information.Contact.Position + information.Contact.Normal * (information.Contact.PenetrationDepth + .3f), Color.White));
                    }
                }
            }

            if (contactCount > 0)
            {
                foreach (var pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    graphics.DrawUserPrimitives(PrimitiveType.LineList, contactLines.Elements, 0, contactLines.Count / 2);
                }
            }

        }
    }
}
