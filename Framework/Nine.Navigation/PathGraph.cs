#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace Nine.Navigation
{
    /// <summary>
    /// Defines a navigation graph for path finding.
    /// </summary>
    public interface IPathGraph
    {
        /// <summary>
        /// Begins an asynchronous path query.
        /// </summary>
        /// <param name="start">The start position of the path.</param>
        /// <param name="end">The end position of the path.</param>
        /// <param name="radius">The radius of the moving entity.</param>
        /// <param name="cancellationToken">The task cancellation token</param>
        /// <param name="wayPoints">
        /// A resulting empty list to hold the smoothed path waypoints including the end location but excluding the start location.
        /// </param>
        IAsyncResult QueryPathTaskAsync(Vector3 start, Vector3 end, float radius, IList<Vector3> wayPoints);
    }
}
