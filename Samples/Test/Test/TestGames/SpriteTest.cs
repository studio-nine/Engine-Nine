namespace Test
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine;
    using Nine.Components;
    using Nine.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.Cameras;

    public class SpriteTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            scene.Add(new Camera2D(graphics));

            var texture = content.Load<Texture2D>("Textures/Butterfly");
            var link = new Group();
            scene.Add(link);
            scene.Add(new TextSprite(graphics) { Text = "Engine Nine", Font = content.Load<SpriteFont>("Consolas") });

            var size = 8;
            var step = 50;
            for (var i = 0; i < size; i++)
            {
                var child = new Group();
                child.Transform = Matrix.CreateRotationZ(MathHelper.ToRadians(10)) * Matrix.CreateTranslation(step, 0, 0);
                child.Add(new Sprite(graphics)
                {
                    Alpha = 0.5f,
                    Texture = texture,
                    Color = Color.Yellow,
                    Anchor = new Vector2(0, 0),
                    Size = new Vector2(25, 50),
                    Scale = new Vector2(2, 1),
                    //Material = new BasicMaterial(graphics) { VertexColorEnabled = true },
                    BlendState = BlendState.Opaque,
                    SamplerState = SamplerState.LinearWrap,
                    SourceRectangle = new Rectangle(0, 0, texture.Width * 2, texture.Height * 2),
                });
                link.Add(child);
                link = child;
            }

            return scene;
        }
    }
}
