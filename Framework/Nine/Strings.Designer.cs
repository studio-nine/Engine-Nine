﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Nine {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Nine.Strings", typeof(Strings).Assembly);
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
        ///   Looks up a localized string similar to Begin cannot be called until End has been successfully called..
        /// </summary>
        internal static string AlreadyInBeginEndPair {
            get {
                return ResourceManager.GetString("AlreadyInBeginEndPair", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot perform this operation when the avatar is not loaded..
        /// </summary>
        internal static string AvatarNotReady {
            get {
                return ResourceManager.GetString("AvatarNotReady", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Target matrix cannot be decomposed..
        /// </summary>
        internal static string CannotDecomposeMatrix {
            get {
                return ResourceManager.GetString("CannotDecomposeMatrix", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The bone transforms of the input skeleton is not valid..
        /// </summary>
        internal static string InvalidateSkeleton {
            get {
                return ResourceManager.GetString("InvalidateSkeleton", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The input animation clip is not a valid avatar animation..
        /// </summary>
        internal static string InvalidAvatarAnimationClip {
            get {
                return ResourceManager.GetString("InvalidAvatarAnimationClip", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The specified graphics profile is not supported..
        /// </summary>
        internal static string InvalidGraphicsProfile {
            get {
                return ResourceManager.GetString("InvalidGraphicsProfile", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The input primitive is invalid..
        /// </summary>
        internal static string InvalidPrimitive {
            get {
                return ResourceManager.GetString("InvalidPrimitive", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The target object is not a drawable view..
        /// </summary>
        internal static string NotADrawableView {
            get {
                return ResourceManager.GetString("NotADrawableView", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Begin must be called successfully before End can be called..
        /// </summary>
        internal static string NotInBeginEndPair {
            get {
                return ResourceManager.GetString("NotInBeginEndPair", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The Input primitive is too large for a single draw call. Try increase the capability of PrimitiveBatch..
        /// </summary>
        internal static string PrimitiveTooLarge {
            get {
                return ResourceManager.GetString("PrimitiveTooLarge", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The input skeleton does not support skinning..
        /// </summary>
        internal static string SkeletonNotSupportSkin {
            get {
                return ResourceManager.GetString("SkeletonNotSupportSkin", resourceCulture);
            }
        }
    }
}
