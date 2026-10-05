using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed class GunnerCameraPresenter : IPresentationTickable
    {
        private readonly Transform _cameraMount;
        private readonly Transform _gunnerEye;
        private readonly ITurretRig _turretRig;
        private readonly Camera _camera;
        private readonly GunnerZoomState _zoom;

        private readonly float _baseFov;
        private readonly float _zoomFov;

        public GunnerCameraPresenter(
            Transform cameraMount,
            Transform gunnerEye,
            ITurretRig turretRig,
            Camera camera,
            GunnerZoomState zoom)
        {
            _cameraMount = cameraMount ?? throw new ArgumentNullException(nameof(cameraMount));
            _gunnerEye = gunnerEye ?? throw new ArgumentNullException(nameof(gunnerEye));
            _turretRig = turretRig ?? throw new ArgumentNullException(nameof(turretRig));
            _camera = camera ?? throw new ArgumentNullException(nameof(camera));
            _zoom = zoom ?? throw new ArgumentNullException(nameof(zoom));

            _baseFov = _camera.fieldOfView;

            _zoomFov = 2f * Mathf.Atan(
                Mathf.Tan(_baseFov * Mathf.Deg2Rad * 0.5f) / 2f
            ) * Mathf.Rad2Deg;
        }

        public void Tick(float deltaTime)
        {
            _cameraMount.SetPositionAndRotation(
                _gunnerEye.position,
                _turretRig.MuzzlePose.rotation);

            _camera.fieldOfView = _zoom.IsHeld ? _zoomFov : _baseFov;
        }

        public void ResetZoom()
        {
            _zoom.Clear();
            _camera.fieldOfView = _baseFov;
        }
    }
}