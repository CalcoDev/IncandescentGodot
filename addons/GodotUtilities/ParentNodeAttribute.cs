/*
The contents of this file were taken from Firebelley's GodotUtilities project, which can be found HERE:

Firebelley: https://github.com/firebelley
GodotUtilities Repo: https://github.com/firebelley/GodotUtilities
This File: https://raw.githubusercontent.com/firebelley/GodotUtilities/godot-4/GodotUtilities/src/ParentNodeAttribute.cs
*/

using System;

namespace GodotUtilities
{
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false)]
    public class ParentAttribute : Attribute
    {
        public ParentAttribute() { }
    }
}