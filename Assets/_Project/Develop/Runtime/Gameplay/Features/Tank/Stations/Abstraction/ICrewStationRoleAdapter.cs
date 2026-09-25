using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    public interface ICrewStationRoleAdapter
    {
        bool IsActive { get; }

        bool CanUse(IInteractionActor actor);

        bool TryActivate(IInteractionActor actor);

        void Tick(float deltaTime);

        void ClearOutput();

        void Deactivate();
    }
}