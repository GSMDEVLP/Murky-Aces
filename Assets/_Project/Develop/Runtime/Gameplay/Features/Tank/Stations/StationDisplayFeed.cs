using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class StationDisplayFeed : MonoBehaviour
    {
        private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");

        [SerializeField] private Camera _camera;
        [SerializeField] private StationPanelRoot _panelRoot;
        [SerializeField, Min(1)] private int _width = 1024;
        [SerializeField, Min(1)] private int _height = 512;

        private RenderTexture _ownedTexture;

        public bool IsActive =>
            isActiveAndEnabled &&
            _camera != null &&
            _camera.isActiveAndEnabled &&
            _ownedTexture != null &&
            _ownedTexture.IsCreated() &&
            _camera.targetTexture == _ownedTexture;


        private void Awake()
        {
            if (_camera != null)
            {
                _camera.enabled = false;
                _camera.targetTexture = null;
            }

            SetScreenTexture(Texture2D.blackTexture);
        }

        public bool TryActivate()
        {
            // Отключённый компонент может означать отказ модуля.
            if (!isActiveAndEnabled)
                return false;

            if (_camera == null ||
                _panelRoot == null ||
                !_panelRoot.TryGetPrimaryScreen(
                    out Renderer renderer,
                    out int materialIndex))
            {
                Debug.LogError(
                    "StationDisplayFeed references are not assigned.",
                    this);
                return false;
            }

            Material screenMaterial =
                renderer.sharedMaterials[materialIndex];

            if (screenMaterial == null ||
                !screenMaterial.HasProperty(BaseMap))
            {
                Debug.LogError(
                    "Station screen material has no _BaseMap property.",
                    this);
                return false;
            }

            if (_ownedTexture == null)
            {
                _ownedTexture = new RenderTexture(
                    _width, _height, 24, RenderTextureFormat.ARGB32)
                {
                    name = $"{name}_StationDisplay_RT",
                    hideFlags = HideFlags.DontSave
                };

                if (!_ownedTexture.Create())
                {
                    Destroy(_ownedTexture);
                    _ownedTexture = null;

                    Debug.LogError(
                        "Could not create station RenderTexture.",
                        this);
                    return false;
                }
            }

            _camera.targetTexture = _ownedTexture;
            SetScreenTexture(_ownedTexture);
            _camera.enabled = true;

            return IsActive;
        }

        public void Deactivate()
        {
            if (_camera != null)
            {
                _camera.enabled = false;

                if (_camera.targetTexture == _ownedTexture)
                    _camera.targetTexture = null;
            }

            SetScreenTexture(Texture2D.blackTexture);

            if (_ownedTexture == null)
                return;

            _ownedTexture.Release();
            Destroy(_ownedTexture);
            _ownedTexture = null;
        }

        private void OnDisable()
        {
            Deactivate();
        }

        private void OnDestroy()
        {
            Deactivate();
        }

        private void SetScreenTexture(Texture texture)
        {
            if (_panelRoot == null ||
                _panelRoot.TryGetPrimaryScreen(
                    out Renderer renderer,
                    out int materialIndex) == false)
            {
                return;
            }

            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties, materialIndex);
            properties.SetTexture(BaseMap, texture);
            renderer.SetPropertyBlock(properties, materialIndex);
        }
    }
}