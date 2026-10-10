using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Abstractions
{
    public interface IRadarTarget
    {
        ulong TargetId { get; }
        string TypeId { get; }
        Vector3 WorldPosition { get; }
        bool IsAvailable { get; }
    }
}