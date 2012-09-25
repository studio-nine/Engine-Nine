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
    using Nine.Graphics.PostEffects;

    public class PixelPerfectTest : ITestGame
    {
        public Scene CreateTestScene(GraphicsDevice graphics, ContentManager content)
        {
            var scene = new Scene();
            var bits = content.Load<Texture2D>("Textures/Bits");
            scene.Add(new Camera2D(graphics));
            scene.Add(new FullScreenQuad(graphics) { Texture = bits, Material = new BasicMaterial(graphics) { SamplerState = SamplerState.PointClamp } });
            //scene.Add(new Sprite(graphics) { Texture = bits, Size = new Vector2(graphics.Viewport.Width / 2, graphics.Viewport.Height / 2), Material = new BasicMaterial(graphics) { SamplerState = SamplerState.PointClamp }, Anchor = Vector2.Zero });
            //scene.Add(new Sprite(graphics) { Texture = bits, Anchor = Vector2.Zero });
            return scene;
        }
    }
}
