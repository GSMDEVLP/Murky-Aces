using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Presentation
{
    public sealed class GpsScreenRenderer
    {
        private static readonly int BASE_MAP_ID = Shader.PropertyToID("_BaseMap");
        private static readonly int TANK_UV_ID = Shader.PropertyToID("_TankUv");
        private static readonly int TANK_FORWARD_ID = Shader.PropertyToID("_TankForward");
        private static readonly int MARKER_VISIBLE_ID = Shader.PropertyToID("_MarkerVisible");
        private static readonly int MAP_SIZE_ID = Shader.PropertyToID("_MapSize");
        private static readonly int VIEW_SIZE_ID = Shader.PropertyToID("_ViewSize");
        private static readonly int CURSOR_UV_ID = Shader.PropertyToID("_CursorUv");
        private static readonly int CURSOR_VISIBLE_ID = Shader.PropertyToID("_CursorVisible");

        private static readonly int[] REQUIRED_PROPERTIES =
        {
            BASE_MAP_ID,
            TANK_UV_ID,
            TANK_FORWARD_ID,
            MARKER_VISIBLE_ID,
            MAP_SIZE_ID,
            VIEW_SIZE_ID,
            CURSOR_UV_ID,
            CURSOR_VISIBLE_ID
        };

        private readonly MaterialPropertyBlock _originalProperties = new();
        private readonly MaterialPropertyBlock _properties = new();

        private Renderer _screen;

        public bool IsBound => _screen != null;

        public static bool IsSupported(Renderer screen)
        {
            if (screen == null)
                return false;

            Material material = screen.sharedMaterial;

            if (material == null)
                return false;

            foreach (int property in REQUIRED_PROPERTIES)
            {
                if (!material.HasProperty(property))
                    return false;
            }

            return true;
        }

        public bool TryBind(Renderer screen, Texture texture)
        {
            Unbind();

            if (!IsSupported(screen) || texture == null)
                return false;

            _screen = screen;
            _screen.GetPropertyBlock(_originalProperties, 0);
            _screen.GetPropertyBlock(_properties, 0);

            _properties.SetTexture(BASE_MAP_ID, texture);
            _properties.SetFloat(MARKER_VISIBLE_ID, 0f);
            _properties.SetFloat(CURSOR_VISIBLE_ID, 0f);

            _screen.SetPropertyBlock(_properties, 0);
            return true;
        }

        public void SetCursor(Vector2 screenUv, bool visible)
        {
            if (!IsBound)
                return;

            _screen.GetPropertyBlock(_properties, 0);

            _properties.SetVector(
                CURSOR_UV_ID,
                new Vector4(
                    Mathf.Clamp01(screenUv.x),
                    Mathf.Clamp01(screenUv.y),
                    0f,
                    0f));

            _properties.SetFloat(
                CURSOR_VISIBLE_ID,
                visible ? 1f : 0f);

            _screen.SetPropertyBlock(_properties, 0);
        }

        public void SetNavigationPose(GpsViewport viewport)
        {
            if (!IsBound)
                return;

            _screen.GetPropertyBlock(_properties, 0);

            _properties.SetVector(TANK_UV_ID, ToVector(viewport.CenterUv));
            _properties.SetVector(TANK_FORWARD_ID, ToVector(viewport.Forward));
            _properties.SetVector(MAP_SIZE_ID, ToVector(viewport.MapSize));
            _properties.SetVector(VIEW_SIZE_ID, ToVector(viewport.ViewSize));

            _properties.SetFloat(
                MARKER_VISIBLE_ID,
                viewport.HasPose ? 1f : 0f);

            _screen.SetPropertyBlock(_properties, 0);
        }

        public void Unbind()
        {
            if (_screen != null)
                _screen.SetPropertyBlock(_originalProperties, 0);

            _screen = null;
            _originalProperties.Clear();
            _properties.Clear();
        }

        private static Vector4 ToVector(Vector2 value)
        {
            return new Vector4(value.x, value.y, 0f, 0f);
        }
    }
}