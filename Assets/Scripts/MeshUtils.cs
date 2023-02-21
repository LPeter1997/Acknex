﻿using UnityEngine;

namespace Acknex
{
    public class MeshUtils
    {
        public static Vector3 GetNormal(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
        {
            var side1 = b - a;
            var side2 = c - a;
            return Vector3.Cross(side1, side2).normalized;
        }
    }
}