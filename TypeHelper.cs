/*
	wizdl - Web Service GUI
    Copyright (C) 2008  Ajai Shankar

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;

namespace WebServices.UI
{
	/// <summary>
	/// Summary description for TypeHelper.
	/// </summary>
	public class TypeHelper
	{
		public static ArrayList GetInstantiableTypes(Type baseType)
		{
			ArrayList types = new ArrayList();

			if(!baseType.IsAbstract) 
			{
				types.Add(baseType);
			}

			foreach(Type t in baseType.Assembly.GetTypes()) 
			{
				if(t.IsSubclassOf(baseType) && !t.IsAbstract) 
				{
					types.Add(t);
				}
			}

			return types;
		}
	}
}
