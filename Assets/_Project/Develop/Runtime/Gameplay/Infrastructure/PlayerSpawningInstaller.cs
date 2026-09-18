using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Infrastructure
{
public class PlayerSpawningInstaller : MonoInstaller
{
    [SerializeField] private GameObject _playerPrefab;
    public override void InstallBindings()
    {
        Container.Bind<GameLoopRegistry>().AsSingle();
        Container.Bind<PlayerRegistry>().AsSingle();
        Container.Bind<PlayerSpawner>().AsSingle();

        Container.BindFactory<PlayerFacade, PlayerFacade.Factory>()
            .FromSubContainerResolve()
            .ByNewContextPrefab(_playerPrefab);
    }
}
}
