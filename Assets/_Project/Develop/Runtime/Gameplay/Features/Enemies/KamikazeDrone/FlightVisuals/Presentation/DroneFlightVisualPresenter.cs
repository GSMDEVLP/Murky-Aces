using System;
using UnityEngine;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Presentation
{
    public sealed class DroneFlightVisualPresenter : IPresentationTickable, IEnemyRuntimeModule
    {
        private readonly IEnemy _owner;
        private readonly IDroneFlightBody _body;
        private readonly DroneMovementSettings _movement;
        private readonly DroneFlightVisualConfig _config;
        private readonly DroneFlightVisualView _view;

        private bool _isActive;
        private Vector3 _lastVelocity;
        private Vector3 _smoothedAcceleration;
        private float _pitch;

        public DroneFlightVisualPresenter(
            IEnemy owner,
            IDroneFlightBody body,
            DroneMovementSettings movement,
            DroneFlightVisualConfig config,
            DroneFlightVisualView view)
        {
            _owner = owner ?? throw new ArgumentNullException(nameof(owner));
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _movement = movement ?? throw new ArgumentNullException(nameof(movement));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _view = view != null ? view : throw new ArgumentNullException(nameof(view));
            _config.Validate();
        }

        public void Activate(GameLoopRegistry registry)
        {
            if (_isActive)
                return;

            _isActive = true;

            _view.Initialize();
            Reset();
            registry.RegisterPresentation(this);
        }

        public void Deactivate(GameLoopRegistry registry)
        {
            if (_isActive == false)
                return;

            _isActive = false;

            registry.UnregisterPresentation(this);
            Reset();
        }

        public void Tick(float deltaTime)
        {
            if (_isActive == false ||
                _owner.IsAlive == false ||
                deltaTime <= 0f ||
                _view == null ||
                _view.isActiveAndEnabled == false)
                return;

            Vector3 velocity = _body.Velocity;

            UpdateAcceleration(velocity, deltaTime);

            float targetPitch = CalculateTargetPitch(velocity);

            UpdatePitch(targetPitch, deltaTime);
            _view.ApplyPitch(_pitch);
        }

        private void UpdateAcceleration(Vector3 velocity, float deltaTime)
        {
            Vector3 acceleration = (velocity - _lastVelocity) / deltaTime;
            _lastVelocity = velocity;

            float accelerationBlend = 1f - Mathf.Exp(-_config.AccelerationResponse * deltaTime);
            _smoothedAcceleration = Vector3.Lerp(_smoothedAcceleration, acceleration, accelerationBlend);
        }

        private float CalculateTargetPitch(Vector3 velocity)
        {
            Vector3 localVelocity = _view.ToFlightLocalDirection(velocity);
            Vector3 localAcceleration = _view.ToFlightLocalDirection(_smoothedAcceleration);

            float cruisePitch = CalculateCruisePitch(localVelocity.z);
            float accelerationPitch = CalculateAccelerationPitch(localAcceleration.z);

            return Mathf.Clamp(
                cruisePitch + accelerationPitch,
                -_config.BrakingPitch,
                _config.CruisePitch + _config.AccelerationPitch);
        }

        private float CalculateCruisePitch(float forwardSpeed)
        {
            return _config.CruisePitch * Mathf.Clamp(forwardSpeed / _movement.MaxSpeed, -1f, 1f);
        }

        private float CalculateAccelerationPitch(float forwardAcceleration)
        {
            return forwardAcceleration >= 0f
                ? _config.AccelerationPitch * Mathf.Clamp01(forwardAcceleration / _movement.Acceleration)
                : -_config.BrakingPitch * Mathf.Clamp01(-forwardAcceleration / _movement.Deceleration);
        }

        private void UpdatePitch(float targetPitch, float deltaTime)
        {
            float tiltBlend = 1f - Mathf.Exp(-_config.TiltResponse * deltaTime);
            _pitch = Mathf.Lerp(_pitch, targetPitch, tiltBlend);
        }

        private void Reset()
        {
            _lastVelocity = _body.Velocity;
            _smoothedAcceleration = Vector3.zero;
            _pitch = 0f;

            if (_view != null)
                _view.ResetPose();
        }
    }
}
