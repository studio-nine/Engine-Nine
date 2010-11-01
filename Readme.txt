-------------------------------------------------------------
To build framework and samples, build Framework\Nine.sln and Samples\Samples.sln

You'll need:

- Visual Studio 2010 (or Visual C# Express 2010)
- Xna Game Studio 4.0 (Aka, Windows Phone Developer Tools)


-------------------------------------------------------------
For a fullcycle build, run Build\Build.bat

You need some additional requirements in order to build successfully:

- Visual Studio 2010 (Professional or higher)
- Visual Studio 2010 SDK
- Xna Game Studio 4.0 (Aka, Windows Phone Developer Tools)
- Sandcastle
- Sandcastle Help File Builder
- InstallShield Limited Edition for Visual Studio 2010

You also need to place the root folder at "D:\Nine\", since some InstallShield and Sandcastle features does not support relative file path.



-------------------------------------------------------------
For developers, run Clean.bat before checkin to remove all bin,obj,TestResults folders and .user files.