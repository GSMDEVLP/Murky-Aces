using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class StationDisplayFeed : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public bool IsActive =>
            _camera != null &&
            _camera.enabled;

        private void Awake()
        {
            Deactivate();
        }

        public void Activate()
        {
            if (_camera == null)
            {
                Debug.LogError(
                    $"{nameof(StationDisplayFeed)}: Camera is not assigned.",
                    this);

                return;
            }

            if (_camera.targetTexture == null)
            {
                Debug.LogError(
                    $"{nameof(StationDisplayFeed)}: Camera Target Texture is not assigned.",
                    this);

                return;
            }

            _camera.enabled = true;
        }

        public void Deactivate()
        {
            if (_camera == null)
                return;

            _camera.enabled = false;
        }

        private void OnDisable()
        {
            Deactivate();
        }

        private void OnDestroy()
        {
            if (_camera == null)
                return;

            _camera.enabled = false;
            _camera.targetTexture = null;
        }
    }
}