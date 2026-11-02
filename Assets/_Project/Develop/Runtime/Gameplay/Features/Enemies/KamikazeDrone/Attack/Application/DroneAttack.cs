using System;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application
{
    public sealed class DroneAttack : IEnemyDeathSource
    {
        private readonly IEnemy _owner;
        private readonly DroneBehaviour _behaviour;

        public event Action DeathRequested;

        public bool HasDetonated { get; private set; }

        public DroneAttack(
            IEnemy owner,
            DroneBehaviour behaviour)
        {
            _owner = owner ??
                throw new ArgumentNullException(nameof(owner));

            _behaviour = behaviour ??
                throw new ArgumentNullException(nameof(behaviour));
        }

        public bool TryDetonate()
        {
            if (HasDetonated || _owner.IsAlive == false)
                return false;

            if (_behaviour.AttackDestination.HasValue == false)
                return false;

            HasDetonated = true;
            DeathRequested?.Invoke();

            return true;
        }
    }
}
