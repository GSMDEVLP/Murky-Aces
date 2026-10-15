using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Crew.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Stations;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.GPS
{
    public sealed class PlayerGpsCameraView : MonoBehaviour, IPlayerGpsCamera
    {
        [SerializeField] private Transform _cameraPivot;

        [SerializeField, Min(0f)]
        private float _transitionDuration = 0.25f;

        [Header("Temporary Preview")]
        [SerializeField] private Transform _previewAnchor;

        private PlayerLook _look;
        private ICrewLocationReader _location;

        private Transform _targetAnchor;

        private Vector3 _savedPosition;
        private Quaternion _savedRotation;
        private bool _savedLookEnabled;
        private long _savedLocationRevision;

        private Vector3 _transitionStartPosition;
        private Quaternion _transitionStartRotation;
        private float _elapsed;
        private bool _isReturning;

        public bool IsActive { get; private set; }
        public bool IsAvailable => this != null && isActiveAndEnabled;

        public bool IsFocused =>
            IsActive &&
            !_isReturning &&
            _elapsed >= Mathf.Max(0f, _transitionDuration);

        [Inject]
        public void Construct(PlayerLook look, ICrewLocationReader location)
        {
            _look = look ??
                throw new ArgumentNullException(nameof(look));

            _location = location ??
                throw new ArgumentNullException(nameof(location));
        }

        public bool TryFocus(Transform anchor)
        {
            if (IsActive ||
                !IsAvailable  ||
                _cameraPivot == null ||
                _cameraPivot.parent == null ||
                anchor == null ||
                !anchor.gameObject.activeInHierarchy ||
                anchor == _cameraPivot ||
                anchor.IsChildOf(_cameraPivot) ||
                _look == null ||
                _location == null)
            {
                return false;
            }

            CrewLocation current = _location.Current;

            if (current.Kind != CrewLocationKind.Station ||
                (current.StationRole != CrewRoleId.Driver &&
                 current.StationRole != CrewRoleId.CommanderLoader) ||
                !_look.IsCockpitMode ||
                !_look.IsGameplayEnabled)
            {
                return false;
            }

            _savedPosition = _cameraPivot.localPosition;
            _savedRotation = _cameraPivot.localRotation;
            _savedLookEnabled = _look.IsGameplayEnabled;
            _savedLocationRevision = _location.Revision;

            _targetAnchor = anchor;
            _isReturning = false;
            IsActive = true;

            _look.SetGameplayEnabled(false);
            BeginTransition();

            return true;
        }

        public bool TryReturn()
        {
            if (!IsActive || _isReturning)
                return false;

            _isReturning = true;
            BeginTransition();

            return true;
        }

        public void Tick(float deltaTime)
        {
            if (!IsActive)
                return;

            if (_cameraPivot == null ||
                _cameraPivot.parent == null ||
                _location.Revision != _savedLocationRevision)
            {
                ForceRestore();
                return;
            }

            if (!_isReturning &&
                (_targetAnchor == null ||
                 !_targetAnchor.gameObject.activeInHierarchy))
            {
                ForceRestore();
                return;
            }

            Vector3 targetPosition;
            Quaternion targetRotation;

            if (_isReturning)
            {
                targetPosition = _savedPosition;
                targetRotation = _savedRotation;
            }
            else
            {
                Transform parent = _cameraPivot.parent;

                targetPosition =
                    parent.InverseTransformPoint(_targetAnchor.position);

                targetRotation =
                    Quaternion.Inverse(parent.rotation) *
                    _targetAnchor.rotation;
            }

            _elapsed += Mathf.Max(0f, deltaTime);

            float progress = _transitionDuration <= 0f
                ? 1f
                : Mathf.Clamp01(_elapsed / _transitionDuration);

            float eased = Mathf.SmoothStep(0f, 1f, progress);

            _cameraPivot.localPosition = Vector3.Lerp(
                _transitionStartPosition,
                targetPosition,
                eased);

            _cameraPivot.localRotation = Quaternion.Slerp(
                _transitionStartRotation,
                targetRotation,
                eased);

            if (_isReturning && progress >= 1f)
                ForceRestore();
        }

        public void ForceRestore()
        {
            if (!IsActive)
                return;

            bool sameLocation =
                _location != null &&
                _location.Revision == _savedLocationRevision;

            if (_cameraPivot != null)
            {
                _cameraPivot.localPosition = _savedPosition;

                if (sameLocation)
                    _cameraPivot.localRotation = _savedRotation;
            }

            if (sameLocation && _look != null)
                _look.SetGameplayEnabled(_savedLookEnabled);

            IsActive = false;
            _isReturning = false;
            _targetAnchor = null;
            _elapsed = 0f;
        }

        private void BeginTransition()
        {
            _transitionStartPosition = _cameraPivot.localPosition;
            _transitionStartRotation = _cameraPivot.localRotation;
            _elapsed = 0f;
        }

        private void OnDisable()
        {
            ForceRestore();
        }

        [ContextMenu("Preview GPS Focus")]
        private void PreviewFocus()
        {
            if (UnityEngine.Application.isPlaying)
                TryFocus(_previewAnchor);
        }

        [ContextMenu("Preview GPS Return")]
        private void PreviewReturn()
        {
            if (UnityEngine.Application.isPlaying)
                TryReturn();
        }
    }
}