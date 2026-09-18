using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Movement
{
    [CreateAssetMenu(fileName = "TankMovementConfig", menuName = "Murky Aces/Tank/Tank Movement Config")]
    public sealed class TankMovementConfig : ScriptableObject
    {
        [Header("Speed")]
        [SerializeField, Min(0f)] private float _forwardMaxSpeed;

        [SerializeField, Min(0f)] private float _reverseMaxSpeed;

        [Header("Acceleration")]
        [SerializeField, Min(0f)] private float _forwardAcceleration;

        [SerializeField, Min(0f)] private float _reverseAcceleration;

        [SerializeField, Min(0f)] private float _deceleration;

        [SerializeField, Min(0f)] private float _brakeDeceleration;

        [Header("Steering")]
        [SerializeField, Min(0f)] private float _turnSpeed;

        [Header("Direction switching")]
        [SerializeField, Min(0f)] private float _directionSwitchDelay;

        [Header("Ground")]
        [SerializeField, Min(0f)] private float _groundProbeDistance;

        [SerializeField, Min(0f)] private float _groundOffset;

        [SerializeField, Min(0f)] private float _groundAlignmentSpeed;

        [SerializeField, Range(0f, 90f)] private float _maximumSlopeAngle;

        [Header("Collision")]
        [SerializeField, Min(0f)] private float _collisionSkin;

        public float ForwardMaxSpeed => _forwardMaxSpeed;
        public float ReverseMaxSpeed => _reverseMaxSpeed;
        public float ForwardAcceleration => _forwardAcceleration;
        public float ReverseAcceleration => _reverseAcceleration;
        public float Deceleration => _deceleration;
        public float BrakeDeceleration => _brakeDeceleration;
        public float TurnSpeed => _turnSpeed;
        public float DirectionSwitchDelay => _directionSwitchDelay;
        public float GroundProbeDistance => _groundProbeDistance;
        public float GroundOffset => _groundOffset;
        public float GroundAlignmentSpeed => _groundAlignmentSpeed;
        public float MaximumSlopeAngle => _maximumSlopeAngle;
        public float CollisionSkin => _collisionSkin;
    }
}