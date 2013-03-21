namespace Nine.Studio.Test
{
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Studio;
    using Microsoft.Xna.Framework.Graphics;
    
    [TestClass]
    public class SerializationTest
    {
        string ContentDirectory = @"..\..\..\..\Samples\Content\";

        Editor Editor;
        Project Project;

        [TestInitialize]
        public void Initialize()
        {
            Editor = new Editor();
            Editor.LoadExtensions();
            Project = Editor.CreateProject("Project", Path.GetRandomFileName());
        }

        [TestCleanup]
        public void Cleanup()
        {
            Project.Close();
            Editor.Close();
        }

        [TestMethod]
        public void TestSaveLoadTexture()
        {
            var doc = Project.Import(Path.GetFullPath(Path.Combine(ContentDirectory, @"Textures\fire.png")));
            Texture2D imported = (Texture2D)doc.ObjectModel;
            Assert.AreEqual(256, imported.Width);
            Assert.AreEqual(256, imported.Height);
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject(Editor.RecentFiles[0]);
            Assert.AreEqual(1, Project.ProjectItems.Count);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is Texture2D);
        }

        [TestMethod]
        public void TestSaveLoadFbxModel()
        {
            var doc = Project.Import(Path.GetFullPath(Path.Combine(ContentDirectory, @"Models\Terrain\Terrain.fbx")));
            Model imported = (Model)doc.ObjectModel;
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject(Editor.RecentFiles[0]);
            Assert.IsTrue(Project.ProjectItems.Count > 0);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is Model);
            Assert.IsTrue(Project.ProjectItems[0].Importer.Value is Nine.Graphics.Design.FbxModelImporter);
        }

        [TestMethod]
        public void TestSaveLoadXModel()
        {
            var doc = Project.Import(Path.GetFullPath(Path.Combine(ContentDirectory, @"Models\Palm\AlphaPalm.X")));
            Model imported = (Model)doc.ObjectModel;
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject(Editor.RecentFiles[0]);
            Assert.IsTrue(Project.ProjectItems.Count > 0);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is Model);
            Assert.IsTrue(Project.ProjectItems[0].Importer.Value is Nine.Graphics.Design.XModelImporter);
        }

        [TestMethod]
        public void TestCreateSaveLoadScene()
        {
            var doc = Project.Create(new Scene());
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject(Editor.RecentFiles[0]);
            Assert.IsTrue(Project.ProjectItems.Count > 0);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is Scene);
            Assert.IsTrue(Project.ProjectItems[0].Importer.Value is Nine.Design.SceneImporter);
        }
    }
}
