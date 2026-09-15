using UnityEngine;
using Zenject;

public sealed class PlayerFacade : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    public InputPhase Input { get; private set; }
    public GameplayPhase Gameplay { get; private set; }
    public Transform CameraPivot => _cameraPivot;

    [Inject]
    public void Construct(InputPhase input, GameplayPhase gameplay)
    {
        Input = input;
        Gameplay = gameplay;
    }

    public sealed class Factory : PlaceholderFactory<PlayerFacade> { }
}