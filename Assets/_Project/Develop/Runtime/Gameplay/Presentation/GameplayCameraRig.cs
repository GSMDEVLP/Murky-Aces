using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Presentation
{
    public sealed class GameplayCameraRig : MonoBehaviour, IPlayerStationCamera
    {
        private Transform _walkingAnchor;

        public Transform CurrentAnchor { get; private set; }

        public bool TryBindWalkingAnchor(Transform anchor)
        {
            if (anchor == null)
                return false;

            _walkingAnchor = anchor;

            return TryMoveTo(anchor);
        }

        public bool TryUseStationAnchor(Transform anchor)
        {
            if (!CanUseAnchor(anchor))
                return false;

            return TryMoveTo(anchor);
        }

        public bool TryRestoreWalkingAnchor()
        {
            return TryMoveTo(_walkingAnchor);
        }

        private bool TryMoveTo(Transform anchor)
        {
            if (anchor == null)
                return false;

            transform.SetParent(anchor, false);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            CurrentAnchor = anchor;

            return true;
        }
        public bool CanUseAnchor(Transform anchor)
        {
            return _walkingAnchor != null &&
                anchor != null;
        }
    }
}