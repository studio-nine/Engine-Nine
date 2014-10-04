namespace Nine.Samples
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI;
    using Nine.Graphics.UI.Controls;
    
    public class UIPassTest : Sample
    {
        class ImageScenePass : Image
        {
            public Scene Scene
            {
                get { return ScenePass.Scene; }
                set { ScenePass.Scene = value; }
            }
            public Nine.Graphics.UI.ScenePass ScenePass;

            private GraphicsDevice graphics;

            public ImageScenePass(GraphicsDevice graphics, Scene scene)
                : base()
            {
                ScenePass = new ScenePass(scene);
                this.graphics = graphics;
            }

            protected override void OnDraw(Graphics.UI.Renderer.Renderer renderer)
            {
                this.Source = ScenePass.InputTexture;
                base.OnDraw(renderer);
            }
        }

        public override string Title { get { return "[UI] Pass Test"; } }
        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
            var scene = new Scene();

            var inlineScene = new Scene();
            inlineScene.Add(new FreeCamera(graphics, new Vector3(0, 10, 40)) { InputEnabled = true });
            Nine.Graphics.SceneExtensions.SetDrawingContext(inlineScene, new DrawingContext3D(graphics, inlineScene) { 
                BackgroundColor = Color.Green 
            });

            inlineScene.Add(new Box(graphics));

            ImageScenePass imageScene;

            Grid grid;
            Border border1;
            var window = new Window(scene)
            {
                Content = grid = new Grid(
                    new ColumnDefinition[] {
                        new ColumnDefinition(1),
                        new ColumnDefinition(3),
                        new ColumnDefinition(1),
                    },
                    new RowDefinition[] {
                        new RowDefinition(1), 
                        new RowDefinition(8), 
                        new RowDefinition(1), 
                    },
                    new UIElement[] {
                        border1 = new Border() {
                            Content = imageScene = new ImageScenePass(graphics, inlineScene) { 

                            }
                        }
                    }) {
                    }
            };
            scene.Add(window);

            Grid.SetColumn(border1, 1);
            Grid.SetRow(border1, 1);


            window.AddDependency(imageScene.ScenePass);
            scene.Add(imageScene.ScenePass);

            return scene;
        }
    }
}
