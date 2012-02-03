#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;


#endregion

namespace Nine
{
    /// <summary>
    /// Event arguments that contains the elapsed time.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TimeEventArgs : EventArgs
    {
        internal TimeEventArgs() { }

        /// <summary>
        /// Gets the elapsed time since last update or draw call.
        /// </summary>
        public TimeSpan ElapsedTime { get; internal set; }
    }
}
