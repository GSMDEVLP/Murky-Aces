using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station
{
    public sealed class UnityPlayerStationBody : MonoBehaviour, IPlayerStationBody
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private CapsuleCollider _collider;

        private PlayerStationBodySnapshot _snapshot;
        public bool IsAttached { get; private set; }

        public bool TryAttach(Transform seatAnchor)
        {
            if (IsAttached || seatAnchor == null || !HasRequiredReferences())
            {
                return false;
            }

            CacheState();
            DisableWorldBody();
            AttachTo(seatAnchor);

            IsAttached = true;

            return true;
        }

        public bool TryDetach(Transform exitAnchor)
        {
            if (!IsAttached || exitAnchor == null || !HasRequiredReferences())
            {
                return false;
            }

            DetachTo(exitAnchor);
            RestoreState();

            IsAttached = false;

            return true;
        }

        private bool HasRequiredReferences()
        {
            return _body != null && _collider != null;
        }

        private void CacheState()
        {
            Transform playerTransform = _body.transform;

            _snapshot = new PlayerStationBodySnapshot(
                playerTransform.parent,
                playerTransform.localScale,
                _body.isKinematic,
                _body.useGravity,
                _body.detectCollisions,
                _collider.enabled);
        }

        private void DisableWorldBody()
        {
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }

            _collider.enabled = false;

            _body.detectCollisions = false;
            _body.useGravity = false;
            _body.isKinematic = true;
        }

        private void AttachTo(Transform seatAnchor)
        {
            Transform playerTransform = _body.transform;

            playerTransform.SetParent(seatAnchor, false);
            playerTransform.localPosition = Vector3.zero;
            playerTransform.localRotation = Quaternion.identity;
            playerTransform.localScale = Vector3.one;
        }

        private void DetachTo(Transform exitAnchor)
        {
            Transform playerTransform = _body.transform;
            playerTransform.SetParent(_snapshot.Parent, true);
            playerTransform.localScale = _snapshot.LocalScale;
            playerTransform.SetPositionAndRotation(exitAnchor.position, exitAnchor.rotation);
            Physics.SyncTransforms();
        }

        private void RestoreState()
        {
            _body.useGravity =
                _snapshot.UseGravity;

            _body.detectCollisions =
                _snapshot.DetectCollisions;

            _collider.enabled =
                _snapshot.ColliderEnabled;

            _body.isKinematic =
                _snapshot.IsKinematic;

            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
                _body.WakeUp();
            }
        }

        private void Reset()
        {
            _body = GetComponentInParent<Rigidbody>();

            if (_body != null)
            {
                _collider =
                    _body.GetComponentInChildren<CapsuleCollider>(
                        true);
            }
        }
    }
}