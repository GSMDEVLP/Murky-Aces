using UnityEngine;
using Zenject;

public sealed class GameLoop : MonoBehaviour
{
    private PlayerRegistry _registry;    
    // private AIPhase _ai;
    // private PresentationPhase _presentation;
    // private CleanupPhase _cleanup;

    // [Inject]
    // public void Construct(
    //     InputPhase input,
    //     AIPhase ai,
    //     GameplayPhase gameplay,
    //     PresentationPhase presentation,
    //     CleanupPhase cleanup)
    // {
    //     _input = input;
    //     _ai = ai;
    //     _gameplay = gameplay;
    //     _presentation = presentation;
    //     _cleanup = cleanup;
    // }

    [Inject]
    public void Construct(PlayerRegistry registry)
    {
        _registry = registry;
    }


    private void Update()
    {
        float deltaTime = Time.deltaTime;
        foreach (var player in _registry.Players)
            player.Input.Tick(deltaTime);

        
        foreach (var player in _registry.Players)
            player.Gameplay.Tick(deltaTime);

        foreach (PlayerFacade player in _registry.Players)
            player.Presentation.Tick();
    }

    private void FixedUpdate()
    {
        float fixedDeltaTime = Time.fixedDeltaTime;

        foreach (var player in _registry.Players)
            player.Gameplay.FixedTick(fixedDeltaTime);
    }
}
