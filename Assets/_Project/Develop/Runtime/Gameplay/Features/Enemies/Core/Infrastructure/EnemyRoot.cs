using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class EnemyRoot : MonoBehaviour, IInitializable, IDisposable
    {
        private EnemyLifecycle _lifecycle;

        private bool _isInitialized;
        private bool _isDisposed;

        [Inject]
        public void Construct(EnemyLifecycle lifecycle)
        {
            if (lifecycle == null)
                throw new ArgumentNullException(nameof(lifecycle));

            if (_isDisposed)
                throw new ObjectDisposedException(nameof(EnemyRoot));

            if (_lifecycle != null)
            {
                throw new InvalidOperationException(
                    "EnemyRoot has already been constructed.");
            }

            _lifecycle = lifecycle;
        }

        public void Initialize()
        {
            if (_isDisposed || _isInitialized)
                return;

            if (_lifecycle == null)
            {
                throw new InvalidOperationException(
                    "EnemyRoot must be constructed before initialization.");
            }

            _isInitialized = true;
            TryActivate();
        }

        private void OnEnable()
        {
            TryActivate();
        }

        private void OnDisable()
        {
            _lifecycle?.Deactivate();
        }

        private void OnDestroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            EnemyLifecycle lifecycle = _lifecycle;
            _lifecycle = null;

            lifecycle?.Dispose();
        }

        private void TryActivate()
        {
            if (_isInitialized == false ||
                _isDisposed ||
                isActiveAndEnabled == false)
            {
                return;
            }

            _lifecycle.Activate();
        }
    }
}
