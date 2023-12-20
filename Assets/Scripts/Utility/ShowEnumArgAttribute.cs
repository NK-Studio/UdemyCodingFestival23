using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class ShowEnumArgAttribute : PropertyAttribute
{
    public Type EnumType { get; }
    public ShowEnumArgAttribute(Type enumType) => EnumType = enumType;
}