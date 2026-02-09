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
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SetAppVersionLib;

using static InnoSetupTools.InnoSetupToolsUtil;

//	TODO: Set script from template.
//	TODO: Add PrivilegesRequired=admin if running machine install.
//	TODO: Set AppCopyright message.

namespace InnoSetupTools
{
	//*-------------------------------------------------------------------------*
	//*	ActionCollection																												*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of ActionItem Items.
	/// </summary>
	public class ActionCollection : List<ActionItem>
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

		////*-----------------------------------------------------------------------*
		////*	Active																																*
		////*-----------------------------------------------------------------------*
		//private bool mActive = true;
		///// <summary>
		///// Get/Set a value indicating whether this item is active.
		///// </summary>
		///// <remarks>
		///// By default, this flag is true. Whether it has been set to false locally
		///// or by a parent, it will act as false.
		///// </remarks>
		//[JsonIgnore]
		//public bool Active
		//{
		//	get
		//	{
		//		bool result = mActive;

		//		if(result && mParent != null)
		//		{
		//			result = mParent.Active;
		//		}
		//		return result;
		//	}
		//	set { mActive = value; }
		//}
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* AddOptions																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Add the members of the parent item options list to the provided list.
		/// </summary>
		/// <param name="optionsList">
		/// Reference to the target options list.
		/// </param>
		public void AddOptions(List<string> optionsList)
		{
			if(optionsList != null && Parent != null)
			{
				Parent.AddOptions(optionsList);
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		private ActionItem mParent = null;
		/// <summary>
		/// Get/Set a reference to the action to which this collection is attached.
		/// </summary>
		[JsonIgnore]
		public ActionItem Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ResolveEnvironmentStrings																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// The user is now allowed to specified environment strings. Resolve all
		/// of these uses.
		/// </summary>
		public void ResolveEnvironmentStrings()
		{
			foreach(ActionItem actionItem in this)
			{
				actionItem.ResolveEnvironmentStrings();
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the actions in this collection.
		/// </summary>
		public void Run()
		{
			foreach(ActionItem actionItem in this)
			{
				actionItem.Run();
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SetParent																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the parent property on the children of this item, and all
		/// descendants.
		/// </summary>
		public void SetParent()
		{
			foreach(ActionItem actionItem in this)
			{
				actionItem.Parent = this;
				actionItem.SetParent();
			}
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	ActionItem																															*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Individual action to be taken.
	/// </summary>
	public class ActionItem
	{
		//*************************************************************************
		//*	Private																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//* CompileAndPublish																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Compile and publish the 
		/// </summary>
		/// <param name="item">
		/// Reference to the action item for which the compile and publish
		/// activity will take place.
		/// </param>
		/// <remarks>
		/// <para>Following are the steps to compile and publish.</para>
		/// <list type="bullet">
		/// <item>Optionally set the version in the C# project and the InnoSetup
		/// script.</item>
		/// <item>Delete the bin and obj folders from previous compile.</item>
		/// <item>Restore libraries on application.</item>
		/// <item>Build the executable file.</item>
		/// <item>Sign the executable file.</item>
		/// <item>Update the setup script files list from current output
		/// files.</item>
		/// <item>Delete previous .E32 files from setup output.</item>
		/// <item>Run Inno Setup to create unsigned uninstaller.</item>
		/// <item>Sign the uninstaller file.</item>
		/// <item>Run Inno Setup to create the setup file.</item>
		/// <item>Sign the setup file.</item>
		/// </list>
		/// </remarks>
		private static void CompileAndPublish(ActionItem item)
		{
			AppVersionLib appVer = null;
			bool bContinue = true;
			string certFilename = "";
			string csProjectFilename = "";
			string exeFilename = "";
			string exeFolderName = "";
			string innoScriptFilename = "";
			NameValueCollection properties = null;
			string setupFilename = "";
			List<string> setupScript = null;
			string uninstallerFilename = "";

			if(item != null)
			{
				csProjectFilename = ValidatePath("CSharpProjectFilename",
					item.CSharpProjectFilename, item.WorkingPath, false);
				exeFilename = ValidatePath("ExeFilename",
					item.ExeFilename, item.WorkingPath, true);
				setupFilename = ValidatePath("SetupFilename",
					item.SetupFilename, item.WorkingPath, true);
				certFilename = ValidatePath("CertFilename",
					item.CertFilename, item.WorkingPath, false, false);
				innoScriptFilename = ValidatePath("InnoScriptFilename",
					item.InnoScriptFilename, item.WorkingPath, false);
				InnoSetupToolsUtil.InnoSetupCompilerFilename =
					ValidatePath("InnoSetupCompilerFilename",
					item.InnoSetupCompilerFilename, item.WorkingPath, false);

				if(csProjectFilename.Length > 0 &&
					exeFilename.Length > 0 &&
					setupFilename.Length > 0 &&
					innoScriptFilename.Length > 0)
				{
					appVer = new AppVersionLib();

					exeFolderName = Path.GetDirectoryName(exeFilename);

					setupScript = LoadInnoSetupScript(innoScriptFilename);
					if(ToBool(GetOptionValue(item.Options, "SetVersion")))
					{
						appVer.CsProject = csProjectFilename;
						appVer.InnoSetupFile = setupScript;
						appVer.InnoVariableName = item.InnoVersionVariable;
						//	The version will follow yy.20+mmdd.30+hhmm format.
						appVer.SetVersion(true);
					}
					CsDeleteBinAndObjFolders(csProjectFilename);
					bContinue = CsProjectRestore(csProjectFilename);

					//	Build the executable file.
					if(bContinue)
					{
						bContinue = CsBuildRelease(csProjectFilename,
							item.ProjectBuildLevel);
					}

					//	Sign the executable file.
					if(bContinue)
					{
						bContinue = SignAndVerify(item.CertFilename, item.CertPassword,
							item.ShaThumbprint, exeFilename);
					}

					//	Set the files list in the package.
					if(bContinue)
					{
						InnoSetupSetPackageFiles(setupScript, item.NETMajorVersion,
							item.ProjectBuildLevel, exeFolderName);
					}

					//	Configure the setup for the type of .NET support it will receive.
					if(bContinue)
					{
						InnoSetupUpdateCode(setupScript, item.NETMajorVersion,
							item.ProjectBuildLevel);
					}

					SaveInnoSetupScript(setupScript, innoScriptFilename);

					//	Delete any existing .E32 files from the output folder.
					if(bContinue)
					{
						bContinue = InnoSetupDeleteE32(innoScriptFilename);
					}

					//	Run the InnoSetup compiler to create the signed uninstaller.
					if(bContinue)
					{
						uninstallerFilename =
							InnoSetupCreateSignedUninstaller(innoScriptFilename);
						bContinue = (uninstallerFilename?.Length > 0);
					}

					//	Sign the uninstaller file.
					if(bContinue)
					{
						bContinue = SignAndVerify(item.CertFilename, item.CertPassword,
							item.ShaThumbprint, uninstallerFilename);
					}

					//	Run the setup compiler again to fully compile the setup
					//	application.
					if(bContinue)
					{
						bContinue = InnoSetupCreateSetup(innoScriptFilename);
					}

					//	Sign the setup file.
					if(bContinue)
					{
						bContinue = SignAndVerify(item.CertFilename, item.CertPassword,
							item.ShaThumbprint, setupFilename);
					}

					if(bContinue)
					{
						Console.WriteLine(
							$" Setup file created: {Path.GetFileName(setupFilename)}");
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* SetVersion																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the version on the specified files.
		/// </summary>
		/// <param name="item">
		/// Reference to the current action for which the version will be set.
		/// </param>
		private static void SetVersion(ActionItem item)
		{
			AppVersionLib appVer = null;
			string csProjectFilename = "";
			string innoScriptFilename = "";

			if(item != null)
			{
				appVer = new AppVersionLib();

				innoScriptFilename = ValidatePath("InnoScriptFilename",
					item.InnoScriptFilename, item.WorkingPath, true);
				csProjectFilename = ValidatePath("CSharpProjectFilename",
					item.CSharpProjectFilename, item.WorkingPath, true);

				if((innoScriptFilename.Length > 0 ||
					csProjectFilename.Length > 0))
				{
					//	Necessary files were provided.
					appVer.InnoVariableName = item.InnoVersionVariable;
					appVer.InnoProject = innoScriptFilename;
					appVer.CsProject = csProjectFilename;
					appVer.SetVersion(false);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*************************************************************************
		//*	Protected																															*
		//*************************************************************************
		//*************************************************************************
		//*	Public																																*
		//*************************************************************************
		//*-----------------------------------------------------------------------*
		//*	Actions																																*
		//*-----------------------------------------------------------------------*
		private ActionCollection mActions = new ActionCollection();
		/// <summary>
		/// Get a reference to the collection of actions handled by this action.
		/// </summary>
		[JsonProperty(Order = 8)]
		public ActionCollection Actions
		{
			get { return mActions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ActionType																														*
		//*-----------------------------------------------------------------------*
		private ActionTypeEnum mActionType = ActionTypeEnum.None;
		/// <summary>
		/// Get/Set the action type to use in this iteration.
		/// </summary>
		[JsonProperty(Order = 3)]
		public ActionTypeEnum ActionType
		{
			get { return mActionType; }
			set { mActionType = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Active																																*
		//*-----------------------------------------------------------------------*
		private bool mActive = true;
		/// <summary>
		/// Get/Set a value indicating whether this item is active.
		/// </summary>
		/// <remarks>
		/// By default, this flag is true. Whether it has been set to false locally
		/// or by a parent, it will act as false.
		/// </remarks>
		public bool Active
		{
			get
			{
				bool result = mActive;

				if(result && mParent?.Parent != null)
				{
					result = mParent.Parent.Active;
				}
				return result;
			}
			set { mActive = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* AddOptions																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Add the members of the local options list to the provided list.
		/// </summary>
		/// <param name="optionsList">
		/// Reference to the target options list.
		/// </param>
		public void AddOptions(List<string> optionsList)
		{
			if(optionsList != null)
			{
				foreach(string optionItem in mOptions)
				{
					if(!optionsList.Contains(optionItem,
						StringComparer.OrdinalIgnoreCase))
					{
						optionsList.Add(optionItem);
					}
				}
				if(Parent != null)
				{
					Parent.AddOptions(optionsList);
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CertFilename																													*
		//*-----------------------------------------------------------------------*
		private string mCertFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the certificate file for this project.
		/// </summary>
		public string CertFilename
		{
			get
			{
				string result = mCertFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CertFilename;
				}
				return result;
			}
			set { mCertFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CertPassword																													*
		//*-----------------------------------------------------------------------*
		private string mCertPassword = "";
		/// <summary>
		/// Get/Set the password for the certificate file on this project.
		/// </summary>
		public string CertPassword
		{
			get
			{
				string result = mCertPassword;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CertPassword;
				}
				return result;
			}
			set { mCertPassword = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CertTimestampUrl																											*
		//*-----------------------------------------------------------------------*
		private string mCertTimestampUrl = "";
		/// <summary>
		/// Get/Set the certificate service timestamp URL.
		/// </summary>
		public string CertTimestampUrl
		{
			get
			{
				string result = mCertTimestampUrl;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CertTimestampUrl;
				}
				return result;
			}
			set { mCertTimestampUrl = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CSharpProjectFilename																									*
		//*-----------------------------------------------------------------------*
		private string mCSharpProjectFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the C# project to be versioned and
		/// compiled.
		/// </summary>
		public string CSharpProjectFilename
		{
			get
			{
				string result = mCSharpProjectFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CSharpProjectFilename;
				}
				return result;
			}
			set { mCSharpProjectFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CSharpPublishSettingsFilename																					*
		//*-----------------------------------------------------------------------*
		private string mCSharpPublishSettingsFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the C# publish settings file.
		/// </summary>
		public string CSharpPublishSettingsFilename
		{
			get
			{
				string result = mCSharpPublishSettingsFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CSharpPublishSettingsFilename;
				}
				return result;
			}
			set { mCSharpPublishSettingsFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	CSharpSolutionFilename																								*
		//*-----------------------------------------------------------------------*
		private string mCSharpSolutionFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the C# solution file to be compiled.
		/// </summary>
		public string CSharpSolutionFilename
		{
			get
			{
				string result = mCSharpSolutionFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.CSharpSolutionFilename;
				}
				return result;
			}
			set { mCSharpSolutionFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ExeFilename																														*
		//*-----------------------------------------------------------------------*
		private string mExeFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the main EXE file for this deployment.
		/// </summary>
		public string ExeFilename
		{
			get
			{
				string result = mExeFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.ExeFilename;
				}
				return result;
			}
			set { mExeFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoScriptFilename																										*
		//*-----------------------------------------------------------------------*
		private string mInnoScriptFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the InnoSetup script file for this
		/// session.
		/// </summary>
		public string InnoScriptFilename
		{
			get
			{
				string result = mInnoScriptFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.InnoScriptFilename;
				}
				return result;
			}
			set { mInnoScriptFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoSetupCompilerFilename																							*
		//*-----------------------------------------------------------------------*
		private string mInnoSetupCompilerFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the InnoSetup compiler.
		/// </summary>
		public string InnoSetupCompilerFilename
		{
			get
			{
				string result = mInnoSetupCompilerFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.InnoSetupCompilerFilename;
				}
				return result;
			}
			set { mInnoSetupCompilerFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InnoVersionVariable																										*
		//*-----------------------------------------------------------------------*
		private string mInnoVersionVariable = "";
		/// <summary>
		/// Get/Set the name of the custom version variable being used within the
		/// InnoScript file.
		/// </summary>
		public string InnoVersionVariable
		{
			get
			{
				string result = mInnoVersionVariable;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.InnoVersionVariable;
				}
				return result;
			}
			set { mInnoVersionVariable = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	InputFoldername																												*
		//*-----------------------------------------------------------------------*
		private string mInputFoldername = "";
		/// <summary>
		/// Get/Set the path to the input folder for the current action.
		/// </summary>
		public string InputFoldername
		{
			get
			{
				string result = mInputFoldername;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.InputFoldername;
				}
				return result;
			}
			set { mInputFoldername = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	NETMajorVersion																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="NETMajorVersion">NETMajorVersion</see>.
		/// </summary>
		private int mNETMajorVersion = 0;
		/// <summary>
		/// Get/Set the .NET major version required required for this application.
		/// </summary>
		/// <remarks>
		/// This value is inherited.
		/// </remarks>
		public int NETMajorVersion
		{
			get
			{
				int result = mNETMajorVersion;

				if(result == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.NETMajorVersion;
				}
				return result;
			}
			set { mNETMajorVersion = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Options																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Options">Options</see>.
		/// </summary>
		private List<string> mOptions = new List<string>();
		/// <summary>
		/// Get a reference to the list of options on this action.
		/// </summary>
		/// <remarks>
		/// Individual options are inherited.
		/// </remarks>
		public List<string> Options
		{
			get
			{
				List<string> result = new List<string>(mOptions);

				if(Parent != null)
				{
					Parent.AddOptions(result);
				}
				return mOptions;
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputFilename																												*
		//*-----------------------------------------------------------------------*
		private string mOutputFilename = "";
		/// <summary>
		/// Get/Set the relative or absolute filename of the output file to be
		/// generated.
		/// </summary>
		[JsonProperty(Order = 6)]
		public string OutputFilename
		{
			get
			{
				string result = mOutputFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.OutputFilename;
				}
				return result;
			}
			set { mOutputFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	OutputFoldername																											*
		//*-----------------------------------------------------------------------*
		private string mOutputFoldername = "";
		/// <summary>
		/// Get/Set the relative or absolute folder path of the output files to be
		/// generated.
		/// </summary>
		public string OutputFoldername
		{
			get
			{
				string result = mOutputFoldername;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.OutputFoldername;
				}
				return result;
			}
			set { mOutputFoldername = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Parent																																*
		//*-----------------------------------------------------------------------*
		private ActionCollection mParent = null;
		/// <summary>
		/// Get/Set a reference to the collection of which this item is a member.
		/// </summary>
		[JsonIgnore]
		public ActionCollection Parent
		{
			get { return mParent; }
			set { mParent = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ProjectBuildLevel																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="ProjectBuildLevel">ProjectBuildLevel</see>.
		/// </summary>
		private ProjectBuildLevelEnum mProjectBuildLevel =
			ProjectBuildLevelEnum.None;
		/// <summary>
		/// Get/Set the build-level of the .NET project.
		/// </summary>
		[JsonConverter(typeof(StringEnumConverter))]
		public ProjectBuildLevelEnum ProjectBuildLevel
		{
			get
			{
				ProjectBuildLevelEnum result = mProjectBuildLevel;

				if(result == ProjectBuildLevelEnum.None && mParent?.Parent != null)
				{
					result = mParent.Parent.ProjectBuildLevel;
				}
				return result;
			}
			set { mProjectBuildLevel = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ProjectFoldername																											*
		//*-----------------------------------------------------------------------*
		private string mProjectFoldername = "";
		/// <summary>
		/// Get/Set the path name to the folder containing the project.
		/// </summary>
		public string ProjectFoldername
		{
			get
			{
				string result = mProjectFoldername;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.ProjectFoldername;
				}
				return result;
			}
			set { mProjectFoldername = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ProjectName																														*
		//*-----------------------------------------------------------------------*
		private string mProjectName = "";
		/// <summary>
		/// Get/Set the base project name for this session.
		/// </summary>
		public string ProjectName
		{
			get
			{
				string result = mProjectName;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.ProjectName;
				}
				return result;
			}
			set { mProjectName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Remarks																																*
		//*-----------------------------------------------------------------------*
		private List<string> mRemarks = new List<string>();
		/// <summary>
		/// Get a reference to the comments and remarks about the current action.
		/// </summary>
		[JsonProperty(Order = 0)]
		public List<string> Remarks
		{
			get { return mRemarks; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* ResolveEnvironmentStrings																							*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// The user is now allowed to specified environment strings. Resolve all
		/// of these uses.
		/// </summary>
		public void ResolveEnvironmentStrings()
		{
			ResolveEnvironmentStrings("mCertFilename");
			ResolveEnvironmentStrings("mCSharpProjectFilename");
			ResolveEnvironmentStrings("mCSharpPublishSettingsFilename");
			ResolveEnvironmentStrings("mCSharpSolutionFilename");
			ResolveEnvironmentStrings("mExeFilename");
			ResolveEnvironmentStrings("mInnoScriptFilename");
			ResolveEnvironmentStrings("mInnoSetupCompilerFilename");
			ResolveEnvironmentStrings("mInputFoldername");
			ResolveEnvironmentStrings("mOutputFilename");
			ResolveEnvironmentStrings("mOutputFoldername");
			ResolveEnvironmentStrings("mProjectFoldername");
			ResolveEnvironmentStrings("mSetAppVersionFilename");
			ResolveEnvironmentStrings("mWorkingPath");

			mActions.ResolveEnvironmentStrings();
		}
		//*- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -*
		/// <summary>
		/// Resolve all of the environment strings found in a specified 
		/// </summary>
		/// <param name="fieldName">
		/// Name of a private field within which to check for environment
		/// variables.
		/// </param>
		private void ResolveEnvironmentStrings(string fieldName)
		{
			BindingFlags bindingFlags =
				BindingFlags.Instance | BindingFlags.NonPublic;
			FieldInfo field;
			string propertyValue = "";
			Type type = this.GetType();

			if(fieldName?.Length > 0)
			{
				type = this.GetType();
				field = type.GetField(fieldName, bindingFlags);
				if(field != null)
				{
					propertyValue = (string)field.GetValue(this);
					if(propertyValue?.Length > 0)
					{
						propertyValue = Regex.Replace(
							propertyValue,
							@"(?i:(?<source>\%USERPROFILE\%))",
							Environment.GetFolderPath(
								Environment.SpecialFolder.UserProfile));
						field.SetValue(this, propertyValue);
					}
				}
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the activities of this action.
		/// </summary>
		public void Run()
		{
			string inputFoldername = "";
			string outputFilename = "";

			NameValueCollection properties = new NameValueCollection();

			if(Active)
			{
				if(mOutputFilename?.Length > 0)
				{
					Console.WriteLine($"Output File: {Path.GetFileName(mOutputFilename)}");
				}
				if(mActions.Count > 0)
				{
					//	Run the sub-actions first so this item will aggregate their
					//	results.
					mActions.Run();
				}
			}
			if(mActionType != ActionTypeEnum.None)
			{
				Console.WriteLine($" Command: {mActionType}");
			}
			if(Active)
			{
				switch(mActionType)
				{
					case ActionTypeEnum.CompileAndPublish:
						//	TODO: Integrate package version selection.
						//	TODO: DotnetRuntimeInstallerName
						//	TODO: Separate auto-generated code sections.
						if(
							(ProjectBuildLevel == ProjectBuildLevelEnum.NETInstallDownload ||
							ProjectBuildLevel == ProjectBuildLevelEnum.NETInstallIncluded) &&
							NETMajorVersion == 0)
						{
							Trace.WriteLine(
								".NET Installer was specified in ProjectBuildLevel.");
							Trace.WriteLine(
								" Please provide the needed .NET major version in " +
								"'NETMajorVersion'");
							Trace.WriteLine("Publish cancelled...");
						}
						else
						{
							CompileAndPublish(this);
						}
						break;
					case ActionTypeEnum.SetPackageFiles:
						//	Set the package files list.
						inputFoldername = ValidatePath("InputFoldername",
							InputFoldername, WorkingPath, false);
						outputFilename = ValidatePath("OutputFilename",
							OutputFilename, WorkingPath, false);

						if(inputFoldername.Length > 0 &&
							outputFilename.Length > 0)
						{
							InnoSetupSetPackageFiles(inputFoldername, outputFilename);
						}
						break;
					case ActionTypeEnum.SetVersion:
						//	Set the version on all interested project files.
						//	In this version, C# and InnoSetup projects are supported.
						SetVersion(this);
						break;
					case ActionTypeEnum.None:
						//	The main action probably won't be set.
						break;
					default:
						Console.WriteLine($"Unknown action: {mActionType}");
						break;
				}
			}
			else
			{
				Console.WriteLine("  Skipping step...");
			}
		}
		//*-----------------------------------------------------------------------*

		////*-----------------------------------------------------------------------*
		////*	SetAppVersionFilename																									*
		////*-----------------------------------------------------------------------*
		//private string mSetAppVersionFilename = "";
		///// <summary>
		///// Get/Set the path and filename of SetAppVersion.exe
		///// </summary>
		//public string SetAppVersionFilename
		//{
		//	get
		//	{
		//		string result = mSetAppVersionFilename;

		//		if(result.Length == 0 && mParent != null)
		//		{
		//			result = mParent.SetAppVersionFilename;
		//		}
		//		return result;
		//	}
		//	set { mSetAppVersionFilename = value; }
		//}
		////*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SetParent																															*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Set the parent property on the this item's actions, and all
		/// descendants.
		/// </summary>
		public void SetParent()
		{
			mActions.Parent = this;
			mActions.SetParent();
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	SetupFilename																													*
		//*-----------------------------------------------------------------------*
		private string mSetupFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the resulting setup file.
		/// </summary>
		public string SetupFilename
		{
			get
			{
				string result = mSetupFilename;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.SetupFilename;
				}
				return result;
			}
			set { mSetupFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ShaThumbprint																													*
		//*-----------------------------------------------------------------------*
		private string mShaThumbprint = "";
		/// <summary>
		/// Get/Set the public SHA1 thumbprint for the certificate on this project.
		/// </summary>
		public string ShaThumbprint
		{
			get
			{
				string result = mShaThumbprint;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.ShaThumbprint;
				}
				return result;
			}
			set { mShaThumbprint = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Value																																	*
		//*-----------------------------------------------------------------------*
		private string mValue = "";
		/// <summary>
		/// Get/Set the general parameter value for this action.
		/// </summary>
		public string Value
		{
			get
			{
				string result = mValue;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.Value;
				}
				return result;
			}
			set { mValue = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingPath																														*
		//*-----------------------------------------------------------------------*
		private string mWorkingPath = "";
		/// <summary>
		/// Get/Set the base working path for this instance.
		/// </summary>
		[JsonProperty(Order = 1)]
		public string WorkingPath
		{
			get
			{
				string result = mWorkingPath;

				if(result.Length == 0 && mParent?.Parent != null)
				{
					result = mParent.Parent.WorkingPath;
				}
				return result;
			}
			set { mWorkingPath = value; }
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
