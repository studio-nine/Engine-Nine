namespace Nine.Studio.Test
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Nine.Studio;
    using Nine.Studio.Extensibility;
    using Nine.Graphics.Design;
    using Nine.Design;

    [TestClass]
    public class EditorTest
    {
        const string ProjectName = "TestProject";

        [TestMethod]
        public void TestNewProjectSaveLoad()
        {
            var editor = new Editor();
            editor.LoadExtensions();

            var project = editor.CreateProject("Project", Path.GetRandomFileName());
            project.Save();
            editor.Close();

            Assert.IsTrue(File.Exists(project.FileName));
        }

        [TestMethod]
        public void TestNewProjectNewSceneSaveLoad()
        {
            var editor = new Editor();
            editor.LoadExtensions();

            var project = editor.CreateProject("Project", Path.GetRandomFileName());
            var projectItem = project.Create(new SceneFactory());

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
            var editor = new Editor();
            editor.LoadExtensions();

            var project = editor.CreateProject("Project", Path.GetRandomFileName());

            project.Import(Path.GetFullPath(@"TerrainTex.png"));
            project.Save();
            editor.Close();
            
            Assert.AreEqual(0, editor.Projects.Count);
            project = editor.OpenProject(project.FileName);

            Assert.IsNotNull(project.ProjectItems[0].ObjectModel);
        }

        [TestMethod]
        public void TestNewProjectNewSceneAddReferenceSaveLoad()
        {
            var editor = new Editor();
            editor.LoadExtensions();
            
            var p1 = editor.CreateProject("Proj1", Path.GetRandomFileName());
            var p2 = editor.CreateProject("Proj2", Path.GetRandomFileName());

            var d1 = p1.Create(new Scene());
            var d2 = p2.Create(new Scene());

            d1.AddReference(d2);

            Assert.IsTrue(d1.References.Contains(d2));
            Assert.IsTrue(p1.References.Contains(p2));

            p2.Save();
            p1.Save();
            editor.Close();

            Assert.AreEqual(0, editor.Projects.Count);
            
            p1 = editor.OpenProject(editor.RecentFiles[0]);

            Assert.AreEqual(2, editor.Projects.Count);
            Assert.AreEqual(1, p1.ProjectItems.Count);
            Assert.AreEqual(1, p1.ProjectItems[0].References.Count);
        }
        
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNewProjectAddCircularReference()
        {
            var editor = new Editor();
            Project p1 = editor.CreateProject("p1", Path.GetRandomFileName());
            Project p2 = editor.CreateProject("p2", Path.GetRandomFileName());
            Project p3 = editor.CreateProject("p3", Path.GetRandomFileName());
            p1.AddReference(p2);
            p2.AddReference(p3);
            p3.AddReference(p1);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void TestNewProjectNewSceneAddCircularReference()
        {
            var editor = new Editor();
            editor.LoadExtensions();
            Project p1 = editor.CreateProject("p1", Path.GetRandomFileName());
            Project p2 = editor.CreateProject("p2", Path.GetRandomFileName());
            ProjectItem d1 = p1.Create(new Scene());
            ProjectItem d2 = p2.Create(new Scene());
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
            var editor = new Editor();
            editor.Extensions.AttributeProviders.Add(new TestAttributeProvider());

            var properties = TypeDescriptor.GetProperties(new TestAttributeProvider())
                                           .OfType<PropertyDescriptor>()
                                           .Where(p => !p.IsReadOnly).ToList();
            
            Assert.AreEqual(1, properties.Count);
            Assert.AreEqual("DisplayName", properties[0].DisplayName);
            Assert.AreEqual("Categorized", properties[0].Category);
        }
    }
}
