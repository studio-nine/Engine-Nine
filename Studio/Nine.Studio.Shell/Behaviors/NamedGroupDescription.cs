#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.ComponentModel;

#endregion

namespace Nine.Studio.Shell.Behaviors
{
    public class NamedGroupDescription : GroupDescription
    {
        public string GroupName { get; set; }

        public override object GroupNameFromItem(object item, int level, System.Globalization.CultureInfo culture)
        {
            return GroupName;
        }

        public override bool NamesMatch(object groupName, object itemName)
        {
            return true;
        }
    }
}
