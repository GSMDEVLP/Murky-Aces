using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public sealed class GunnerStationAdapter : ICrewStationRoleAdapter
    {
        private readonly TurretAimRuntime _turret;
        private readonly GunnerZoomState _zoom;
        private readonly TankGun _gun;
        private readonly ITurretRig _rig;
        private readonly ITankMotionBody _motionBody;

        private IGunnerIntentSource _intentSource;
        private ulong _actorId;

        public bool IsActive => _intentSource != null;

        public GunnerStationAdapter(
            TurretAimRuntime turret,
            GunnerZoomState zoom,
            TankGun gun,
            ITurretRig rig,
            ITankMotionBody motionBody)
        {
            _turret = turret
                ?? throw new ArgumentNullException(nameof(turret));

            _zoom = zoom
                ?? throw new ArgumentNullException(nameof(zoom));

            _gun = gun
                ?? throw new ArgumentNullException(nameof(gun));

            _rig = rig
                ?? throw new ArgumentNullException(nameof(rig));

            _motionBody = motionBody
                ?? throw new ArgumentNullException(nameof(motionBody));
        }

        public bool CanUse(IInteractionActor actor)
        {
            return actor is IGunnerIntentSource;
        }

        public bool TryActivate(IInteractionActor actor)
        {
            if (IsActive)
                return false;

            _intentSource = actor as IGunnerIntentSource;

            if (_intentSource == null)
                return false;

            _actorId = actor.Id;
            ClearOutput();
            return true;
        }

        public void Tick(float deltaTime)
        {
            if (_intentSource == null)
            {
                ClearOutput();
                return;
            }

            GunnerIntentSnapshot intent =
                _intentSource.ConsumeGunnerIntent();

            _zoom.SetHeld(intent.ZoomHeld);

            _turret.SetInput(
                intent.Traverse,
                intent.Elevation);

            if (intent.ToggleReferenceModeRequested)
                _turret.ToggleReferenceMode();

            if (intent.FireRequested)
            {
                Pose muzzlePose = _rig.MuzzlePose;

                Vector3 inheritedVelocity =
                    _motionBody.LinearVelocity +
                    Vector3.Cross(
                        _motionBody.AngularVelocity,
                        muzzlePose.position - _motionBody.Position);

                _gun.TryFire(
                    _actorId,
                    muzzlePose,
                    inheritedVelocity);
            }
        }

        public void ClearOutput()
        {
            _turret.ClearInput();
            _zoom.Clear();
        }

        public void Deactivate()
        {
            ClearOutput();
            _intentSource = null;
            _actorId = 0;
        }
    }
}