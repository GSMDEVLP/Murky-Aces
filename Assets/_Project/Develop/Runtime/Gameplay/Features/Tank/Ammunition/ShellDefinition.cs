using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition
{
    [CreateAssetMenu(fileName = "ShellDefinition", menuName = "Murky Aces/Tank/Shell Definition")]
    public sealed class ShellDefinition : ScriptableObject
    {
        [SerializeField] private string _id;
        [SerializeField] private string _displayName;
        [SerializeField] private string _compatibleWeaponId;

        [Header("Projectile")]
        [SerializeField] private GameObject _projectilePrefab;
        [SerializeField, Min(0.01f)] private float _muzzleSpeed;
        [SerializeField, Min(0.01f)] private float _projectileMass;
        [SerializeField, Min(0.01f)] private float _projectileLifetime;

        public string Id => _id;
        public string DisplayName => _displayName;
        public string CompatibleWeaponId => _compatibleWeaponId;

        public GameObject ProjectilePrefab => _projectilePrefab;
        public float MuzzleSpeed => _muzzleSpeed;
        public float ProjectileMass => _projectileMass;
        public float ProjectileLifetime => _projectileLifetime;

        public bool IsValid =>
            !string.IsNullOrWhiteSpace(_id) &&
            !string.IsNullOrWhiteSpace(_compatibleWeaponId);

        public bool HasValidProjectileSetup =>
            IsValid &&
            _projectilePrefab != null &&
            IsPositiveFinite(_muzzleSpeed) &&
            IsPositiveFinite(_projectileMass) &&
            IsPositiveFinite(_projectileLifetime);

        private static bool IsPositiveFinite(float value)
        {
            return value > 0f && !float.IsInfinity(value);
        }
    }
}