using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.ArmoredPersonnelCarrier.Targeting.Abstractions
{
    public interface IApcVisionSensor
    {
        Vector3 WorldPosition { get; }
        bool CanSee(ICombatTarget target);
        bool CanTrack(ICombatTarget target);
    }
}
