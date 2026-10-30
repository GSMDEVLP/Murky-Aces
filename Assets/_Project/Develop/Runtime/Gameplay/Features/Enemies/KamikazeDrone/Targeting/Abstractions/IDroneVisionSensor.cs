using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Abstractions
{
    public interface IDroneVisionSensor
    {
        Vector3 WorldPosition { get; }
        bool CanSee(ICombatTarget target);
    }
}