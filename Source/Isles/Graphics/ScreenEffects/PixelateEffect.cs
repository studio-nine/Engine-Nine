// -----------------------------------------------------------------------------
// Changes to this file will not be modified by the code generator.
// -----------------------------------------------------------------------------
namespace Isles.Graphics.ScreenEffects
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    
    public partial class PixelateEffect
    {
        
        public PixelateEffect(GraphicsDevice graphicsDevice) : 
                this(graphicsDevice, null)
        {
        }
        
        public PixelateEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
