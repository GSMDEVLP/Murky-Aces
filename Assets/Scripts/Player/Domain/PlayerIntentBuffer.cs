using UnityEngine;

public sealed class PlayerIntentBuffer
{
    public Vector2 Move { get; private set; }
    public Vector2 LookDelta { get; private set; }
    public bool SprintHeld { get; private set; }

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

    public void RequestJump() => _jumpPending = true;

    public bool ConsumeJump()
    {
        bool requested = _jumpPending;
        _jumpPending = false;
        return requested;
    }
}
