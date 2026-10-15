using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain
{
    public sealed class GpsPolylineRoutePlanner
    {
        public bool TryBuild(IReadOnlyList<Vector2> roadPoints, GpsRoutePoint start, GpsRoutePoint destination, out IReadOnlyList<GpsRoutePoint> route)
        {
            route = Array.Empty<GpsRoutePoint>();

            if (roadPoints == null || roadPoints.Count < 2)
                return false;

            for (int i = 0; i < roadPoints.Count; i++)
            {
                if (!IsFinite(roadPoints[i]))
                    return false;
            }

            if (!TryProject(
                    roadPoints,
                    start.WorldPosition,
                    out int startSegment,
                    out Vector2 projectedStart))
            {
                return false;
            }

            if (!TryProject(
                    roadPoints,
                    destination.WorldPosition,
                    out int destinationSegment,
                    out Vector2 projectedDestination))
            {
                return false;
            }

            var points = new List<GpsRoutePoint>();

            AddPoint(points, projectedStart);

            if (startSegment < destinationSegment)
            {
                for (int i = startSegment + 1;
                     i <= destinationSegment;
                     i++)
                {
                    AddPoint(points, roadPoints[i]);
                }
            }
            else if (startSegment > destinationSegment)
            {
                for (int i = startSegment;
                     i > destinationSegment;
                     i--)
                {
                    AddPoint(points, roadPoints[i]);
                }
            }

            AddPoint(points, projectedDestination);

            route = points.AsReadOnly();
            return true;
        }

        private static bool TryProject(IReadOnlyList<Vector2> roadPoints, Vector2 point, out int segmentIndex, out Vector2 projectedPoint)
        {
            segmentIndex = -1;
            projectedPoint = default;

            float nearestSquaredDistance = float.PositiveInfinity;

            for (int i = 0; i < roadPoints.Count - 1; i++)
            {
                Vector2 from = roadPoints[i];
                Vector2 to = roadPoints[i + 1];
                Vector2 direction = to - from;

                float squaredLength = direction.sqrMagnitude;

                if (squaredLength <= 0f ||
                    float.IsInfinity(squaredLength))
                {
                    continue;
                }

                float progress = Mathf.Clamp01(
                    Vector2.Dot(point - from, direction) /
                    squaredLength);

                Vector2 candidate = Vector2.Lerp(
                    from,
                    to,
                    progress);

                if (!IsFinite(candidate))
                    continue;

                float squaredDistance =
                    (point - candidate).sqrMagnitude;

                if (squaredDistance >= nearestSquaredDistance)
                    continue;

                nearestSquaredDistance = squaredDistance;
                segmentIndex = i;
                projectedPoint = candidate;
            }

            return segmentIndex >= 0;
        }

        private static void AddPoint(List<GpsRoutePoint> points, Vector2 position)
        {
            if (points.Count > 0 &&
                points[points.Count - 1]
                    .WorldPosition.Equals(position))
            {
                return;
            }

            points.Add(new GpsRoutePoint(position));
        }

        private static bool IsFinite(Vector2 point)
        {
            return !float.IsNaN(point.x) &&
                   !float.IsInfinity(point.x) &&
                   !float.IsNaN(point.y) &&
                   !float.IsInfinity(point.y);
        }
    }
}