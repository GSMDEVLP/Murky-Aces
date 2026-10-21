using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    public sealed class ExteriorAttachmentMountBody
    {
        private readonly Transform _root;
        private readonly Rigidbody _body;
        private readonly PickupInteractable _pickup;

        public ExteriorAttachmentMountBody(
            Transform root,
            Rigidbody body,
            PickupInteractable pickup)
        {
            _root = root;
            _body = body;
            _pickup = pickup;
        }

        public bool CanMount(Transform anchor)
        {
            return _root != null &&
                _root.gameObject.activeInHierarchy &&
                _root.parent != null &&
                _body != null &&
                _body.transform == _root &&
                _body.isKinematic &&
                !_body.useGravity &&
                !_body.detectCollisions &&
                _pickup != null &&
                _pickup.transform == _root &&
                _pickup.isActiveAndEnabled &&
                _pickup.CarrierCount == 1 &&
                anchor != null &&
                anchor.gameObject.activeInHierarchy &&
                anchor != _root &&
                !anchor.IsChildOf(_root);
        }
        public bool CanRemove(Transform anchor)
        {
            return _root != null &&
                _root.gameObject.activeInHierarchy &&
                anchor != null &&
                anchor.gameObject.activeInHierarchy &&
                _root.parent == anchor &&
                _body != null &&
                _body.transform == _root &&
                _body.isKinematic &&
                !_body.useGravity &&
                !_body.detectCollisions &&
                _pickup != null &&
                _pickup.transform == _root &&
                !_pickup.enabled &&
                _pickup.CarrierCount == 0;
        }

        public bool TryPrepareMount(Transform anchor, out Action apply,out Func<bool> restore)
        {
            apply = null;
            restore = null;

            if (!CanMount(anchor))
                return false;

            Transform previousParent = _root.parent;
            Vector3 previousPosition = _root.localPosition;
            Quaternion previousRotation = _root.localRotation;
            Vector3 previousScale = _root.localScale;
            bool previousPickupEnabled = _pickup.enabled;

            apply = () =>
            {
                if (!CanMount(anchor))
                {
                    throw new InvalidOperationException(
                        "Prepared mount is no longer available.");
                }

                _root.SetParent(anchor, false);
                _root.localPosition = Vector3.zero;
                _root.localRotation = Quaternion.identity;
                _root.localScale = Vector3.one;

                _pickup.enabled = false;
            };

            restore = () =>
            {
                if (_root == null ||
                    _pickup == null ||
                    previousParent == null)
                {
                    return false;
                }

                _root.SetParent(previousParent, false);
                _root.localPosition = previousPosition;
                _root.localRotation = previousRotation;
                _root.localScale = previousScale;

                _pickup.enabled = previousPickupEnabled;

                return true;
            };

            return true;
        }
        public bool TryPrepareRemoval(Transform anchor, out Action apply, out Func<bool> restore)
        {
            apply = null;
            restore = null;

            if (!CanRemove(anchor))
                return false;

            Vector3 previousPosition = _root.localPosition;
            Quaternion previousRotation = _root.localRotation;
            Vector3 previousScale = _root.localScale;
            bool previousPickupEnabled = _pickup.enabled;

            apply = () =>
            {
                if (!CanRemove(anchor))
                {
                    throw new InvalidOperationException(
                        "Prepared removal is no longer available.");
                }

                _pickup.enabled = true;
            };

            restore = () =>
            {
                if (_root == null ||
                    _pickup == null ||
                    anchor == null)
                {
                    return false;
                }

                _root.SetParent(anchor, false);
                _root.localPosition = previousPosition;
                _root.localRotation = previousRotation;
                _root.localScale = previousScale;

                _pickup.enabled = previousPickupEnabled;

                return true;
            };

            return true;
        }
    }
}