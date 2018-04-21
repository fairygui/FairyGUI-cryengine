﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2014/08/17 19:40
// 
// License Copyright (c) Daniele Giardini.
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using CryEngine;
using System;

namespace DG.Tweening.Core
{
    internal static class Utils
    {
        /// <summary>
        /// Returns a Vector3 with z = 0
        /// </summary>
        internal static Vector3 Vector3FromAngle(float degrees, float magnitude)
		{
            float radians =  MathHelpers.DegreesToRadians(degrees);
            return new Vector3((float)(magnitude * Math.Cos(radians)),(float)(magnitude * Math.Sin(radians)), 0);
        }

        /// <summary>
        /// Returns the 2D angle between two vectors
        /// </summary>
        internal static float Angle2D(Vector3 from, Vector3 to)
        {
            Vector2 baseDir = new Vector2(1, 0);
            to -= from;
            float ang = Vector2.Angle(baseDir, (Vector2)to);
            Vector3 cross = Vector3.Cross((Vector3)baseDir, to);
            if (cross.z > 0) ang = 360 - ang;
            ang *= -1f;
            return ang;
        }

        /// <summary>
        /// Uses approximate equality on each axis instead of Unity's Vector3 equality,
        /// because the latter fails (in some cases) when assigning a Vector3 to a transform.position and then checking it.
        /// </summary>
        internal static bool Vector3AreApproximatelyEqual(Vector3 a, Vector3 b)
        {
            return MathHelpers.Approximately(a.x, b.x)
                   && MathHelpers.Approximately(a.y, b.y)
                   && MathHelpers.Approximately(a.z, b.z);
        }

        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████
        // ███ INTERNAL CLASSES ████████████████████████████████████████████████████████████████████████████████████████████████
        // █████████████████████████████████████████████████████████████████████████████████████████████████████████████████████

        // Uses code from BK > http://stackoverflow.com/a/1280832
        // (scrapped > doesn't work with IL2CPP)
//        public class InstanceCreator<T> where T : new()
//        {
//            static readonly Expression<Func<T>> _TExpression = () => new T();
//            static readonly Func<T> _TBuilder = _TExpression.Compile();
//            public static T Create() { return _TBuilder(); }
//        }
    }
}