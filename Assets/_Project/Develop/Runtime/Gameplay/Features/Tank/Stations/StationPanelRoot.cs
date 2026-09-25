using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    [DisallowMultipleComponent]
    public sealed class StationPanelRoot : MonoBehaviour, IInteractionScope
    {
        [Header("Primary Display")]
        [SerializeField] private Renderer _primaryScreenRenderer;

        [SerializeField, Min(0)] private int _primaryScreenMaterialIndex;

        public Transform InteractionRoot => transform;

        public bool HasValidPrimaryScreen
        {
            get
            {
                if (_primaryScreenRenderer == null)
                    return false;

                return _primaryScreenMaterialIndex >= 0 &&
                       _primaryScreenMaterialIndex <
                       _primaryScreenRenderer
                           .sharedMaterials.Length;
            }
        }

        public bool TryGetPrimaryScreen(out Renderer renderer, out int materialIndex)
        {
            renderer = _primaryScreenRenderer;
            materialIndex = _primaryScreenMaterialIndex;

            return HasValidPrimaryScreen;
        }

        public bool IsInsideInteractionScope(Component candidate)
        {
            if (candidate == null)
                return false;

            Transform candidateTransform = candidate.transform;

            return candidateTransform == transform || candidateTransform.IsChildOf(transform);
        }

        public bool Allows(IInteractable target)
        {
            Component component = target as Component;

            return IsInsideInteractionScope(component);
        }
    }
}