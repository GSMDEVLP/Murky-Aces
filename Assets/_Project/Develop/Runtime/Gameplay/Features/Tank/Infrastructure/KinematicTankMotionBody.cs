using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Infrastructure
{
    public sealed class KinematicTankMotionBody : MonoBehaviour, ITankMotionBody
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Vector3 Position => _rigidbody.position;
        public Quaternion Rotation => _rigidbody.rotation;

        private void Awake()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(KinematicTankMotionBody)} requires a Rigidbody.");
            }

            _rigidbody.isKinematic = true;
        }

        public void ResolveAndApplyMotion(Vector3 desiredDisplacement, Quaternion desiredRotation)
        {
            Vector3 targetPosition = _rigidbody.position + desiredDisplacement;

            _rigidbody.MovePosition(targetPosition);
            _rigidbody.MoveRotation(desiredRotation);
        }

        private void Reset()
        {
            _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody != null)
                _rigidbody.isKinematic = true;
        }

        private void OnValidate()
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();

            if (_rigidbody != null)
                _rigidbody.isKinematic = true;
        }
    }
}