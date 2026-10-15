using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain
{
    public sealed class GpsMapData
    {
        public Vector2 Center { get; }
        public Vector2 Size { get; }
        public IReadOnlyList<Vector2> RoadPoints { get; }

        public GpsMapData(
            Vector2 center,
            Vector2 size,
            IEnumerable<Vector2> roadPoints)
        {
            if (roadPoints == null)
                throw new ArgumentNullException(nameof(roadPoints));

            if (!IsFinite(center.x) || !IsFinite(center.y))
                throw new ArgumentOutOfRangeException(nameof(center));

            if (!IsFinite(size.x) ||
                !IsFinite(size.y) ||
                size.x <= 0f ||
                size.y <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }

            Center = center;
            Size = size;

            RoadPoints = new List<Vector2>(roadPoints).AsReadOnly();
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}