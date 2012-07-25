using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Nine.Navigation")]
[assembly: AssemblyDescription("Nine.Navigation.dll")]
[assembly: AssemblyProduct("Engine Nine")]
[assembly: AssemblyCopyright("Copyright © 2009 - 2012 Engine Nine")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type. Only Windows
// assemblies support COM.
[assembly: ComVisible(false)]

// On Windows, the following GUID is for the ID of the typelib if this
// project is exposed to COM. On other platforms, it unique identifies the
// title storage container when deploying this assembly to the device.
[assembly: Guid("17b331d9-66aa-429f-bc89-4f3d49efedaf")]


#if WINDOWS
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Navigation")]
#endif