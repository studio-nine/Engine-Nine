#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using Nine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Nine.Test
{
    /// <summary>
    ///This is a test class for EnumerableCollectionTest and is intended
    ///to contain all EnumerableCollectionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnumerableCollectionTest
    {
        /// <summary>
        ///A test for Item
        ///</summary>
        public void ItemTestHelper<T>()
        {
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            IEnumerable target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<int> target = new EnumerableCollection<int>();
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
            EnumerableCollection<int> target = new EnumerableCollection<int>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<int> target = new EnumerableCollection<int>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<T> target = new EnumerableCollection<T>();
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
            EnumerableCollection<int> target = new EnumerableCollection<int>();
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
        public void ChangedEventTest()
        {
            int prev = 0;
            int index = 0;
            int expected = 0;
            EnumerableCollection<int> target = new EnumerableCollection<int>();
            target.Changed += (o, e) => { expected = e.Value; index = e.Index; prev = e.PreviousValue; };

            target.Add(0);
            target.Add(1);
            target.Add(2);

            target[2] = 4;

            Assert.AreEqual(2, index);
            Assert.AreEqual(2, prev);
            Assert.AreEqual(4, expected);
        }
    }
}
