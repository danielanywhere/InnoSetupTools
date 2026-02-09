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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InnoSetupTools
{
	//*-------------------------------------------------------------------------*
	//*	RuntimeInstallerReferenceCollection																			*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of RuntimeInstallerReferenceItem Items.
	/// </summary>
	public class RuntimeInstallerReferenceCollection :
		List<RuntimeInstallerReferenceItem>
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
		//* GetInstallerName																											*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the name of the installer for the specified major .NET version.
		/// </summary>
		/// <param name="majorDotnetVersion">
		/// The major .NET version for which the runtime installer will be found.
		/// </param>
		/// <returns>
		/// The name of the runtime installer associated with the specified
		/// major .NET version, if found. Otherwise, an empty string.
		/// </returns>
		public string GetInstallerName(int majorDotnetVersion)
		{
			RuntimeInstallerReferenceItem reference =
				this.FirstOrDefault(x => x.MajorVersion == majorDotnetVersion);
			string result = "";

			if(reference != null)
			{
				result = reference.RuntimeInstallerName;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//* GetInstallerVersion																										*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Return the version of the installer for the specified major .NET
		/// version.
		/// </summary>
		/// <param name="majorDotnetVersion">
		/// The major .NET version for which the runtime installer will be found.
		/// </param>
		/// <returns>
		/// The version of the runtime installer associated with the specified
		/// major .NET version, if found. Otherwise, an empty string.
		/// </returns>
		public string GetInstallerVersion(int majorDotnetVersion)
		{
			RuntimeInstallerReferenceItem reference =
				this.FirstOrDefault(x => x.MajorVersion == majorDotnetVersion);
			string result = "";

			if(reference != null)
			{
				result = reference.RuntimeVersion;
			}
			return result;
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	RuntimeInstallerReferenceItem																						*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Single runtime installer reference.
	/// </summary>
	public class RuntimeInstallerReferenceItem
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
		//*	MajorVersion																													*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="MajorVersion">MajorVersion</see>.
		/// </summary>
		private int mMajorVersion = 0;
		/// <summary>
		/// Get/Set the major runtime version represented by this installer.
		/// </summary>
		public int MajorVersion
		{
			get { return mMajorVersion; }
			set { mMajorVersion = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RuntimeInstallerName																									*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for
		/// <see cref="RuntimeInstallerName">RuntimeInstallerName</see>.
		/// </summary>
		private string mRuntimeInstallerName = "";
		/// <summary>
		/// Get/Set the name of the runtime installer.
		/// </summary>
		public string RuntimeInstallerName
		{
			get { return mRuntimeInstallerName; }
			set { mRuntimeInstallerName = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	RuntimeVersion																												*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="RuntimeVersion">RuntimeVersion</see>.
		/// </summary>
		private string mRuntimeVersion = "";
		/// <summary>
		/// Get/Set the runtime version number.
		/// </summary>
		public string RuntimeVersion
		{
			get { return mRuntimeVersion; }
			set { mRuntimeVersion = value; }
		}
		//*-----------------------------------------------------------------------*

	}
	//*-------------------------------------------------------------------------*


}
