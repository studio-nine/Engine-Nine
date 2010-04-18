// -----------------------------------------------------------------------------
// Changes to this file will not be modified by the code generator.
// -----------------------------------------------------------------------------
namespace Isles.Graphics.ScreenEffects
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    
    
    public partial class SilouetteEffect
    {
        
        public SilouetteEffect(GraphicsDevice graphicsDevice) : 
                this(graphicsDevice, null)
        {
        }
        
        public SilouetteEffect(GraphicsDevice graphicsDevice, EffectPool effectPool) : 
                base(graphicsDevice, effectCode, CompilerOptions.None, effectPool)
        {
            InitializeComponent();
        }
    }
}
