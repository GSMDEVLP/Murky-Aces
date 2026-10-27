using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application
{
    public sealed class TankFireRequestHandler : IGameplayTickable
    {
        private readonly TankGun _gun;
        private readonly ITurretRig _rig;
        private readonly ITankMotionBody _motionBody;

        private ulong? _pendingActorId;
        private bool _isEnabled;

        public TankFireRequestHandler(
            TankGun gun,
            ITurretRig rig,
            ITankMotionBody motionBody)
        {
            _gun = gun ??
                throw new ArgumentNullException(nameof(gun));

            _rig = rig ??
                throw new ArgumentNullException(nameof(rig));

            _motionBody = motionBody ??
                throw new ArgumentNullException(nameof(motionBody));
        }

        public void SetEnabled(bool isEnabled)
        {
            _isEnabled = isEnabled;

            if (isEnabled == false)
                _pendingActorId = null;
        }

        public bool TryRequestFire(ulong actorId)
        {
            if (_isEnabled == false || _pendingActorId.HasValue)
                return false;

            _pendingActorId = actorId;
            return true;
        }

        public void CancelRequest(ulong actorId)
        {
            if (_pendingActorId == actorId)
                _pendingActorId = null;
        }

        public void Tick(float deltaTime)
        {
            if (_isEnabled == false || _pendingActorId.HasValue == false)
                return;

            ulong actorId = _pendingActorId.Value;
            _pendingActorId = null;

            Pose muzzlePose = _rig.MuzzlePose;

            Vector3 inheritedVelocity =
                _motionBody.LinearVelocity +
                Vector3.Cross(
                    _motionBody.AngularVelocity,
                    muzzlePose.position - _motionBody.Position);

            _gun.TryFire(
                actorId,
                muzzlePose,
                inheritedVelocity);
        }
    }
}
