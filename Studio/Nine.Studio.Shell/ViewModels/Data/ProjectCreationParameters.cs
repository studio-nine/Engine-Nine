#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Nine.Studio.Extensibility;
using System.IO;
using System;
#endregion

namespace Nine.Studio.Shell.ViewModels.Data
{
    public class ProjectCreationParameters
    {
        [LocalizedDisplayName("ProjectName", typeof(Strings))]
        public string ProjectName
        {
            get { return name; }
            set { Verify.IsValidName(value, "value"); name = value; }
        }
        private string name;

        [LocalizedDisplayName("ProjectDirectory", typeof(Strings))]
        public string ProjectDirectory
        {
            get { return directory; }
            set { Verify.IsValidPath(value, "value"); directory = value; }
        }
        private string directory;

        public ProjectCreationParameters()
        {
            ProjectDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Global.Title);
            ProjectName = Global.NextName(Strings.Untitled, Global.ProjectExtension);
        }

        public string ProjectFilename
        {
            get { return Path.Combine(ProjectDirectory, ProjectName); }
        }
    }
}
