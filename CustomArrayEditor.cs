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
using System.ComponentModel;
using System.ComponentModel.Design;

namespace WebServices.UI
{
	/// <summary>
	/// Summary description for CustomArrayEditor.
	/// </summary>
	public class CustomArrayEditor : ArrayEditor
	{
		Type[] _newItemTypes;

		public CustomArrayEditor(Type arrayType)
			: base(arrayType)
		{
			ArrayList types = TypeHelper.GetInstantiableTypes(arrayType.GetElementType());
			_newItemTypes = types.ToArray(typeof(Type)) as Type[];
		}

		protected override Type[] CreateNewItemTypes()
		{
			return _newItemTypes;
		}
	}
}
