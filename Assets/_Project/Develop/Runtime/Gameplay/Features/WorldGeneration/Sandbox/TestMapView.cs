using UnityEngine;
using System;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.WorldGeneration.Sandbox
{
    public sealed class TestMapView : MonoBehaviour
    {
        [SerializeField] private LocationGenerationPreview _source;
        [SerializeField] private Renderer _screen;

        [SerializeField, Range(128, 1024)]
        private int _resolution = 512;

        [SerializeField, Min(1f)]
        private float _visibleWorldHeight = 80f;

        [SerializeField, Min(0.1f)]
        private float _screenAspect = 4f / 3f;
        [SerializeField] private Color _routeColor = new Color(1f, 0.25f, 0.8f, 1f);

        [SerializeField, Range(1, 8)] private int _routeRadius = 2;

        [SerializeField, Min(1f)]
        private float _minimumVisibleHeight = 20f;

        [SerializeField, Min(1f)]
        private float _maximumVisibleHeight = 300f;

        [SerializeField, Range(1.01f, 2f)]
        private float _zoomStepFactor = 1.2f;
        
        [SerializeField] private Color _destinationColor = new Color(1f, 0.4f, 0.05f, 1f);

        [SerializeField, Range(2, 12)] private int _destinationRadius = 5;

        private Color32[] _baseMapPixels;
        private Color32[] _routePixels;

        private IGpsRouteReader _displayedRoute;
        private long _displayedRouteRevision = -1;


        private static readonly int BASE_MAP_ID =
            Shader.PropertyToID("_BaseMap");
        private static readonly int TANK_UV_ID =
            Shader.PropertyToID("_TankUv");
        private static readonly int TANK_FORWARD_ID =
            Shader.PropertyToID("_TankForward");
        private static readonly int MARKER_VISIBLE_ID =
            Shader.PropertyToID("_MarkerVisible");
        private static readonly int MAP_SIZE_ID =
            Shader.PropertyToID("_MapSize");
        private static readonly int VIEW_SIZE_ID =
            Shader.PropertyToID("_ViewSize");

        private static readonly int CURSOR_UV_ID =
            Shader.PropertyToID("_CursorUv");

        private static readonly int CURSOR_VISIBLE_ID =
            Shader.PropertyToID("_CursorVisible");

        private MaterialPropertyBlock _cursorProperties;

        public float ScreenAspect => Mathf.Max(0.1f, _screenAspect);

        private Texture2D _texture;
        private Renderer _boundScreen;
        private MaterialPropertyBlock _originalProperties;
        private MaterialPropertyBlock _markerProperties;
        public LocationLayout DisplayedLayout { get; private set; }

        private Vector2 _presentedCenterUv;
        private Vector2 _presentedForward = Vector2.up;
        private Vector2 _presentedViewSize;
        private bool _hasNavigationPose;

        public bool HasMap =>
            isActiveAndEnabled &&
            _texture != null &&
            _boundScreen != null &&
            DisplayedLayout != null;

        private void OnEnable()
        {
            if (Application.isPlaying)
                RebuildMap();
        }

        [ContextMenu("Rebuild Map")]
        public void RebuildMap()
        {
            ReleaseTexture();

            if (_source == null || _screen == null)
            {
                Debug.LogWarning("Map references are not assigned.", this);
                return;
            }

            Material material = _screen.sharedMaterial;

            if (material == null || !material.HasProperty(BASE_MAP_ID))
            {
                Debug.LogWarning(
                    "Map material must support _BaseMap.",
                    this);

                return;
            }

            if (_source.Layout == null)
                _source.Generate();

            LocationLayout layout = _source.Layout;

            if (layout == null)
                return;

            int resolution = Mathf.Clamp(_resolution, 128, 1024);
            _texture = CreateTexture(layout, resolution);
            DisplayedLayout = layout;

            _boundScreen = _screen;
            _originalProperties = new MaterialPropertyBlock();

            _boundScreen.GetPropertyBlock(_originalProperties, 0);

            var properties = new MaterialPropertyBlock();
            _boundScreen.GetPropertyBlock(properties, 0);

            properties.SetTexture(BASE_MAP_ID, _texture);
            properties.SetFloat(MARKER_VISIBLE_ID, 0f);
            properties.SetFloat(CURSOR_VISIBLE_ID, 0f);

            _boundScreen.SetPropertyBlock(properties, 0);
        }

        public void SetCursor(Vector2 screenUv, bool visible)
        {
            if (_boundScreen == null)
                return;

            _cursorProperties ??= new MaterialPropertyBlock();
            _boundScreen.GetPropertyBlock(_cursorProperties, 0);

            _cursorProperties.SetVector(
                CURSOR_UV_ID,
                new Vector4(
                    Mathf.Clamp01(screenUv.x),
                    Mathf.Clamp01(screenUv.y),
                    0f,
                    0f));

            _cursorProperties.SetFloat(
                CURSOR_VISIBLE_ID,
                visible ? 1f : 0f);

            _boundScreen.SetPropertyBlock(_cursorProperties, 0);
        }

        private Texture2D CreateTexture(LocationLayout layout, int resolution)
        {
            var pixels = new Color32[resolution * resolution];

            var background = new Color32(24, 31, 29, 255);
            var grid = new Color32(65, 75, 72, 255);
            var road = new Color32(220, 200, 110, 255);

            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = background;

            // Делим участок на восемь частей по каждой оси.
            for (int i = 0; i <= 8; i++)
            {
                int coordinate = Mathf.RoundToInt(
                    i / 8f * (resolution - 1));

                DrawLine(
                    pixels, resolution,
                    new Vector2Int(coordinate, 0),
                    new Vector2Int(coordinate, resolution - 1),
                    grid, 0);

                DrawLine(
                    pixels, resolution,
                    new Vector2Int(0, coordinate),
                    new Vector2Int(resolution - 1, coordinate),
                    grid, 0);
            }

            var points = layout.RoadPoints;

            for (int i = 1; i < points.Count; i++)
            {
                DrawLine(
                    pixels, resolution,
                    WorldToPixel(points[i - 1], layout, resolution),
                    WorldToPixel(points[i], layout, resolution),
                    road, 2);
            }

            if (points.Count > 0)
            {
                DrawDot(
                    pixels, resolution,
                    WorldToPixel(points[0], layout, resolution),
                    new Color32(70, 210, 120, 255), 6);

                DrawDot(
                    pixels, resolution,
                    WorldToPixel(
                        points[points.Count - 1],
                        layout, resolution),
                    new Color32(235, 80, 80, 255), 6);
            }

            var texture = new Texture2D(
                resolution, resolution,
                TextureFormat.RGBA32, false)
            {
                name = "Test Location Map",
                hideFlags = HideFlags.DontSave,
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            
            _baseMapPixels = pixels;
            _routePixels = new Color32[pixels.Length];

            texture.SetPixels32(pixels);
            texture.Apply(false);

            return texture;
        }

        public void ApplyZoom(float zoomInput)
        {
            if (!HasMap ||
                float.IsNaN(zoomInput) ||
                float.IsInfinity(zoomInput) ||
                zoomInput == 0f)
            {
                return;
            }

            float minimum = Mathf.Max(1f, _minimumVisibleHeight);
            float maximum = Mathf.Max(minimum, _maximumVisibleHeight);
            float stepFactor = Mathf.Max(1.01f, _zoomStepFactor);

            float input = Mathf.Clamp(zoomInput, -1f, 1f);

            _visibleWorldHeight = Mathf.Clamp(
                _visibleWorldHeight * Mathf.Pow(stepFactor, -input),
                minimum,
                maximum);
        }

        private static Vector2Int WorldToPixel(
            Vector2 point,
            LocationLayout layout,
            int resolution)
        {
            Vector2 minimum = layout.Center - layout.Size * 0.5f;
            Vector2 offset = point - minimum;

            return new Vector2Int(
                Mathf.RoundToInt(
                    offset.x / layout.Size.x * (resolution - 1)),
                Mathf.RoundToInt(
                    offset.y / layout.Size.y * (resolution - 1)));
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

                DrawDot(pixels, resolution, point, color, radius);
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

        private void OnDisable()
        {
            ReleaseTexture();
        }

        private void ReleaseTexture()
        {
            _hasNavigationPose = false;
            _baseMapPixels = null;
            _routePixels = null;

            _displayedRoute = null;
            _displayedRouteRevision = -1;

            DisplayedLayout = null;
            if (_boundScreen != null)
                _boundScreen.SetPropertyBlock(_originalProperties, 0);

            _boundScreen = null;
            _originalProperties = null;

            if (_texture == null)
                return;

            if (Application.isPlaying)
                Destroy(_texture);
            else
                DestroyImmediate(_texture);

            _texture = null;
        }
        public void SetNavigationPose(Vector2 centerUv, Vector2 worldForward, bool active)
        {
            if (_boundScreen == null)
                return;

            LocationLayout layout = DisplayedLayout;
            Vector2 mapSize = layout != null
                ? layout.Size
                : Vector2.one;

            float height = Mathf.Max(1f, _visibleWorldHeight);
            float aspect = Mathf.Max(0.1f, _screenAspect);
            _presentedCenterUv = centerUv;

            _presentedForward = worldForward.sqrMagnitude > 0.0001f
                ? worldForward.normalized
                : Vector2.up;

            _presentedViewSize = new Vector2(height * aspect, height);
            _hasNavigationPose = active && layout != null;

            _markerProperties ??= new MaterialPropertyBlock();
            _boundScreen.GetPropertyBlock(_markerProperties, 0);

            _markerProperties.SetVector(
                TANK_UV_ID,
                new Vector4(centerUv.x, centerUv.y, 0f, 0f));

            _markerProperties.SetVector(
                TANK_FORWARD_ID,
                new Vector4(
                    _presentedForward.x,
                    _presentedForward.y,
                    0f,
                    0f));
                
            _markerProperties.SetVector(
                MAP_SIZE_ID,
                new Vector4(mapSize.x, mapSize.y, 0f, 0f));

            _markerProperties.SetVector(
                VIEW_SIZE_ID,
                new Vector4(
                    _presentedViewSize.x,
                    _presentedViewSize.y,
                    0f,
                    0f));

            _markerProperties.SetFloat(
                MARKER_VISIBLE_ID,
                active && layout != null ? 1f : 0f);

            _boundScreen.SetPropertyBlock(_markerProperties, 0);
        }

        public void SetRoute(IGpsRouteReader route)
        {
            if (route == null)
                throw new ArgumentNullException(nameof(route));

            if (_texture == null ||
                _boundScreen == null ||
                DisplayedLayout == null ||
                _baseMapPixels == null)
            {
                return;
            }

            if (ReferenceEquals(_displayedRoute, route) &&
                _displayedRouteRevision == route.Revision)
            {
                return;
            }

            Array.Copy(
                _baseMapPixels,
                _routePixels,
                _baseMapPixels.Length);

            var points = route.Points;
            Color32 color = _routeColor;
            int resolution = _texture.width;
            int radius = Mathf.Clamp(_routeRadius, 1, 8);

            for (int i = 0; i < points.Count; i++)
            {
                Vector2 current = points[i].WorldPosition;
                Vector2 previous = i == 0
                    ? current
                    : points[i - 1].WorldPosition;

                if (!TryClipRouteSegment(
                        previous,
                        current,
                        DisplayedLayout,
                        resolution,
                        out Vector2Int from,
                        out Vector2Int to))
                {
                    continue;
                }

                DrawLine(
                    _routePixels,
                    resolution,
                    from,
                    to,
                    color,
                    radius);
            }

            if (route.Destination.HasValue)
            {
                Vector2 destination =
                    route.Destination.Value.WorldPosition;

                if (TryClipRouteSegment(
                        destination,
                        destination,
                        DisplayedLayout,
                        resolution,
                        out Vector2Int pixel,
                        out _))
                {
                    DrawDot(
                        _routePixels,
                        resolution,
                        pixel,
                        _destinationColor,
                        Mathf.Clamp(_destinationRadius, 2, 12));
                }
            }
            _texture.SetPixels32(_routePixels);
            _texture.Apply(false);

            _displayedRoute = route;
            _displayedRouteRevision = route.Revision;
        }

        private static bool TryClipRouteSegment(
            Vector2 fromWorld,
            Vector2 toWorld,
            LocationLayout layout,
            int resolution,
            out Vector2Int fromPixel,
            out Vector2Int toPixel)
        {
            fromPixel = default;
            toPixel = default;

            if (layout.Size.x <= 0f || layout.Size.y <= 0f)
                return false;

            double minimumX =
                (double)layout.Center.x - layout.Size.x * 0.5d;

            double minimumZ =
                (double)layout.Center.y - layout.Size.y * 0.5d;

            double deltaX = (double)toWorld.x - fromWorld.x;
            double deltaZ = (double)toWorld.y - fromWorld.y;

            double enter = 0d;
            double exit = 1d;

            if (!ClipRouteAxis(
                    fromWorld.x,
                    deltaX,
                    minimumX,
                    minimumX + layout.Size.x,
                    ref enter,
                    ref exit) ||
                !ClipRouteAxis(
                    fromWorld.y,
                    deltaZ,
                    minimumZ,
                    minimumZ + layout.Size.y,
                    ref enter,
                    ref exit))
            {
                return false;
            }

            fromPixel = new Vector2Int(
                RouteCoordinateToPixel(
                    fromWorld.x + deltaX * enter,
                    minimumX,
                    layout.Size.x,
                    resolution),
                RouteCoordinateToPixel(
                    fromWorld.y + deltaZ * enter,
                    minimumZ,
                    layout.Size.y,
                    resolution));

            toPixel = new Vector2Int(
                RouteCoordinateToPixel(
                    fromWorld.x + deltaX * exit,
                    minimumX,
                    layout.Size.x,
                    resolution),
                RouteCoordinateToPixel(
                    fromWorld.y + deltaZ * exit,
                    minimumZ,
                    layout.Size.y,
                    resolution));

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
        public bool TryScreenToWorld(Vector2 screenUv, out Vector2 worldPosition)
        {
            worldPosition = default;

            if (!HasMap ||
                !_hasNavigationPose ||
                float.IsNaN(screenUv.x) ||
                float.IsNaN(screenUv.y) ||
                screenUv.x < 0f || screenUv.x > 1f ||
                screenUv.y < 0f || screenUv.y > 1f)
            {
                return false;
            }

            LocationLayout layout = DisplayedLayout;
            Vector2 minimum = layout.Center - layout.Size * 0.5f;

            Vector2 mapCenter = minimum +
                Vector2.Scale(_presentedCenterUv, layout.Size);

            Vector2 screenOffset = Vector2.Scale(
                screenUv - new Vector2(0.5f, 0.5f),
                _presentedViewSize);

            Vector2 right = new Vector2(
                _presentedForward.y,
                -_presentedForward.x);

            Vector2 position = mapCenter +
                right * screenOffset.x +
                _presentedForward * screenOffset.y;

            Vector2 maximum = minimum + layout.Size;

            if (position.x < minimum.x || position.x > maximum.x ||
                position.y < minimum.y || position.y > maximum.y)
            {
                return false;
            }

            worldPosition = position;
            return true;
        }
    }
}
