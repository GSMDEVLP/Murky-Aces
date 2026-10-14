using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain
{
    public readonly struct GpsRoutePoint
    {
        public Vector2 WorldPosition { get; }

        public GpsRoutePoint(Vector2 worldPosition)
        {
            if (float.IsNaN(worldPosition.x) ||
                float.IsInfinity(worldPosition.x) ||
                float.IsNaN(worldPosition.y) ||
                float.IsInfinity(worldPosition.y))
            {
                throw new ArgumentException(
                    "GPS route coordinates must be finite.",
                    nameof(worldPosition));
            }

            WorldPosition = worldPosition;
        }
    }
}