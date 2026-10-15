using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsViewport
    {
        private readonly float _minimumHeight;
        private readonly float _maximumHeight;
        private readonly float _zoomStepFactor;

        private float _visibleWorldHeight;
        private Vector2 _mapMinimum;

        public float ScreenAspect { get; }

        public Vector2 MapSize { get; private set; }
        public Vector2 CenterUv { get; private set; }
        public Vector2 Forward { get; private set; } = Vector2.up;
        public Vector2 ViewSize { get; private set; }

        public bool HasPose { get; private set; }

        public GpsViewport(
            float initialVisibleHeight,
            float screenAspect,
            float minimumVisibleHeight,
            float maximumVisibleHeight,
            float zoomStepFactor)
        {
            _minimumHeight = Mathf.Max(1f, minimumVisibleHeight);
            _maximumHeight = Mathf.Max(
                _minimumHeight,
                maximumVisibleHeight);

            _zoomStepFactor = Mathf.Max(1.01f, zoomStepFactor);
            _visibleWorldHeight = Mathf.Max(1f, initialVisibleHeight);

            ScreenAspect = Mathf.Max(0.1f, screenAspect);
        }

        public void ApplyZoom(float zoomInput)
        {
            if (!IsFinite(zoomInput) || zoomInput == 0f)
                return;

            float input = Mathf.Clamp(zoomInput, -1f, 1f);

            _visibleWorldHeight = Mathf.Clamp(
                _visibleWorldHeight *
                Mathf.Pow(_zoomStepFactor, -input),
                _minimumHeight,
                _maximumHeight);
        }

        public void Present(
            Vector2 mapCenter,
            Vector2 mapSize,
            Vector2 centerUv,
            Vector2 worldForward,
            bool active)
        {
            MapSize = mapSize;
            _mapMinimum = mapCenter - mapSize * 0.5f;

            CenterUv = centerUv;

            Forward = worldForward.sqrMagnitude > 0.0001f
                ? worldForward.normalized
                : Vector2.up;

            ViewSize = new Vector2(
                _visibleWorldHeight * ScreenAspect,
                _visibleWorldHeight);

            HasPose = active &&
                      mapSize.x > 0f &&
                      mapSize.y > 0f;
        }

        public void ClearPose()
        {
            HasPose = false;
        }

        public bool TryScreenToWorld(
            Vector2 screenUv,
            out Vector2 worldPosition)
        {
            worldPosition = default;

            if (!HasPose ||
                !IsFinite(screenUv.x) ||
                !IsFinite(screenUv.y) ||
                screenUv.x < 0f || screenUv.x > 1f ||
                screenUv.y < 0f || screenUv.y > 1f)
            {
                return false;
            }

            Vector2 mapCenter = _mapMinimum +
                Vector2.Scale(CenterUv, MapSize);

            Vector2 screenOffset = Vector2.Scale(
                screenUv - new Vector2(0.5f, 0.5f),
                ViewSize);

            var right = new Vector2(Forward.y, -Forward.x);

            Vector2 position = mapCenter +
                right * screenOffset.x +
                Forward * screenOffset.y;

            Vector2 maximum = _mapMinimum + MapSize;

            if (position.x < _mapMinimum.x ||
                position.x > maximum.x ||
                position.y < _mapMinimum.y ||
                position.y > maximum.y)
            {
                return false;
            }

            worldPosition = position;
            return true;
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) &&
                   !float.IsInfinity(value);
        }
    }
}