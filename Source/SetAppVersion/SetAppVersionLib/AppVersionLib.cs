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
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SetAppVersionLib
{
	//*-------------------------------------------------------------------------*
	//*	AppVersionLib																														*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Library functionality for setting a common application version in
	/// multiple projects.
	/// </summary>
	public class AppVersionLib
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	_Constructor																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Create a new instance of the AppVersionLib item.
		/// </summary>
		public AppVersionLib()
		{
			DateTime now = DateTime.Now;

			mVersion = $"{now.Year.ToString().Substring(2)}." +
				$"{now.Month + 20}{now.Day.ToString().PadLeft(2, '0')}." +
				$"{(now.Hour + 30).ToString().PadLeft(2, '0')}" +
				$"{now.Minute.ToString().PadLeft(2, '0')}";
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CsProject																															*
		//*-----------------------------------------------------------------------*
		private string mCsProject = "";
		/// <summary>
		/// Get/Set the full path and filename of the C# Project to open.
		/// </summary>
		public string CsProject
		{
			get { return mCsProject; }
			set { mCsProject = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoProject																														*
		//*-----------------------------------------------------------------------*
		private string mInnoProject = "";
		/// <summary>
		/// Get/Set the path and filename of the InnoSetup project to update.
		/// </summary>
		public string InnoProject
		{
			get { return mInnoProject; }
			set { mInnoProject = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoSetupFile																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="InnoSetupFile">InnoSetupFile</see>.
		/// </summary>
		private List<string> mInnoSetupFile = null;
		/// <summary>
		/// Get/Set a reference to the contents of a loaded InnoSetup script file,
		/// expressed as a list of individual lines.
		/// </summary>
		public List<string> InnoSetupFile
		{
			get { return mInnoSetupFile; }
			set { mInnoSetupFile = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoVariableName																											*
		//*-----------------------------------------------------------------------*
		private string mInnoVariableName = "";
		/// <summary>
		/// Get/Set the name of the variable to write to in the InnoSetup project.
		/// </summary>
		public string InnoVariableName
		{
			get { return mInnoVariableName; }
			set { mInnoVariableName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SetVersion																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the version using all of the collected information.
		/// </summary>
		/// <param name="quiet">
		/// Optional value indicating whether to suppress messages.
		/// Default = false.
		/// </param>
		public void SetVersion(bool quiet = false)
		{
			bool bRecompileCSProject = false;
			string content = "";
			int count = 0;
			int index = 0;

			Trace.WriteLine($"Version: {mVersion}");

			if(mVersionOutputFilename.Length > 0)
			{
				File.WriteAllText(mVersionOutputFilename, mVersion);
			}

			if(mCsProject.Length > 0)
			{
				//	A C# project file was specified.
				Trace.WriteLine("Processing C# Project...");
				//	Check to see if version or version prefix variables have been
				//	specified. If neither been used in the PROJ file,
				//	<VersionPrefix>...</VersionPrefix> needs to be
				//	inserted into the <PropertyGroup> node.
				content = File.ReadAllText(mCsProject);
				if(Regex.IsMatch(content,
					"(" +
					"\\<Version\\>.*?\\</Version\\>|" +
					"\\<VersionPrefix\\>.*?\\</VersionPrefix\\>" +
					")"))
				{
					//	Version or version prefix values are already defined.
					content = Regex.Replace(content,
						"\\<Version\\>.*?\\</Version\\>",
						$"<Version>{mVersion}</Version>");
					content = Regex.Replace(content,
						"\\<VersionPrefix\\>.*?\\</VersionPrefix\\>",
						$"<VersionPrefix>{mVersion}</VersionPrefix>");
				}
				else
				{
					//	Initialize version prefix.
					content = Regex.Replace(content,
						"(?s:\\<PropertyGroup\\>(?<content>.*?)</PropertyGroup>)",
						"<PropertyGroup>${content}" +
						$"  <VersionPrefix>{mVersion}</VersionPrefix>\r\n" +
						"  </PropertyGroup>");
				}
				File.WriteAllText(mCsProject, content);
				bRecompileCSProject = true;
			}
			if(mInnoProject.Length > 0)
			{
				//	An InnoSetup project file was specified.
				Trace.WriteLine("Processing InnoSetup...");
				content = File.ReadAllText(mInnoProject);
				if(mInnoVariableName.Length > 0)
				{
					//	A variable name was used.
					content = Regex.Replace(content,
						$"(?m:(?<appVersion>^#define\\s*{mInnoVariableName}\\s*).*" + "$)",
						"${appVersion}\"" + mVersion + "\"");
				}
				else
				{
					//	If no variable was used, then the AppVersion system variable
					//	is used.
					content = Regex.Replace(content,
						"(?m:(?<appVersion>^AppVersion\\s*=\\s*).*$)",
						"${appVersion}\"" + mVersion + "\"\r\n");
				}
				File.WriteAllText(mInnoProject, content);
			}
			if(mInnoSetupFile?.Count > 0)
			{
				//	A loaded InnoSetup profile file was specified.
				Trace.WriteLine("Processing InnoSetup...");
				count = mInnoSetupFile.Count;
				for(index = 0; index < count; index ++)
				{
					content = mInnoSetupFile[index];
					if(mInnoVariableName.Length > 0)
					{
						//	A variable name was used.
						content = Regex.Replace(content,
							$"(?m:(?<appVersion>^#define\\s*{mInnoVariableName}\\s*).*" + "$)",
							"${appVersion}\"" + mVersion + "\"");
					}
					else
					{
						//	If no variable was used, then the AppVersion system variable
						//	is used.
						content = Regex.Replace(content,
							"(?m:(?<appVersion>^AppVersion\\s*=\\s*).*$)",
							"${appVersion}\"" + mVersion + "\"");
					}
					mInnoSetupFile[index] = content;
				}
			}
			if(mWAPProject.Length > 0)
			{
				//	A Windows Application Packaging project file was specified.
				//	Windows Application Packaging requires 4 digits.
				Trace.WriteLine("Processing Windows Application Package...");
				content = File.ReadAllText(mWAPProject);
				content = Regex.Replace(content,
					"(?s:(?<ct1>\\<Identity.*?)" +
					"(?<ver>Version\\s*=\\s*\\\"[^\\\"]*\\\")" +
					"(?<ct2>.*?/\\>))",
					"${ct1}Version=\"" + mVersion + ".0\"${ct2}");
				File.WriteAllText(mWAPProject, content);
				bRecompileCSProject = true;
			}
			if(bRecompileCSProject && !quiet)
			{
				Trace.WriteLine(
					"NOTE: Please recompile your C# project before proceeding.");
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Version																																*
		//*-----------------------------------------------------------------------*
		private string mVersion = "";
		/// <summary>
		/// Get/Set the version number to set on the target files.
		/// </summary>
		public string Version
		{
			get { return mVersion; }
			set { mVersion = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	VersionOutputFilename																									*
		//*-----------------------------------------------------------------------*
		private string mVersionOutputFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the text file to which the new
		/// version will be output.
		/// </summary>
		public string VersionOutputFilename
		{
			get { return mVersionOutputFilename; }
			set { mVersionOutputFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WAPProject																														*
		//*-----------------------------------------------------------------------*
		private string mWAPProject = "";
		/// <summary>
		/// Get/Set the full path and filename of the Windows Application
		/// Packaging project containing the version to set.
		/// </summary>
		public string WAPProject
		{
			get { return mWAPProject; }
			set { mWAPProject = value; }
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
