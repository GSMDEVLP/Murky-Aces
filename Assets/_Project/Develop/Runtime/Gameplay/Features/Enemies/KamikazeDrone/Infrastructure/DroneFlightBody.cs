using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Infrastructure
{
    public sealed class DroneFlightBody : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private Vector3 _lastPosition;
        private float _initialYaw;

        public void Initialize()
        {
            _rigidbody = GetComponent<Rigidbody>();

            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.linearDamping = 0f;
            _rigidbody.angularDamping = 0f;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;

            _initialYaw = _rigidbody.rotation.eulerAngles.y;

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
    }
}