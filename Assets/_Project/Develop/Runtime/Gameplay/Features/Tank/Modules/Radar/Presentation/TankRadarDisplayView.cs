using System;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Modules.Radar.Presentation
{
    [DisallowMultipleComponent]
    public sealed class TankRadarDisplayView : MonoBehaviour
    {
        public const int MaxContacts = 64;

        private static readonly int RadarEnabledId =
            Shader.PropertyToID("_RadarEnabled");

        private static readonly int ContactCountId =
            Shader.PropertyToID("_ContactCount");

        private static readonly int ContactPositionsId =
            Shader.PropertyToID("_ContactPositions");

        private static readonly int ContactColorsId =
            Shader.PropertyToID("_ContactColors");

        [SerializeField] private Renderer _screen;
        [SerializeField, Min(0)] private int _materialIndex;

        private MaterialPropertyBlock _propertyBlock;

        public bool HasValidReferences
        {
            get
            {
                if (_screen == null || _materialIndex < 0)
                    return false;

                Material[] materials = _screen.sharedMaterials;

                if (_materialIndex >= materials.Length)
                    return false;

                Material material = materials[_materialIndex];

                return material != null
                    && material.HasProperty(RadarEnabledId)
                    && material.HasProperty(ContactCountId);
            }
        }

        private void Awake()
        {
            if (!HasValidReferences)
            {
                throw new InvalidOperationException(
                    "Radar display renderer or material is invalid.");
            }

            TurnOff();
        }

        private void OnDisable()
        {
            TurnOff();
        }

        public void Render(
            bool enabled,
            Vector4[] positions,
            Vector4[] colors,
            int contactCount)
        {
            if (!isActiveAndEnabled)
                return;

            if (!enabled)
            {
                TurnOff();
                return;
            }

            if (positions == null || positions.Length != MaxContacts)
            {
                throw new ArgumentException(
                    $"Expected {MaxContacts} contact positions.",
                    nameof(positions));
            }

            if (colors == null || colors.Length != MaxContacts)
            {
                throw new ArgumentException(
                    $"Expected {MaxContacts} contact colors.",
                    nameof(colors));
            }

            if (contactCount < 0 || contactCount > MaxContacts)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(contactCount));
            }

            ReadPropertyBlock();

            _propertyBlock.SetFloat(RadarEnabledId, 1f);
            _propertyBlock.SetFloat(ContactCountId, contactCount);

            _propertyBlock.SetVectorArray(
                ContactPositionsId,
                positions);

            _propertyBlock.SetVectorArray(
                ContactColorsId,
                colors);

            ApplyPropertyBlock();
        }

        public void TurnOff()
        {
            if (_screen == null)
                return;

            ReadPropertyBlock();

            _propertyBlock.SetFloat(RadarEnabledId, 0f);
            _propertyBlock.SetFloat(ContactCountId, 0f);

            ApplyPropertyBlock();
        }

        private void ReadPropertyBlock()
        {
            if (_propertyBlock == null)
                _propertyBlock = new MaterialPropertyBlock();

            _screen.GetPropertyBlock(
                _propertyBlock,
                _materialIndex);
        }

        private void ApplyPropertyBlock()
        {
            _screen.SetPropertyBlock(
                _propertyBlock,
                _materialIndex);
        }
    }
}