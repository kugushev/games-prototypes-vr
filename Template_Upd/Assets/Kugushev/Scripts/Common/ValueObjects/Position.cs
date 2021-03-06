﻿using UnityEngine;

namespace Kugushev.Scripts.Common.ValueObjects
{
    public readonly struct Position
    {
        public Vector3 Point { get; }

        public Position(Vector3 point) => Point = point;
    }
}