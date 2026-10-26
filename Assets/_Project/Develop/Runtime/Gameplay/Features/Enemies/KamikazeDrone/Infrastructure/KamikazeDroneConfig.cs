using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Infrastructure
{
    [CreateAssetMenu(fileName = "KamikazeDroneConfig", menuName = "Murky Aces/Enemies/Kamikaze Drone")]
    public sealed class KamikazeDroneConfig : ScriptableObject
    {
        [Header("Flight")]
        [SerializeField, Min(0.01f)] private float _maxSpeed = 12f;
        [SerializeField, Min(0.01f)] private float _acceleration = 6f;
        [SerializeField, Min(0.01f)] private float _deceleration = 8f;

        [Header("Patrol")]
        [SerializeField, Min(0.01f)] private float _legDistance = 100f;
        [SerializeField, Min(0f)] private float _pauseDuration = 5f;

        public float MaxSpeed => _maxSpeed;
        public float Acceleration => _acceleration;
        public float Deceleration => _deceleration;
        public float LegDistance => _legDistance;
        public float PauseDuration => _pauseDuration;
    }
}