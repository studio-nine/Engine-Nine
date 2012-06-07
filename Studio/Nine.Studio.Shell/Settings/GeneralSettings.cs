#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;
using System.ComponentModel.Composition;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Settings
{
    [Export(typeof(ISettings))]
    [LocalizedDisplayName("General")]
    [LocalizedCategory("General")]
    public class GeneralSettings : ISettings
    {
        #region ISettings
        public static GeneralSettings Current { get; private set; }

        static GeneralSettings()
        {
            Current = new GeneralSettings();
        }

        object ISettings.SettingsObject
        {
            get { return Current; }
        }
        #endregion

        [EditorBrowsable]
        public bool ShowStartPage { get; set; }
    }
}
