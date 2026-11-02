using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.FlightVisuals.Infrastructure
{
    [CreateAssetMenu(fileName = "DroneFlightVisualConfig", menuName = "Murky Aces/Enemies/Drone Flight Visuals")]
    public sealed class DroneFlightVisualConfig : ScriptableObject
    {
        [Header("Pitch (degrees)")]
        [SerializeField, Range(0f, 30f)] private float _cruisePitch = 8f;
        [SerializeField, Range(0f, 15f)] private float _accelerationPitch = 6f;
        [SerializeField, Range(0f, 30f)] private float _brakingPitch = 16f;

        [Header("Smoothing (response per second)")]
        [SerializeField, Min(0.01f)] private float _tiltResponse = 6f;
        [SerializeField, Min(0.01f)] private float _accelerationResponse = 8f;

        public float CruisePitch => _cruisePitch;
        public float AccelerationPitch => _accelerationPitch;
        public float BrakingPitch => _brakingPitch;
        public float TiltResponse => _tiltResponse;
        public float AccelerationResponse => _accelerationResponse;

        public void Validate()
        {
            ValidateValue(_cruisePitch, 0f, 30f, nameof(_cruisePitch));
            ValidateValue(_accelerationPitch, 0f, 15f, nameof(_accelerationPitch));
            ValidateValue(_brakingPitch, 0f, 30f, nameof(_brakingPitch));
            ValidateValue(_tiltResponse, 0.01f, float.MaxValue, nameof(_tiltResponse));
            ValidateValue(_accelerationResponse, 0.01f, float.MaxValue, nameof(_accelerationResponse));
        }

        private static void ValidateValue(float value, float minimum, float maximum, string name)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(name);
        }
    }
}
