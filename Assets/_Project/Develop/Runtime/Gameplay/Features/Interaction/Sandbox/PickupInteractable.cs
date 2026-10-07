using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Sandbox
{
    public sealed class PickupInteractable : MonoBehaviour, IInteractable, IHoldableItem
    {
        [Header("Interaction")] 
        [SerializeField] private InteractionPromts _prompt;

        [Header("World Physics")]
        [SerializeField] private Rigidbody _body;
        [SerializeField] private Collider[] _worldColliders;

        private bool _isHeld;

        public bool IsHeld => _isHeld;

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
        }

        public InteractionInfo GetInteractionInfo(in InteractionContext context)
        {
            IPickupReceiver pickupReceiver = context.Actor as IPickupReceiver;

            bool isAvailable = isActiveAndEnabled && !_isHeld && pickupReceiver != null && !pickupReceiver.IsOccupied;

            return InteractionInfo.Press(_prompt, isAvailable);
        }

        public bool Begin(in InteractionContext context)
        {
            InteractionInfo interactionInfo = GetInteractionInfo(context);

            return interactionInfo.IsAvailable;
        }

        public void Complete(in InteractionContext context)
        {
            if (!GetInteractionInfo(context).IsAvailable)
                return;

            IPickupReceiver receiver = context.Actor as IPickupReceiver;
            receiver?.TryReceive(this);
        }

        public void Cancel(in InteractionContext context,InteractionCancelReason reason){}

        public void Drop(Vector3 position, Vector3 impulse)
        {
            if (!_isHeld)
                return;

            transform.SetParent(null, true);
            transform.position = position;

            _isHeld = false;

            EnableWorldPhysics();

            if (_body != null)
                _body.AddForce(impulse, ForceMode.Impulse);
        }
        public bool TryHold(Transform anchor)
        {
            if (!isActiveAndEnabled || _isHeld || anchor == null)
                return false;

            EnterHeldState();

            // Сохраняем мировой масштаб предмета.
            transform.SetParent(anchor, true);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            return true;
        }

        public bool TryDrop(Vector3 position, Vector3 impulse)
        {
            if (!_isHeld)
                return false;

            Drop(position, impulse);
            return true;
        }

        private void EnterHeldState()
        {
            _isHeld = true;

            DisableWorldPhysics();
        }
        private void EnableWorldPhysics()
        {
            if (_body != null)
            {
                _body.useGravity = true;
                _body.isKinematic = false;
                _body.detectCollisions = true;
                _body.WakeUp();
            }

            if (_worldColliders == null)
                return;

            foreach (Collider worldCollider in _worldColliders)
            {
                if (worldCollider != null)
                    worldCollider.enabled = true;
            }
        }
        
        private void DisableWorldPhysics()
        {
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }
            if (_body != null)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.useGravity = false;
                _body.isKinematic = true;
                _body.detectCollisions = false;
            }

            if (_worldColliders == null)
                return;

            foreach (Collider worldCollider in _worldColliders)
            {
                if (worldCollider != null)
                    worldCollider.enabled = false;
            }
        }

        private void Reset()
        {
            _body = GetComponent<Rigidbody>();
            _worldColliders = GetComponentsInChildren<Collider>(true);
        }
    }
}
