using System;
using _Project.Develop.Runtime.Core.GameLoop.Abstractions;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    public sealed class DroneAttackModule : IDroneRuntimeModule, IGameplayTickable
    {
        private readonly DroneBehaviour _behaviour;
        private readonly DroneAttack _attack;
        private readonly DroneCollisionSensor _collisionSensor;

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

        public void Register(GameLoopRegistry registry)
        {
            _collisionSensor.ClearContact();
            _behaviour.OnAttackPointReached += HandleDetonationRequest;
            registry.RegisterGameplay(this);
        }

        public void Unregister(GameLoopRegistry registry)
        {
            registry.UnregisterGameplay(this);
            _behaviour.OnAttackPointReached -= HandleDetonationRequest;
            _collisionSensor.ClearContact();
        }

        public void Tick(float deltaTime)
        {
            if (deltaTime > 0f && _collisionSensor.ConsumeContact())
                _attack.TryDetonate();
        }

        private void HandleDetonationRequest()
        {
            _attack.TryDetonate();
        }
    }
}
