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
            _camera != null &&
            _camera.enabled &&
            _ownedTexture != null;

        private void Awake()
        {
            if (_camera != null)
            {
                _camera.enabled = false;
                _camera.targetTexture = null;
            }

            SetScreenTexture(Texture2D.blackTexture);
        }

        public void Activate()
        {
            if (_camera == null ||
                _panelRoot == null ||
                _panelRoot.HasValidPrimaryScreen == false)
            {
                Debug.LogError("StationDisplayFeed references are not assigned.", this);
                return;
            }

            if (_ownedTexture == null)
            {
                _ownedTexture = new RenderTexture(
                    _width, _height, 24, RenderTextureFormat.ARGB32)
                {
                    name = $"{name}_StationDisplay_RT",
                    hideFlags = HideFlags.DontSave
                };

                if (_ownedTexture.Create() == false)
                {
                    Destroy(_ownedTexture);
                    _ownedTexture = null;
                    Debug.LogError("Could not create station RenderTexture.", this);
                    return;
                }
            }

            _camera.targetTexture = _ownedTexture;
            SetScreenTexture(_ownedTexture);
            _camera.enabled = true;
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