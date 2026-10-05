using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Interior
{
    public sealed class PlayerTankInteriorBody : MonoBehaviour, IPlayerInteriorBody
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private PlayerLook _look;

        public bool CanTeleport =>
            _body != null &&
            _look != null &&
            !_body.isKinematic &&
            _look.TryCaptureWalkingView(out _);

        public bool TryPrepareTeleport(
            Transform destination,
            out Func<bool> tryApply,
            out Func<bool> tryRollback)
        {
            tryApply = null;
            tryRollback = null;

            if (!CanTeleport || destination == null)
                return false;

            if (!_look.TryCaptureWalkingView(
                    out Quaternion viewRotation))
            {
                return false;
            }

            Vector3 position = _body.position;
            Quaternion rotation = _body.rotation;
            Vector3 linearVelocity = _body.linearVelocity;
            Vector3 angularVelocity = _body.angularVelocity;

            tryApply = () =>
            {
                if (!CanTeleport || destination == null)
                    return false;

                _body.linearVelocity = Vector3.zero;
                _body.angularVelocity = Vector3.zero;

                _body.position = destination.position;
                _body.rotation = destination.rotation;

                Physics.SyncTransforms();
                _look.EnterWalkingMode();

                return true;
            };

            tryRollback = () =>
            {
                if (_body == null ||
                    _look == null ||
                    _body.isKinematic)
                {
                    return false;
                }

                _body.position = position;
                _body.rotation = rotation;

                _body.linearVelocity = linearVelocity;
                _body.angularVelocity = angularVelocity;

                Physics.SyncTransforms();

                return _look.TryRestoreWalkingView(viewRotation);
            };

            return true;
        }
    }
}