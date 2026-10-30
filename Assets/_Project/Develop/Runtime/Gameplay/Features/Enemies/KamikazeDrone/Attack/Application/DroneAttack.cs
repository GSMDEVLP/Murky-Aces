using System;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application
{
    public sealed class DroneAttack
    {
        private readonly DroneBehaviour _behaviour;
        private readonly IDroneDetonationBody _body;

        public bool HasDetonated { get; private set; }

        public DroneAttack(
            DroneBehaviour behaviour,
            IDroneDetonationBody body)
        {
            _behaviour = behaviour ??
                throw new ArgumentNullException(nameof(behaviour));

            _body = body ??
                throw new ArgumentNullException(nameof(body));
        }

        public bool TryDetonate()
        {
            if (HasDetonated)
                return false;

            if (!_behaviour.AttackDestination.HasValue)
                return false;

            HasDetonated = true;
            _body.CompleteDetonation();

            return true;
        }
    }
}
