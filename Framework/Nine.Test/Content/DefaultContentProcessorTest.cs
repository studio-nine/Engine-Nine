#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework.Content.Pipeline;

namespace Nine.Content.Pipeline.Test
{
    [TestClass]
    public class DefaultContentProcessorTest : ContentPipelineTest
    {
        public class TestContent
        {
            [SelfProcess]
            static int Process(TestContent input, ContentProcessorContext context)
            {
                return 10;
            }
        }

        public class TestContentContainer
        {
            public object Content { get; set; }
        }

        [TestMethod()]
        public void DefaultContentProcessorBasicTest()
        {
            Test(() =>
            {   
                var content = new TestContent();
                var built = BuildObjectUsingDefaultContentProcessor(content);
                RunTheBuild();

                Assert.AreEqual(10, Content.Load<int>(built));
            }); 
        }

        [TestMethod()]
        public void DefaultContentProcessorPropertyTest()
        {
            Test(() =>
            {
                var content = new TestContentContainer() { Content = new TestContent() };
                var built = BuildObjectUsingDefaultContentProcessor(content);
                RunTheBuild();

                var load = Content.Load<TestContentContainer>(built);
                Assert.AreEqual(10, load.Content);
            });
        }

        [TestMethod()]
        public void DefaultContentProcessorListTest()
        {
            Test(() =>
            {
                var content = new List<object>();
                content.AddRange(Enumerable.Range(0, 10).Select(i => new TestContent()));
                var built = BuildObjectUsingDefaultContentProcessor(content);
                RunTheBuild();

                var list = Content.Load<List<object>>(built);
                Assert.AreEqual(10, list.Count);
                Assert.AreEqual(10, list[0]);
            });
        }

        [TestMethod()]
        public void DefaultContentProcessorListPropertyTest()
        {
            Test(() =>
            {
                var content = new List<object>();
                content.AddRange(Enumerable.Range(0, 10).Select(i => new TestContent()));
                var built = BuildObjectUsingDefaultContentProcessor(new TestContentContainer() { Content = content });
                RunTheBuild();

                var list = (List<object>)Content.Load<TestContentContainer>(built).Content;
                Assert.AreEqual(10, list.Count);
                Assert.AreEqual(10, list[0]);
            });
        }

        [TestMethod()]
        public void DefaultContentProcessorDictionaryTest()
        {
            Test(() =>
            {
                var content = new Dictionary<string, object>();
                content.Add("A", new TestContent());
                var built = BuildObjectUsingDefaultContentProcessor(content);
                RunTheBuild();

                var dict = Content.Load<Dictionary<string, object>>(built);
                Assert.AreEqual(1, dict.Count);
                Assert.AreEqual(10, dict["A"]);
            });
        }

        [TestMethod()]
        public void DefaultContentProcessorDictionaryPropertyTest()
        {
            Test(() =>
            {
                var content = new Dictionary<string, object>();
                content.Add("A", new TestContent());
                var built = BuildObjectUsingDefaultContentProcessor(new TestContentContainer() { Content = content });
                RunTheBuild();

                var dict = (Dictionary<string, object>)Content.Load<TestContentContainer>(built).Content;
                Assert.AreEqual(1, dict.Count);
                Assert.AreEqual(10, dict["A"]);
            });
        }
    }
}
