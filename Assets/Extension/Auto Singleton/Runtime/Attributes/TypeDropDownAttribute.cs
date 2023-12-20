using System;
using UnityEngine;

namespace AutoSingleton
{
    public class TypeDropDownAttribute : PropertyAttribute
    {
        public readonly Type BaseType;

        public TypeDropDownAttribute(Type baseType)
        {
            BaseType = baseType;
        }
    }
}
