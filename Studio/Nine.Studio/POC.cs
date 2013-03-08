namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Serialization;
    using System.Threading.Tasks;
    using Nine.Studio.Extensibility;

    public abstract class PackageItem
    {
        public abstract Package Package { get; }

        public abstract string FileName { get; }

        public abstract object ObjectModel {get;}

        public abstract ICollection<ProjectItem> References { get; }

        public abstract Task<object> LoadAsync();
    }

    public abstract class Package
    {
        public abstract string FileName { get; }

        public abstract ICollection<ProjectItem> PackageItems { get; }

        public abstract ICollection<Package> References { get; }

        void Test()
        {

        }
    }
}