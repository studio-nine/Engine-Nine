namespace Nine.Content.Pipeline.Processors
{
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Materials.MaterialParts;
    using Nine.Graphics;

    [DefaultContentProcessor]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class InstancedModelProcessor : ContentProcessor<InstancedModel, InstancedModel>
    {
        public override InstancedModel Process(InstancedModel input, ContentProcessorContext context)
        {
            if (input != null && input.Template != null)
            {
                for (int i = 0; i < input.Template.Count; i++)
                {
                    var material = input.Template.GetMaterial(i);
                    while (material != null)
                    {
                        var mg = material as MaterialGroup;
                        if (mg != null && !mg.MaterialParts.OfType<InstancedMaterialPart>().Any())
                            mg.MaterialParts.Add(new InstancedMaterialPart());
                        material = material.NextMaterial;
                    }
                }
            }
            return input;
        }
    }
}
