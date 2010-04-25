// -----------------------------------------------------------------------------
// Changes to this file will not be modified by the code generator.
// -----------------------------------------------------------------------------
namespace Isles.Graphics.Effects
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    
    public partial class SkinnedEffect
    {
        
        public SkinnedEffect(GraphicsDevice graphicsDevice) : 
                this(graphicsDevice, null)
        {
        }
        
        public SkinnedEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
