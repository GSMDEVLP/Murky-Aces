using UnityEngine;
using Zenject;

public sealed class PlayerInputSystem : MonoBehaviour, ITickable
{
    [SerializeField] private float _mouseLookDegreesPerPixel = 0.15f;
    [SerializeField] private float _stickLookDegreesPerSecond = 180f;

    private InputService _input;
    private PlayerIntentBuffer _intent;

    [Inject]
    public void Construct(InputService input, PlayerIntentBuffer intent)
    {
        _input = input;
        _intent = intent;
    }

    public void Tick(float deltaTime)
    {
        _intent.SetMovement(_input.Move, _input.SprintHeld);

        float lookScale = _input.LookIsPointerDelta
            ? _mouseLookDegreesPerPixel
            : _stickLookDegreesPerSecond * Time.deltaTime;
        _intent.SetLook(_input.Look * lookScale);

        if (_input.JumpPressedThisFrame)
            _intent.RequestJump();

        _intent.SetInteraction(_input.InteractPressedThisFrame,_input.InteractHeld,_input.InteractReleasedThisFrame);
        _intent.SetDrop(_input.DropPressedThisFrame);
    }
}
