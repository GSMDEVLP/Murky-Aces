using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorMountSlotAuthoring : MonoBehaviour
    {
        [SerializeField] private TankRoot _owner;
        [SerializeField] private ExteriorMountType _mountTypeId;
        [SerializeField] private Transform _placementAnchor;

        [Header("Installation")]
        [SerializeField] private Rigidbody _tankBody;

        [SerializeField, Min(0f)] private float _maxInstallLinearSpeed = 0.03f;

        [SerializeField, Min(0f)] private float _maxInstallAngularSpeed = 0.01f;

        private ExteriorMountSlotState _state;
        public TankRoot Owner => _owner;
        public Transform PlacementAnchor => _placementAnchor;
        public bool CanRemoveNow => CanInstallNow;
        public ExteriorMountSlotState State
        {
            get
            {
                EnsureInitialized();
                return _state;
            }
        }
        public bool CanInstallNow =>
            isActiveAndEnabled &&
            _owner != null &&
            _owner.isActiveAndEnabled &&
            transform.IsChildOf(_owner.transform) &&
            _placementAnchor != null &&
            _placementAnchor.gameObject.activeInHierarchy &&
            _placementAnchor.IsChildOf(transform) &&
            _tankBody != null &&
            _tankBody.transform == _owner.transform &&
            _maxInstallLinearSpeed >= 0f &&
            _maxInstallAngularSpeed >= 0f &&
            _tankBody.linearVelocity.sqrMagnitude <=
                _maxInstallLinearSpeed * _maxInstallLinearSpeed &&
            _tankBody.angularVelocity.sqrMagnitude <=
                _maxInstallAngularSpeed * _maxInstallAngularSpeed;

        private void Awake()
        {
            EnsureInitialized();
        }

        public bool CanAccept(ExteriorAttachmentAuthoring attachment)
        {
            return isActiveAndEnabled &&
                _owner != null &&
                _owner.isActiveAndEnabled &&
                _placementAnchor != null &&
                attachment != null &&
                attachment.isActiveAndEnabled &&
                State.CanAccept(attachment.State);
        }

        private void EnsureInitialized()
        {
            if (_state != null)
                return;

            if (_owner == null || _placementAnchor == null)
            {
                throw new InvalidOperationException(
                    $"{name}: assign owner and placement anchor.");
            }

            if (!transform.IsChildOf(_owner.transform))
            {
                throw new InvalidOperationException(
                    $"{name}: mount slot must belong to its tank.");
            }

            if (!_placementAnchor.IsChildOf(transform))
            {
                throw new InvalidOperationException(
                    $"{name}: placement anchor must belong to this slot.");
            }

            _state = new ExteriorMountSlotState(_mountTypeId);
        }
        public bool TryGetInstalledAttachment(out ExteriorAttachmentAuthoring attachment)
        {
            attachment = null;

            if (!CanRemoveNow || State.Attachment == null)
                return false;

            for (int i = 0; i < _placementAnchor.childCount; i++)
            {
                Transform child = _placementAnchor.GetChild(i);

                if (child.TryGetComponent(
                        out ExteriorAttachmentAuthoring candidate) &&
                    candidate.isActiveAndEnabled &&
                    ReferenceEquals(candidate.State, State.Attachment))
                {
                    attachment = candidate;
                    return true;
                }
            }

            return false;
        }
    }
}
