using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class InputInstaller : MonoInstaller
{
    [SerializeField] private PlayerInputSystem _playerInputSystem;
    [SerializeField] private PlayerMovement _playerMovement;
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

        Container.Bind<InputPhase>().AsSingle();
        Container.Bind<GameplayPhase>().AsSingle();
    }
}

