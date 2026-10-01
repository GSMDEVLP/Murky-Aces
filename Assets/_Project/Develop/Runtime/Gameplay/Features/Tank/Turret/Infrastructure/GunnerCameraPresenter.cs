using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed class GunnerCameraPresenter : IPresentationTickable
    {
        private readonly Transform _cameraMount;
        private readonly Transform _gunnerEye;
        private readonly ITurretRig _turretRig;

        public GunnerCameraPresenter(
            Transform cameraMount,
            Transform gunnerEye,
            ITurretRig turretRig)
        {
            _cameraMount = cameraMount ??
                throw new ArgumentNullException(nameof(cameraMount));
            _gunnerEye = gunnerEye ??
                throw new ArgumentNullException(nameof(gunnerEye));
            _turretRig = turretRig ??
                throw new ArgumentNullException(nameof(turretRig));
        }

        public void Tick(float deltaTime)
        {
            _cameraMount.SetPositionAndRotation(
                _gunnerEye.position,
                _turretRig.MuzzlePose.rotation);
        }
    }
}