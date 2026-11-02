using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class DroneDetonationBody : MonoBehaviour, IEnemyDeathListener
    {
        [SerializeField] private EnemyRoot _droneRoot;

        private DroneAttack _attack;
        private bool _isRemovalRequested;

        public EnemyRoot DroneRoot => _droneRoot;

        [Inject]
        public void Construct(DroneAttack attack)
        {
            _attack = attack ??
                throw new ArgumentNullException(nameof(attack));
        }

        public void OnOwnerDied()
        {
            if (_isRemovalRequested || _attack.HasDetonated == false)
                return;

            _isRemovalRequested = true;
            Destroy(_droneRoot.gameObject);
        }
    }
}
