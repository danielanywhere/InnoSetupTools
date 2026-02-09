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
	//*	ProjectBuildLevelEnum																										*
	//*-------------------------------------------------------------------------*
	/// <summary>
	/// Enumeration of recognized project build levels.
	/// </summary>
	public enum ProjectBuildLevelEnum
	{
		/// <summary>
		///	Build level not specified or unknown.
		/// </summary>
		None = 0,
		/// <summary>
		/// Compile the project for minimum size. .NET is required on the target
		/// system.
		/// </summary>
		Minimum,
		/// <summary>
		/// All .NET components have been included with the project so no .NET
		/// platform is required on the target system.
		/// </summary>
		StandAlone,
		/// <summary>
		/// Minimum project size with .NET installer included in Setup file.
		/// </summary>
		NETInstallIncluded,
		/// <summary>
		/// Minimum project size with .NET installer downloaded if needed.
		/// </summary>
		NETInstallDownload
	}
	//*-------------------------------------------------------------------------*

}
