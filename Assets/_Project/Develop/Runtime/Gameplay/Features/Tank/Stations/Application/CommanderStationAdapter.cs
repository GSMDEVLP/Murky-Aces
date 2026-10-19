using _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations.Application
{
    public sealed class CommanderStationAdapter : ICrewStationRoleAdapter
    {
        public bool IsActive { get; private set; }

        public bool CanUse(IInteractionActor actor)
        {
            return actor is IStationOccupant &&
                actor is IStationIntentSource;
        }

        public bool TryActivate(IInteractionActor actor)
        {
            if (IsActive || !CanUse(actor))
                return false;

            IsActive = true;
            return true;
        }

        public void Tick(float deltaTime)
        {
        }

        public void ClearOutput()
        {
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}