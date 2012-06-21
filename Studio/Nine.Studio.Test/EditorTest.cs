using System;
using System.ComponentModel;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nine.Studio;
using Nine.Studio.Extensibility;
using Nine.Studio.Serializers;
using System.IO;

namespace Nine.Studio.Test
{
    [TestClass]
    public class EditorTest
    {
        const string ProjectName = "TestProject";
        const string WorldFactory = "WorldFactory";

        [TestMethod]
        public void TestNewProjectSaveLoad()
        {
            Editor editor = Editor.Launch();
            editor.Extensions.LoadDefault();

            var project = editor.CreateProject(ProjectName);
            project.Save();
            editor.Close();

            Assert.IsTrue(File.Exists(project.FileName));
        }

        [TestMethod]
        public void TestNewProjectNewWorldSaveLoad()
        {
            Editor editor = Editor.Launch();
            editor.Extensions.LoadDefault();
            
            var project = editor.CreateProject(ProjectName);
            var projectItem = project.CreateProjectItem(WorldFactory);

            Assert.IsNotNull(projectItem.ObjectModel);
            
            project.Save();
            editor.Close();

            Assert.AreEqual(0, editor.Projects.Count);
            project = editor.OpenProject(project.FileName);

            Assert.IsNotNull(project.ProjectItems[0].ObjectModel);
        }

        [TestMethod]
        [DeploymentItem(@"Content\TerrainTex.png")]
        public void TestNewProjectImportTextureSaveLoad()
        {
            Editor editor = Editor.Launch();
            editor.Extensions.LoadDefault();

            var project = editor.CreateProject(ProjectName);

            project.Import(@"TerrainTex.png");
            project.Save();
            editor.Close();
            
            Assert.AreEqual(0, editor.Projects.Count);
            project = editor.OpenProject(project.FileName);

            Assert.IsNotNull(project.ProjectItems[0].ObjectModel);
        }

        [TestMethod]
        public void TestNewProjectNewDocumentAddReferenceSaveLoad()
        {
            Editor editor = Editor.Launch();
            editor.Extensions.LoadDefault();
            Project p1 = editor.CreateProject("Proj1.xml");
            Project p2 = editor.CreateProject("Proj2.xml");
            ProjectItem d1 = p1.CreateProjectItem("Hi");
            ProjectItem d2 = p2.CreateProjectItem("Bye");
            d1.AddReference(d2);
            Assert.IsTrue(d1.References.Contains(d2));
            Assert.IsTrue(p1.References.Contains(p2));
            d2.Save("Doc2.txt");
            p2.Save();
            d1.Save("Doc1.txt");
            p1.Save();
            editor.Close();
            Assert.AreEqual(0, editor.Projects.Count);
            p1 = editor.OpenProject("Proj1.xml");
            Assert.AreEqual(2, editor.Projects.Count);
            Assert.AreEqual(1, p1.ProjectItems.Count);
            Assert.AreEqual(1, p1.ProjectItems[0].References.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNewProjectAddCircularReference()
        {
            Editor editor = Editor.Launch();
            Project p1 = editor.CreateProject("p1");
            Project p2 = editor.CreateProject("p2");
            Project p3 = editor.CreateProject("p3");
            p1.AddReference(p2);
            p2.AddReference(p3);
            p3.AddReference(p1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNewProjectNewDocumentAddCircularReference()
        {
            Editor editor = Editor.Launch();
            Project p1 = editor.CreateProject("p1");
            Project p2 = editor.CreateProject("p2");
            ProjectItem d1 = p1.CreateProjectItem("Hi");
            ProjectItem d2 = p2.CreateProjectItem("Bye");
            d1.AddReference(d2);
            d2.AddReference(d1);
        }

        class TestAttributeProvider : AttributeProvider<TestAttributeProvider>
        {
            public string Name { get; set; }

            public TestAttributeProvider()
            {
                AddCustomAttribute("Name", new DisplayNameAttribute("DisplayName"),
                                           new CategoryAttribute("Categorized"));
            }
        }

        [TestMethod]
        public void TestCustomAttributeProvider()
        {
            Editor editor = Editor.Launch();
            editor.Extensions.AttributeProviders = new IAttributeProvider[] { new TestAttributeProvider() };

            var properties = TypeDescriptor.GetProperties(new TestAttributeProvider())
                                           .OfType<PropertyDescriptor>()
                                           .Where(p => !p.IsReadOnly).ToList();
            
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("DisplayName", properties[0].DisplayName);
            Assert.AreEqual("Categorized", properties[0].Category);
        }
    }
}
