using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Lights.Presentation
{
    public sealed class TankLightsView : MonoBehaviour
    {
        [SerializeField] private Light[] _lights = Array.Empty<Light>();

        public bool HasValidLights
        {
            get
            {
                if (_lights == null || _lights.Length == 0)
                    return false;

                foreach (Light light in _lights)
                {
                    if (light == null)
                        return false;
                }

                return true;
            }
        }

        public void ApplyState(bool enabled)
        {
            SetLightsEnabled(enabled && isActiveAndEnabled);
        }

        public void TurnOff()
        {
            SetLightsEnabled(false);
        }

        private void Awake()
        {
            TurnOff();
        }

        private void OnDisable()
        {
            TurnOff();
        }

        private void SetLightsEnabled(bool enabled)
        {
            if (_lights == null)
                return;

            foreach (Light light in _lights)
            {
                if (light != null && light.enabled != enabled)
                    light.enabled = enabled;
            }
        }
    }
}