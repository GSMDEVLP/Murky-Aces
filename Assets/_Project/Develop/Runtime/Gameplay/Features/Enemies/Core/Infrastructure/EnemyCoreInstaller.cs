using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.Core.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class EnemyCoreInstaller : MonoInstaller
    {
        [SerializeField] private EnemyRoot _root;

        public override void InstallBindings()
        {
            ValidateReferences();

            ulong enemyId = EntityId.ToULong(_root.GetEntityId());

            Container.Bind<Enemy>()
                .AsSingle()
                .WithArguments(enemyId);

            Container.Bind<IEnemy>()
                .To<Enemy>()
                .FromResolve();

            Container.Bind<EnemyLifecycle>()
                .FromMethod(CreateLifecycle)
                .AsSingle();

            Container.BindInterfacesAndSelfTo<EnemyRoot>()
                .FromInstance(_root)
                .AsSingle();
        }

        private EnemyLifecycle CreateLifecycle(InjectContext context)
        {
            DiContainer container = context.Container;

            return new EnemyLifecycle(
                container.Resolve<Enemy>(),
                container.Resolve<GameLoopRegistry>(),
                ResolveLocal<IEnemyRuntimeModule>(
                    container,
                    EnemyModuleGroup.Gameplay),
                ResolveLocal<IEnemyRuntimeModule>(
                    container,
                    EnemyModuleGroup.Presentation),
                ResolveLocal<IEnemyDeathSource>(container),
                ResolveLocal<IEnemyDeathListener>(container));
        }

        private static List<T> ResolveLocal<T>(
            DiContainer container,
            object identifier = null)
        {
            InjectContext context = new InjectContext(
                container,
                typeof(List<T>),
                identifier)
            {
                SourceType = InjectSources.Local
            };

            return (List<T>)container.Resolve(context);
        }

        private void ValidateReferences()
        {
            if (_root == null)
            {
                throw new InvalidOperationException(
                    "Enemy core installer requires an EnemyRoot reference.");
            }

            GameObjectContext context =
                GetComponentInParent<GameObjectContext>(true);

            if (context == null || context.gameObject != _root.gameObject)
            {
                throw new InvalidOperationException(
                    "Enemy core installer must belong to its root context.");
            }
        }
    }
}
