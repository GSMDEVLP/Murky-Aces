using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class ExteriorAttachmentAuthoring : MonoBehaviour
    {
        [SerializeField] private ExteriorAttachmentConfig _config;

        private ExteriorAttachmentMountBody _mountBody;
        private ExteriorAttachmentState _state;
        public ExteriorAttachmentConfig Config => _config;

        public ExteriorAttachmentMountBody MountBody
        {
            get
            {
                EnsureInitialized();
                return _mountBody;
            }
        }

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
        private void OnDestroy()
        {
            if (_state == null)
                return;

            if (_state.TryDestroy() == false)
            {
                Debug.LogError(
                    "Failed to release the attachment state on destroy.",
                    this);
            }
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
                
            _mountBody = new ExteriorAttachmentMountBody(
                transform,
                GetComponent<Rigidbody>(),
                GetComponent<PickupInteractable>());
        }
    }
}