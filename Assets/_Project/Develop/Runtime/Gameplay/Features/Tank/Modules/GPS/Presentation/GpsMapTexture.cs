using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsMapTexture : IDisposable
    {
        private readonly GpsMapTextureBuilder _builder;

        private Texture2D _texture;
        private IGpsRouteReader _displayedRoute;
        private long _displayedRouteRevision = -1;

        private Color32 _routeColor;
        private int _routeRadius;
        private Color32 _destinationColor;
        private int _destinationRadius;

        public Texture Texture => _texture;
        public bool IsCreated => _texture != null;

        public GpsMapTexture(GpsMapData map, int resolution)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            resolution = Mathf.Clamp(resolution, 128, 1024);

            _builder = new GpsMapTextureBuilder(
                map.Center,
                map.Size,
                map.RoadPoints,
                resolution);

            _texture = new Texture2D(
                resolution,
                resolution,
                TextureFormat.RGBA32,
                false)
            {
                name = "GPS Map",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };

            try
            {
                _texture.SetPixels32(_builder.BasePixels);
                _texture.Apply(false);
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void UpdateRoute(
            IGpsRouteReader route,
            Color32 routeColor,
            int routeRadius,
            Color32 destinationColor,
            int destinationRadius)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (!IsCreated)
                throw new ObjectDisposedException(nameof(GpsMapTexture));

            long revision = route.Revision;

            if (ReferenceEquals(_displayedRoute, route) &&
                _displayedRouteRevision == revision &&
                _routeColor.Equals(routeColor) &&
                _routeRadius == routeRadius &&
                _destinationColor.Equals(destinationColor) &&
                _destinationRadius == destinationRadius)
            {
                return;
            }

            Color32[] pixels = _builder.BuildRoute(
                route,
                routeColor,
                routeRadius,
                destinationColor,
                destinationRadius);

            _texture.SetPixels32(pixels);
            _texture.Apply(false);

            _displayedRoute = route;
            _displayedRouteRevision = revision;
            _routeColor = routeColor;
            _routeRadius = routeRadius;
            _destinationColor = destinationColor;
            _destinationRadius = destinationRadius;
        }

        public void Dispose()
        {
            Texture2D texture = _texture;

            _texture = null;
            _displayedRoute = null;
            _displayedRouteRevision = -1;

            if (texture == null)
                return;

            if (UnityEngine.Application.isPlaying)
                UnityEngine.Object.Destroy(texture);
            else
                UnityEngine.Object.DestroyImmediate(texture);
        }
    }
}