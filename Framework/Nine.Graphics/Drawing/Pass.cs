#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xaml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Content;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public abstract class Pass : IAttachedPropertyStore
    {
        #region Properties
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Pass"/> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the preferred drawing order of this drawing pass.
        /// </summary>
        public int Order
        {
            get { return order; }
            set
            {
                if (order != value)
                {
                    if (Container != null)
                        Container.PassOrderChanged = true;
                    order = value;
                }
            }
        }
        internal int order;

        /// <summary>
        /// Gets or sets the view matrix that is specific for this pass. If this
        /// value is null, the view matrix in the drawing context is used, otherwise
        /// this value will override the matrix currently in the drawing context.
        /// </summary>
        public Matrix? View { get; set; }

        /// <summary>
        /// Gets or sets the projection matrix that is specific for this pass. If this
        /// value is null, the projection matrix in the drawing context is used, otherwise
        /// this value will override the matrix currently in the drawing context.
        /// </summary>        
        public Matrix? Projection { get; set; }
        #endregion

        #region Field
        /// <summary>
        /// Keeps a reference to a render target in case some drawing passes put 
        /// the result onto an intermediate render target.
        /// This value is set to null when the drawing pass should draw everything
        /// onto the screen directly.
        /// </summary>
        private RenderTarget2D renderTarget;

        /// <summary>
        /// Id for this pass, used for dependency sorting.
        /// </summary>
        internal int Id;
        
        /// <summary>
        /// Keeps track of the parent container.
        /// </summary>
        internal PassCollection Container;

        /// <summary>
        /// Each drawing pass can have several dependent passes. All dependent 
        /// passes are drawn before this passes draws.
        /// </summary>
        internal FastList<Pass> DependentPasses;
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Pass"/> class.
        /// </summary>
        public Pass()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Indicats this pass with be executed after the specified pass has been executed.
        /// </summary>
        public void AddDependency(Pass pass)
        {
            if (DependentPasses == null)
                DependentPasses = new FastList<Pass>();
            DependentPasses.Add(pass);
            if (Container != null)
                Container.TopologyChanged = true;
        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void GetActivePasses(IList<Pass> result)
        {
            if (Enabled)
                result.Add(this);
        }

        /// <summary>
        /// Prepares a render target to hold the result of this pass.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input)
        {
            GraphicsDevice graphics = context.GraphicsDevice;

            if (input != null)
            {
                // Create a render target similar to the input texture and draw the scene onto it.
                return RenderTargetPool.GetRenderTarget(graphics, input.Width, input.Height, input.Format, DepthFormat.Depth24Stencil8);
            }
            
            // Create a render target similar to the back buffer and draw the scene onto it.
            return RenderTargetPool.GetRenderTarget(graphics, graphics.Viewport.Width, graphics.Viewport.Height, graphics.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8);
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be drawed in this drawing pass.
        /// </param>
        public abstract void Draw(DrawingContext context, IList<IDrawableObject> drawables);
        #endregion

        #region IAttachedPropertyStore
        void IAttachedPropertyStore.CopyPropertiesTo(KeyValuePair<AttachableMemberIdentifier, object>[] array, int index)
        {
            if (attachedProperties != null)
                ((ICollection<KeyValuePair<AttachableMemberIdentifier, object>>)attachedProperties).CopyTo(array, index);
        }

        int IAttachedPropertyStore.PropertyCount
        {
            get { return attachedProperties != null ? attachedProperties.Count : 0; }
        }

        bool IAttachedPropertyStore.RemoveProperty(AttachableMemberIdentifier attachableMemberIdentifier)
        {
            if (attachedProperties == null)
                return false;

            object oldValue;
            attachedProperties.TryGetValue(attachableMemberIdentifier, out oldValue);
            AttachedPropertyChangedEventArgs.OldValue = oldValue;
            if (attachedProperties.Remove(attachableMemberIdentifier))
            {
                if (AttachedPropertyChanged != null)
                {
                    AttachedPropertyChangedEventArgs.Property = attachableMemberIdentifier;
                    AttachedPropertyChangedEventArgs.NewValue = null;
                    AttachedPropertyChanged(this, AttachedPropertyChangedEventArgs);
                }
                return true;
            }
            return false;
        }

        void IAttachedPropertyStore.SetProperty(AttachableMemberIdentifier attachableMemberIdentifier, object value)
        {
            if (attachedProperties == null)
                attachedProperties = new Dictionary<AttachableMemberIdentifier, object>();

            object oldValue;
            attachedProperties.TryGetValue(attachableMemberIdentifier, out oldValue);
            AttachedPropertyChangedEventArgs.OldValue = oldValue;
            attachedProperties[attachableMemberIdentifier] = value;

            if (AttachedPropertyChanged != null)
            {
                AttachedPropertyChangedEventArgs.Property = attachableMemberIdentifier;
                AttachedPropertyChangedEventArgs.NewValue = value;
                AttachedPropertyChanged(this, AttachedPropertyChangedEventArgs);
            }
        }

        bool IAttachedPropertyStore.TryGetProperty(AttachableMemberIdentifier attachableMemberIdentifier, out object value)
        {
            value = null;
            return attachedProperties != null && attachedProperties.TryGetValue(attachableMemberIdentifier, out value);
        }

        [ContentSerializer]
        internal Dictionary<AttachableMemberIdentifier, object> AttachedProperties
        {
            get { return attachedProperties; }
            set
            {
                if (value != null)
                    foreach (var pair in value)
                        pair.Key.Apply(this, pair.Value);
            }
        }
        private Dictionary<AttachableMemberIdentifier, object> attachedProperties;

        /// <summary>
        /// Reusing this same event args.
        /// </summary>
        private static AttachedPropertyChangedEventArgs AttachedPropertyChangedEventArgs = new AttachedPropertyChangedEventArgs(null, null, null);

        /// <summary>
        /// Occurs when any of the attached property changed.
        /// </summary>
        public event EventHandler<AttachedPropertyChangedEventArgs> AttachedPropertyChanged;
        #endregion
    }
}