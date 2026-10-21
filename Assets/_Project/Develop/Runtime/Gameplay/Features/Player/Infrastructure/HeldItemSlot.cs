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
        private ulong _actorId;
        private bool _isInitialized;


        public void Initialize(ulong actorId)
        {
            if (_isInitialized && _actorId != actorId)
            {
                throw new System.InvalidOperationException(
                    "HeldItemSlot already belongs to another actor.");
            }

            _actorId = actorId;
            _isInitialized = true;
        }

        public bool TryReceive(IInteractable item)
        {
            if (!_isInitialized || IsOccupied || _anchor == null)
                return false;

            if (item is not IHoldableItem holdableItem)
                return false;

            if (!holdableItem.TryHold(_actorId, _anchor))
                return false;

            _heldItem = holdableItem;
            return true;
        }

        public bool TryDrop()
        {
            IHoldableItem item = HeldItem;

            if (!_isInitialized || item == null || _anchor == null)
                return false;

            Vector3 direction = _anchor.forward;
            Vector3 position =
                _anchor.position + direction * _dropDistance;

            if (!item.TryDrop(_actorId, position, direction * _dropForce))
                return false;

            _heldItem = null;
            return true;
        }
        public bool TryReleaseHeldItem(IHoldableItem expectedItem)
        {
            IHoldableItem currentItem = HeldItem;

            if (!_isInitialized ||
                currentItem == null ||
                !currentItem.IsHeld ||
                !ReferenceEquals(currentItem, expectedItem))
            {
                return false;
            }

            if (!currentItem.TryEndCarry(_actorId))
                return false;

            _heldItem = null;
            return true;
        }
        public bool TryPrepareDespawn()
        {
            IHoldableItem item = HeldItem;

            if (item == null)
            {
                _heldItem = null;
                return true;
            }

            if (_isInitialized == false || _anchor == null)
                return false;

            if (item.TryDrop(
                    _actorId,
                    _anchor.position,
                    Vector3.zero) == false)
            {
                return false;
            }

            _heldItem = null;
            return true;
        }
    }
}