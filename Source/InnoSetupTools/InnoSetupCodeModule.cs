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
	//*	InnoSetupCodeModuleCollection																						*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Collection of InnoSetupCodeModuleItem Items.
	/// </summary>
	public class InnoSetupCodeModuleCollection : List<InnoSetupCodeModuleItem>
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


	}
	//*-------------------------------------------------------------------------*

	//*-------------------------------------------------------------------------*
	//*	InnoSetupCodeModuleItem																									*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Information about an individual code module reference.
	/// </summary>
	public class InnoSetupCodeModuleItem
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
		//*	BuildLevel																														*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="BuildLevel">BuildLevel</see>.
		/// </summary>
		private ProjectBuildLevelEnum mBuildLevel =
			ProjectBuildLevelEnum.NETInstallIncluded;
		/// <summary>
		/// Get/Set the project build level at which this entry will be included.
		/// </summary>
		public ProjectBuildLevelEnum BuildLevel
		{
			get { return mBuildLevel; }
			set { mBuildLevel = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Module																																*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Module">Module</see>.
		/// </summary>
		private string mModule = "";
		/// <summary>
		/// Get/Set the code text to be included when this module is activated.
		/// </summary>
		public string Module
		{
			get { return mModule; }
			set { mModule = value; }
		}
		//*-----------------------------------------------------------------------*

		//*-----------------------------------------------------------------------*
		//*	Name																																	*
		//*-----------------------------------------------------------------------*
		/// <summary>
		/// Private member for <see cref="Name">Name</see>.
		/// </summary>
		private string mName = "";
		/// <summary>
		/// Get/Set the name of the function or procedure.
		/// </summary>
		public string Name
		{
			get { return mName; }
			set { mName = value; }
		}
		//*-----------------------------------------------------------------------*


	}
	//*-------------------------------------------------------------------------*

}
