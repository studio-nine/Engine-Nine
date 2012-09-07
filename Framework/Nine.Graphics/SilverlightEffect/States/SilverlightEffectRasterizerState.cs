// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Xna.Framework.Graphics
{
    internal class SilverlightEffectRasterizerState : SilverlightEffectState
    {
        #region Properties

        public CullMode? CullMode;
        public FillMode? FillMode;
        public bool? ScissorTestEnable;
        public bool? MultiSampleAntiAlias;
        public float? DepthBias;
        public float? SlopeScaleDepthBias;

        #endregion

        #region Methods

        public void Affect(GraphicsDevice device, RasterizerState currentState)
        {
            RasterizerState internalState = new RasterizerState();

            // CullMode
            internalState.CullMode = CullMode.HasValue ? CullMode.Value : currentState.CullMode;

            // FillMode
            internalState.FillMode = FillMode.HasValue ? FillMode.Value : currentState.FillMode;

            // ScissorTestEnable
            internalState.ScissorTestEnable = ScissorTestEnable.HasValue ? ScissorTestEnable.Value : currentState.ScissorTestEnable;

            // MultiSampleAntiAlias
            internalState.MultiSampleAntiAlias = MultiSampleAntiAlias.HasValue ? MultiSampleAntiAlias.Value : currentState.MultiSampleAntiAlias;

            // DepthBias
            internalState.DepthBias = DepthBias.HasValue ? DepthBias.Value : currentState.DepthBias;

            // SlopeScaleDepthBias
            internalState.SlopeScaleDepthBias = SlopeScaleDepthBias.HasValue ? SlopeScaleDepthBias.Value : currentState.SlopeScaleDepthBias;

            // Finally apply the state
            device.RasterizerState = internalState;
        }

        public override void ProcessState(GraphicsDevice device)
        {
            RasterizerState currentState = device.RasterizerState;

            // CullMode
            if (CullMode.HasValue && CullMode.Value != currentState.CullMode)
            {
                Affect(device, currentState);
                return;
            }

            // FillMode
            if (FillMode.HasValue && FillMode.Value != currentState.FillMode)
            {
                Affect(device, currentState);
                return;
            }

            // ScissorTestEnable
            if (ScissorTestEnable.HasValue && ScissorTestEnable.Value != currentState.ScissorTestEnable)
            {
                Affect(device, currentState);
                return;
            }

            // MultiSampleAntiAlias
            if (MultiSampleAntiAlias.HasValue && MultiSampleAntiAlias.Value != currentState.MultiSampleAntiAlias)
            {
                Affect(device, currentState);
                return;
            }

            // DepthBias
            if (DepthBias.HasValue && DepthBias.Value != currentState.DepthBias)
            {
                Affect(device, currentState);
                return;
            }

            // SlopeScaleDepthBias
            if (SlopeScaleDepthBias.HasValue && SlopeScaleDepthBias.Value != currentState.SlopeScaleDepthBias)
            {
                Affect(device, currentState);
                return;
            }
        }

        #endregion
    }
}
