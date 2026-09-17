using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputService
{
    private readonly PlayerInput _playerInput;
    private InputActionAsset _actions;
    private InputAction _move;
    private InputAction _look;
    private InputAction _sprint;
    private InputAction _jump;
    private InputAction _attack;
    private InputAction _interact;
    private InputAction _drop;
    public InputService(PlayerInput playerInput)
    {
        _playerInput = playerInput;
    }

    public Vector2 Move
    {
        get { RefreshActions(); return _move.ReadValue<Vector2>(); }
    }

    public Vector2 Look
    {
        get { RefreshActions(); return _look.ReadValue<Vector2>(); }
    }

    public bool LookIsPointerDelta
    {
        get { RefreshActions(); return _look.activeControl?.device is Pointer; }
    }

    public bool SprintHeld
    {
        get { RefreshActions(); return _sprint.IsPressed(); }
    }

    public bool JumpPressedThisFrame
    {
        get { RefreshActions(); return _jump.IsPressed(); }
    }

    public bool AttackPressedThisFrame
    {
        get { RefreshActions(); return _attack.WasPressedThisFrame(); }
    }

    public bool InteractPressedThisFrame
    {
        get { RefreshActions(); return _interact.WasPressedThisFrame(); }
    }

    public bool InteractHeld
    {
        get { RefreshActions(); return _interact.IsPressed(); }
    }

    public bool InteractReleasedThisFrame
    {
        get { RefreshActions(); return _interact.WasReleasedThisFrame(); }
    }

    public bool DropPressedThisFrame
    {
        get { RefreshActions(); return _drop.WasPressedThisFrame(); }
    }

    private void RefreshActions()
    {
        if (_actions == _playerInput.actions)
            return;

        _actions = _playerInput.actions;
        _move = _actions.FindAction("Move", true);
        _look = _actions.FindAction("Look", true);
        _sprint = _actions.FindAction("Sprint", true);
        _jump = _actions.FindAction("Jump", true);
        _attack = _actions.FindAction("Attack", true);
        _interact = _actions.FindAction("Interact", true);
        _drop = _actions.FindAction("Drop", true);
    }
}
