using UnityEngine;
using Zenject;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsMapView : MonoBehaviour
    {
        [SerializeField] private Renderer _screen;

        [SerializeField, Range(128, 1024)] private int _resolution = 512;

        [SerializeField, Min(1f)] private float _visibleWorldHeight = 80f;

        [SerializeField, Min(0.1f)] private float _screenAspect = 4f / 3f;

        [SerializeField] private Color _routeColor = new Color(1f, 0.25f, 0.8f, 1f);

        [SerializeField, Range(1, 8)] private int _routeRadius = 2;

        [SerializeField, Min(1f)] private float _minimumVisibleHeight = 20f;

        [SerializeField, Min(1f)] private float _maximumVisibleHeight = 300f;

        [SerializeField, Range(1.01f, 2f)] private float _zoomStepFactor = 1.2f;
        
        [SerializeField] private Color _destinationColor = new Color(1f, 0.4f, 0.05f, 1f);

        [SerializeField, Range(2, 12)] private int _destinationRadius = 5;


        private GpsScreenRenderer _screenRenderer;
        private GpsMapTexture _mapTexture;
        private IGpsMapSource _mapSource;
        private GpsViewport _viewport;

        public GpsMapData DisplayedMap { get; private set; }
        public float ScreenAspect => Viewport.ScreenAspect;

        private GpsViewport Viewport => _viewport ??= new GpsViewport(
            _visibleWorldHeight,
            _screenAspect,
            _minimumVisibleHeight,
            _maximumVisibleHeight,
            _zoomStepFactor);

        public bool HasMap =>
            isActiveAndEnabled &&
            _mapTexture != null &&
            _mapTexture.IsCreated &&
            _screenRenderer != null &&
            _screenRenderer.IsBound &&
            DisplayedMap != null;

        private void OnEnable()
        {
            if (UnityEngine.Application.isPlaying &&
                _mapSource != null &&
                !HasMap)
            {
                RebuildMap();
            }
        }


        [Inject]
        public void Construct(IGpsMapSource mapSource)
        {
            _mapSource = mapSource ??
                throw new ArgumentNullException(nameof(mapSource));

            if (UnityEngine.Application.isPlaying && isActiveAndEnabled)
                RebuildMap();
        }


        [ContextMenu("Rebuild Map")]
        public void RebuildMap()
        {
            ReleaseMap();

            if (_mapSource == null || _screen == null)
            {
                Debug.LogWarning(
                    "GPS map source is not injected or screen is not assigned.",
                    this);

                return;
            }


            if (!GpsScreenRenderer.IsSupported(_screen))
            {
                Debug.LogWarning(
                    "GPS screen material does not support the required properties.",
                    this);

                return;
            }

            if (!_mapSource.TryGetMap(out GpsMapData map) || map == null)
            {
                Debug.LogWarning(
                    "GPS map source has no map data.",
                    this);

                return;
            }

            _mapTexture = new GpsMapTexture(map, _resolution);
            DisplayedMap = map;

            _screenRenderer ??= new GpsScreenRenderer();

            if (!_screenRenderer.TryBind(_screen, _mapTexture.Texture))
            {
                ReleaseMap();

                Debug.LogWarning(
                    "Failed to bind the GPS screen.",
                    this);
            }
        }

        public void SetCursor(Vector2 screenUv, bool visible)
        {
           _screenRenderer?.SetCursor(screenUv, visible);    
        }

        public void ApplyZoom(float zoomInput)
        {
            if (!HasMap)
                return;

            Viewport.ApplyZoom(zoomInput);
        }

        private void OnDisable()
        {
            ReleaseMap();
        }

        private void ReleaseMap()
        {
            _viewport?.ClearPose();

            _screenRenderer?.Unbind();

            _mapTexture?.Dispose();
            _mapTexture = null;

            DisplayedMap = null;
        }

        public void SetNavigationPose(Vector2 centerUv, Vector2 worldForward, bool active)
        {
            if (_screenRenderer == null || !_screenRenderer.IsBound)
                return;

            GpsMapData map = DisplayedMap;
            GpsViewport viewport = Viewport;

            viewport.Present(
                map != null ? map.Center : Vector2.zero,
                map != null ? map.Size : Vector2.one,
                centerUv,
                worldForward,
                active && map != null);

            _screenRenderer.SetNavigationPose(viewport);
        }

        public void SetRoute(IGpsRouteReader route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (!HasMap)
                return;

            _mapTexture.UpdateRoute(
                route,
                _routeColor,
                _routeRadius,
                _destinationColor,
                _destinationRadius);
        }
        
        public bool TryScreenToWorld(Vector2 screenUv, out Vector2 worldPosition)
        {
            worldPosition = default;

            return HasMap &&
                Viewport.TryScreenToWorld(
                    screenUv,
                    out worldPosition);
        }
    }
}
