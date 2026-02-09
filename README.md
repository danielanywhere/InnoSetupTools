# InnoSetupTools

This is a one-stop script management and packaging tool for Inno Setup that compiles code-signed Windows .NET applications for distribution.

In the current version, much of the content of your \[Files\], \[Run\], and \[Code\] sections is handled for you, and it is left up to you to manage the definitions, \[Setup\], and other similar sections for now.

<p>&nbsp;</p>

## Prerequisites

For this application to produce reliable results, you will need to have a somewhat recent version of the full .NET SDK installed on your PC, which also includes MSBUILD.

You will also need to make sure that you have the active MSBUILD application in your path.

<p>&nbsp;</p>

### SignTool.exe

SignTool is not included with the .NET SDK. However, you can either get it by installing the Windows SDK, or by adding it via Visual Studio installer if you are using the full version of Visual Studio.

When you have a version of SignTool.exe on your PC, locate the full path of the tool, and assign that value to an environment variable named SIGNTOOLPATH.

<p>&nbsp;</p>

## Examples

The following command is used to run InnoSetupTools when compiling [danielanywhere/CaptionAll](https://github.com/danielanywhere/CaptionAll).

```plaintext
InnoSetupTools /wait
 /config:C:\Files\Dropbox\Develop\Shared\CaptionAll\Scripts\InnoSetupToolsVersionCompilePublish.json

```

The following configuration file is loaded by the command and used to compile the signed setup application for [danielanywhere/CaptionAll](https://github.com/danielanywhere/CaptionAll).

```json
{
	"ProjectBuildLevel": "NETInstallIncluded",
	"NETMajorVersion": 6,
	"WorkingPath": "C:\\Files\\Dropbox\\Develop\\Shared\\CaptionAll",
	"CSharpProjectFilename": "Source\\CaptionAll\\CaptionAll.csproj",
	"ExeFilename": "Source\\CaptionAll\\bin\\Release\\net6.0-windows\\win-x64\\CaptionAll.exe",
	"ShaThumbprint": "5852B965BC9A803E815363E73DAB64345E9B84BC",
	"InnoScriptFilename": "SetupProject\\CaptionAllSetup.iss",
	"InnoVersionVariable": "MyAppVersion",
	"SetupFilename": "C:\\Files\\Dropbox\\Setups\\CaptionAllSetup.exe",
	"Actions":
	[
		{
			"ActionType": "CompileAndPublish",
			"Options": [ "SetVersion:true" ]
		}
	]
}

```

<p>&nbsp;</p>

The following process is used in the above configuration.

-   All of the specified filenames are verified to exist.

-   A new version is created.

    -   The C# project is updated with the new version.
    -   The MyAppVersion value of your .iss file is updated with the new version.

-   The C# **bin** and **obj** folders are deleted.

-   C# project dependencies are restored.

-   C# project is compiled using MSBUILD and parameters associated with the specifications of the configuration file above.

-   The C# executable file is signed and time-stamped using SIGNTOOL.

-   If a version of .NET runtime installer will be packaged with your setup, the appropriate version of that installer for the major runtime version is sent over to your project's binary output directory.

-   The list of files in your project's binary output directory is written to the \[Files\] section of the your .iss file.

-   Any supporting customization code needed to perform the .NET installation on your client's computer is posted to the .iss file.

-   The signed Inno Setup .e32 uninstall file is created and signed.

-   The final setup is compiled and signed.

-   Your application setup is now ready to distribute.
