using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.GPS.Infrastructure
{
    public sealed class GpsGridMapSource : MonoBehaviour, IGpsMapSource
    {
        [SerializeField] private Vector2 _center = Vector2.zero;
        [SerializeField] private Vector2 _size = new Vector2(200f, 200f);

        private GpsMapData _map;

        public bool TryGetMap(out GpsMapData map)
        {
            _map ??= new GpsMapData(
                _center,
                _size,
                Array.Empty<Vector2>());

            map = _map;
            return true;
        }
    }
}