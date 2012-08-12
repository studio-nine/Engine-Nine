using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nine;

namespace CubeStressTest
{
    class PassThroughSceneManager : ISceneManager<ISpatialQueryable>
    {
        List<ISpatialQueryable> list = new List<ISpatialQueryable>();

        public void Add(ISpatialQueryable item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(ISpatialQueryable item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(ISpatialQueryable[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(ISpatialQueryable item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<ISpatialQueryable> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void FindAll(Microsoft.Xna.Framework.BoundingFrustum boundingFrustum, ICollection<ISpatialQueryable> result)
        {
            var count = list.Count;
            for (int i = 0; i < count; i++)
                result.Add(list[i]);
        }

        public void FindAll(ref Microsoft.Xna.Framework.BoundingBox boundingBox, ICollection<ISpatialQueryable> result)
        {
            throw new NotImplementedException();
        }

        public void FindAll(ref Microsoft.Xna.Framework.BoundingSphere boundingSphere, ICollection<ISpatialQueryable> result)
        {
            throw new NotImplementedException();
        }

        public void FindAll(ref Microsoft.Xna.Framework.Ray ray, ICollection<ISpatialQueryable> result)
        {
            throw new NotImplementedException();
        }
    }
}
