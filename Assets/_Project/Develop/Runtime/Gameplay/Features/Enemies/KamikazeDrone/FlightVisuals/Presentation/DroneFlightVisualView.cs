using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Presentation
{
    [DisallowMultipleComponent]
    public sealed class DroneFlightVisualView : MonoBehaviour
    {
        [SerializeField] private Transform _flightFrame;
        [SerializeField] private Transform _tiltPivot;

        private Quaternion _baseRotation;
        private bool _isInitialized;

        public Transform FlightFrame => _flightFrame;
        public Transform TiltPivot => _tiltPivot;

        public void ValidateReferences(Transform droneRoot)
        {
            if (droneRoot == null || _flightFrame != droneRoot ||
                !transform.IsChildOf(droneRoot) || transform == droneRoot ||
                _tiltPivot == null || _tiltPivot == droneRoot ||
                !_tiltPivot.IsChildOf(droneRoot) ||
                _tiltPivot.GetComponentInChildren<Rigidbody>(true) != null ||
                _tiltPivot.GetComponentInChildren<Collider>(true) != null)
            {
                throw new InvalidOperationException(
                    "Drone flight view requires its own flight frame and a visual-only child pivot.");
            }
        }

        public void Initialize()
        {
            if (_isInitialized)
                return;

            if (_flightFrame == null || _tiltPivot == null)
                throw new InvalidOperationException("Drone flight visuals require a flight frame and tilt pivot.");

            _baseRotation = _tiltPivot.localRotation;
            _isInitialized = true;
        }

        public Vector3 ToFlightLocalDirection(Vector3 direction)
        {
            return _flightFrame.InverseTransformDirection(direction);
        }

        public void ApplyPitch(float pitch)
        {
            if (isActiveAndEnabled && _isInitialized && _tiltPivot != null)
                _tiltPivot.localRotation = _baseRotation * Quaternion.Euler(pitch, 0f, 0f);
        }

        public void ResetPose()
        {
            if (_isInitialized && _tiltPivot != null)
                _tiltPivot.localRotation = _baseRotation;
        }
    }
}
