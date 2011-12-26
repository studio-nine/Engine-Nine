#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Xaml;
using System.Reflection;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nine.Graphics;
using Nine.Content.Pipeline.Components;
using Nine.Content.Pipeline.Processors;
using Nine.Content.Pipeline.Graphics.ObjectModel;
using Nine.Graphics.ObjectModel;
using Nine.Navigation;
using Microsoft.Xna.Framework;
#endregion

namespace Nine.Content.Pipeline.Navigation
{
    /// <summary>
    /// Contains attached properties for path grid.
    /// </summary>
    public static class PathGridContent
    {
        #region Attached Properties
        static readonly AttachableMemberIdentifier IsPathProperty = new AttachableMemberIdentifier(typeof(PathGridContent), "IsPath");
        static readonly AttachableMemberIdentifier IsObstacleProperty = new AttachableMemberIdentifier(typeof(PathGridContent), "IsObstacle");

        /// <summary>
        /// Gets or sets whether the target is a path.
        /// </summary>
        public static bool GetIsPath(object target)
        {
            bool isPath = false;
            AttachablePropertyServices.TryGetProperty(target, IsPathProperty, out isPath);
            return isPath;
        }

        /// <summary>
        /// Gets or sets whether the target is a path.
        /// </summary>
        public static void SetIsPath(object target, bool value)
        {
            AttachablePropertyServices.SetProperty(target, IsPathProperty, value);
            SetTag(target, "IsPath", true);
        }

        /// <summary>
        /// Gets or sets whether the target is an obstacle.
        /// </summary>
        public static bool GetIsObstacle(object target)
        {
            bool isObstacle = false;
            AttachablePropertyServices.TryGetProperty(target, IsObstacleProperty, out isObstacle);
            return isObstacle;
        }

        /// <summary>
        /// Gets or sets whether the target is an obstacle.
        /// </summary>
        public static void SetIsObstacle(object target, bool value)
        {
            AttachablePropertyServices.SetProperty(target, IsObstacleProperty, value);
            SetTag(target, "IsObstacle", true);
        }

        private static void SetTag(object target, string key, object value)
        {
            dynamic tag = target;
            var dictionary = tag.Tag as IDictionary<string, object>;
            if (dictionary == null)
                dictionary = tag.Tag = new Dictionary<string, object>();
            dictionary[key] = value;
        }
        #endregion
    }
}