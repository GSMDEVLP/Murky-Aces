using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure
{
    public sealed class DynamicTankMotionBody : MonoBehaviour, ITankMotionBody
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Vector3 Position => _rigidbody.position;
        public Quaternion Rotation => _rigidbody.rotation;
        public Vector3 LinearVelocity => _rigidbody.linearVelocity;
        public Vector3 AngularVelocity => _rigidbody.angularVelocity;

        private void Awake()
        {
            ResolveRigidbody();

            if (_rigidbody == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DynamicTankMotionBody)} requires a Rigidbody.");
            }

            ConfigureRigidbody();
        }

        public void ApplyLinearAcceleration(Vector3 acceleration)
        {
            if (acceleration.sqrMagnitude <= Mathf.Epsilon)
                return;

            _rigidbody.WakeUp();
            _rigidbody.AddForce(
                acceleration,
                ForceMode.Acceleration);
        }

        public void ApplyAngularAcceleration(Vector3 acceleration)
        {
            if (acceleration.sqrMagnitude <= Mathf.Epsilon)
                return;

            _rigidbody.WakeUp();
            _rigidbody.AddTorque(
                acceleration,
                ForceMode.Acceleration);
        }

        private void ResolveRigidbody()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
        }

        private void ConfigureRigidbody()
        {
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        }

        private void Reset()
        {
            ResolveRigidbody();

            if (_rigidbody != null)
                ConfigureRigidbody();
        }

        private void OnValidate()
        {
            ResolveRigidbody();

            if (_rigidbody != null)
                ConfigureRigidbody();
        }
    }
}