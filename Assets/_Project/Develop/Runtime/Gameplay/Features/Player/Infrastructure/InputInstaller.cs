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
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Interior;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure
{
    public class InputInstaller : MonoInstaller
    {
        [SerializeField] private PlayerInputSystem _playerInputSystem;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private UnityPlayerStationBody _stationBody;
        [SerializeField] private PlayerLook _playerLook;
        [SerializeField] private PlayerTankInteriorBody _interiorBody;


        private PlayerInput _playerInput;

        public override void InstallBindings()
        {
            CacheComponents();
            BindCrewTransitions();
            BindPlayerFacade();
            BindInput();
            BindMovementAndLook();
            BindStation();
            BindPhases();
        }

        private void CacheComponents()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void BindPlayerFacade()
        {
            Container.Bind<PlayerFacade>().FromComponentOnRoot().AsSingle();
        }


        private void BindCrewTransitions()
        {
            Container.Bind<CrewTransitionCoordinator>()
                .AsSingle();

            Container.Bind<ICrewLocationReader>()
                .FromResolveGetter<CrewTransitionCoordinator>(
                    coordinator => coordinator)
                .AsSingle();
        }
        private void BindInput()
        {
            Container.Bind<PlayerInput>().FromInstance(_playerInput);

            Container.BindInterfacesAndSelfTo<PlayerActionsReader>().AsSingle();
            Container.BindInterfacesAndSelfTo<DrivingActionsReader>().AsSingle();
            Container.Bind<ActiveCommonActionsReader>().AsSingle();
            
            Container.Bind<PlayerIntentBuffer>().AsSingle();
            Container.Bind<DrivingIntentBuffer>().AsSingle();
            Container.Bind<IPlayerInputContext>().To<PlayerInputContext>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerDrivingInputMode>().AsSingle();

            Container.BindInterfacesAndSelfTo<GunnerActionsReader>().AsSingle();
            Container.Bind<GunnerIntentBuffer>().AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerGunnerInputMode>().AsSingle();
            
            Container.Bind<PlayerInputSystem>().FromInstance(_playerInputSystem);
        }

        private void BindMovementAndLook()
        {
            Container.Bind<PlayerMovement>().FromInstance(_playerMovement);

            Container.Bind<PlayerLook>().FromInstance(_playerLook);
        }

        private void BindStation()
        {
            Container.Bind<IPlayerStationBody>().FromInstance(_stationBody).AsSingle();
            Container.Bind<PlayerStationCapabilities>().AsSingle();
            Container.Bind<PlayerStationController>().AsSingle();
            Container.Bind<IStationOccupant>().FromResolveGetter<PlayerStationController>(controller => controller).AsSingle();
            Container.Bind<IPlayerInteriorBody>().FromInstance(_interiorBody).AsSingle();
            Container.Bind<ITankInteriorOccupant>().To<PlayerTankInteriorController>().AsSingle();
        }

        private void BindPhases()
        {
            Container.Bind<InputPhase>().AsSingle();
            Container.Bind<GameplayPhase>().AsSingle();
        }
    }
}

