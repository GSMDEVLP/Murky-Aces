using System;
using System.Collections.Generic;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsMapTextureBuilder
    {
        private readonly Vector2 _mapCenter;
        private readonly Vector2 _mapSize;
        private readonly int _resolution;

        private readonly Color32[] _basePixels;
        private readonly Color32[] _routePixels;

        public Color32[] BasePixels => _basePixels;

        public GpsMapTextureBuilder(
            Vector2 mapCenter,
            Vector2 mapSize,
            IReadOnlyList<Vector2> roadPoints,
            int resolution)
        {
            if (roadPoints == null)
                throw new ArgumentNullException(nameof(roadPoints));

            if (mapSize.x <= 0f || mapSize.y <= 0f)
                throw new ArgumentOutOfRangeException(nameof(mapSize));

            if (resolution < 2)
                throw new ArgumentOutOfRangeException(nameof(resolution));

            _mapCenter = mapCenter;
            _mapSize = mapSize;
            _resolution = resolution;

            _basePixels = CreateBasePixels(roadPoints);
            _routePixels = new Color32[_basePixels.Length];
        }

        public Color32[] BuildRoute(
            IGpsRouteReader route,
            Color32 routeColor,
            int routeRadius,
            Color32 destinationColor,
            int destinationRadius)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            Array.Copy(
                _basePixels,
                _routePixels,
                _basePixels.Length);

            var points = route.Points;
            int radius = Mathf.Clamp(routeRadius, 1, 8);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i].WorldPosition;
                Vector2 previous = i == 0
                    ? current
                    : points[i - 1].WorldPosition;

                if (!TryClipRouteSegment(
                        previous,
                        current,
                        out Vector2Int from,
                        out Vector2Int to))
                {
                    continue;
                }

                DrawLine(
                    _routePixels,
                    _resolution,
                    from,
                    to,
                    routeColor,
                    radius);
            }

            if (route.Destination.HasValue)
            {
                Vector2 destination =
                    route.Destination.Value.WorldPosition;

                if (TryClipRouteSegment(
                        destination,
                        destination,
                        out Vector2Int pixel,
                        out _))
                {
                    DrawDot(
                        _routePixels,
                        _resolution,
                        pixel,
                        destinationColor,
                        Mathf.Clamp(destinationRadius, 2, 12));
                }
            }

            return _routePixels;
        }

        private Color32[] CreateBasePixels(
            IReadOnlyList<Vector2> roadPoints)
        {
            var pixels = new Color32[_resolution * _resolution];

            var background = new Color32(24, 31, 29, 255);
            var grid = new Color32(65, 75, 72, 255);
            var road = new Color32(220, 200, 110, 255);

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = background;

            for (int i = 0; i <= 8; i++)
            {
                int coordinate = Mathf.RoundToInt(
                    i / 8f * (_resolution - 1));

                DrawLine(
                    pixels,
                    _resolution,
                    new Vector2Int(coordinate, 0),
                    new Vector2Int(coordinate, _resolution - 1),
                    grid,
                    0);

                DrawLine(
                    pixels,
                    _resolution,
                    new Vector2Int(0, coordinate),
                    new Vector2Int(_resolution - 1, coordinate),
                    grid,
                    0);
            }

            for (int i = 1; i < roadPoints.Count; i++)
            {
                DrawLine(
                    pixels,
                    _resolution,
                    WorldToPixel(roadPoints[i - 1]),
                    WorldToPixel(roadPoints[i]),
                    road,
                    2);
            }

            if (roadPoints.Count > 0)
            {
                DrawDot(
                    pixels,
                    _resolution,
                    WorldToPixel(roadPoints[0]),
                    new Color32(70, 210, 120, 255),
                    6);

                DrawDot(
                    pixels,
                    _resolution,
                    WorldToPixel(roadPoints[roadPoints.Count - 1]),
                    new Color32(235, 80, 80, 255),
                    6);
            }

            return pixels;
        }

        private Vector2Int WorldToPixel(Vector2 point)
        {
            Vector2 minimum = _mapCenter - _mapSize * 0.5f;
            Vector2 offset = point - minimum;

            return new Vector2Int(
                Mathf.RoundToInt(
                    offset.x / _mapSize.x * (_resolution - 1)),
                Mathf.RoundToInt(
                    offset.y / _mapSize.y * (_resolution - 1)));
        }

        private bool TryClipRouteSegment(
            Vector2 fromWorld,
            Vector2 toWorld,
            out Vector2Int fromPixel,
            out Vector2Int toPixel)
        {
            fromPixel = default;
            toPixel = default;

            double minimumX =
                (double)_mapCenter.x - _mapSize.x * 0.5d;

            double minimumZ =
                (double)_mapCenter.y - _mapSize.y * 0.5d;

            double deltaX = (double)toWorld.x - fromWorld.x;
            double deltaZ = (double)toWorld.y - fromWorld.y;

            double enter = 0d;
            double exit = 1d;

            if (!ClipRouteAxis(
                    fromWorld.x,
                    deltaX,
                    minimumX,
                    minimumX + _mapSize.x,
                    ref enter,
                    ref exit) ||
                !ClipRouteAxis(
                    fromWorld.y,
                    deltaZ,
                    minimumZ,
                    minimumZ + _mapSize.y,
                    ref enter,
                    ref exit))
            {
                return false;
            }

            fromPixel = new Vector2Int(
                RouteCoordinateToPixel(
                    fromWorld.x + deltaX * enter,
                    minimumX, _mapSize.x, _resolution),
                RouteCoordinateToPixel(
                    fromWorld.y + deltaZ * enter,
                    minimumZ, _mapSize.y, _resolution));

            toPixel = new Vector2Int(
                RouteCoordinateToPixel(
                    fromWorld.x + deltaX * exit,
                    minimumX, _mapSize.x, _resolution),
                RouteCoordinateToPixel(
                    fromWorld.y + deltaZ * exit,
                    minimumZ, _mapSize.y, _resolution));

            return true;
        }

        private static bool ClipRouteAxis(
            double start,
            double delta,
            double minimum,
            double maximum,
            ref double enter,
            ref double exit)
        {
            if (delta == 0d)
                return start >= minimum && start <= maximum;

            double first = (minimum - start) / delta;
            double second = (maximum - start) / delta;

            enter = Math.Max(enter, Math.Min(first, second));
            exit = Math.Min(exit, Math.Max(first, second));

            return enter <= exit;
        }

        private static int RouteCoordinateToPixel(
            double coordinate,
            double minimum,
            double size,
            int resolution)
        {
            float uv = Mathf.Clamp01(
                (float)((coordinate - minimum) / size));

            return Mathf.RoundToInt(uv * (resolution - 1));
        }

        private static void DrawLine(
            Color32[] pixels,
            int resolution,
            Vector2Int from,
            Vector2Int to,
            Color32 color,
            int radius)
        {
            int steps = Mathf.Max(
                Mathf.Abs(to.x - from.x),
                Mathf.Abs(to.y - from.y));

            if (steps == 0)
            {
                DrawDot(pixels, resolution, from, color, radius);
                return;
            }

            for (int i = 0; i <= steps; i++)
            {
                float progress = (float)i / steps;

                var point = new Vector2Int(
                    Mathf.RoundToInt(
                        Mathf.Lerp(from.x, to.x, progress)),
                    Mathf.RoundToInt(
                        Mathf.Lerp(from.y, to.y, progress)));

                DrawDot(
                    pixels, resolution, point, color, radius);
            }
        }

        private static void DrawDot(
            Color32[] pixels,
            int resolution,
            Vector2Int center,
            Color32 color,
            int radius)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y > radius * radius)
                        continue;

                    int pixelX = center.x + x;
                    int pixelY = center.y + y;

                    if (pixelX < 0 || pixelX >= resolution ||
                        pixelY < 0 || pixelY >= resolution)
                    {
                        continue;
                    }

                    pixels[pixelY * resolution + pixelX] = color;
                }
            }
        }
    }
}