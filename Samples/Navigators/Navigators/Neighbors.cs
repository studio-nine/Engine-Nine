using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine.Navigation;
using Nine;

namespace Navigators
{
    public class Neighbors : List<Navigator>, ISpatialQuery<Navigator>
    {

        public IEnumerable<Navigator> FindAll(Microsoft.Xna.Framework.BoundingFrustum frustum)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Navigator> FindAll(Microsoft.Xna.Framework.BoundingBox boundingBox)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Navigator> FindAll(Microsoft.Xna.Framework.Ray ray)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Navigator> FindAll(Microsoft.Xna.Framework.Vector3 position, float radius)
        {
            List<Navigator> list = new List<Navigator>();

            ForEach(n =>
            {
                var distance = Math.Sqrt(Math.Pow(n.Position.X - position.X, 2) + Math.Pow(n.Position.Y - position.Y, 2));
                if (distance < radius)
                    list.Add(n);
            });
            return list;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
