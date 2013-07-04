using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace WebServices.UI
{
    /// <summary>
    ///     Summary description for CustomPropertyEditor.
    /// </summary>
    public class CustomPropertyEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        private class CloseDropDown
        {
            private readonly IWindowsFormsEditorService _svc;

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

            foreach (Type type in TypeHelper.GetInstantiableTypes(baseType))
            {
                types[type.Name] = type;
                box.Items.Add(type.Name);
            }

            string current = value != null ? value.GetType().Name : null;

            if (current != null)
            {
                box.SelectedItem = current;
            }

            IWindowsFormsEditorService svc =
                (IWindowsFormsEditorService) provider.GetService(typeof (IWindowsFormsEditorService));

            if (svc != null)
            {
                box.SelectedIndexChanged += new CloseDropDown(svc).Close;
                svc.DropDownControl(box);
            }

            if (box.SelectedIndex != -1 && !box.SelectedItem.Equals(current))
            {
                value = Activator.CreateInstance(types[box.SelectedItem] as Type);
            }

            return value;
        }
    }
}