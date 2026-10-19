using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure
{
    public sealed class HoldableItemBody
    {
        private readonly Rigidbody _body;
        private readonly Collider[] _colliders;

        public bool IsAvailable => _body != null;

        public HoldableItemBody(
            Rigidbody body,
            Collider[] colliders)
        {
            _body = body;
            _colliders = colliders ?? Array.Empty<Collider>();
        }

        public bool TryHold(Transform anchor)
        {
            if (_body == null || anchor == null)
                return false;

            Transform root = _body.transform;

            if (anchor == root || anchor.IsChildOf(root))
                return false;

            SetWorldPhysics(false);

            root.SetParent(anchor, true);
            root.localPosition = Vector3.zero;
            root.localRotation = Quaternion.identity;

            return true;
        }

        public bool TryDrop(Vector3 position, Vector3 impulse)
        {
            if (_body == null)
                return false;

            Transform root = _body.transform;
            root.SetParent(null, true);
            root.position = position;

            SetWorldPhysics(true);

            _body.WakeUp();
            _body.AddForce(impulse, ForceMode.Impulse);

            return true;
        }

        private void SetWorldPhysics(bool enabled)
        {
            if (!_body.isKinematic)
            {
                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;
            }

            _body.isKinematic = !enabled;
            _body.useGravity = enabled;
            _body.detectCollisions = enabled;

            foreach (Collider collider in _colliders)
            {
                if (collider != null)
                    collider.enabled = enabled;
            }
        }
    }
}