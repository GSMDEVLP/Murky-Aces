using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Player.Domain;
using GameLoopFixedTickable = _Project.Develop.Runtime.Core.GameLoop.Abstractions.IFixedTickable;

namespace _Project.Develop.Runtime.Gameplay.Features.Player.Infrastructure.Movement
{
    public class PlayerMovement : MonoBehaviour, GameLoopFixedTickable
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private CapsuleCollider _capsule;
        [SerializeField] private float _walkSpeed = 4f;
        [SerializeField] private float _sprintSpeed = 7f;
        [SerializeField] private float _jumpHeight = 1.5f;
        [SerializeField] private float _groundCheckDistance = 0.1f;

        private PlayerIntentBuffer _intent;
        private PlayerLook _look;
        private bool _gameplayEnabled = true;

        public bool IsGameplayEnabled => _gameplayEnabled;

        [Inject]
        public void Construct(PlayerIntentBuffer intent, PlayerLook look)
        {
            _intent = intent;
            _look = look;
        }

        public void FixedTick(float fixedDeltaTime)
        {
            if (!_gameplayEnabled)
                return;

            Vector3 velocity = PlayerMove();
            velocity = PlayerJump(velocity);
            _rb.linearVelocity = velocity;
        }

        private Vector3 PlayerJump(Vector3 velocity)
        {
            bool jumpRequested = _intent.ConsumeJump();
            if (jumpRequested && IsGrounded())
                velocity.y = Mathf.Sqrt(
                    2f * Mathf.Abs(Physics.gravity.y) * _jumpHeight);
            return velocity;
        }

        private Vector3 PlayerMove()
        {
            Vector2 move = _intent.Move;
            float speed = _intent.SprintHeld ? _sprintSpeed : _walkSpeed;

            Vector3 direction = _look.Heading *
                new Vector3(move.x, 0f, move.y);

            Vector3 velocity = _rb.linearVelocity;
            velocity.x = direction.x * speed;
            velocity.z = direction.z * speed;
            
            return velocity;
        }

        private bool IsGrounded()
        {
            if (_rb.linearVelocity.y > 0.1f)
                return false;

            Bounds bounds = _capsule.bounds;
            return Physics.Raycast(
                    bounds.center,
                    Vector3.down,
                    out RaycastHit hit,
                    bounds.extents.y + _groundCheckDistance,
                    Physics.DefaultRaycastLayers,
                    QueryTriggerInteraction.Ignore)
                && hit.rigidbody != _rb
                && hit.normal.y > 0.6f;
        }

        public void SetGameplayEnabled(bool isEnabled)
        {
            if (_gameplayEnabled == isEnabled)
                return;

            _gameplayEnabled = isEnabled;

            if (!isEnabled)
                _intent.ConsumeJump();
        }
    }
}
