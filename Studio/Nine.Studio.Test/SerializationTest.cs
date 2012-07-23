namespace Nine.Studio.Test
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
    using Microsoft.Xna.Framework.Content.Pipeline.Processors;
    using Nine.Studio;


    [TestClass]
    public class SerializationTest
    {
        Editor Editor;
        Project Project;

        [TestInitialize]
        public void Initialize()
        {
            Editor = Editor.Launch();
            Editor.Extensions.LoadDefault();
            Project = Editor.CreateProject("Project.xml");
        }

        [TestCleanup]
        public void Cleanup()
        {
            Project.Close();
            Editor.Close();
        }

        [TestMethod]        
        public void TestSaveLoadText()
        {
            var doc = Project.CreateProjectItem("HaHa");
            doc.Save("Doc.txt");
            doc = Project.Import("Doc.txt");
            Assert.AreEqual("HaHa", doc.ObjectModel);
        }

        [TestMethod]
        [DeploymentItem(@"Content/Background.png", "Content")]
        public void TestSaveLoadTexture()
        {
            var doc = Project.Import(@"Content/Background.png");
            TextureContent imported = (TextureContent)doc.ObjectModel;
            Assert.AreEqual(173, imported.Faces[0][0].Width);
            Assert.AreEqual(173, imported.Faces[0][0].Height);
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject("Project.xml");
            Assert.AreEqual(1, Project.ProjectItems.Count);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is TextureContent);
        }

        [TestMethod]
        [DeploymentItem(@"Content/Terrain.fbx", "Content")]
        [DeploymentItem(@"Content/TerrainTex.png", "Content")]
        public void TestSaveLoadFbxModel()
        {
            var doc = Project.Import(@"Content/Terrain.fbx");
            ModelContent imported = (ModelContent)doc.ObjectModel;
            Project.Save();
            Project.Close();
            Project = Editor.OpenProject("Project.xml");
            Assert.IsTrue(Project.ProjectItems.Count > 0);
            Assert.IsTrue(Project.ProjectItems[0].ObjectModel is ModelContent);
        }

        [TestMethod]
        public void TestNewSaveLoadXnaXmlContent()
        {
            var doc = Project.CreateProjectItem(Matrix.Identity);
            doc.Save("Doc.xml");
            doc.Close();
            doc = Project.Import("Doc.xml");
            Assert.AreEqual(Matrix.Identity, doc.ObjectModel);
        }
    }
}
