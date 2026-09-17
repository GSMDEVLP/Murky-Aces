using UnityEngine;

public sealed class PlayerIntentBuffer
{
    public Vector2 Move { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public bool SprintHeld { get; private set; }
    public bool InteractPressedThisFrame { get; private set; }
    public bool InteractHeld { get; private set; }
    public bool InteractReleasedThisFrame { get; private set; }
    public bool DropPressedThisFrame { get; private set; }

    private bool _jumpPending;

    public void SetMovement(Vector2 move, bool sprintHeld)
    {
        Move = Vector2.ClampMagnitude(move, 1f);
        SprintHeld = sprintHeld;
    }

    public void SetLook(Vector2 lookDelta)
    {
        LookDelta = lookDelta;
    }

    public void SetInteraction(bool pressedThisFrame, bool held, bool releasedThisFrame)
    {
        InteractPressedThisFrame = pressedThisFrame;
        InteractHeld = held;
        InteractReleasedThisFrame = releasedThisFrame;
    }

    public void SetDrop(bool pressedThisFrame)
    {
        DropPressedThisFrame = pressedThisFrame;
    }
    public void RequestJump() => _jumpPending = true;

    public bool ConsumeJump()
    {
        bool requested = _jumpPending;
        _jumpPending = false;
        return requested;
    }
}
