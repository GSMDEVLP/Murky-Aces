using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class CombatTarget : MonoBehaviour, ICombatTarget
    {
        [SerializeField] private CombatTargetKind _kind;
        [SerializeField] private Transform _positionAnchor;

        private CombatTargetRegistry _registry;
        private ulong _targetId;

        public ulong TargetId
        {
            get
            {
                if (_targetId == 0)
                    _targetId = EntityId.ToULong(GetEntityId());

                return _targetId;
            }
        }

        public CombatTargetKind Kind => _kind;

        public Vector3 WorldPosition =>
            _positionAnchor != null
                ? _positionAnchor.position
                : transform.position;

        public bool IsAvailable => this != null && isActiveAndEnabled;

        [Inject]
        public void Construct(CombatTargetRegistry registry)
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

        private void OnDestroy()
        {
            _registry?.Unregister(this);
        }

        private void RegisterIfActive()
        {
            if (_registry != null && IsAvailable)
                _registry.Register(this);
        }
    }
}