using System;
using System.Collections.Generic;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Application
{
    public sealed class EnemyLifecycle : IDisposable
    {
        private readonly Enemy _enemy;
        private readonly GameLoopRegistry _registry;

        private readonly IEnemyRuntimeModule[] _gameplayModules;
        private readonly IEnemyRuntimeModule[] _presentationModules;
        private readonly IEnemyDeathSource[] _deathSources;
        private readonly IEnemyDeathListener[] _deathListeners;

        private int _activeGameplayCount;
        private int _activePresentationCount;

        private bool _isEnabled;
        private bool _isDisposed;
        private bool _isProcessing;
        private bool _deathPending;

        public bool IsActive => _isEnabled && _isDisposed == false;

        public EnemyLifecycle(
            Enemy enemy,
            GameLoopRegistry registry,
            IReadOnlyList<IEnemyRuntimeModule> gameplayModules,
            IReadOnlyList<IEnemyRuntimeModule> presentationModules,
            IReadOnlyList<IEnemyDeathSource> deathSources,
            IReadOnlyList<IEnemyDeathListener> deathListeners)
        {
            _enemy = enemy ??
                throw new ArgumentNullException(nameof(enemy));

            _registry = registry ??
                throw new ArgumentNullException(nameof(registry));

            _gameplayModules = CopyUnique(
                gameplayModules, nameof(gameplayModules));

            _presentationModules = CopyUnique(
                presentationModules, nameof(presentationModules));

            _deathSources = CopyUnique(
                deathSources, nameof(deathSources));

            _deathListeners = CopyUnique(
                deathListeners, nameof(deathListeners));

            foreach (IEnemyRuntimeModule gameplay in _gameplayModules)
            {
                foreach (IEnemyRuntimeModule presentation in _presentationModules)
                {
                    if (ReferenceEquals(gameplay, presentation))
                    {
                        throw new ArgumentException(
                            "A module cannot belong to both runtime groups.",
                            nameof(presentationModules));
                    }
                }
            }

            foreach (IEnemyDeathSource source in _deathSources)
                source.DeathRequested += HandleDeathRequested;
        }

        public void Activate()
        {
            if (_isDisposed || _isEnabled)
                return;

            _isEnabled = true;
            Synchronize();
        }

        public void Deactivate()
        {
            if (_isDisposed || _isEnabled == false)
                return;

            _isEnabled = false;
            Synchronize();
        }

        public bool TryDie()
        {
            if (_isDisposed || _enemy.TryDie() == false)
                return false;

            _deathPending = true;
            Synchronize();

            return true;
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            _isEnabled = false;

            Synchronize();
        }

        private void HandleDeathRequested()
        {
            TryDie();
        }

        private void Synchronize()
        {
            if (_isProcessing)
                return;

            _isProcessing = true;
            List<Exception> errors = new();

            try
            {
                while (true)
                {
                    bool runPresentation = IsActive;
                    bool runGameplay = runPresentation && _enemy.IsAlive;

                    if (runGameplay == false && _activeGameplayCount > 0)
                    {
                        IEnemyRuntimeModule module =
                            _gameplayModules[--_activeGameplayCount];

                        Execute(() => module.Deactivate(_registry), errors);
                        continue;
                    }

                    if (runPresentation == false &&
                        _activePresentationCount > 0)
                    {
                        IEnemyRuntimeModule module =
                            _presentationModules[--_activePresentationCount];

                        Execute(() => module.Deactivate(_registry), errors);
                        continue;
                    }

                    if (_deathPending)
                    {
                        _deathPending = false;

                        foreach (IEnemyDeathListener listener in _deathListeners)
                            Execute(listener.OnOwnerDied, errors);

                        continue;
                    }

                    if (_isDisposed)
                    {
                        foreach (IEnemyDeathSource source in _deathSources)
                        {
                            Execute(
                                () => source.DeathRequested -= HandleDeathRequested,
                                errors);
                        }

                        break;
                    }

                    if (runPresentation &&
                        _activePresentationCount < _presentationModules.Length)
                    {
                        IEnemyRuntimeModule module =
                            _presentationModules[_activePresentationCount++];

                        Execute(() => module.Activate(_registry), errors);
                        continue;
                    }

                    if (runGameplay &&
                        _activeGameplayCount < _gameplayModules.Length)
                    {
                        IEnemyRuntimeModule module =
                            _gameplayModules[_activeGameplayCount++];

                        Execute(() => module.Activate(_registry), errors);
                        continue;
                    }

                    break;
                }
            }
            finally
            {
                _isProcessing = false;
            }

            if (errors.Count > 0)
            {
                throw new AggregateException(
                    "Enemy lifecycle transition failed.",
                    errors);
            }
        }

        private void Execute(Action action, List<Exception> errors)
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                errors.Add(exception);

                _isEnabled = false;
                _isDisposed = true;
            }
        }

        private static T[] CopyUnique<T>(
            IReadOnlyList<T> items,
            string parameterName)
            where T : class
        {
            if (items == null)
                throw new ArgumentNullException(parameterName);

            T[] result = new T[items.Count];

            for (int i = 0; i < items.Count; i++)
            {
                T item = items[i];

                if (item == null)
                {
                    throw new ArgumentException(
                        "Collection cannot contain null.",
                        parameterName);
                }

                for (int j = 0; j < i; j++)
                {
                    if (ReferenceEquals(result[j], item))
                    {
                        throw new ArgumentException(
                            "Collection cannot contain duplicate instances.",
                            parameterName);
                    }
                }

                result[i] = item;
            }

            return result;
        }
    }
}
