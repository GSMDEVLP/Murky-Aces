using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Ammunition;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class TankWeaponView : MonoBehaviour
    {
        [SerializeField] private string _weaponId = "main_gun";

        private GunLoadingService _loading;

        public string WeaponId => _weaponId;

        public bool IsLoaded =>
            _loading != null && _loading.IsLoaded;

        public string LoadedShellId =>
            _loading?.LoadedShellId;

        public ShellDefinition LoadedShell =>
            _loading?.LoadedShell;

        [Inject]
        public void Construct(GunLoadingService loading)
        {
            _loading = loading
                ?? throw new ArgumentNullException(nameof(loading));
        }
    }
}