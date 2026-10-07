using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Configs
{
    [CreateAssetMenu(fileName = "GunConfig", menuName = "Murky Aces/Tank/Gun Config")]
    public sealed class GunConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)]
        private float _fireCooldown;

        public float FireCooldown => _fireCooldown;

        public bool IsValid =>
            _fireCooldown > 0f &&
            !float.IsInfinity(_fireCooldown);
    }
}