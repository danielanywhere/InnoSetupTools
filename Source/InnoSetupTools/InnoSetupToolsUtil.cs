/*
* Copyright (c). 2022-2026 Daniel Patterson, MCSD (danielanywhere).
* 
* This program is free software: you can redistribute it and/or modify
* it under the terms of the GNU General Public License as published by
* the Free Software Foundation, either version 3 of the License, or
* (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
* 
* You should have received a copy of the GNU General Public License
* along with this program.  If not, see <https://www.gnu.org/licenses/>.
* 
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace InnoSetupTools
{
	//*-------------------------------------------------------------------------*
	//*	InnoSetupToolsUtil																											*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Tools and utilities for the InnoSetupTools application.
	/// </summary>
	public class InnoSetupToolsUtil
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		///// <summary>
		///// Installer lines to include when the .NET installer will be downloaded
		///// on setup, if needed.
		///// </summary>
		//private static string[] mInstallerDownloadLines = new string[]
		//{
		//	"  { Download the runtime installer if needed. }",
		//	"  if NeedsDotNet then",
		//	"  begin",
		//	"    WizardForm.StatusLabel.Caption := 'Downloading .NET Runtime...';",
		//	"    idpDownloadFile('" +
		//	"https://builds.dotnet.microsoft.com/dotnet/WindowsDesktop/10.0.2/windowsdesktop-runtime-10.0.2-win-x64.exe'" +
		//	",ExpandConstant('{tmp}\\dotnet-runtime.exe'));",
		//	"  end;"
		//};

		///// <summary>
		///// Installer lines representing an unused Initialize Wizard call-back.
		///// </summary>
		//private static string[] mInstallerInitializeWizardEmpty = new string[]
		//{
		//	"procedure InitializeWizard;",
		//	"begin",
		//	"end;"
		//};

		///// <summary>
		///// Installer lines representing the NeedsDotNet function that tests
		///// to see if .NET is already installed on the user's machine.
		///// </summary>
		//private static string[] mInstallerNeedsDotNet = new string[]
		//{
		//	"function NeedsDotNet: Boolean;",
		//	"var",
		//	"  Release: Cardinal;",
		//	"begin",
		//	"Log('NeedsDotNet()');",
		//	"  Result := True;",
		//	"  { Check for .NET shared runtime. }",
		//	"  if RegQueryDWordValue(HKLM64, 'SOFTWARE\\dotnet\\Setup\\InstalledVersions\\x64\\sharedhost', 'Version', Release) then",
		//	"  begin",
		//	"    Log(Format('Found sharedhost version: %d', [Release]));",
		//	"    { SharedHost version 8.0.0+ starts at 80000. }",
		//	"    if Release >= 80000 then",
		//	"    begin",
		//	"      Log('.NET 8+ detected - skipping runtime install.');",
		//	"      Result := False;",
		//	"    end",
		//	"    else",
		//	"    begin",
		//	"      Log('.NET version too low - runtime install required.');",
		//	"    end;",
		//	"  end",
		//	"  else",
		//	"  begin",
		//	"    Log('No .NET sharedhost registry key found - runtime install required.');",
		//	"  end;",
		//	"end;"
		//};

		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//* AbsolutePath																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the absolute path found between the working and relative paths.
		/// </summary>
		/// <param name="relPath">
		/// The relative path or possible fully qualified override.
		/// </param>
		/// <param name="workingPath">
		/// The working or default path.
		/// </param>
		/// <returns>
		/// The absolute path found for the two components.
		/// </returns>
		public static string AbsolutePath(string relPath, string workingPath)
		{
			string result = "";

			if(workingPath?.Length > 0 && (relPath == null || relPath.Length == 0))
			{
				//	Only the working path was specified.
				result = workingPath;
			}
			else if((workingPath == null || workingPath.Length == 0) &&
				relPath?.Length > 0)
			{
				//	Only the relative path was specified.
				result = relPath;
			}
			else if(relPath.Contains(':') || relPath.StartsWith("\\\\") ||
				relPath.StartsWith("//"))
			{
				//	Relative path is a full path.
				result = relPath;
			}
			else
			{
				//	Both the working and relative paths contain information.
				while(relPath.StartsWith('\\'))
				{
					relPath = relPath.Substring(1);
				}
				result = Path.Combine(workingPath, relPath);
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* Clear																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Clear the contents of the specified string builder.
		/// </summary>
		/// <param name="builder">
		/// Reference to the builder to clear.
		/// </param>
		public static void Clear(StringBuilder builder)
		{
			if(builder?.Length > 0)
			{
				builder.Remove(0, builder.Length);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* Compare																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the result of a case-insensitive comparison between two or more
		/// strings.
		/// </summary>
		/// <param name="values">
		/// The array of strings to compare.
		/// </param>
		/// <returns>
		/// True if the strings are equal in a case-insensitive manner. Otherwise,
		/// false.
		/// </returns>
		public static bool Compare(params string[] values)
		{
			int index = 0;
			string reference = "";
			bool result = true;

			if(values?.Length > 0)
			{
				foreach(string valueItem in values)
				{
					if(index == 0)
					{
						reference = valueItem.ToLower();
					}
					else
					{
						if(reference != valueItem.ToLower())
						{
							result = false;
							break;
						}
					}
					index++;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* CsBuildRelease																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Build the C# project in release mode.
		/// </summary>
		/// <param name="csProjectFilename">
		/// The fully qualified path and filename of the project to build.
		/// </param>
		/// <param name="csPublishProfileName">
		/// Name of the publish profile under which to publish the result.
		/// </param>
		/// <param name="buildLevel">
		/// The build level to use on this build.
		/// </param>
		/// <returns>
		/// True if the operation was a success. Otherwise, false.
		/// </returns>
		public static bool CsBuildRelease(string csProjectFilename,
			ProjectBuildLevelEnum buildLevel)
		{
			StringBuilder builder = new StringBuilder();
			List<string> consoles = null;
			bool result = false;
			string text = "";

			if(csProjectFilename?.Length > 0)
			{
				//	NOTE: Make sure your CSPROJ project file has a
				//	<RuntimeIdentifiers>win-x64</RuntimeIdentifiers>
				//	node within <PropertyGroup>.
				builder.Append($"\"{csProjectFilename}\" ");
				//	Commented DEP:20260203.1114
				//builder.Append("/p:Configuration=Release ");
				//builder.Append("/p:DeployOnBuild=true ");
				//builder.Append($"/p:PublishProfile={csPublishProfileName}");
				//	/Commented DEP:20260203.1114
				//	Added DEP:20260203.1114
				builder.Append("/t:Publish ");
				builder.Append("/p:Configuration=Release ");
				builder.Append("/p:RuntimeIdentifier=win-x64 ");
				if(buildLevel == ProjectBuildLevelEnum.StandAlone)
				{
					builder.Append("/p:SelfContained=true ");
					builder.Append("/p:PublishSingleFile=true ");
					builder.Append("/p:PublishTrimmed=true");
				}
				else
				{
					builder.Append("/p:SelfContained=false ");
					builder.Append("/p:PublishSingleFile=false ");
					builder.Append("/p:PublishTrimmed=false");
				}
				//	/Added DEP:20260203.1114
				consoles = RunExe(
					//@"C:\Program Files\Microsoft Visual Studio\2022\Community\" +
					//@"MSBuild\Current\Bin",
					"MSBUILD.EXE",
					builder.ToString());
				text = string.Join("\r\n", consoles);
				result = (Regex.IsMatch(text,
					@"(?s:Build succeeded\.\s+\d+ Warning\(s\)\s+0 Error\(s\))"));
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* CsDeleteBinAndObjFolders																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Delete the bin and obj folders on the specified C# project.
		/// </summary>
		/// <param name="csProjectFilename">
		/// The fully qualified path and filename of the C# project having bin
		/// and obj folders to delete.
		/// </param>
		public static void CsDeleteBinAndObjFolders(string csProjectFilename)
		{
			List<DirectoryInfo> delDirs = null;
			DirectoryInfo dir = null;
			DirectoryInfo[] dirs = null;

			if(csProjectFilename?.Length > 0)
			{
				//	Before compiling the project, delete the bin and obj folders.
				dir = new DirectoryInfo(Path.GetDirectoryName(csProjectFilename));
				if(dir.Exists)
				{
					delDirs = new List<DirectoryInfo>();
					Console.WriteLine(" Removing previous bin and obj folders...");
					dirs = dir.GetDirectories();
					foreach(DirectoryInfo dirItem in dirs)
					{
						if(dirItem.Name == "bin" || dirItem.Name == "obj")
						{
							delDirs.Add(dirItem);
						}
					}
					foreach(DirectoryInfo dirItem in delDirs)
					{
						dirItem.Delete(true);
					}
					delDirs.Clear();
					delDirs = null;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* CsProjectRestore																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Restore the libraries and links in the specified C# project.
		/// </summary>
		/// <param name="csProjectFilename">
		/// Fully qualified path and filename of the project to restore.
		/// </param>
		/// <returns>
		/// True if the operation was a success. Otherwise, false.
		/// </returns>
		public static bool CsProjectRestore(string csProjectFilename)
		{
			List<string> consoles = null;
			bool result = false;
			string text = "";

			if(csProjectFilename?.Length > 0)
			{
				consoles = RunExe(
					//@"C:\Program Files\Microsoft Visual Studio\2022\Community\" +
					//@"dotnet\runtime",
					//"dotnet.exe",
					"DOTNET.EXE",
					"restore",
					Path.GetDirectoryName(csProjectFilename)
					);
				text = string.Join("\r\n", consoles);
				result = (Regex.IsMatch(text, @"\s+Restored\s+"));
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		////*-----------------------------------------------------------------------*
		////*	DotnetInstallerFilename																								*
		////*-----------------------------------------------------------------------*
		///// <summary>
		///// Private member for
		///// <see cref="DotnetInstallerFilename">DotnetInstallerFilename</see>.
		///// </summary>
		//private static string mDotnetInstallerFilename =
		//	"windowsdesktop-runtime-10.0.2-win-x64.exe";
		///// <summary>
		///// Get/Set the .NET installer runtime for this instance.
		///// </summary>
		//public static string DotnetInstallerFilename
		//{
		//	get { return mDotnetInstallerFilename; }
		//	set { mDotnetInstallerFilename = value; }
		//}
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetFullFilename																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the fully qualified path and filename of the relatively or fully
		/// specified file.
		/// </summary>
		/// <param name="filename">
		/// Relative or absolute name of the file to retrieve.
		/// </param>
		/// <param name="create">
		/// Value indicating whether the file can be created if it does not exist.
		/// </param>
		/// <param name="message">
		/// Message to display with file and folder name.
		/// </param>
		/// <returns>
		/// Fully qualified path and filename of the specified file, if found.
		/// Otherwise, an empty string.
		/// </returns>
		public static string GetFullFilename(string filename,
			bool create = false, string message = "")
		{
			DirectoryInfo dir = null;
			bool exists = false;
			FileInfo file = null;
			bool isDir = false;
			string result = "";

			if(filename?.Length > 0)
			{
				//	Some type of filename has been specified.
				if(filename.StartsWith("\\") || filename.StartsWith("/") ||
					filename.IndexOf(":") > -1)
				{
					//	Absolute.
					file = new FileInfo(filename);
				}
				else
				{
					//	Relative.
					file = new FileInfo(
						Path.Combine(System.Environment.CurrentDirectory, filename));
				}
				exists = file.Exists;
				if(!exists)
				{
					//	If the file doesn't exist, check to see if it is a directory.
					dir = new DirectoryInfo(file.FullName);
					exists = dir.Exists;
					if(exists)
					{
						isDir = true;
					}
				}
				if(!exists && !create)
				{
					Console.WriteLine($"Path not found: {message} {file.FullName}");
					file = null;
					dir = null;
				}
				else if(!exists && create)
				{
					//	File can be created.
					if(file.Name.IndexOf('.') > -1)
					{
						//	Filename has an extension.
						//	Assure that the directory exists.
						dir = new DirectoryInfo(file.Directory.FullName);
						if(!dir.Exists)
						{
							dir.Create();
						}
					}
					else
					{
						//	The entire name is a directory.
						dir = new DirectoryInfo(file.FullName);
						if(!dir.Exists)
						{
							dir.Create();
						}
						isDir = true;
					}
				}
			}
			if(file != null || dir != null)
			{
				if(isDir)
				{
					Console.WriteLine($"{message} Directory: {dir.FullName}");
				}
				else
				{
					Console.WriteLine($"{message} File: {file.Name}");
				}
				result = file.FullName;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	GetFullFoldername																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the fully qualified path of the relatively or fully specified
		/// folder.
		/// </summary>
		/// <param name="foldername">
		/// Relative or absolute name of the folder to retrieve.
		/// </param>
		/// <param name="create">
		/// Value indicating whether the folder can be created if it does not
		/// exist.
		/// </param>
		/// <param name="message">
		/// Message to display with folder name.
		/// </param>
		/// <returns>
		/// Fully qualified path of the specified folder, if found.
		/// Otherwise, an empty string.
		/// </returns>
		public static string GetFullFoldername(string foldername,
			bool create = false, string message = "")
		{
			DirectoryInfo dir = null;
			bool exists = false;
			string result = "";

			if(foldername?.Length == 0)
			{
				//	If no folder was specified, use the current working directory.
				dir = new DirectoryInfo(System.Environment.CurrentDirectory);
			}
			else
			{
				//	Some type of filename has been specified.
				if(foldername.StartsWith("\\") || foldername.StartsWith("/") ||
					foldername.IndexOf(":") > -1)
				{
					//	Absolute.
					dir = new DirectoryInfo(foldername);
				}
				else
				{
					//	Relative.
					dir = new DirectoryInfo(
						Path.Combine(System.Environment.CurrentDirectory, foldername));
				}
				exists = dir.Exists;
				if(!exists && !create)
				{
					Console.WriteLine($"Path not found: {message} {dir.FullName}");
					dir = null;
				}
				else if(!exists && create)
				{
					//	Folder can be created.
					dir.Create();
				}
			}
			if(dir != null)
			{
				Console.WriteLine($"{message} Directory: {dir.FullName}");
				result = dir.FullName;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetLines																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the individual lines of a multi-line string.
		/// </summary>
		/// <param name="multiLineString">
		/// The multi-line string to split.
		/// </param>
		/// <returns>
		/// Array of individual lines found in the multi-line string.
		/// </returns>
		public static string[] GetLines(string multiLineString)
		{
			int index = 0;
			MatchCollection matches = null;
			string[] result = null;

			if(multiLineString?.Length > 0)
			{
				matches = Regex.Matches(multiLineString, ResourceMain.rxLine);
				result = new string[matches.Count];
				foreach(Match matchItem in matches)
				{
					result[index] = GetValue(matchItem, "line");
					index++;
				}
			}
			if(result == null)
			{
				result = new string[0];
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetOptionValue																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the value of the specified option on the options list.
		/// </summary>
		/// <param name="options">
		/// List of string options, stored as 'Name:Value'.
		/// </param>
		/// <param name="optionName">
		/// Name of the option to locate.
		/// </param>
		/// <returns>
		/// The value of the specified option, if found. Otherwise, an empty
		/// string.
		/// </returns>
		public static string GetOptionValue(List<string> options,
			string optionName)
		{
			string option = "";
			string result = "";

			if(options?.Count > 0 && optionName?.Length > 0)
			{
				option = options.FirstOrDefault(x =>
					Compare(LeftOf(x, ":"), optionName));
				if(option?.Length > 0)
				{
					result = RightOf(option, ":");
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetValue																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the value of the specified group within the provided match.
		/// </summary>
		/// <param name="match">
		/// Reference to the match to be searched.
		/// </param>
		/// <param name="groupName">
		/// Name of the group to search for.
		/// </param>
		/// <returns>
		/// Value of the specified group, if found. Otherwise, an empty string.
		/// </returns>
		public static string GetValue(Match match, string groupName)
		{
			string result = "";

			if(match?.Success == true && groupName?.Length > 0 &&
				match.Groups[groupName] != null &&
				match.Groups[groupName].Value != null)
			{
				result = match.Groups[groupName].Value;
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Return the value of the specified group member in a match found with
		/// the provided source and pattern.
		/// </summary>
		/// <param name="source">
		/// Source string to search.
		/// </param>
		/// <param name="pattern">
		/// Regular expression pattern to apply.
		/// </param>
		/// <param name="groupName">
		/// Name of the group for which the value will be found.
		/// </param>
		/// <returns>
		/// The value found in the specified group, if found. Otherwise, empty
		/// string.
		/// </returns>
		public static string GetValue(string source, string pattern,
			string groupName)
		{
			Match match = null;
			string result = "";

			if(source?.Length > 0 && pattern?.Length > 0 && groupName?.Length > 0)
			{
				match = Regex.Match(source, pattern);
				if(match.Success)
				{
					result = GetValue(match, groupName);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupAssureSection																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Assure that the specified section exists in the provided InnoSetup
		/// file, creating it if necessary, then returning the index of that
		/// section to the caller.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="sectionName">
		/// Name of the section to find.
		/// </param>
		/// <returns>
		/// The index at which the specified section started or was created,
		/// if successful. Otherwise, -1.
		/// </returns>
		public static int InnoSetupAssureSection(List<string> innoSetupFile,
			string sectionName)
		{
			string line = "";
			int result = -1;

			if(innoSetupFile?.Count > 0 && sectionName?.Length > 0)
			{
				line = innoSetupFile.FirstOrDefault(x =>
					Compare(x.Trim(), $"[{sectionName}]"));
				if(line != null)
				{
					result = innoSetupFile.IndexOf(line);
				}
				else
				{
					result = innoSetupFile.Count;
					innoSetupFile.Add($"[{sectionName}]");
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoSetupCodeModules																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="InnoSetupCodeModules">InnoSetupCodeModules</see>.
		/// </summary>
		private static InnoSetupCodeModuleCollection mInnoSetupCodeModules =
			new InnoSetupCodeModuleCollection()
		{
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleGetLineCount,
				Name = "GetLineCount"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleGetLine,
				Name = "GetLine"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.modulePosEx,
				Name = "PosEx"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleExecAndCaptureOutput,
				Name = "ExecAndCaptureOutput"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleExtractMajorVersionFromInstaller,
				Name = "ExtractMajorVersionFromInstaller"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleIsDotnetRuntimeInstalled,
				Name = "IsDotnetRuntimeInstalled"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallIncluded,
				Module = ResourceMain.moduleNeedsDotNet,
				Name = "NeedsDotNet"
			},
			new InnoSetupCodeModuleItem()
			{
				BuildLevel = ProjectBuildLevelEnum.NETInstallDownload,
				Module = ResourceMain.moduleInitializeWizard,
				Name = "InitializeWizard"
			}
		};
		/// <summary>
		/// Get a reference to the list of code modules to place in the InnoSetup
		/// script, given the current .NET support method.
		/// </summary>
		public static InnoSetupCodeModuleCollection InnoSetupCodeModules
		{
			get { return mInnoSetupCodeModules; }
			set { mInnoSetupCodeModules = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoSetupCompilerFilename																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="InnoSetupCompilerFilename">InnoSetupCompilerFilename</see>.
		/// </summary>
		private static string mInnoSetupCompilerFilename =
			@"C:\Program Files (x86)\Inno Setup 6\ISCC.exe";
		/// <summary>
		/// Get/Set the executable filename of the Inno Setup compiler.
		/// </summary>
		public static string InnoSetupCompilerFilename
		{
			get { return mInnoSetupCompilerFilename; }
			set
			{
				if(value?.Length > 0)
				{
					//	Only override with a legitimate value.
					mInnoSetupCompilerFilename = value;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupCreateSetup																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create the InnoSetup installer.
		/// </summary>
		/// <param name="innoScriptFilename">
		/// The fully qualified path and filename of the InnoSetup script file.
		/// </param>
		/// <returns>
		/// True if the operation was a success. Otherwise, false.
		/// </returns>
		public static bool InnoSetupCreateSetup(string innoScriptFilename)
		{
			List<string> consoles = null;
			bool result = false;
			string text = "";

			if(innoScriptFilename?.Length > 0)
			{
				consoles = RunExe(
					mInnoSetupCompilerFilename,
					$"\"{innoScriptFilename}\"");
				text = string.Join("\r\n", consoles);
				result = Regex.IsMatch(text, @"Successful compile \(");
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupCreateSignedUninstaller																			*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the InnoSetup compiler to generate the Signed Uninstaller.
		/// </summary>
		/// <param name="innoScriptFilename">
		/// The fully qualified path and filename of the InnoSetup script to
		/// compile.
		/// </param>
		/// <returns>
		/// True if the operation was a success. Otherwise, false.
		/// </returns>
		public static string InnoSetupCreateSignedUninstaller(
			string innoScriptFilename)
		{
			List<string> consoles = null;
			DirectoryInfo dir = null;
			FileInfo[] files = null;
			string innoSetupPath = "";
			string result = "";
			string text = "";

			if(innoScriptFilename?.Length > 0)
			{
				innoSetupPath = Path.GetDirectoryName(innoScriptFilename);
				dir = new DirectoryInfo(innoSetupPath);
				if(dir.Exists)
				{
					consoles = RunExe(
						mInnoSetupCompilerFilename,
						$"\"{innoScriptFilename}\"");
					text = string.Join("\r\n", consoles);
					files = dir.GetFiles();
					foreach(FileInfo fileItem in files)
					{
						if(fileItem.Extension.ToLower() == ".e32")
						{
							result = fileItem.FullName;
							break;
						}
					}
					if(result.Length == 0)
					{
						Console.WriteLine(" Error: Uninstaller file was not created.");
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupDeleteE32																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Delete any InnoSetup .E32 uninstaller files found in the specified
		/// InnoSetup path.
		/// </summary>
		/// <param name="innoScriptFilename">
		/// The fully qualified path and filename of the of the InnoSetup script.
		/// </param>
		/// <returns>
		/// True if the operation was a success. Otherwise, false.
		/// </returns>
		public static bool InnoSetupDeleteE32(string innoScriptFilename)
		{
			DirectoryInfo dir = null;
			FileInfo[] files = null;
			string innoSetupPath = "";
			bool result = true;

			if(innoScriptFilename?.Length > 0)
			{
				innoSetupPath = Path.GetDirectoryName(innoScriptFilename);
				dir = new DirectoryInfo(innoSetupPath);
				if(dir.Exists)
				{
					files = dir.GetFiles();
					foreach(FileInfo fileItem in files)
					{
						if(fileItem.Extension.ToLower() == ".e32")
						{
							fileItem.Delete();
						}
					}
				}
				else
				{
					Console.WriteLine("Error: InnoSetup folder not found...");
					result = false;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupGetCodeModuleEnd																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the index of the final end; keyword.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the loaded InnoSetup script file.
		/// </param>
		/// <param name="indexStart">
		/// The index at which to start.
		/// </param>
		/// <returns>
		/// The index of the final end; keyword of the block, function, or
		/// procedure, if found. Otherwise, -1.
		/// </returns>
		public static int InnoSetupGetCodeModuleEnd(List<string> innoSetupFile,
			int indexStart)
		{
			bool bBlockUsed = false;
			int count = 0;
			int depth = 0;
			int index = 0;
			string line = "";
			int result = -1;

			if(innoSetupFile?.Count > 0 && indexStart > -1)
			{
				count = innoSetupFile.Count;
				for(index = indexStart; index < count; index ++)
				{
					line = innoSetupFile[index];
					if(Regex.IsMatch(line, ResourceMain.rxPascalBegin))
					{
						bBlockUsed = true;
						depth++;
					}
					if(Regex.IsMatch(line, ResourceMain.rxPascalEnd))
					{
						if(depth > 0)
						{
							depth--;
						}
					}
					if(bBlockUsed && depth == 0)
					{
						result = index;
						break;
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupGetSectionEnd																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the ending index of the specified section within the loaded
		/// InnoSetup script file.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="sectionName">
		/// Name of the section to find.
		/// </param>
		/// <returns>
		/// The beginning index of the specified section, if found. Otherwise,
		/// -1.
		/// </returns>
		public static int InnoSetupGetSectionEnd(List<string> innoSetupFile,
			string sectionName)
		{
			int count = 0;
			int index = -1;
			string line = "";
			int result = -1;

			if(innoSetupFile?.Count > 0)
			{
				if(sectionName?.Length > 0)
				{
					index = InnoSetupGetSectionStart(innoSetupFile, sectionName);
					if(index > -1)
					{
						for(index ++; index < count; index ++)
						{
							if(Regex.IsMatch(innoSetupFile[index],
								ResourceMain.rxInnoSetupSectionName))
							{
								//	Next section found.
								result = index - 1;
								break;
							}
						}
						if(result == -1)
						{
							//	Section runs to end of file.
							result = count;
						}
					}
				}
				else
				{
					//	The blank section is the series of lines prior to the first
					//	section name.
					line = innoSetupFile.FirstOrDefault(x =>
						Regex.IsMatch(x, ResourceMain.rxInnoSetupSectionName));
					if(line != null)
					{
						index = innoSetupFile.IndexOf(line);
						if(index > 0)
						{
							index--;
						}
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupGetSectionStart																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the starting index of the specified section within the loaded
		/// InnoSetup script file.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="sectionName">
		/// Name of the section to find.
		/// </param>
		/// <returns>
		/// The beginning index of the specified section, if found. Otherwise,
		/// -1.
		/// </returns>
		public static int InnoSetupGetSectionStart(List<string> innoSetupFile,
			string sectionName)
		{
			string line = "";
			int result = -1;

			if(innoSetupFile?.Count > 0 && sectionName?.Length > 0)
			{
				line = innoSetupFile.FirstOrDefault(x =>
					Compare(x.Trim(), $"[{sectionName}]"));
				if(line != null)
				{
					result = innoSetupFile.IndexOf(line);
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupInsertSectionBefore																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Insert a section in the InnoSetup file prior to another specified
		/// section, or at the end of the file, if the reference section doesn't
		/// exist.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="insertSection">
		/// Name of the section to insert.
		/// </param>
		/// <param name="insertBefore">
		/// Name of the section before which the new section will be inserted.
		/// If this section is not found, the new section will be inserted at the
		/// end of the file.
		/// </param>
		/// <returns>
		/// Index at which the new section was inserted, if found. Otherwise, -1.
		/// </returns>
		public static int InnoSetupInsertSectionBefore(List<string> innoSetupFile,
			string insertSection, string insertBefore)
		{
			int index = 0;
			string line = "";
			int result = -1;

			if(innoSetupFile?.Count > 0 && insertSection?.Length > 0 &&
				insertBefore?.Length > 0)
			{
				line = innoSetupFile.FirstOrDefault(x =>
					Compare(x.Trim(), $"[{insertBefore}]"));
				if(line != null)
				{
					index = innoSetupFile.IndexOf(line);
					if(index > -1)
					{
						innoSetupFile.Insert(index, $"[{insertSection}]");
						result = index;
					}
				}
				else
				{
					result = innoSetupFile.Count;
					innoSetupFile.Add($"[{insertSection}]");
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupRemoveInstallerLines																					*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Find a set of lines in the installer and remove them.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the loaded multi-line InnoSetup installer file.
		/// </param>
		/// <param name="lines">
		/// Set of lines to remove.
		/// </param>
		public static void InnoSetupRemoveInstallerLines(
			List<string> innoSetupFile, string[] lines)
		{
			bool bMatch = false;
			int count = 0;
			int fileCount = 0;
			int fileIndex = 0;
			string fileLine = "";
			List<int> foundIndices = null;
			int index = 0;
			string line = "";
			int lineCount = 0;
			int lineIndex = 0;

			if(innoSetupFile?.Count > 0 && lines?.Length > 0)
			{
				line = lines[0].Trim();
				foundIndices = new List<int>();
				foreach(string fileLineItem in innoSetupFile)
				{
					if(Compare(fileLineItem.Trim(), line))
					{
						foundIndices.Add(index);
					}
					index++;
				}
				//	Check for matching sections.
				fileCount = innoSetupFile.Count;
				lineCount = lines.Length;
				count = foundIndices.Count;
				for(index = 0; index < count; index ++)
				{
					bMatch = true;
					for(fileIndex = foundIndices[index], lineIndex = 0;
						fileIndex < fileCount && lineIndex < lineCount;
						fileIndex ++, lineIndex ++)
					{
						fileLine = innoSetupFile[fileIndex];
						line = lines[lineIndex];
						if(fileLine.Trim().Length == 0)
						{
							if(line.Trim().Length > 0)
							{
								//	Skip blank line on file.
								lineIndex--;
							}
						}
						else if(line.Trim().Length == 0)
						{
							//	Skip blank line on reference lines.
							fileIndex--;
						}
						else if(!Compare(fileLine.Trim(), line.Trim()))
						{
							bMatch = false;
							break;
						}
					}
					if(!bMatch)
					{
						foundIndices.RemoveAt(index);
						index--;		//	Deindex.
						count--;		//	Discount.
					}
				}
				//	All of the remaining found indices represent matching sections.
				//	Remove from end to avoid having to reindex.
				for(index = count - 1; index > -1; index --)
				{
					fileIndex = foundIndices[index];
					for(lineIndex = 0;
						lineIndex < lineCount;
						lineIndex++)
					{
						fileLine = innoSetupFile[fileIndex];
						line = lines[lineIndex];
						innoSetupFile.RemoveAt(fileIndex);
						if(fileLine.Trim().Length == 0)
						{
							if(line.Trim().Length > 0)
							{
								//	Skip blank line on file.
								lineIndex--;
							}
						}
						else if(line.Trim().Length == 0)
						{
							lineIndex--;
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupRemoveSection																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Remove the specified section from the loaded InnoSetup file.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="sectionName">
		/// Name of the section to find.
		/// </param>
		public static void InnoSetupRemoveSection(List<string> innoSetupFile,
			string sectionName)
		{
			int count = 0;
			int index = 0;
			string line = "";

			if(innoSetupFile?.Count > 0 && sectionName?.Length > 0)
			{
				line = innoSetupFile.FirstOrDefault(x =>
					Compare(x.Trim(), $"[{sectionName}]"));
				if(line != null)
				{
					count = innoSetupFile.Count;
					index = innoSetupFile.IndexOf(line);
					if(index > -1)
					{
						for(; index < count; index++)
						{
							line = innoSetupFile[index];
							if(Regex.IsMatch(line, ResourceMain.rxInnoSetupSectionName))
							{
								//	Next section found.
								break;
							}
							else
							{
								//	Non-blank area found.
								innoSetupFile.RemoveAt(index);
								index--;	//	Repeat line.
								count--;
							}
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupResolveVariables																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Resolve the variables for a line on an InnoSetup file.
		/// </summary>
		/// <param name="majorDotnetVersion">
		/// The major .NET version to be installed.
		/// </param>
		/// <param name="sourceValue">
		/// The line to inspect.
		/// </param>
		/// <returns>
		/// A version of the caller's line where all of the interpolated variable
		/// names have been replaced with their resolved static values.
		/// </returns>
		public static string InnoSetupResolveVariables(
			int majorDotnetVersion, string sourceValue)
		{
			string result = "";

			if(sourceValue?.Length > 0)
			{
				result = sourceValue.Replace("{RuntimeDownloadUrl}",
					RuntimeInstallerDownloadUrl);
				result = sourceValue.Replace("{RuntimeVersion}",
					RuntimeInstallerReferences.GetInstallerVersion(majorDotnetVersion));
				result = sourceValue.Replace("{RuntimeInstallerName}",
					RuntimeInstallerReferences.GetInstallerName(majorDotnetVersion));
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupSectionIsEmpty																								*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return a value indicating whether the specified section is empty.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="sectionName">
		/// Name of the section to find.
		/// </param>
		/// <returns>
		/// Value indicating whether the section is empty.
		/// </returns>
		public static bool InnoSetupSectionIsEmpty(List<string> innoSetupFile,
			string sectionName)
		{
			int count = 0;
			int index = 0;
			string line = "";
			bool result = true;

			if(innoSetupFile?.Count > 0 && sectionName?.Length > 0)
			{
				line = innoSetupFile?.FirstOrDefault(x =>
					Compare(x.Trim(), $"[{sectionName}]"));
				if(line != null)
				{
					count = innoSetupFile.Count;
					index = innoSetupFile.IndexOf(line);
					if(index > -1)
					{
						for(index++; index < count; index++)
						{
							line = innoSetupFile[index];
							if(Regex.IsMatch(line, ResourceMain.rxInnoSetupSectionName))
							{
								//	Next section found.
								break;
							}
							else if(line.Trim().Length > 0)
							{
								//	Non-blank area found.
								result = false;
								break;
							}
						}
					}
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupSetPackageFiles																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the package files on the specified open InnoSetup file content.
		/// </summary>
		/// <param name="innoSetupFile">
		/// Reference to the InnoSetup script file, loaded as individual text
		/// lines.
		/// </param>
		/// <param name="majorDotnetVersion">
		/// The major .NET version to be accomodated in this package.
		/// </param>
		/// <param name="buildLevel">
		/// The active build level for the current project.
		/// </param>
		/// <param name="inputFoldername">
		/// The fully qualified path and filename of the folder containing the
		/// files to enumerate.
		/// </param>
		/// <returns>
		/// Value indicating whether changes were made.
		/// </returns>
		public static bool InnoSetupSetPackageFiles(List<string> innoSetupFile,
			int majorDotnetVersion, ProjectBuildLevelEnum buildLevel,
			string inputFoldername)
		{
			StringBuilder builder = new StringBuilder();
			int count = 0;
			DirectoryInfo dir = null;
			FileInfo[] files = null;
			string[] fileTypes = new string[]
			{
				".bat", ".bmp", ".cmd", ".dll", ".exe", ".ico", ".png", ".json"
			};
			int index = 0;
			int indexStart = 0;
			string installerFilename =
				mRuntimeInstallerReferences.GetInstallerName(majorDotnetVersion);
			string line = "";
			bool result = false;

			if(innoSetupFile?.Count > 0 && inputFoldername?.Length > 0)
			{

				dir = new DirectoryInfo(inputFoldername);
				if(dir.Exists)
				{
					if(buildLevel == ProjectBuildLevelEnum.NETInstallIncluded)
					{
						//	The .NET installer will be included with this setup.
						try
						{
							File.Copy(
								Path.Combine(
									AppContext.BaseDirectory, "Resources",
										installerFilename),
								Path.Combine(dir.FullName,
									installerFilename),
								true);
						}
						catch(Exception ex)
						{
							Trace.WriteLine(
								"Error: Could not copy dotnet installer to publish folder." +
								$"\r\n{ex.Message}");
						}
					}
					line = innoSetupFile.FirstOrDefault(x =>
						x.Trim().ToLower().StartsWith("[files]"));
					if(line?.Length > 0)
					{
						//	Files section was found.
						indexStart = innoSetupFile.IndexOf(line);
					}
					else
					{
						Trace.WriteLine(
							"Error: Count not find [Files] section in " +
							$"InnoSetup script file.");
					}
					if(indexStart > -1)
					{
						//	Clear out the files section.
						count = innoSetupFile.Count;
						index = indexStart + 1;
						for(index = indexStart + 1; index < count; index++)
						{
							line = innoSetupFile[index].Trim();
							if(Regex.IsMatch(line, @"^\[[^\]]+\]"))
							{
								//	We found the start of the next section.
								break;
							}
							else
							{
								//	This line is not the start of another section.
								//	In other words, this is a member of the files section.
								innoSetupFile.RemoveAt(index);
								count--;    //	Discount.
								index--;    //	Decrement.
								result = true;
							}
						}
						//	At this point, all of the items are cleared from the files
						//	section.
						index = indexStart + 1;
						innoSetupFile.Insert(index, "");
						files = dir.GetFiles();
						foreach(FileInfo fileItem in files)
						{
							if(LeftOf(fileItem.Name, "-").ToLower() ==
								LeftOf(installerFilename, "-").ToLower())
							{
								//	This is a designated dotnet installer file.
								Clear(builder);
								builder.Append("Source: \"{#SourcePath}\\");
								builder.Append(fileItem.Name);
								builder.Append("\"; DestDir: \"{tmp}\"; ");
								builder.Append("Flags: deleteafterinstall");
								innoSetupFile.Insert(index, builder.ToString());
								count++;
								index++;
								//Console.WriteLine($" File Added: {fileItem.Name}");
								Console.Write($"{fileItem.Name}\t");
								if(index % 2 == 0)
								{
									Console.WriteLine("");
								}
								result = true;
							}
							else if(fileTypes.Contains(fileItem.Extension.ToLower()))
							{
								//	This is an accepted member file.
								Clear(builder);
								builder.Append("Source: \"{#SourcePath}\\");
								builder.Append(fileItem.Name);
								builder.Append("\"; DestDir: \"{app}\"; Flags: ignoreversion");
								innoSetupFile.Insert(index, builder.ToString());
								count++;
								index++;
								//Console.WriteLine($" File Added: {fileItem.Name}");
								Console.Write($"{fileItem.Name}\t");
								if(index % 2 == 0)
								{
									Console.WriteLine("");
								}
								result = true;
							}
						}
					}
				}
				else
				{
					Trace.WriteLine("Error: Specified input folder not found: " +
						$"{dir.FullName}");
				}
			}
			else if(innoSetupFile == null || innoSetupFile.Count == 0)
			{
				Trace.WriteLine("Error: InnoSetup script file not specified...");
			}
			else
			{
				Trace.WriteLine("Error: Source files folder not specified...");
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Set the package files on the specified output file.
		/// </summary>
		/// <param name="inputFoldername">
		/// The fully qualified path of the input folder from which the files
		/// will be found.
		/// </param>
		/// <param name="outputFilename">
		/// The fully qualified filename of the InnoSetup project file to maintain.
		/// </param>
		/// <remarks>
		/// In this version, the InnoSetup variable {SourcePath} is assumed as
		/// the base path in the [Files] list.
		/// </remarks>
		public static void InnoSetupSetPackageFiles(string inputFoldername,
			string outputFilename)
		{
			string content = "";
			string line = "";
			List<string> lines = new List<string>();

			if(inputFoldername?.Length > 0 &&
				Path.Exists(inputFoldername) &&
				outputFilename?.Length > 0 &&
				Path.Exists(outputFilename))
			{
				using(FileStream stream = File.OpenRead(outputFilename))
				{
					using(StreamReader reader = new StreamReader(stream))
					{
						while((line = reader.ReadLine()) != null)
						{
							lines.Add(line);
						}
					}
				}
				if(InnoSetupSetPackageFiles(lines, 0, ProjectBuildLevelEnum.None,
					inputFoldername))
				{
					content = string.Join("\r\n", lines);
					File.WriteAllText(outputFilename, content);
					Console.WriteLine(" Project file updated...");
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* InnoSetupUpdateCode																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Assure that the appropriate InnoSetup code exists to match the current
		/// build level.
		/// </summary>
		/// <param name="innoSetupFile">
		/// The multi-line content of the loaded InnoSetup file.
		/// </param>
		/// <param name="buildLevel">
		/// The active build level for the current project.
		/// </param>
		public static void InnoSetupUpdateCode(List<string> innoSetupFile,
			int majorDotnetVersion, ProjectBuildLevelEnum buildLevel)
		{
			bool bFunction = false;
			int count = 0;
			int index = 0;
			int indexEnd = 0;
			int indexStart = 0;
			string installerFilename =
				mRuntimeInstallerReferences.GetInstallerName(majorDotnetVersion);
			string line = "";
			string[] sourceLines = null;

			if(innoSetupFile?.Count > 0)
			{
				//	Clean up from previous configurations.
				foreach(InnoSetupCodeModuleItem codeModuleItem in mInnoSetupCodeModules)
				{
					sourceLines = GetLines(codeModuleItem.Module);
					if(sourceLines?.Length > 0)
					{
						line = innoSetupFile.FirstOrDefault(x =>
							Compare(x, sourceLines[1]));
						if(line != null)
						{
							index = innoSetupFile.IndexOf(line) - 1;
							if(index > -1)
							{
								line = innoSetupFile[index];
								if(Regex.IsMatch(line, ResourceMain.rxAutogeneratedTag))
								{
									indexStart = index;
									indexEnd = InnoSetupGetCodeModuleEnd(innoSetupFile, index);
									if(indexEnd > indexStart)
									{
										count = indexEnd - indexStart + 1;
										for(index = 0; index < count; index++)
										{
											innoSetupFile.RemoveAt(indexStart);
										}
										count = innoSetupFile.Count;
										while(indexStart < count &&
											innoSetupFile[indexStart].Trim().Length == 0)
										{
											innoSetupFile.RemoveAt(indexStart);
											count--;
										}
									}
								}
							}
						}
					}
				}

				//	Remove the installer from the run section.
				innoSetupFile.RemoveAll(x =>
					Regex.IsMatch(x,
						ResourceMain.rxDotNetRunFilenameFamily.Replace(
							"{dotnetInstallerFamily}",
								Regex.Escape(mRuntimeInstallerFilenameFamily))));

				if(buildLevel < ProjectBuildLevelEnum.NETInstallIncluded)
				{
					//	If no code is included, remove its section.
					if(InnoSetupSectionIsEmpty(innoSetupFile, "Code"))
					{
						InnoSetupRemoveSection(innoSetupFile, "Code");
					}
					////	Also remove #define DotnetRuntimeInstallerName "..."
					//line = innoSetupFile.FirstOrDefault(x =>
					//	Regex.IsMatch(x,
					//		ResourceMain.rxInnoSetupDefineInstallerName));
					//if(line != null)
					//{
					//	innoSetupFile.Remove(line);
					//}
				}
				else
				{
					////	Add necessary code for install and download of .NET runtime.
					////	(definitions) section.
					//line = innoSetupFile.FirstOrDefault(x =>
					//	Regex.IsMatch(x,
					//		ResourceMain.rxInnoSetupDefineInstallerName));
					//if(line == null)
					//{
					//	index = InnoSetupGetSectionEnd(innoSetupFile, "");
					//	if(index > -1)
					//	{
					//		innoSetupFile.Insert(index + 1,
					//			"#define DotnetRuntimeInstallerName " +
					//			$"\"{installerFilename}\"");
					//	}
					//}
					//else
					//{
					//	index = innoSetupFile.IndexOf(line);
					//	if(index > -1)
					//	{
					//		innoSetupFile[index] =
					//			"#define DotnetRuntimeInstallerName " +
					//			$"\"{installerFilename}\"";
					//	}
					//}

					//	[Run] section.
					indexStart = InnoSetupGetSectionStart(innoSetupFile, "Run");
					if(indexStart == -1)
					{
						indexStart = InnoSetupInsertSectionBefore(innoSetupFile,
							"Run", "Code");
					}

					if(indexStart > -1)
					{
						//	Code, wizard initialization, and files have already been
						//	removed.
						innoSetupFile.Insert(indexStart + 1,
							"Filename: " +
							$"\"{{tmp}}\\{installerFilename}\"; " +
							"Parameters: \"/install /quiet /norestart\"; " +
							"Check: NeedsDotNet");
					}

					//	[Code] Section.
					index = InnoSetupAssureSection(innoSetupFile, "Code");
					if(index > -1)
					{
						index++;
						foreach(InnoSetupCodeModuleItem moduleItem in
							mInnoSetupCodeModules)
						{
							if(moduleItem.BuildLevel <= buildLevel)
							{
								sourceLines = GetLines(moduleItem.Module);
								if(sourceLines.Length > 1)
								{
									//	Skip first line, which is an auto-generated tag.
									line = sourceLines[1];
									if(!innoSetupFile.Exists(x =>
										Compare(x.Trim(), line.Trim())))
									{
										//	There is no overridden version of this line present.
										foreach(string sourceLineItem in sourceLines)
										{
											line = InnoSetupResolveVariables(majorDotnetVersion,
												sourceLineItem);
											innoSetupFile.Insert(index, line);
											index++;
										}
										innoSetupFile.Insert(index, "");
										index++;
									}
								}
							}
						}
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* LeftOf																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the portion of the source string to the left of the pattern.
		/// </summary>
		/// <param name="source">
		/// Source string to inspect.
		/// </param>
		/// <param name="pattern">
		/// Pattern to find within the source.
		/// </param>
		/// <returns>
		/// Portion of the source string to the left of the pattern, if found.
		/// Otherwise, the source string.
		/// </returns>
		public static string LeftOf(string source, string pattern)
		{
			int index = 0;
			string result = "";

			if(source?.Length > 0)
			{
				index = source.IndexOf(pattern);
				if(pattern?.Length > 0 && index > -1)
				{
					result = source.Substring(0, index);
				}
				else
				{
					result = source;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* LoadInnoSetupScript																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Load the specified InnoSetup script as a list of lines.
		/// </summary>
		/// <param name="filename">
		/// The fully qualified path and filename of the InnoSetup script.
		/// </param>
		/// <returns>
		/// Reference to a list of text lines in the specified file.
		/// </returns>
		public static List<string> LoadInnoSetupScript(string filename)
		{
			string line = "";
			List<string> lines = new List<string>();

			if(filename?.Length > 0)
			{
				using(FileStream stream = File.OpenRead(filename))
				{
					using(StreamReader reader = new StreamReader(stream))
					{
						while((line = reader.ReadLine()) != null)
						{
							lines.Add(line);
						}
					}
				}
			}
			return lines;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* OutputInfo																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Output a message to the console and to the provided string builder.
		/// </summary>
		/// <param name="message">
		/// The message to output.
		/// </param>
		/// <param name="builder">
		/// Reference to the builder to receive the message.
		/// </param>
		public static void OutputInfo(string message, StringBuilder builder)
		{
			Console.WriteLine(message);
			builder.Append(message);
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* RightOf																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the string portion to the right of the specified pattern.
		/// </summary>
		/// <param name="source">
		/// Source value to inspect.
		/// </param>
		/// <param name="pattern">
		/// The pattern to test for.
		/// </param>
		/// <returns>
		/// The portion of the supplied string to the right of the specified
		/// pattern.
		/// </returns>
		public static string RightOf(string source, string pattern)
		{
			int position = 0;
			string result = "";

			if(source?.Length > 0)
			{
				if(pattern?.Length > 0 && source.IndexOf(pattern) > -1)
				{
					//	The pattern exists in the string.
					position = source.LastIndexOf(pattern);
					if(source.Length > position + 1)
					{
						result = source.Substring(position + 1);
					}
				}
				else
				{
					result = source;
				}
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* RunExe																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run a generic command and return the resulting strings.
		/// </summary>
		/// <param name="exePath">
		/// The path of the executable utility to run.
		/// </param>
		/// <param name="exeName">
		/// The name of the executable file to run.
		/// </param>
		/// <param name="arguments">
		/// Arguments to place on the command line.
		/// </param>
		/// <param name="workingDirectory">
		/// Optional path name of the working directory.
		/// </param>
		/// <returns>
		/// Reference to the list of outputs generated during the operation.
		/// [0] - Standard.
		/// [1] - Error.
		/// </returns>
		public static List<string> RunExe(
			string exeFilename,
			//string exePath, string exeName,
			string arguments, string workingDirectory = "")
		{
			//string exeFilename = "";
			List<string> consoles = new List<string>();
			StringBuilder errorBuilder = new StringBuilder();
			Process process = null;
			StringBuilder standardBuilder = new StringBuilder();

			if(exeFilename?.Length > 0
				//	exePath?.Length > 0 && exeName?.Length > 0
				)
			{
				//exeFilename = Path.Combine(exePath, exeName);
				//if(Path.Exists(exeFilename))
				//{
					process = new Process();
					process.StartInfo.RedirectStandardOutput = true;
					process.StartInfo.RedirectStandardError = true;
					//process.StartInfo.FileName = Path.Combine(exePath, exeName);
					process.StartInfo.FileName = exeFilename;
					process.StartInfo.Arguments = arguments;
					if(workingDirectory?.Length > 0)
					{
						process.StartInfo.WorkingDirectory = workingDirectory;
					}

					process.StartInfo.UseShellExecute = false;
					process.StartInfo.CreateNoWindow = true;

					process.OutputDataReceived += (sender, args) =>
						OutputInfo($"  {args.Data}", standardBuilder);
					process.ErrorDataReceived += (sender, args) =>
						OutputInfo($"  {args.Data}", errorBuilder);

					//process.OutputDataReceived +=
					//	(sender, args) => Console.WriteLine("  {0}", args.Data);
					//process.ErrorDataReceived +=
					//	(sender, args) => Console.WriteLine("  {0}", args.Data);
					process.Start();
					process.BeginOutputReadLine();
					process.BeginErrorReadLine();
					process.WaitForExit();

					consoles.Add(standardBuilder.ToString().Trim());
					consoles.Add(errorBuilder.ToString().Trim());
				//}
				//else
				//{
				//	Console.WriteLine(" Error in RunExe. Command not found:");
				//	Console.WriteLine($"  {exeFilename}");
				//}
			}
			else
			{
				Console.WriteLine(" Error in RunExe. Application not specified.");
			}
			return consoles;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RuntimeInstallerFilenameFamily																				*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="RuntimeInstallerFilenameFamily">
		/// RuntimeInstallerFilenameFamily
		/// </see>.
		/// </summary>
		private static string mRuntimeInstallerFilenameFamily =
			"windowsdesktop-runtime-";
		/// <summary>
		/// Get/Set the .NET runtime installer filename family pattern
		/// </summary>
		public static string RuntimeInstallerFilenameFamily
		{
			get { return mRuntimeInstallerFilenameFamily; }
			set { mRuntimeInstallerFilenameFamily = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RuntimeInstallerDownloadUrl																						*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="RuntimeInstallerDownloadUrl">
		/// RuntimeInstallerDownloadUrl
		/// </see>.
		/// </summary>
		private static string mRuntimeInstallerDownloadUrl = "";
		/// <summary>
		/// Get/Set the download URL for the .NET runtime installer.
		/// </summary>
		public static string RuntimeInstallerDownloadUrl
		{
			get { return mRuntimeInstallerDownloadUrl; }
			set { mRuntimeInstallerDownloadUrl = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RuntimeInstallerReferences																						*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="RuntimeInstallerReferences">
		/// RuntimeInstallerReferences
		/// </see>.
		/// </summary>
		private static RuntimeInstallerReferenceCollection
			mRuntimeInstallerReferences = new RuntimeInstallerReferenceCollection()
		{
			new RuntimeInstallerReferenceItem()
			{
				MajorVersion = 6,
				RuntimeVersion = "6.0.36",
				RuntimeInstallerName = "windowsdesktop-runtime-6.0.36-win-x64.exe"
			},
			new RuntimeInstallerReferenceItem()
			{
				MajorVersion = 7,
				RuntimeVersion = "7.0.20",
				RuntimeInstallerName = "windowsdesktop-runtime-7.0.20-win-x64.exe"
			},
			new RuntimeInstallerReferenceItem()
			{
				MajorVersion = 8,
				RuntimeVersion = "8.0.23",
				RuntimeInstallerName = "windowsdesktop-runtime-8.0.23-win-x64.exe"
			},
			new RuntimeInstallerReferenceItem()
			{
				MajorVersion = 9,
				RuntimeVersion = "9.0.12",
				RuntimeInstallerName = "windowsdesktop-runtime-9.0.12-win-x64.exe"
			},
			new RuntimeInstallerReferenceItem()
			{
				MajorVersion = 10,
				RuntimeVersion = "10.0.2",
				RuntimeInstallerName = "windowsdesktop-runtime-10.0.2-win-x64.exe"
			}
		};
		/// <summary>
		/// Get a reference to the collection of runtime installers for this
		/// session.
		/// </summary>
		public static RuntimeInstallerReferenceCollection
			RuntimeInstallerReferences
		{
			get { return mRuntimeInstallerReferences; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SaveInnoSetupScript																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Save the contents of the line-based InnoSetup script file to the
		/// specified text file.
		/// </summary>
		/// <param name="innoSetupScript">
		/// Reference to the list of file lines to save.
		/// </param>
		/// <param name="filename">
		/// Fully qualified path and filename of the text file to save.
		/// </param>
		public static void SaveInnoSetupScript(List<string> innoSetupScript,
			string filename)
		{
			string content = "";

			if(innoSetupScript?.Count > 0 && filename?.Length > 0)
			{
				content = string.Join("\r\n", innoSetupScript);
				File.WriteAllText(filename, content);
				Console.WriteLine(" Project file updated...");
			}
		}
		//*-----------------------------------------------------------------------*

		////*-----------------------------------------------------------------------*
		////* SetVersion																														*
		////*-----------------------------------------------------------------------*
		///// <summary>
		///// Set the version on the various files.
		///// </summary>
		///// <param name="setVersionFilename">
		///// Path and filename of the executable file to run.
		///// </param>
		///// <param name="projectProperties">
		///// List of project filenames to stamp with the current version.
		///// </param>
		///// <returns>
		///// Version number that has been applied to the projects.
		///// </returns>
		///// <remarks>
		///// In this version, only CSharpProjectFilename and
		///// InnoScriptFilename are recognized.
		///// </remarks>
		//public static string SetVersion(string setVersionFilename,
		//	List<NameValueItem> projectProperties)
		//{
		//	bool bAction = false;
		//	StringBuilder builder = new StringBuilder();
		//	List<string> consoles = new List<string>();
		//	Match match = null;
		//	NameValueItem nameValue = null;
		//	string result = "";

		//	if(setVersionFilename?.Length > 0 &&
		//		projectProperties?.Count > 0)
		//	{
		//		builder.Append("/nowait /now ");
		//		nameValue = projectProperties.FirstOrDefault(x =>
		//			x.Name == "InnoScriptFilename");
		//		if(nameValue != null)
		//		{
		//			builder.Append($"\"/inoproject:{nameValue.Value}\" ");
		//			nameValue = projectProperties.FirstOrDefault(x =>
		//				x.Name == "InnoVersionVariable");
		//			if(nameValue != null)
		//			{
		//				builder.Append($"/inovar:{nameValue.Value} ");
		//			}
		//			bAction = true;
		//		}
		//		nameValue = projectProperties.FirstOrDefault(x =>
		//			x.Name == "CSharpProjectFilename");
		//		if(nameValue != null)
		//		{
		//			builder.Append($"\"/csproject:{nameValue.Value}\" ");
		//			bAction = true;
		//		}
		//		if(bAction)
		//		{
		//			//	Some action has been defined.
		//			consoles = RunExe(
		//				Path.Combine(
		//				Path.GetDirectoryName(setVersionFilename),
		//				Path.GetFileName(setVersionFilename)),
		//				builder.ToString());
		//			foreach(string consoleItem in consoles)
		//			{
		//				match = Regex.Match(consoleItem,
		//					@"\s*Version\:\s*(?<version>\d+\.\d+\.\d+(\.\d+){0,1})");
		//				if(match.Success)
		//				{
		//					//	Version was found on this console.
		//					result = GetValue(match, "version");
		//					break;
		//				}
		//			}
		//		}
		//		else
		//		{
		//			//	No actions were defined.
		//			Console.WriteLine(" Nothing to do...");
		//		}
		//	}
		//	return result;
		//}
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SignAndVerify																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Attempt to sign, timestamp, and verify the specified target file,
		/// returning a flag indicating success.
		/// </summary>
		/// <param name="certificateFilename">
		/// Fully qualified path and filename of the certificate file used for
		/// signing.
		/// </param>
		/// <param name="certificatePassword">
		/// The case-sensitive certificate to use for accessing certificate
		/// store information.
		/// </param>
		/// <param name="shaThumbprint">
		/// The public SHA thumbprint for the certificate on this file.
		/// </param>
		/// <param name="targetFilename">
		/// Fully qualified path and filename of the target file to sign and
		/// test.
		/// </param>
		/// <returns>
		/// True if the entire operation was successful. Otherwise, false.
		/// </returns>
		public static bool SignAndVerify(string certificateFilename,
			string certificatePassword, string shaThumbprint, string targetFilename)
		{
			bool result = false;

			if(targetFilename?.Length > 0)
			{
				if(certificateFilename?.Length > 0 &&
					certificatePassword?.Length > 0)
				{
					result = SignAndVerify(certificateFilename, certificatePassword,
						targetFilename);
				}
				else if(shaThumbprint?.Length > 0)
				{
					result = SignAndVerify(shaThumbprint, targetFilename);
				}
				else
				{
					result = SignAndVerify(targetFilename);
				}
			}
			else
			{
				Console.WriteLine("Target filename not provided for Sign and Verify.");
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Attempt to sign, timestamp, and verify the specified target file,
		/// returning a flag indicating success.
		/// </summary>
		/// <param name="certificateFilename">
		/// Fully qualified path and filename of the certificate file used for
		/// signing.
		/// </param>
		/// <param name="certificatePassword">
		/// The case-sensitive certificate to use for accessing certificate
		/// store information.
		/// </param>
		/// <param name="targetFilename">
		/// Fully qualified path and filename of the target file to sign and
		/// test.
		/// </param>
		/// <returns>
		/// True if the entire operation was successful. Otherwise, false.
		/// </returns>
		public static bool SignAndVerify(string certificateFilename,
			string certificatePassword, string targetFilename)
		{
			bool bContinue = true;
			StringBuilder builder = new StringBuilder();
			List<string> consoles = new List<string>();
			bool result = false;
			FileInfo signTool = new FileInfo(mSignToolExeFilename);
			string text = "";

			if(
				signTool.Exists &&
				certificateFilename?.Length > 0 &&
				certificatePassword?.Length > 0 &&
				targetFilename?.Length > 0)
			{
				//	Sign the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("sign ");
					builder.Append($"/f \"{certificateFilename}\" ");
					builder.Append($"/p {certificatePassword} ");
					builder.Append($"/fd sha256 ");
					builder.Append($"/a \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully signed\:"));
				}

				//	Set the timestamp on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("timestamp ");
					builder.Append("/tr http://timestamp.digicert.com ");
					builder.Append("/td SHA256 ");
					builder.Append($"\"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully timestamped\:"));
				}

				//	Verify the signature on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("verify ");
					builder.Append($"/pa \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully verified\:"));
				}
				result = bContinue;
			}
			else if(!signTool.Exists)
			{
				Console.WriteLine(ResourceMain.msgSignToolNotFound);
			}
			else
			{
				Console.WriteLine("Target filename not provided for Sign and Verify.");
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Attempt to sign, timestamp, and verify the specified target file,
		/// returning a flag indicating success.
		/// </summary>
		/// <param name="shaThumbprint">
		/// The public SHA thumbprint for the certificate on this file.
		/// </param>
		/// <param name="targetFilename">
		/// Fully qualified path and filename of the target file to sign and
		/// test.
		/// </param>
		/// <returns>
		/// True if the entire operation was successful. Otherwise, false.
		/// </returns>
		public static bool SignAndVerify(string shaThumbprint,
			string targetFilename)
		{
			bool bContinue = true;
			StringBuilder builder = new StringBuilder();
			List<string> consoles = new List<string>();
			bool result = false;
			FileInfo signTool = new FileInfo(mSignToolExeFilename);
			string text = "";

			if(
				signTool.Exists &&
				shaThumbprint?.Length > 0 &&
				targetFilename?.Length > 0)
			{
				//	Sign the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("sign ");
					builder.Append($"/v /sha1 {shaThumbprint} ");
					builder.Append($"/fd sha256 ");
					builder.Append($"/a \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully signed\:"));
				}

				//	Set the timestamp on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("timestamp ");
					builder.Append("/tr http://timestamp.digicert.com ");
					builder.Append("/td SHA256 ");
					builder.Append($"\"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully timestamped\:"));
				}

				//	Verify the signature on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("verify ");
					builder.Append($"/pa \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully verified\:"));
				}
				result = bContinue;
			}
			else if(!signTool.Exists)
			{
				Console.WriteLine(ResourceMain.msgSignToolNotFound);
			}
			else
			{
				Console.WriteLine("Target filename not provided for Sign and Verify.");
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Attempt to sign, timestamp, and verify the specified target file,
		/// returning a flag indicating success.
		/// </summary>
		/// <param name="targetFilename">
		/// Fully qualified path and filename of the target file to sign and
		/// test.
		/// </param>
		/// <returns>
		/// True if the entire operation was successful. Otherwise, false.
		/// </returns>
		public static bool SignAndVerify(string targetFilename)
		{
			bool bContinue = true;
			StringBuilder builder = new StringBuilder();
			List<string> consoles = new List<string>();
			bool result = false;
			FileInfo signTool = new FileInfo(mSignToolExeFilename);
			string text = "";

			if(
				signTool.Exists &&
				targetFilename?.Length > 0)
			{
				//	Sign the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("sign ");
					builder.Append($"/fd sha256 ");
					builder.Append("/tr http://timestamp.globalsign.com/?signature=sha2 ");
					builder.Append($"/td sha256 ");
					builder.Append($"/a \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully signed\:"));
				}

				//	Set the timestamp on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("timestamp ");
					builder.Append("/tr http://timestamp.digicert.com ");
					builder.Append("/td SHA256 ");
					builder.Append($"\"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully timestamped\:"));
				}

				//	Verify the signature on the target file.
				if(bContinue)
				{
					Clear(builder);
					builder.Append("verify ");
					builder.Append($"/pa \"{targetFilename}\"");
					consoles = RunExe(
						signTool.FullName,
						builder.ToString());
					text = string.Join("\r\n", consoles);
					Thread.Sleep(1000);
					bContinue = (Regex.IsMatch(text, @"Successfully verified\:"));
				}
				result = bContinue;
			}
			else if(!signTool.Exists)
			{
				Console.WriteLine(ResourceMain.msgSignToolNotFound);
			}
			else
			{
				Console.WriteLine("Target filename not provided for Sign and Verify.");
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SignToolExeFilename																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="SignToolExeFilename">SignToolExeFilename</see>.
		/// </summary>
		private static string mSignToolExeFilename =
			Path.Combine(Environment.GetEnvironmentVariable("SIGNTOOLPATH"),
				"SIGNTOOL.EXE");
		/// <summary>
		/// Get/Set the fully qualified path and filename of the SignTool.exe file.
		/// </summary>
		/// <remarks>
		/// By default, this library will load the current environment value
		/// of %SIGNTOOLPATH%\SIGNTOOL.EXE.
		/// </remarks>
		public static string SignToolExeFilename
		{
			get { return mSignToolExeFilename; }
			set
			{
				if(value?.Length > 0)
				{
					//	Only override with legitimate value.
					mSignToolExeFilename = value;
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ToBool																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Provide fail-safe conversion of string to boolean value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <returns>
		/// Boolean value. False if not convertible.
		/// </returns>
		public static bool ToBool(object value)
		{
			bool result = false;
			if(value != null)
			{
				result = ToBool(value.ToString());
			}
			return result;
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Provide fail-safe conversion of string to boolean value.
		/// </summary>
		/// <param name="value">
		/// Value to convert.
		/// </param>
		/// <param name="defaultValue">
		/// The default value to return if the value was not present.
		/// </param>
		/// <returns>
		/// Boolean value. False if not convertible.
		/// </returns>
		public static bool ToBool(string value, bool defaultValue = false)
		{
			//	A try .. catch block was originally implemented here, but the
			//	following text was being sent to output on each unsuccessful
			//	match.
			//	Exception thrown: 'System.FormatException' in mscorlib.dll
			bool result = false;

			if(value?.Length > 0)
			{
				result = Regex.IsMatch(value, ResourceMain.rxBoolTrue);
			}
			else
			{
				result = defaultValue;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ValidatePath																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the fully qualified path of the folder or file if it exists or
		/// can be created.
		/// </summary>
		/// <param name="pathType">
		/// Name of the type of path to inspect.
		/// </param>
		/// <param name="relativePath">
		/// The relative or absolute path to validate.
		/// </param>
		/// <param name="workingPath">
		/// The working path to reference if the main path was relative.
		/// </param>
		/// <param name="canCreate">
		/// Value indicating whether the folder or file can be created.
		/// </param>
		/// <param name="required">
		/// Value indicating whether the file is required.
		/// </param>
		/// <returns>
		/// If the path or filename was legitimate, the fully qualified name is
		/// returned. Otherwise, an empty string is returned.
		/// </returns>
		public static string ValidatePath(string pathType,
			string relativePath, string workingPath,
			bool canCreate = false, bool required = true)
		{
			string path = "";
			string result = "";

			if(relativePath?.Length > 0)
			{
				path = Path.GetFullPath(AbsolutePath(relativePath, workingPath));
				if(path.Length > 0 &&
					(Path.Exists(path) || canCreate))
				{
					result = path;
				}
				else if(path.Length == 0)
				{
					Console.WriteLine($" {pathType} Error: No path was specified.");
				}
				else
				{
					Console.WriteLine($" {pathType} Error: Path or file not found...");
				}
			}
			else if(required)
			{
				Console.WriteLine($" {pathType} Error: No filename was specified.");
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

}
