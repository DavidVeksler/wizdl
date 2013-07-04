using System;
using System.Collections;
using System.ComponentModel.Design;

namespace WebServices.UI
{
    /// <summary>
    ///     Summary description for CustomArrayEditor.
    /// </summary>
    public class CustomArrayEditor : ArrayEditor
    {
        private readonly Type[] _newItemTypes;

        public CustomArrayEditor(Type arrayType)
            : base(arrayType)
        {
            ArrayList types = TypeHelper.GetInstantiableTypes(arrayType.GetElementType());
            _newItemTypes = types.ToArray(typeof (Type)) as Type[];
        }

        protected override Type[] CreateNewItemTypes()
        {
            return _newItemTypes;
        }
    }
}