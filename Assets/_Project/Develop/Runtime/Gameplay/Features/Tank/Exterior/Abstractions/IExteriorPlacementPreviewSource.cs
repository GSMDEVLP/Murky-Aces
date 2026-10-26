using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Abstractions
{
    public interface IExteriorPlacementPreviewSource
    {
        bool TryGetPlacementPreview(in InteractionContext context,
            out GameObject previewPrefab, out Transform placementAnchor);
    }
}
