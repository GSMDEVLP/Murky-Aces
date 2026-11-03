using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Infrastructure
{
    public sealed class ApcTurretModule :
        IEnemyRuntimeModule,
        IGameplayTickable
    {
        private const float MIN_DIRECTION_SQUARED = 0.000001f;

        private readonly ApcTurretSearch _search;
        private readonly ApcTurretAim _aim;
        private readonly ApcTargetSelector _targets;
        private readonly Transform _turret;
        private readonly Transform _gun;
        private readonly Quaternion _initialYawRotation;
        private readonly Quaternion _initialPitchRotation;
        private readonly Vector3 _localYawAxis;
        private readonly Vector3 _localPitchAxis;
        private readonly Vector3 _neutralGunForward;
        private readonly float _searchSpeed;
        private readonly float _combatYawSpeed;
        private readonly float _pitchSpeed;

        private bool _isActive;

        public ApcTurretModule(
            ApcTurretSearch search,
            ApcTurretAim aim,
            ApcTargetSelector targets,
            ApcTurretSearchConfig searchConfig,
            ApcTurretAimConfig aimConfig,
            Transform turret,
            Transform gun)
        {
            _search = search ??
                throw new ArgumentNullException(nameof(search));
            _aim = aim ??
                throw new ArgumentNullException(nameof(aim));
            _targets = targets ??
                throw new ArgumentNullException(nameof(targets));

            if (turret == null || gun == null || turret.parent == null ||
                gun.parent != turret || searchConfig == null ||
                aimConfig == null)
            {
                throw new ArgumentException("Incomplete APC turret rig.");
            }

            aimConfig.Validate();
            float axisLength = searchConfig.LocalYawAxis.sqrMagnitude;
            if (float.IsNaN(axisLength) || float.IsInfinity(axisLength) ||
                axisLength <= MIN_DIRECTION_SQUARED)
            {
                throw new ArgumentException("Invalid APC yaw axis.");
            }

            _turret = turret;
            _gun = gun;
            _initialYawRotation = turret.localRotation;
            _initialPitchRotation = gun.localRotation;
            _localYawAxis = searchConfig.LocalYawAxis.normalized;
            _localPitchAxis = aimConfig.LocalPitchAxis.normalized;
            _neutralGunForward =
                _initialPitchRotation *
                aimConfig.LocalForwardAxis.normalized;
            _searchSpeed = searchConfig.DegreesPerSecond;
            _combatYawSpeed = aimConfig.YawDegreesPerSecond;
            _pitchSpeed = aimConfig.PitchDegreesPerSecond;

            if (Vector3.ProjectOnPlane(
                    _neutralGunForward, _localYawAxis).sqrMagnitude
                <= MIN_DIRECTION_SQUARED)
            {
                throw new ArgumentException(
                    "APC forward direction must differ from its yaw axis.");
            }
        }

        public void Activate(GameLoopRegistry registry)
        {
            if (_isActive)
                return;

            registry.RegisterGameplay(this);
            _isActive = true;
        }

        public void Deactivate(GameLoopRegistry registry)
        {
            if (_isActive == false)
                return;

            _isActive = false;
            registry.UnregisterGameplay(this);
        }

        public void Tick(float deltaTime)
        {
            if (_isActive == false || _turret == null || _gun == null)
                return;

            Vector3? position = _targets.AimPosition;
            float desiredYaw;
            float desiredPitch;
            float yawSpeed;

            if (_targets.CurrentTarget != null && position.HasValue)
            {
                Vector2 desired = CalculateAimAngles(position.Value);
                desiredYaw = desired.x;
                desiredPitch = desired.y;
                yawSpeed = _combatYawSpeed;
            }
            else
            {
                _search.Tick(deltaTime);
                desiredYaw = _search.Angle;
                desiredPitch = 0f;
                yawSpeed = _searchSpeed;
            }

            _aim.Tick(
                desiredYaw, desiredPitch,
                yawSpeed, _pitchSpeed, deltaTime);

            _turret.localRotation =
                _initialYawRotation *
                Quaternion.AngleAxis(_aim.Yaw, _localYawAxis);

            _gun.localRotation =
                _initialPitchRotation *
                Quaternion.AngleAxis(_aim.Pitch, _localPitchAxis);
        }

        private Vector2 CalculateAimAngles(Vector3 position)
        {
            Quaternion reference =
                _turret.parent.rotation * _initialYawRotation;

            Vector3 up = reference * _localYawAxis;
            Vector3 forward = reference * _neutralGunForward;
            Vector3 horizontalForward =
                Vector3.ProjectOnPlane(forward, up);
            Vector3 horizontalDirection =
                Vector3.ProjectOnPlane(position - _turret.position, up);

            float yaw = horizontalDirection.sqrMagnitude >
                        MIN_DIRECTION_SQUARED
                ? Vector3.SignedAngle(
                    horizontalForward, horizontalDirection, up)
                : _aim.Yaw;

            Vector3 gunOffset = position - _gun.position;

            if (gunOffset.sqrMagnitude <= MIN_DIRECTION_SQUARED)
                return new Vector2(yaw, _aim.Pitch);

            float elevation = Mathf.Atan2(
                Vector3.Dot(gunOffset, up),
                Vector3.ProjectOnPlane(gunOffset, up).magnitude)
                * Mathf.Rad2Deg;

            float neutralElevation = Mathf.Asin(
                Mathf.Clamp(
                    Vector3.Dot(forward.normalized, up), -1f, 1f))
                * Mathf.Rad2Deg;

            return new Vector2(yaw, elevation - neutralElevation);
        }
    }
}
