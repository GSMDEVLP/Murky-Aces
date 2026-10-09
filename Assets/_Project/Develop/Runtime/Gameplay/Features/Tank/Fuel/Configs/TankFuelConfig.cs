using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Configs
{
    [CreateAssetMenu(fileName = "TankFuelConfig", menuName = "Murky Aces/Tank/Fuel Config")]
    public sealed class TankFuelConfig : ScriptableObject
    {
        [SerializeField, Min(0.01f)] private float _capacity = 100f;

        [SerializeField, Range(0f, 1f)] private float _initialFill = 1f;

        public float Capacity => _capacity;
        public float InitialFill => _initialFill;

        public bool IsValid =>
            _capacity > 0f &&
            !float.IsInfinity(_capacity) &&
            _initialFill >= 0f &&
            _initialFill <= 1f;
    }
}