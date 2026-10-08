using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure.Readers;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using GameLoopTickable =
    _Project.Develop.Runtime.Core.GameLoop.Abstractions.ITickable;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Infrastructure
{
    public sealed class PlayerInputSystem : MonoBehaviour, GameLoopTickable
    {
        [SerializeField] private float _mouseLookDegreesPerPixel = 0.15f;

        [SerializeField] private float _stickLookDegreesPerSecond = 180f;

        private PlayerActionsReader _playerActions;
        private DrivingActionsReader _drivingActions;
        private GunnerActionsReader _gunnerActions;
        private CommanderActionsReader _commanderActions;
        private ActiveCommonActionsReader _commonActions;

        private PlayerIntentBuffer _playerIntent;
        private DrivingIntentBuffer _drivingIntent;
        private GunnerIntentBuffer _gunnerIntent;
        private CommanderIntentBuffer _commanderIntent;

        [Inject]
        public void Construct(
            PlayerActionsReader playerActions,
            DrivingActionsReader drivingActions,
            ActiveCommonActionsReader commonActions,
            PlayerIntentBuffer playerIntent,
            DrivingIntentBuffer drivingIntent,
            GunnerActionsReader gunnerActions,
            GunnerIntentBuffer gunnerIntent,
            CommanderActionsReader commanderActions,
            CommanderIntentBuffer commanderIntent)
        {
            _playerActions = playerActions;
            _drivingActions = drivingActions;
            _commonActions = commonActions;
            _playerIntent = playerIntent;
            _drivingIntent = drivingIntent;
            _gunnerActions = gunnerActions;
            _gunnerIntent = gunnerIntent;
            _commanderActions = commanderActions;
            _commanderIntent = commanderIntent;
        }

        public void Tick(float deltaTime)
        {
            CommonInputSnapshot common = _commonActions.ReadCommonActions();

            PlayerActionsSnapshot player = _playerActions.ReadPlayerActions();

            DrivingActionsSnapshot driving = _drivingActions.ReadDrivingActions();

            GunnerActionsSnapshot gunner = _gunnerActions.ReadGunnerActions();
            CommanderActionsSnapshot commander = _commanderActions.ReadCommanderActions();

            UpdatePlayerIntent(common, player, deltaTime);
            UpdateDrivingIntent(driving);
            _gunnerIntent.SetActions(gunner);
            _commanderIntent.SetExitInput(commander.ExitPressedThisFrame, commander.ExitHeld);
        }

        private void UpdatePlayerIntent(CommonInputSnapshot common, PlayerActionsSnapshot player, float deltaTime)
        {
            _playerIntent.SetMovement(
                new Vector2(
                    player.MoveX,
                    player.MoveY),
                player.SprintHeld);

            float lookScale =
                common.LookIsPointerDelta
                    ? _mouseLookDegreesPerPixel
                    : _stickLookDegreesPerSecond *
                      deltaTime;

            _playerIntent.SetLook(
                new Vector2(
                    common.LookX,
                    common.LookY) *
                lookScale);

            if (player.JumpHeld)
                _playerIntent.RequestJump();

            _playerIntent.SetInteraction(
                common.InteractPressedThisFrame,
                common.InteractHeld,
                common.InteractReleasedThisFrame);

            _playerIntent.SetDrop(
                common.DropPressedThisFrame);
        }

        private void UpdateDrivingIntent(
            DrivingActionsSnapshot driving)
        {
            _drivingIntent.SetDriving(
                driving.Throttle,
                driving.Steering,
                driving.BrakeHeld);

            _drivingIntent.SetExitInput(
                driving.ExitPressedThisFrame,
                driving.ExitHeld);
        }
    }
}