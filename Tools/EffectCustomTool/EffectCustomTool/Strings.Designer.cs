﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nine.Tools.EffectCustomTool {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Strings {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Strings() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Nine.Tools.EffectCustomTool.Strings", typeof(Strings).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to #region Using Statements
        ///using System;
        ///using Microsoft.Xna.Framework;
        ///using Microsoft.Xna.Framework.Graphics;
        ///using Microsoft.Xna.Framework.Content;
        ///#endregion
        ///
        ///namespace {$NAMESPACE}
        ///{
        ///#if !WINDOWS_PHONE
        ///
        ///    public partial class {$CLASS}
        ///    {
        ///		private void OnCreated() { }
        ///		private void OnClone({$CLASS} cloneSource) { }
        ///		private void OnApplyChanges() { }
        ///    }	
        ///
        ///#endif
        ///}
        ///.
        /// </summary>
        internal static string Default {
            get {
                return ResourceManager.GetString("Default", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to // -----------------------------------------------------------------------------
        ///// &lt;auto-generated&gt;
        /////     This code was generated by EffectCustomTool v{$VERSION}.
        /////     Runtime Version: {$RUNTIMEVERSION}
        /////
        /////     EffectCustomTool is a part of Engine Nine. (http://nine.codeplex.com)
        /////
        /////     Changes to this file may cause incorrect behavior and will be lost if
        /////     the code is regenerated.
        ///// &lt;/auto-generated&gt;
        ///// -----------------------------------------------------------------------------
        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string Designer {
            get {
                return ResourceManager.GetString("Designer", resourceCulture);
            }
        }
    }
}
