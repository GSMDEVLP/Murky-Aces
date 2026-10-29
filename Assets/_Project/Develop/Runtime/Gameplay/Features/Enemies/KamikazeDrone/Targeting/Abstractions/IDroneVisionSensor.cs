using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions
{
    public interface IDroneVisionSensor
    {
        bool CanSee(ICombatTarget target);
    }
}