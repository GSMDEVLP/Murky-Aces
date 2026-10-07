using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition
{
    public sealed class ShellItem : MonoBehaviour
    {
        [SerializeField] private ShellDefinition _definition;

        public ShellDefinition Definition => _definition;

        public bool IsValid => _definition != null && _definition.IsValid;
    }
}