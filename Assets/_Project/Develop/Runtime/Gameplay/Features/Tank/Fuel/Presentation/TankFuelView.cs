using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Fuel.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TankFuelView : MonoBehaviour
    {
        private static readonly int FillId =
            Shader.PropertyToID("_Fill");

        [SerializeField] private Renderer _screen;
        [SerializeField, Min(0)] private int _materialIndex;

        private MaterialPropertyBlock _properties;
        private float _lastFill = float.NaN;

        public bool HasValidScreen
        {
            get
            {
                if (_screen == null)
                    return false;

                Material[] materials = _screen.sharedMaterials;

                return _materialIndex >= 0 &&
                       _materialIndex < materials.Length &&
                       materials[_materialIndex] != null &&
                       materials[_materialIndex].HasProperty(FillId);
            }
        }

        private void OnEnable()
        {
            _lastFill = float.NaN;
        }

        public void ApplyFill(float normalizedAmount)
        {
            if (!isActiveAndEnabled || _screen == null)
                return;

            float fill = Mathf.Clamp01(normalizedAmount);

            if (_lastFill == fill)
                return;

            _properties ??= new MaterialPropertyBlock();

            _screen.GetPropertyBlock(_properties, _materialIndex);
            _properties.SetFloat(FillId, fill);
            _screen.SetPropertyBlock(_properties, _materialIndex);

            _lastFill = fill;
        }
    }
}