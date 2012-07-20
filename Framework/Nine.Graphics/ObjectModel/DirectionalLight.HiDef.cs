namespace Nine.Graphics.ObjectModel
{/*

    public partial class DirectionalLight : IDeferredLight
    {
        DirectionalLightEffect multiPasseffect;
        public override Effect MultiPassEffect
        {
            get { return multiPasseffect ?? (multiPasseffect = GraphicsResources<DirectionalLightEffect>.GetInstance(GraphicsDevice)); }
        }

        DeferredDirectionalLight deferredLight;
        DeferredDirectionalLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = GraphicsResources<DeferredDirectionalLight>.GetInstance(GraphicsDevice));
        }

        Effect IDeferredLight.Effect
        {
            get
            {
                GetDeferredLight();
                deferredLight.Direction = Direction;
                deferredLight.SpecularColor = SpecularColor;
                deferredLight.DiffuseColor = DiffuseColor;
                return ((IDeferredLight)GetDeferredLight()).Effect;
            }
        }

        IndexBuffer IDeferredLight.IndexBuffer
        {
            get { return ((IDeferredLight)GetDeferredLight()).IndexBuffer; }
        }

        VertexBuffer IDeferredLight.VertexBuffer
        {
            get { return ((IDeferredLight)GetDeferredLight()).VertexBuffer; }
        }
    }*/
}