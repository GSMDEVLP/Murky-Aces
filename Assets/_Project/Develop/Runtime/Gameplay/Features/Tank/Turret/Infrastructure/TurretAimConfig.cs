using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Infrastructure
{
    [CreateAssetMenu(fileName = "TurretAimConfig", menuName = "Murky Aces/Tank/Turret Aim Config")]
    public sealed class TurretAimConfig : ScriptableObject
    {
        [Header("Drive")]
        [SerializeField, Min(0f)]
        private float _traverseDegreesPerSecond;

        [SerializeField, Min(0f)]
        private float _elevationDegreesPerSecond;

        [Header("Pitch limits")]
        [SerializeField] private float _minPitch;
        [SerializeField] private float _maxPitch;

        [Header("Local axes; sign is part of the vector")]
        [SerializeField] private Vector3 _yawAxis = Vector3.up;
        [SerializeField] private Vector3 _pitchAxis = Vector3.forward;
        [SerializeField] private Vector3 _muzzleForwardAxis = Vector3.right;
        [SerializeField] private Vector3 _hullForwardAxis = Vector3.forward;

        [Header("Initial mode")]
        [SerializeField]
        private TurretReferenceMode _defaultReferenceMode = TurretReferenceMode.HullRelative;

        public float TraverseDegreesPerSecond => _traverseDegreesPerSecond;

        public float ElevationDegreesPerSecond => _elevationDegreesPerSecond;

        public float MinPitch => _minPitch;
        public float MaxPitch => _maxPitch;

        public Vector3 YawAxis => _yawAxis.normalized;
        public Vector3 PitchAxis => _pitchAxis.normalized;
        public Vector3 MuzzleForwardAxis => _muzzleForwardAxis.normalized;
        public Vector3 HullForwardAxis => _hullForwardAxis.normalized; 
        public TurretReferenceMode DefaultReferenceMode => _defaultReferenceMode;

        public bool IsValid =>
            _traverseDegreesPerSecond > 0f &&
            _elevationDegreesPerSecond > 0f &&
            _minPitch <= _maxPitch &&
            _yawAxis.sqrMagnitude > 0.000001f &&
            _pitchAxis.sqrMagnitude > 0.000001f &&
            _muzzleForwardAxis.sqrMagnitude > 0.000001f &&
            _hullForwardAxis.sqrMagnitude > 0.000001f;
    }
}