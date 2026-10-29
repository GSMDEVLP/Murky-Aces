using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Movement.Infrastructure
{
    public sealed class DroneFlightBody : MonoBehaviour, IDroneFlightBody
    {
        [SerializeField] private Rigidbody _rigidbody;

        private Vector3 _lastPosition;
        private float _initialYaw;
        private bool _isInitialized;

        private void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_rigidbody == null)
                _rigidbody = GetComponentInParent<Rigidbody>();

            if (_rigidbody == null)
                throw new InvalidOperationException(
                    $"{nameof(DroneFlightBody)} requires a Rigidbody on the drone root.");

            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = 0f;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;

            _initialYaw = _rigidbody.rotation.eulerAngles.y;
            _isInitialized = true;

            Stop();
            ResetDistanceSample();
        }

        public void ResetDistanceSample()
        {
            _lastPosition = _rigidbody.position;
        }

        public float MeasureTravelledDistance()
        {
            Vector3 position = _rigidbody.position;
            float distance = Vector3.Distance(_lastPosition, position);

            _lastPosition = position;

            return distance;
        }

        public void ApplyFlight(float speed, float headingOffsetDegrees)
        {
            Quaternion rotation = Quaternion.Euler(
                0f,
                _initialYaw + headingOffsetDegrees,
                0f);

            Vector3 direction = rotation * Vector3.forward;

            _rigidbody.linearVelocity = direction * speed;
            _rigidbody.MoveRotation(rotation);
        }

        public void Stop()
        {
            if (_rigidbody == null)
                return;

            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        private void Reset()
        {
            _rigidbody = GetComponentInParent<Rigidbody>();
        }
    }
}
