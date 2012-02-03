using System;
using System.Collections.Generic;
using Nine;
using Nine.Navigation;

namespace Navigators
{
    public class Neighbors : List<Navigator>, ISpatialQuery<Navigator>
    {
        public void FindAll(ref Microsoft.Xna.Framework.BoundingFrustum boundingFrustum, ICollection<Navigator> result)
        {
            throw new NotImplementedException();
        }

        public void FindAll(ref Microsoft.Xna.Framework.BoundingBox boundingBox, ICollection<Navigator> result)
        {
            throw new NotImplementedException();
        }

        public void FindAll(ref Microsoft.Xna.Framework.BoundingSphere boundingSphere, ICollection<Navigator> result)
        {
            foreach (var n in this)
            {
                var distance = Math.Sqrt(Math.Pow(n.Position.X - boundingSphere.Center.X, 2) + Math.Pow(n.Position.Y - boundingSphere.Center.Y, 2));
                if (distance < boundingSphere.Radius)
                    result.Add(n);
            }
        }

        public void FindAll(ref Microsoft.Xna.Framework.Ray ray, ICollection<Navigator> result)
        {
            throw new NotImplementedException();
        }
    }
}
