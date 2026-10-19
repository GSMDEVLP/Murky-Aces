using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorMountSlotAuthoring : MonoBehaviour, IInteractionTarget
    {
        [SerializeField] private TankRoot _owner;
        [SerializeField] private string _mountTypeId;
        [SerializeField] private Transform _placementAnchor;

        private ExteriorMountSlotState _state;

        public TankRoot Owner => _owner;

        public Transform PlacementAnchor => _placementAnchor;

        public ExteriorMountSlotState State
        {
            get
            {
                EnsureInitialized();
                return _state;
            }
        }

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
    }
}