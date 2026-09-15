using System;
using System.ComponentModel;
using UnityEngine;
using Zenject;

public class PlayerSpawningInstaller : MonoInstaller
{
    [SerializeField] private GameObject _playerPrefab;
    public override void InstallBindings()
    {
        Container.Bind<PlayerRegistry>().AsSingle();
        Container.Bind<PlayerSpawner>().AsSingle();

        Container.BindFactory<PlayerFacade, PlayerFacade.Factory>()
            .FromSubContainerResolve()
            .ByNewContextPrefab(_playerPrefab);
    }
}
