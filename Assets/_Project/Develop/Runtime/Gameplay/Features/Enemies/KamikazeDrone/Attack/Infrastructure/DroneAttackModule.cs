using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    public sealed class DroneAttackModule : IEnemyRuntimeModule, IGameplayTickable
    {
        private readonly DroneBehaviour _behaviour;
        private readonly DroneAttack _attack;
        private readonly DroneCollisionSensor _collisionSensor;

        private bool _isActive;

        public DroneAttackModule(
            DroneBehaviour behaviour,
            DroneAttack attack,
            DroneCollisionSensor collisionSensor)
        {
            _behaviour = behaviour ??
                throw new ArgumentNullException(nameof(behaviour));

            _attack = attack ??
                throw new ArgumentNullException(nameof(attack));

            _collisionSensor = collisionSensor ??
                throw new ArgumentNullException(nameof(collisionSensor));
        }

        public void Activate(GameLoopRegistry registry)
        {
            if (_isActive)
                return;

            _isActive = true;

            _collisionSensor.StartMonitoring();
            _behaviour.OnAttackPointReached += HandleDetonationRequest;
            registry.RegisterGameplay(this);
        }

        public void Deactivate(GameLoopRegistry registry)
        {
            if (_isActive == false)
                return;

            _isActive = false;

            registry.UnregisterGameplay(this);
            _behaviour.OnAttackPointReached -= HandleDetonationRequest;
            _collisionSensor.StopMonitoring();
        }

        public void Tick(float deltaTime)
        {
            if (_isActive == false || deltaTime <= 0f)
                return;

            if (_collisionSensor.ConsumeContact())
                _attack.TryDetonate();
        }

        private void HandleDetonationRequest()
        {
            if (_isActive)
                _attack.TryDetonate();
        }
    }
}
