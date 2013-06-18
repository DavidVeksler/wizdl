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
using System.Drawing.Design;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WebServices.UI
{
	/// <summary>
	/// Summary description for CustomPropertyEditor.
	/// </summary>
	public class CustomPropertyEditor : UITypeEditor
	{
		public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
		{
			return UITypeEditorEditStyle.DropDown;
		}

		private class CloseDropDown
		{
			IWindowsFormsEditorService _svc;
			internal CloseDropDown(IWindowsFormsEditorService svc)
			{
				_svc = svc;
			}

			internal void Close(Object sender, EventArgs e)
			{
				_svc.CloseDropDown();
			}
		}

		public override Object EditValue(ITypeDescriptorContext context, IServiceProvider provider, Object value)
		{
			Hashtable types = new Hashtable();
			ListBox box = new ListBox();

			Type baseType = context.PropertyDescriptor.PropertyType;

			foreach(Type type in TypeHelper.GetInstantiableTypes(baseType)) 
			{
				types[type.Name] = type;
				box.Items.Add(type.Name);
			}

			string current = value != null ? value.GetType().Name : null;

			if(current != null) 
			{
				box.SelectedItem = current;
			}

			IWindowsFormsEditorService svc = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));

			if(svc != null) 
			{
				box.SelectedIndexChanged += new EventHandler(new CloseDropDown(svc).Close);
				svc.DropDownControl(box);
			}

			if(box.SelectedIndex != -1 && !box.SelectedItem.Equals(current)) 
			{
				value = Activator.CreateInstance(types[box.SelectedItem] as Type);
			}

			return value;
		}
	}

}
