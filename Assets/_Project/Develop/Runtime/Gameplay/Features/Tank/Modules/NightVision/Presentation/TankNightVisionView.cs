using UnityEngine;
using UnityEngine.Rendering;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TankNightVisionView : MonoBehaviour
    {
        [SerializeField] private Volume _volume;
        [SerializeField] private StationDisplayFeed _displayFeed;

        public bool HasValidReferences =>
            _volume != null &&
            _volume.sharedProfile != null &&
            _displayFeed != null;

        public bool IsAvailable =>
            isActiveAndEnabled &&
            HasValidReferences &&
            _volume.enabled &&
            _displayFeed.IsActive;

        private void Awake()
        {
            TurnOff();
        }

        private void OnDisable()
        {
            TurnOff();
        }

        public void ApplyState(bool enabled)
        {
            SetEffect(enabled && IsAvailable);
        }

        public void TurnOff()
        {
            SetEffect(false);
        }

        private void SetEffect(bool enabled)
        {
            if (_volume == null)
                return;

            // При включении профиль должен иметь полный вес.
            if (enabled)
                _volume.weight = 1f;

            GameObject volumeObject = _volume.gameObject;

            if (volumeObject.activeSelf != enabled)
                volumeObject.SetActive(enabled);
        }
    }
}