These are the steps needed to use Engine Nine:


1. Make sure these products are installed.

- Visual Studio 2010 (or Visual C# Express 2010)
- Xna Game Studio 4.0 (Aka, Windows Phone Developer Tools)
- Managed DirectX (included in Installer\Redist)

2. Run Build/Build.bat as administrator or Build Framework\Nine.sln directly.

- You might experence some errors since the full build requires Wix and SandCastle

3. In your Xna project, you can now add Nine.dll/Nine.Content.dll to your project using Add Reference -> .NET

4. In order to build the Engine Nine for Silverlight, you need to install

- Silverlight 5 SDK (http://www.silverlight.net/downloads)
- Silverlight 5 Tools for Visual Studio 2010 SP1 (http://www.silverlight.net/downloads)
- Silverlight Toolkit (http://silverlight.codeplex.com/releases/view/74436)

5. To build the editor, you'll need Visual Studio 2012 RC to support the async keyword.

Special thanks to:

- StitchUp (http://www.roastedamoeba.com/) by Tim Jones 
- WixXna (http://xnainstaller.codeplex.com/) by Cygon