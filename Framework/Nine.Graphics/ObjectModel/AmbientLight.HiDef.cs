namespace Nine.Graphics.ObjectModel
{/*

    public partial class AmbientLight : IDeferredLight
    {
        AmbientLightEffect multiPasseffect;
        public override Effect MultiPassEffect
        {
            get { return multiPasseffect ?? (multiPasseffect = GraphicsResources<AmbientLightEffect>.GetInstance(GraphicsDevice)); }
        }

        DeferredAmbientLight deferredLight;
        DeferredAmbientLight GetDeferredLight()
        {
            return deferredLight ?? (deferredLight = GraphicsResources<DeferredAmbientLight>.GetInstance(GraphicsDevice));
        }

        Effect IDeferredLight.Effect
        {
            get
            {
                GetDeferredLight();
                deferredLight.AmbientLightColor = AmbientLightColor;
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