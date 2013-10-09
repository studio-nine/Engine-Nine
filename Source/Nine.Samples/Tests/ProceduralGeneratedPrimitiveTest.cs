namespace Nine.Samples
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;
    using Nine.Graphics.Primitives;

    public class ProceduralGeneratedPrimitiveTest : Sample
    {
        public static int Count = 10;
        public static float AddInterval = 0.25f;

        public override Scene CreateScene(Microsoft.Xna.Framework.Graphics.GraphicsDevice graphics, Serialization.ContentLoader content)
        {
            var scene = new Scene();

            scene.Add(new AmbientLight(graphics) { AmbientLightColor = new Vector3(0.2f, 0.2f, 0.2f) });
            scene.Add(new Nine.Graphics.DirectionalLight(graphics) { DiffuseColor = new Vector3(0.8f, 0.8f, 0.8f), Direction = new Vector3(-1, -1, -1) });

            scene.Add(new AddPrmitiveComponent(graphics));

            return scene;
        }
    }

    class AddPrmitiveComponent : Component
    {
        float intervalTimer = 0;
        int addedY = 0;
        int addedX = 0;

        GraphicsDevice Graphics;
        public AddPrmitiveComponent(GraphicsDevice graphics)
        {
            this.Graphics = graphics;
        }

        protected override void Update(float elapsedTime)
        {
            intervalTimer += elapsedTime;
            if (intervalTimer > ProceduralGeneratedPrimitiveTest.AddInterval &&
                addedY <= ProceduralGeneratedPrimitiveTest.Count)
            {
                Scene.Add(new RandomGeneratedPrimitive(Graphics)
                    {
                        Transform = Matrix.CreateTranslation(
                        ProceduralGeneratedPrimitiveTest.Count * addedX, 
                        0,
                        ProceduralGeneratedPrimitiveTest.Count * addedY)
                    });
                addedX++;
                if (addedX >= ProceduralGeneratedPrimitiveTest.Count)
                {
                    addedY++;
                    addedX = 0;
                }
                intervalTimer = 0;
            }
        }
    }

    class RandomGeneratedPrimitive : Primitive<VertexPositionNormalTexture>
    {
        public RandomGeneratedPrimitive(GraphicsDevice graphics)
            : base(graphics)
        {

        }

        protected override void OnBuild()
        {
            // I just placed this here for reference
            //System.Threading.Thread.Sleep(100);
            
            var rd = new Random();
            for (int x = 0; x < ProceduralGeneratedPrimitiveTest.Count; x++)
            {
                for (int z = 0; z < ProceduralGeneratedPrimitiveTest.Count; z++)
                {
                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 1);
                    AddIndex(CurrentVertex + 2);

                    AddIndex(CurrentVertex + 0);
                    AddIndex(CurrentVertex + 2);
                    AddIndex(CurrentVertex + 3);

                    AddVertex(new Vector3(x * 1 + 0, (float)rd.NextDouble(), z * 1 + 0), new Vector2(0, 0));
                    AddVertex(new Vector3(x * 1 + 1, (float)rd.NextDouble(), z * 1 + 0), new Vector2(1, 0));
                    AddVertex(new Vector3(x * 1 + 1, (float)rd.NextDouble(), z * 1 + 1), new Vector2(1, 1));
                    AddVertex(new Vector3(x * 1 + 0, (float)rd.NextDouble(), z * 1 + 1), new Vector2(0, 1));
                }
            }
        }

        private void AddVertex(Vector3 position, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture()
            {
                Position = position,
                Normal = Vector3.Zero,
                TextureCoordinate = uv,
            });
        }
    }
}
