using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public sealed class HeldItemSlot : MonoBehaviour, IPickupReceiver
    {
        [SerializeField] private Transform _anchor;

        [Header("Drop")]
        [SerializeField, Min(0f)] private float _dropDistance = 0.5f;
        [SerializeField, Min(0f)] private float _dropForce = 2f;

        private IHoldableItem _heldItem;

        public IHoldableItem HeldItem
        {
            get
            {
                if (_heldItem is UnityEngine.Object unityObject &&
                    unityObject == null)
                {
                    return null;
                }

                return _heldItem;
            }
        }

        public bool IsOccupied => HeldItem != null;

        public bool TryReceive(IInteractable item)
        {
            if (IsOccupied || _anchor == null)
                return false;

            if (item is not IHoldableItem holdableItem)
                return false;

            if (!holdableItem.TryHold(_anchor))
                return false;

            _heldItem = holdableItem;
            return true;
        }

        public bool TryDrop()
        {
            IHoldableItem item = HeldItem;

            if (item == null || _anchor == null)
                return false;

            Vector3 direction = _anchor.forward;
            Vector3 position =
                _anchor.position + direction * _dropDistance;

            if (!item.TryDrop(position, direction * _dropForce))
                return false;

            _heldItem = null;
            return true;
        }
        public bool TryReleaseHeldItem(IHoldableItem expectedItem)
        {
            IHoldableItem currentItem = HeldItem;

            if (currentItem == null ||
                !currentItem.IsHeld ||
                !ReferenceEquals(currentItem, expectedItem))
            {
                return false;
            }

            _heldItem = null;
            return true;
        }
    }
}