using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public sealed class PickupInteractable : SingleActionInteractionTarget, IHoldableItem
    {
        [Header("Interaction")]
        [SerializeField] private InteractionPromts _prompt;

        [Header("World Physics")]
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Collider[] _worldColliders;

        private readonly CarrySession _carrySession = new(1);
        private HoldableItemBody _itemBody;
        public bool IsHeld => _carrySession.ParticipantCount > 0;
        public int CarrierCount => _carrySession.ParticipantCount;
        public long CarryRevision => _carrySession.Revision;

        public bool IsCarriedBy(ulong actorId)
        {
            return CarrierCount == 1 &&
                _carrySession.Contains(actorId);
        }

        private void Awake()
        {
            if (_body == null)
                _body = GetComponent<Rigidbody>();

            if (_worldColliders == null ||
                _worldColliders.Length == 0)
            {
                _worldColliders =
                    GetComponentsInChildren<Collider>(true);
            }

            _itemBody = new HoldableItemBody(
                _body,
                _worldColliders);
        }

        public override InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            bool available =
                isActiveAndEnabled &&
                _itemBody != null &&
                _itemBody.IsAvailable &&
                !IsHeld &&
                context.Actor is IPickupReceiver receiver &&
                !receiver.IsOccupied;

            return InteractionInfo.Press(_prompt, available, CarryRevision);
        }

        public override bool Begin(in InteractionContext context)
        {
            return GetInteractionInfo(context).IsAvailable;
        }

        public override void Complete(in InteractionContext context)
        {
            if (!GetInteractionInfo(context).IsAvailable)
                return;

            var receiver = (IPickupReceiver)context.Actor;
            receiver.TryReceive(this);
        }

        public override void Cancel(
            in InteractionContext context,
            InteractionCancelReason reason)
        {
        }

        public bool TryHold(ulong actorId, Transform anchor)
        {
            if (!isActiveAndEnabled ||
                IsHeld ||
                anchor == null ||
                _itemBody == null)
            {
                return false;
            }

            if (!_carrySession.TryAddParticipant(actorId))
                return false;

            if (_itemBody.TryHold(anchor))
                return true;

            _carrySession.TryRemoveParticipant(actorId);
            return false;
        }

        public bool TryDrop(ulong actorId, Vector3 position, Vector3 impulse)
        {
            if (!_carrySession.Contains(actorId) ||
                _itemBody == null)
            {
                return false;
            }

            if (!_itemBody.TryDrop(position, impulse))
                return false;

            return _carrySession.TryRemoveParticipant(actorId);
        }

        public bool TryEndCarry(ulong actorId)
        {
            return _carrySession.TryRemoveParticipant(actorId);
        }

        private void Reset()
        {
            _body = GetComponent<Rigidbody>();
            _worldColliders =
                GetComponentsInChildren<Collider>(true);
        }
    }
}
