using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Stations
{
    [DisallowMultipleComponent]
    public sealed class StationPanelRoot : MonoBehaviour
    {
        [Header("Primary Display")]
        [SerializeField]
        private Renderer _primaryScreenRenderer;

        [SerializeField, Min(0)]
        private int _primaryScreenMaterialIndex;

        public bool HasValidPrimaryScreen
        {
            get
            {
                if (_primaryScreenRenderer == null)
                    return false;

                return _primaryScreenMaterialIndex >= 0 &&
                       _primaryScreenMaterialIndex <
                       _primaryScreenRenderer.sharedMaterials.Length;
            }
        }

        public bool TryGetPrimaryScreen(
            out Renderer renderer,
            out int materialIndex)
        {
            renderer = _primaryScreenRenderer;
            materialIndex = _primaryScreenMaterialIndex;

            return HasValidPrimaryScreen;
        }
    }
}