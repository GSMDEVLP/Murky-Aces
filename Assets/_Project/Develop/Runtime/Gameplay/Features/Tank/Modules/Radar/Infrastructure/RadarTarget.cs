using System;
using Zenject;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Configs;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class RadarTarget : MonoBehaviour, IRadarTarget
    {
        [SerializeField] private RadarTargetDefinition _definition;
        [SerializeField] private Transform _positionAnchor;

        private RadarTargetRegistry _registry;
        
        public ulong TargetId => EntityId.ToULong(GetEntityId());

        public string TypeId =>
            _definition != null
                ? _definition.Id
                : string.Empty;

        [Inject]
        public void Construct(RadarTargetRegistry registry)
        {
            if (registry == null)
                throw new ArgumentNullException(nameof(registry));

            _registry?.Unregister(this);
            _registry = registry;

            RegisterIfActive();
        }

        private void OnEnable()
        {
            RegisterIfActive();
        }

        private void OnDisable()
        {
            _registry?.Unregister(this);
        }

        private void RegisterIfActive()
        {
            if (_registry != null && isActiveAndEnabled)
                _registry.Register(this);
        }

        public Vector3 WorldPosition =>
            _positionAnchor != null
                ? _positionAnchor.position
                : transform.position;

        public bool HasValidReferences =>
            _definition != null &&
            _definition.IsValid;

        public bool IsAvailable =>
            isActiveAndEnabled &&
            HasValidReferences;
    }
}