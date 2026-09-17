using UnityEngine;
using Zenject;

public sealed class PlayerFacade : MonoBehaviour
{
    [SerializeField] private Transform _cameraPivot;
    public InputPhase Input { get; private set; }
    public GameplayPhase Gameplay { get; private set; }
    public PresentationPhase Presentation { get; private set; }
    public Transform CameraPivot => _cameraPivot;

    [Inject]
    public void Construct(InputPhase input, GameplayPhase gameplay, PresentationPhase presentation)
    {
        Input = input;
        Gameplay = gameplay;
        Presentation = presentation;
    }

    public sealed class Factory : PlaceholderFactory<PlayerFacade> { }
}
