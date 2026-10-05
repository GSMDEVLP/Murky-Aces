using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Player.Application.Abstractions;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Station
{
    public sealed class UnityPlayerStationBody : MonoBehaviour, IPlayerStationBody
    {
        [SerializeField] private Rigidbody _body;
        [SerializeField] private CapsuleCollider _collider;

        [Header("Exit Validation")]
        [SerializeField] private LayerMask _exitBlockingLayers = ~0;
        [SerializeField, Min(0f)] private float _exitSkin = 0.02f;

        private PlayerStationBodySnapshot _snapshot;

        public bool IsAttached { get; private set; }

        public bool TryAttach(Transform seatAnchor)
        {
            if (IsAttached ||
                seatAnchor == null ||
                HasRequiredReferences() == false)
            {
                return false;
            }

            CacheState();
            DisableWorldBody();
            AttachTo(seatAnchor);

            IsAttached = true;

            return true;
        }
        public bool TryRestoreBeforeAttach()
        {
            if (!IsAttached || !HasRequiredReferences())
                return false;

            Transform playerTransform = _body.transform;

            playerTransform.SetParent(_snapshot.Parent, true);
            playerTransform.localScale = _snapshot.LocalScale;

            playerTransform.SetPositionAndRotation(
                _snapshot.Position,
                _snapshot.Rotation);

            _body.position = _snapshot.Position;
            _body.rotation = _snapshot.Rotation;

            RestoreState();

            if (!_body.isKinematic)
            {
                _body.linearVelocity = _snapshot.LinearVelocity;
                _body.angularVelocity = _snapshot.AngularVelocity;
            }

            Physics.SyncTransforms();

            IsAttached = false;
            return true;
        }

        public bool CanDetach(Transform exitAnchor)
        {
            if (IsAttached == false ||
                exitAnchor == null ||
                HasRequiredReferences() == false)
            {
                return false;
            }

            Physics.SyncTransforms();

            BuildExitCapsule(
                exitAnchor,
                out Vector3 pointA,
                out Vector3 pointB,
                out float radius);

            Collider[] overlaps = Physics.OverlapCapsule(
                pointA,
                pointB,
                radius,
                _exitBlockingLayers,
                QueryTriggerInteraction.Ignore);

            foreach (Collider overlap in overlaps)
            {
                if (overlap == null ||
                    IsPlayerCollider(overlap))
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        public bool TryDetach(Transform exitAnchor)
        {
            if (CanDetach(exitAnchor) == false)
                return false;

            DetachTo(exitAnchor);
            RestoreState();

            IsAttached = false;

            return true;
        }

        private void BuildExitCapsule(
            Transform exitAnchor,
            out Vector3 pointA,
            out Vector3 pointB,
            out float radius)
        {
            Transform bodyTransform = _body.transform;
            Transform colliderTransform = _collider.transform;

            Vector3 targetBodyScale =
                GetTargetBodyWorldScale();

            Vector3 currentBodyScale =
                Abs(bodyTransform.lossyScale);

            Vector3 colliderRelativeScale =
                Divide(
                    Abs(colliderTransform.lossyScale),
                    currentBodyScale);

            Vector3 targetColliderScale =
                Vector3.Scale(
                    targetBodyScale,
                    colliderRelativeScale);

            Vector3 currentWorldCenter =
                colliderTransform.TransformPoint(
                    _collider.center);

            Vector3 centerInBodySpace =
                bodyTransform.InverseTransformPoint(
                    currentWorldCenter);

            Vector3 targetWorldCenter =
                exitAnchor.position +
                exitAnchor.rotation *
                Vector3.Scale(
                    centerInBodySpace,
                    targetBodyScale);

            Quaternion colliderRelativeRotation =
                Quaternion.Inverse(bodyTransform.rotation) *
                colliderTransform.rotation;

            Quaternion targetColliderRotation =
                exitAnchor.rotation *
                colliderRelativeRotation;

            Vector3 localAxis =
                GetCapsuleLocalAxis(
                    _collider.direction);

            Vector3 worldAxis =
                targetColliderRotation *
                localAxis;

            worldAxis.Normalize();

            GetCapsuleScale(
                _collider.direction,
                targetColliderScale,
                out float axisScale,
                out float radiusScale);

            radius = Mathf.Max(
                0.01f,
                _collider.radius * radiusScale -
                _exitSkin);

            float height = Mathf.Max(
                radius * 2f,
                _collider.height * axisScale -
                _exitSkin * 2f);

            float segmentHalfLength =
                Mathf.Max(
                    0f,
                    height * 0.5f - radius);

            pointA =
                targetWorldCenter +
                worldAxis * segmentHalfLength;

            pointB =
                targetWorldCenter -
                worldAxis * segmentHalfLength;
        }

        private Vector3 GetTargetBodyWorldScale()
        {
            Vector3 parentScale =
                _snapshot.Parent != null
                    ? Abs(_snapshot.Parent.lossyScale)
                    : Vector3.one;

            return Vector3.Scale(
                parentScale,
                Abs(_snapshot.LocalScale));
        }

        private bool IsPlayerCollider(Collider other)
        {
            Transform bodyTransform = _body.transform;

            return other == _collider ||
                   other.transform == bodyTransform ||
                   other.transform.IsChildOf(
                       bodyTransform);
        }

        private static Vector3 GetCapsuleLocalAxis(int direction)
        {
            switch (direction)
            {
                case 0:
                    return Vector3.right;

                case 2:
                    return Vector3.forward;

                default:
                    return Vector3.up;
            }
        }

        private static void GetCapsuleScale(int direction, Vector3 scale, out float axisScale, out float radiusScale)
        {
            scale = Abs(scale);

            switch (direction)
            {
                case 0:
                    axisScale = scale.x;
                    radiusScale =
                        Mathf.Max(scale.y, scale.z);
                    break;

                case 2:
                    axisScale = scale.z;
                    radiusScale =
                        Mathf.Max(scale.x, scale.y);
                    break;

                default:
                    axisScale = scale.y;
                    radiusScale =
                        Mathf.Max(scale.x, scale.z);
                    break;
            }
        }

        private static Vector3 Abs(Vector3 value)
        {
            return new Vector3(
                Mathf.Abs(value.x),
                Mathf.Abs(value.y),
                Mathf.Abs(value.z));
        }

        private static Vector3 Divide(Vector3 value, Vector3 divisor)
        {
            return new Vector3(
                Divide(value.x, divisor.x),
                Divide(value.y, divisor.y),
                Divide(value.z, divisor.z));
        }

        private static float Divide(float value, float divisor)
        {
            if (Mathf.Approximately(divisor, 0f))
                return 1f;

            return value / divisor;
        }

        private bool HasRequiredReferences()
        {
            return _body != null && _collider != null;
        }

        private void CacheState()
        {
            Transform playerTransform = _body.transform;

            Vector3 linearVelocity = _body.isKinematic
                ? Vector3.zero
                : _body.linearVelocity;

            Vector3 angularVelocity = _body.isKinematic
                ? Vector3.zero
                : _body.angularVelocity;

            _snapshot = new PlayerStationBodySnapshot(
                playerTransform.parent,
                playerTransform.localScale,
                _body.position,
                _body.rotation,
                linearVelocity,
                angularVelocity,
                _body.isKinematic,
                _body.useGravity,
                _body.detectCollisions,
                _collider.enabled,
                _body.interpolation);
        }

        private void DisableWorldBody()
        {
            if (_body.isKinematic == false)
            {
                _body.linearVelocity = Vector3.zero;

                _body.angularVelocity = Vector3.zero;
            }

            _collider.enabled = false;
            _body.detectCollisions = false;
            _body.useGravity = false;
            _body.isKinematic = true;
            _body.interpolation = RigidbodyInterpolation.None;
        }

        private void AttachTo(Transform seatAnchor)
        {
            Transform playerTransform =_body.transform;

            playerTransform.SetParent(seatAnchor, false);

            playerTransform.localPosition = Vector3.zero;

            playerTransform.localRotation = Quaternion.identity;

            playerTransform.localScale = Vector3.one;
        }

        private void DetachTo(Transform exitAnchor)
        {
            Transform playerTransform = _body.transform;

            playerTransform.SetParent(_snapshot.Parent, true);

            playerTransform.localScale = _snapshot.LocalScale;

            playerTransform.SetPositionAndRotation(
                exitAnchor.position,
                exitAnchor.rotation);

            Physics.SyncTransforms();
        }

        private void RestoreState()
        {
            _body.useGravity = _snapshot.UseGravity;
            _body.detectCollisions = _snapshot.DetectCollisions;
            _collider.enabled = _snapshot.ColliderEnabled;
            _body.interpolation = _snapshot.Interpolation;            
            _body.isKinematic = _snapshot.IsKinematic;

            if (_body.isKinematic == false)
            {
                _body.linearVelocity = Vector3.zero;

                _body.angularVelocity = Vector3.zero;

                _body.WakeUp();
            }
        }

        private void Reset()
        {
            _body = GetComponentInParent<Rigidbody>();

            if (_body != null)
            {
                _collider = _body.GetComponentInChildren<CapsuleCollider>(true);
            }
        }

        public bool TryForceDetach(Transform exitAnchor)
        {
            if (IsAttached == false || HasRequiredReferences() == false)
                return false;

            Transform playerTransform = _body.transform;
            playerTransform.SetParent(_snapshot.Parent, true);
            playerTransform.localScale = _snapshot.LocalScale;

            if (exitAnchor != null)
            {
                playerTransform.SetPositionAndRotation(
                    exitAnchor.position,
                    exitAnchor.rotation);
            }

            RestoreState();
            IsAttached = false;
            Physics.SyncTransforms();

            return true;
        }
    }
}