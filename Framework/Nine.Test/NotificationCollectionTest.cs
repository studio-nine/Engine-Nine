#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;

namespace Nine.Test
{
    /// <summary>
    ///This is a test class for EnumerableCollectionTest and is intended
    ///to contain all EnumerableCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NotificationCollectionTest : ContentPipelineTest
    {
        /// <summary>
        ///A test for Item
        ///</summary>
        public void ItemTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            int index = 0;
            T expected = default(T);
            T actual;
            target[index] = expected;
            actual = target[index];
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ItemTest()
        {
            ItemTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for IsReadOnly
        ///</summary>
        public void IsReadOnlyTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            bool actual;
            actual = target.IsReadOnly;
            Assert.AreEqual(false, actual);
        }

        [TestMethod()]
        public void IsReadOnlyTest()
        {
            IsReadOnlyTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Count
        ///</summary>
        public void CountTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            target.Add(default(T));
            target.Add(default(T));
            int actual;
            actual = target.Count;
            Assert.AreEqual(3, actual);
        }

        [TestMethod()]
        public void CountTest()
        {
            CountTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for System.Collections.IEnumerable.GetEnumerator
        ///</summary>
        public void GetEnumeratorTestHelper<T>()
        {
            IEnumerable target = new NotificationCollection<T>() { EnableManipulationWhenEnumerating = true };
            IEnumerator expected = null;
            IEnumerator actual;
            actual = target.GetEnumerator();
            Assert.AreNotEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("Nine.dll")]
        public void GetEnumeratorTest()
        {
            GetEnumeratorTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for RemoveAt
        ///</summary>
        public void RemoveAtTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            target.Add(default(T));
            int index = 1;
            target.RemoveAt(index);
            Assert.AreEqual(1, target.Count);
        }

        [TestMethod()]
        public void RemoveAtTest()
        {
            RemoveAtTestHelper<GenericParameterHelper>();
        }


        [TestMethod()]
        public void RemoveAllTest()
        {
            NotificationCollection<int> target = new NotificationCollection<int>();
            target.Add(1);
            target.Add(2);
            target.Add(2);
            target.Add(2);
            target.Add(5);
            Predicate<int> match = delegate(int v) { return v == 2; };
            int expected = 3;
            int actual;
            actual = target.RemoveAll(match);
            Assert.AreEqual(expected, actual);
        }


        [TestMethod()]
        public void RemoveTest()
        {
            NotificationCollection<int> target = new NotificationCollection<int>();
            target.Add(1);
            target.Add(2);
            target.Add(2);
            target.Add(2);
            target.Add(5);
            bool actual;
            actual = target.Remove(2);
            Assert.AreEqual(true, actual);
            Assert.AreEqual(4, target.Count);
        }

        /// <summary>
        ///A test for Insert
        ///</summary>
        public void InsertTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            int index = 0;
            T item = default(T);
            target.Insert(index, item);
            Assert.AreEqual(1, target.Count);
        }

        [TestMethod()]
        public void InsertTest()
        {
            InsertTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for IndexOf
        ///</summary>
        public void IndexOfTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            T item = default(T);
            int expected = 0;
            int actual;
            actual = target.IndexOf(item);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void IndexOfTest()
        {
            IndexOfTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for GetEnumerator
        ///</summary>
        public void GetEnumeratorTest1Helper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.EnableManipulationWhenEnumerating = true;
            IEnumerator<T> expected = null;
            IEnumerator<T> actual;
            actual = target.GetEnumerator();
            Assert.AreNotEqual(expected, actual);
        }

        [TestMethod()]
        public void GetEnumeratorTest1()
        {
            GetEnumeratorTest1Helper<GenericParameterHelper>();
        }


        [TestMethod()]
        public void CopyToTest()
        {
            NotificationCollection<int> target = new NotificationCollection<int>();
            target.Add(1);
            int[] array = new int[3];
            int arrayIndex = 1;
            target.CopyTo(array, arrayIndex);
            Assert.AreEqual(1, array[1]);
            Assert.AreEqual(0, array[0]);
            Assert.AreEqual(0, array[2]);
        }

        /// <summary>
        ///A test for Contains
        ///</summary>
        public void ContainsTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            T item = default(T);
            bool expected = true;
            bool actual;
            actual = target.Contains(item);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ContainsTest()
        {
            ContainsTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Clear
        ///</summary>
        public void ClearTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            target.Add(default(T));
            target.Clear();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod()]
        public void ClearTest()
        {
            ClearTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for Add
        ///</summary>
        public void AddTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            T e = default(T);
            target.Add(e);
            Assert.AreEqual(1, target.Count);
        }

        [TestMethod()]
        public void AddTest()
        {
            AddTestHelper<GenericParameterHelper>();
        }

        /// <summary>
        ///A test for EnumerableCollection`1 Constructor
        ///</summary>
        public void EnumerableCollectionConstructorTestHelper<T>()
        {
            NotificationCollection<T> target = new NotificationCollection<T>();
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod()]
        public void EnumerableCollectionConstructorTest()
        {
            EnumerableCollectionConstructorTestHelper<GenericParameterHelper>();
        }

        [TestMethod()]
        public void AddedRemovedEventTest()
        {
            int count = 0;
            NotificationCollection<int> target = new NotificationCollection<int>();
            target.Added += (o, e) => { count++; };
            target.Removed += (o, e) => { count--; };

            target.Add(0);
            target.Clear();
            target.Add(0);
            target.Insert(1, 2);
            target.Insert(1, 2);
            target.Remove(0);
            target.RemoveAll((v) => { return v == 2; });
            target.Add(1);
            target.Add(2);
            target.RemoveAt(1);

            Assert.AreEqual(1, target.Count);
        }

        [TestMethod()]
        public void MultiLayerEnumerationTest()
        {
            NotificationCollection<int> target = new NotificationCollection<int>()
            {
                1, 2, 3, 4, 5, 6, 7
            };
            target.EnableManipulationWhenEnumerating = true;

            foreach (int a in target)
            {
                target.Remove(1);
                foreach (int b in target)
                {
                    target.Remove(7);
                    foreach (int c in target)
                    {
                        target.Remove(3);
                        foreach (int d in target)
                        {
                            target.Remove(4);
                        }
                    }
                }
            }

            Assert.IsTrue(!target.Contains(1));
            Assert.IsTrue(!target.Contains(7));
            Assert.IsTrue(!target.Contains(3));
            Assert.IsTrue(!target.Contains(4));

            Assert.IsTrue(target.Contains(2));
            Assert.IsTrue(target.Contains(5));
            Assert.IsTrue(target.Contains(6));

            List<int> copy = new List<int>(target);

            Assert.IsTrue(!copy.Contains(1));
            Assert.IsTrue(!copy.Contains(7));
            Assert.IsTrue(!copy.Contains(3));
            Assert.IsTrue(!copy.Contains(4));

            Assert.IsTrue(copy.Contains(2));
            Assert.IsTrue(copy.Contains(5));
            Assert.IsTrue(copy.Contains(6));
        }

        [TestMethod]
        public void SerializationTest()
        {
            var world = new World();
            world.Add(new WorldObject() { Name = "aaaa" });
            //world.Add(new WorldObject(new object[] { 89, "xx", }) { Name = "Yan" });
            //world.Add(new WorldObject(new object[] { 87, "xy", }) { Name = "Fei" });

            var file = BuildObjectUsingDefaultContentProcessor(world);
            RunTheBuild();

            var loaded = Content.Load<World>(file);

            /*
            using (var writer = XmlWriter.Create("C:\\a.xml"))
            {
                IntermediateSerializer.Serialize(writer, world, null);
            }

            Assert.AreEqual(world.WorldObjects.Count, loaded.WorldObjects.Count);
            Assert.AreEqual(world.WorldObjects[0].Name, loaded.WorldObjects[0].Name);
            Assert.AreEqual(world.WorldObjects[0].Components.Count, loaded.WorldObjects[0].Components.Count);
            Assert.AreEqual(world.WorldObjects[0].Components[0], loaded.WorldObjects[0].Components[0]);
            Assert.AreEqual(world.WorldObjects[0].Components[1], loaded.WorldObjects[0].Components[1]);
            Assert.AreEqual(world.WorldObjects[1].Name, loaded.WorldObjects[1].Name);
            Assert.AreEqual(world.WorldObjects[1].Components.Count, loaded.WorldObjects[1].Components.Count);
            Assert.AreEqual(world.WorldObjects[1].Components[0], loaded.WorldObjects[1].Components[0]);
             */
        }
    }
}
