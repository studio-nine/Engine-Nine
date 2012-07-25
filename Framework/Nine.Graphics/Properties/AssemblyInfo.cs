using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Nine.Graphics")]
[assembly: AssemblyDescription("")]
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
[assembly: Guid("5cb99e29-4839-4efd-a9bf-185baef46826")]

[assembly: InternalsVisibleTo("Nine.Game, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d54af0378afa42a6f5f094c0a4891fd54f7dcd80bcf5d38e56900f0c0a591ffcd4031ec5883a105a96121c0b7625a9de47bd9439c50509388df37cbdbf214038c98f2d837ce53c26b53ed01a593d9c77c5f85a5635031071d295b761e6f0f04ed5c8280f1e65bf8c95d05061c765fad05f4b213b6d7238d834ba0d25b05c8f9d")]

// Make this visible to content pipeline so we can hide content reader.
[assembly: InternalsVisibleTo("Nine.Content.Pipeline, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d54af0378afa42a6f5f094c0a4891fd54f7dcd80bcf5d38e56900f0c0a591ffcd4031ec5883a105a96121c0b7625a9de47bd9439c50509388df37cbdbf214038c98f2d837ce53c26b53ed01a593d9c77c5f85a5635031071d295b761e6f0f04ed5c8280f1e65bf8c95d05061c765fad05f4b213b6d7238d834ba0d25b05c8f9d")]

// Make this visible to test so we can check internal state.
[assembly: InternalsVisibleTo("Nine.Test, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d54af0378afa42a6f5f094c0a4891fd54f7dcd80bcf5d38e56900f0c0a591ffcd4031ec5883a105a96121c0b7625a9de47bd9439c50509388df37cbdbf214038c98f2d837ce53c26b53ed01a593d9c77c5f85a5635031071d295b761e6f0f04ed5c8280f1e65bf8c95d05061c765fad05f4b213b6d7238d834ba0d25b05c8f9d")]

// Enable Nine.Studio input integration.
[assembly: InternalsVisibleTo("Nine.Studio, PublicKey=0024000004800000940000000602000000240000525341310004000001000100d54af0378afa42a6f5f094c0a4891fd54f7dcd80bcf5d38e56900f0c0a591ffcd4031ec5883a105a96121c0b7625a9de47bd9439c50509388df37cbdbf214038c98f2d837ce53c26b53ed01a593d9c77c5f85a5635031071d295b761e6f0f04ed5c8280f1e65bf8c95d05061c765fad05f4b213b6d7238d834ba0d25b05c8f9d")]

// Silverlight will restrict access to internal or private members, 
// causing content loading not working for classes with private constructors.

// Use InternalsVisibleTo to solve this.
#if SILVERLIGHT
[assembly: InternalsVisibleTo("Microsoft.Xna.Framework.Content, PublicKey=0024000004800000940000000602000000240000525341310004000001000100b5fc90e7027f67871e773a8fde8938c81dd402ba65b9201d60593e96c492651e889cc13f1415ebb53fac1131ae0bd333c5ee6021672d9718ea31a8aebd0da0072f25d87dba6fc90ffd598ed4da35e44c398c454307e8e33b8426143daec9f596836f97c8f74750e5975c64e2189f45def46b2a2b1247adc3652bf5c308055da9")]
[assembly: InternalsVisibleTo("System.Core, PublicKey=00240000048000009400000006020000002400005253413100040000010001008d56c76f9e8649383049f383c44be0ec204181822a6c31cf5eb7ef486944d032188ea1d3920763712ccb12d75fb77e9811149e6148e5d32fbaab37611c1878ddc19e20ef135d0cb2cff2bfec3d115810c3d9069638fe4be215dbf795861920e5ab6f7db2e2ceef136ac23d5dd2bf031700aec232f6c6b1c785b4305c123b37ab")]
[assembly: InternalsVisibleTo("mscorlib, PublicKey=00240000048000009400000006020000002400005253413100040000010001008d56c76f9e8649383049f383c44be0ec204181822a6c31cf5eb7ef486944d032188ea1d3920763712ccb12d75fb77e9811149e6148e5d32fbaab37611c1878ddc19e20ef135d0cb2cff2bfec3d115810c3d9069638fe4be215dbf795861920e5ab6f7db2e2ceef136ac23d5dd2bf031700aec232f6c6b1c785b4305c123b37ab")]
#endif

#if WINDOWS
[assembly: XmlnsPrefix("http://schemas.microsoft.com/nine/2011/xaml", "nine")]

[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Animations")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.Materials")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.Materials.MaterialParts")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.ObjectModel")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.ParticleEffects")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.PostEffects")]
[assembly: XmlnsDefinition("http://schemas.microsoft.com/nine/2011/xaml", "Nine.Graphics.Primitives")]
#endif