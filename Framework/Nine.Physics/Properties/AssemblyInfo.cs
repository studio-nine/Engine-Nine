using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Nine.Physics")]
[assembly: AssemblyDescription("Nine.Physics.dll")]
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
[assembly: Guid("12C69065-15F0-4865-9996-D1BFF66497F7")]


[assembly: InternalsVisibleTo("Nine.Content.Pipeline, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d54af0378afa42a6f5f094c0a4891fd54f7dcd80bcf5d38e56900f0c0a591ffcd4031ec5883a105a96121c0b7625a9de47bd9439c50509388df37cbdbf214038c98f2d837ce53c26b53ed01a593d9c77c5f85a5635031071d295b761e6f0f04ed5c8280f1e65bf8c95d05061c765fad05f4b213b6d7238d834ba0d25b05c8f9d")]

#if WINDOWS
[assembly: XmlnsPrefix("http://schemas.microsoft.com/nine/2011/xaml", "nine")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Physics")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Physics.Colliders")]
#endif