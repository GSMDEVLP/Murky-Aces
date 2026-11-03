using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Turret.Infrastructure
{
    [CreateAssetMenu(fileName = "ApcTurretSearchConfig", menuName = "Murky Aces/Enemies/APC/Turret Search Config")]
    public sealed class ApcTurretSearchConfig : ScriptableObject
    {
        [SerializeField, Range(1f, 360f)] private float _searchAngle = 120f;
        [SerializeField, Min(0.01f)] private float _degreesPerSecond = 10f;
        [SerializeField] private Vector3 _localYawAxis = Vector3.forward;

        public float SearchAngle => _searchAngle;
        public float DegreesPerSecond => _degreesPerSecond;
        public Vector3 LocalYawAxis => _localYawAxis;
    }
}