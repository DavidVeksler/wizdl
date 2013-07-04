using System;
using System.Collections;

namespace WebServices.UI
{
    /// <summary>
    ///     Summary description for TypeHelper.
    /// </summary>
    public class TypeHelper
    {
        public static ArrayList GetInstantiableTypes(Type baseType)
        {
            ArrayList types = new ArrayList();

            if (!baseType.IsAbstract)
            {
                types.Add(baseType);
            }

            foreach (Type t in baseType.Assembly.GetTypes())
            {
                if (t.IsSubclassOf(baseType) && !t.IsAbstract)
                {
                    types.Add(t);
                }
            }

            return types;
        }
    }
}