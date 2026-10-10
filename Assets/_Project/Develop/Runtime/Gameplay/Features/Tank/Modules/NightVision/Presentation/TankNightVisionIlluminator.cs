using UnityEngine;
using UnityEngine.Rendering;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.NightVision.Presentation
{
    public sealed class TankNightVisionIlluminator : MonoBehaviour
    {
        [SerializeField] private Camera _nightVisionCamera;
        [SerializeField] private Volume _nightVisionVolume;
        [SerializeField] private Light _infraredLight;

        private void OnEnable()
        {
            TurnOffLight();

            RenderPipelineManager.beginCameraRendering +=
                OnBeginCameraRendering;

            RenderPipelineManager.endCameraRendering +=
                OnEndCameraRendering;
        }

        private void OnDisable()
        {
            RenderPipelineManager.beginCameraRendering -=
                OnBeginCameraRendering;

            RenderPipelineManager.endCameraRendering -=
                OnEndCameraRendering;

            TurnOffLight();
        }

        private void OnBeginCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            if (_infraredLight == null)
                return;

            _infraredLight.enabled =
                _nightVisionCamera != null &&
                camera == _nightVisionCamera &&
                _nightVisionVolume != null &&
                _nightVisionVolume.isActiveAndEnabled &&
                _nightVisionVolume.weight > 0f;
        }

        private void OnEndCameraRendering(
            ScriptableRenderContext context,
            Camera camera)
        {
            TurnOffLight();
        }

        private void TurnOffLight()
        {
            if (_infraredLight != null)
                _infraredLight.enabled = false;
        }
    }
}