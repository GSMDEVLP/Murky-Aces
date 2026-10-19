using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorAttachmentAuthoring : MonoBehaviour
    {
        [SerializeField] private ExteriorAttachmentConfig _config;

        private ExteriorAttachmentState _state;

        public ExteriorAttachmentConfig Config => _config;

        public ExteriorAttachmentState State
        {
            get
            {
                EnsureInitialized();
                return _state;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void EnsureInitialized()
        {
            if (_state != null)
                return;

            if (_config == null || !_config.IsValid)
            {
                throw new InvalidOperationException(
                    $"{name}: assign a valid attachment config.");
            }

            _state = new ExteriorAttachmentState(
                _config.CompatibleMountTypeId,
                _config.RequiresWelding);
        }
    }
}