using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method)]
public class EnumEventAttribute : PropertyAttribute
{
    public Type EnumType { get; }
    public EnumEventAttribute(Type enumType) => EnumType = enumType;
}