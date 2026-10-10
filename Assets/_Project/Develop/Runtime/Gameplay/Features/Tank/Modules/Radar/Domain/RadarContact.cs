using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Domain
{
    public readonly struct RadarContact
    {
        public ulong TargetId { get; }
        public string TypeId { get; }
        public Vector3 WorldPosition { get; }

        public RadarContact(ulong targetId, string typeId, Vector3 worldPosition)
        {
            if (string.IsNullOrWhiteSpace(typeId))
            {
                throw new ArgumentException(
                    "Radar target type must be specified.",
                    nameof(typeId));
            }

            TargetId = targetId;
            TypeId = typeId;
            WorldPosition = worldPosition;
        }
    }
}