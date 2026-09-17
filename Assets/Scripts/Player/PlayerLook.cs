using UnityEngine;
using Zenject;

public sealed class PlayerLook : MonoBehaviour, ITickable, IFixedTickable
{
    [SerializeField] private Rigidbody _body;
    [SerializeField] private Transform _cameraPivot;
    [SerializeField] private float _maxPitch = 85f;

    private PlayerIntentBuffer _intent;
    private float _yaw;
    private float _pitch;
    private bool _initialized;

    public Quaternion Heading
    {
        get
        {
            EnsureInitialized();
            return Quaternion.Euler(0f, _yaw, 0f);
        }
    }

    [Inject]
    public void Construct(PlayerIntentBuffer intent)
    {
        _intent = intent;
    }

    public void Tick(float deltaTime)
    {
        EnsureInitialized();

        Vector2 look = _intent.LookDelta;

        _yaw += look.x;
        _pitch = Mathf.Clamp(_pitch - look.y, -_maxPitch, _maxPitch);
        _cameraPivot.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }

    public void FixedTick(float fixedDeltaTime)
    {
        _body.MoveRotation(Heading);
    }

    private void EnsureInitialized()
    {
        if (_initialized)
            return;

        _yaw = _body.rotation.eulerAngles.y;
        _pitch = Mathf.DeltaAngle(0f, _cameraPivot.localEulerAngles.x);
        _initialized = true;
    }
}
