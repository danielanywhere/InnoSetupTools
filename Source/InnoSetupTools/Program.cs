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
using System.Diagnostics;
using System.IO;
using static InnoSetupTools.InnoSetupToolsUtil;

using Newtonsoft.Json;

namespace InnoSetupTools
{
	//*-------------------------------------------------------------------------*
	//*	Program																																	*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Main instance of the InnoSetupTools application.
	/// </summary>
	public class Program
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
		//*	_Main																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Configure and run the application.
		/// </summary>
		public static void Main(string[] args)
		{
			bool bShowHelp = false; //	Flag - Explicit Show Help.
			string configFilename = "";
			string key = "";        //	Current Parameter Key.
			string lowerArg = "";   //	Current Lowercase Argument.
			string message = "";    //	Message to display in Console.
			Program prg = new Program();  //	Initialized instance.

			ConsoleTraceListener consoleListener = new ConsoleTraceListener();
			Trace.Listeners.Add(consoleListener);

			Console.WriteLine("InnoSetupTools.exe");
			foreach(string arg in args)
			{
				lowerArg = arg.ToLower();
				key = "/?";
				if(lowerArg == key)
				{
					bShowHelp = true;
					continue;
				}
				key = "/config:";
				if(lowerArg.StartsWith(key))
				{
					//	Don't parse until we have possible working directory.
					configFilename = GetFullFilename(arg.Substring(key.Length));
					continue;
				}
				key = "/wait";
				if(lowerArg.StartsWith(key))
				{
					prg.mWaitAfterEnd = true;
					continue;
				}
				key = "/workingpath:";
				if(lowerArg.StartsWith(key))
				{
					prg.mWorkingPath =
						GetFullFoldername(arg.Substring(key.Length), false, "Working");
					continue;
				}
			}
			if(configFilename.Length > 0)
			{
				//	Configuration filename was specified.
				prg.mConfigFilename =
					GetFullFilename(AbsolutePath(configFilename, prg.mWorkingPath),
					false, "Configuration");
				if(prg.mConfigFilename.Length == 0)
				{
					message = "Please specific a valid configuration file...";
					bShowHelp = true;
				}
			}
			else
			{
				message = "Please specify a configuration filename...";
				bShowHelp = true;
			}
			if(bShowHelp)
			{
				//	Display Syntax.
				Console.WriteLine(message.ToString() + "\r\n" + ResourceMain.Syntax);
			}
			else
			{
				//	Run the configured application.
				prg.Run();
			}
			if(prg.mWaitAfterEnd)
			{
				Console.WriteLine("Press [Enter] to exit...");
				Console.ReadLine();
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Actions																																*
		//*-----------------------------------------------------------------------*
		private ActionItem mActions = new ActionItem();
		/// <summary>
		/// Get a reference to the collection of actions to be performed in this
		/// session.
		/// </summary>
		public ActionItem Actions
		{
			get { return mActions; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	ConfigFilename																												*
		//*-----------------------------------------------------------------------*
		private string mConfigFilename = "";
		/// <summary>
		/// Get/Set the path and filename of the configuration file for this
		/// session.
		/// </summary>
		public string ConfigFilename
		{
			get { return mConfigFilename; }
			set { mConfigFilename = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Run																																		*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Run the configured application.
		/// </summary>
		public void Run()
		{
			string content = "";

			if(mConfigFilename.Length > 0)
			{
				content = File.ReadAllText(mConfigFilename);
				mActions = JsonConvert.DeserializeObject<ActionItem>(content);
				mActions.SetParent();
				mActions.ResolveEnvironmentStrings();
				if(mActions.WorkingPath.Length == 0)
				{
					mActions.WorkingPath = mWorkingPath;
				}
				if(mActions.CertTimestampUrl.Length == 0)
				{
					mActions.CertTimestampUrl = "http://timestamp.digicert.com";
				}
				if(mActions.InnoSetupCompilerFilename.Length == 0)
				{
					mActions.InnoSetupCompilerFilename =
						@"C:\Program Files (x86)\Inno Setup 6\ISCC.exe";
				}
				mActions.ResolveEnvironmentStrings();
				mActions.Run();
			}
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WaitAfterEnd																													*
		//*-----------------------------------------------------------------------*
		private bool mWaitAfterEnd = false;
		/// <summary>
		/// Get/Set a value indicating whether to wait for user keypress after 
		/// processing has completed.
		/// </summary>
		public bool WaitAfterEnd
		{
			get { return mWaitAfterEnd; }
			set { mWaitAfterEnd = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	WorkingPath																														*
		//*-----------------------------------------------------------------------*
		private string mWorkingPath = "";
		/// <summary>
		/// Get/Set the working path for this session.
		/// </summary>
		public string WorkingPath
		{
			get { return mWorkingPath; }
			set { mWorkingPath = value; }
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*

}