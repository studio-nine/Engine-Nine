namespace Nine.Studio.Shell
{
    using System.ComponentModel;

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
