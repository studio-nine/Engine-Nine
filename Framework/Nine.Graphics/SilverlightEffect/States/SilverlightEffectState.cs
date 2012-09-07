// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

namespace Microsoft.Xna.Framework.Graphics
{
    /// <summary>
    /// We need a class to handle the difference between .fx files (which use independent states settings) 
    /// and XNA (which uses group of states)
    /// </summary>
    internal abstract class SilverlightEffectState
    {
        #region Properties

        public bool IsActive;

        #endregion

        #region Methods

        /// <summary>
        /// Check if we must emit a new state
        /// </summary>
        public abstract void ProcessState(GraphicsDevice device);

        #endregion
    }
}
