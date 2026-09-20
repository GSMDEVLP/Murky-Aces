using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Phases;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
public class InputInstaller : MonoInstaller
{
    [SerializeField] private PlayerInputSystem _playerInputSystem;
    [SerializeField] private PlayerMovement _playerMovement;
    [SerializeField] private UnityPlayerStationBody _stationBody;
    [SerializeField] private PlayerLook _playerLook;
    private PlayerInput _playerInput;
    public override void InstallBindings()
    {
        _playerInput = GetComponent<PlayerInput>();
        
        Container.Bind<PlayerFacade>().FromComponentOnRoot().AsSingle();

        Container.Bind<PlayerInput>().FromInstance(_playerInput);
        Container.Bind<InputService>().AsSingle();
        Container.Bind<PlayerIntentBuffer>().AsSingle();

        Container.Bind<PlayerInputSystem>().FromInstance(_playerInputSystem);
        Container.Bind<PlayerMovement>().FromInstance(_playerMovement);
        Container.Bind<PlayerLook>().FromInstance(_playerLook);
        Container.Bind<IPlayerStationBody>().FromInstance(_stationBody).AsSingle();
        Container.Bind<PlayerStationCapabilities>().AsSingle();

        Container.Bind<PlayerStationController>().AsSingle();
        Container.Bind<IStationOccupant>().FromResolveGetter<PlayerStationController>(controller => controller).AsSingle();

        Container.Bind<InputPhase>().AsSingle();
        Container.Bind<GameplayPhase>().AsSingle();
    }
}
}

