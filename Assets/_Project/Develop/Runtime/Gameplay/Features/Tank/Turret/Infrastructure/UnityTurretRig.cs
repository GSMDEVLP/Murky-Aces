using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    public sealed class UnityTurretRig : MonoBehaviour, ITurretRig
    {
        [SerializeField] private TurretAimConfig _config;
        [SerializeField] private Transform _hull;
        [SerializeField] private Transform _turretYaw;
        [SerializeField] private Transform _gunPitch;
        [SerializeField] private Transform _muzzle;
        [SerializeField] private Transform _breechLoadPoint;

        private Quaternion _initialYawRotation;
        private Quaternion _initialPitchRotation;
        private float _lastHullYaw;
        private bool _initialized;

        public float HullYaw
        {
            get
            {
                EnsureInitialized();

                Vector3 forward = _hull.TransformDirection(_config.HullForwardAxis);

                forward = Vector3.ProjectOnPlane(forward, Vector3.up);

                if (forward.sqrMagnitude < 0.000001f)
                    return _lastHullYaw;

                forward.Normalize();
 
                _lastHullYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;

                return _lastHullYaw;
            }
        }

        public Pose MuzzlePose
        {
            get
            {
                EnsureInitialized();

                Vector3 direction =
                    _muzzle.TransformDirection(
                        _config.MuzzleForwardAxis);

                return new Pose(
                    _muzzle.position,
                    Quaternion.LookRotation(
                        direction,
                        _muzzle.up));
            }
        }

        public Transform BreechLoadPoint
        {
            get
            {
                EnsureInitialized();
                return _breechLoadPoint;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        public void ApplyAngles(float yaw, float pitch)
        {
            EnsureInitialized();
            _turretYaw.localRotation = _initialYawRotation * Quaternion.AngleAxis(yaw, _config.YawAxis);
            _gunPitch.localRotation = _initialPitchRotation * Quaternion.AngleAxis(pitch, _config.PitchAxis);
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;

            if (_config == null || !_config.IsValid)
                throw new InvalidOperationException(
                    "TurretAimConfig is missing or invalid.");

            if (_hull == null ||
                _turretYaw == null ||
                _gunPitch == null ||
                _muzzle == null ||
                _breechLoadPoint == null)
                throw new InvalidOperationException(
                    "UnityTurretRig references are incomplete.");

            _initialYawRotation =
                _turretYaw.localRotation;
            _initialPitchRotation =
                _gunPitch.localRotation;

            _initialized = true;
        }
    }
}