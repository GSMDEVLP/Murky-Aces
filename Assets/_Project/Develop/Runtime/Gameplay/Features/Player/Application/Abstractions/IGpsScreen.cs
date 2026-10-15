using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions
{
    public interface IGpsScreen
    {
        CrewRoleId Role { get; }
        Transform ViewAnchor { get; }

        float CursorSensitivity { get; }
        float ScreenAspect { get; }

        bool CanUse(IInteractionActor actor);

        void AttachSession(IPlayerGpsSession session);
        void DetachSession(IPlayerGpsSession session);
        void HideCursor();

        bool TryZoom(IInteractionActor actor, float zoomInput);

        bool TrySetCursor(
            IInteractionActor actor,
            Vector2 screenUv);

        bool TrySelectDestination(
            IInteractionActor actor,
            Vector2 screenUv);
    }
}