using System;
using Unity.Collections;
using UnityEngine;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    [DisallowMultipleComponent]
    public sealed class DroneCollisionSensor : MonoBehaviour
    {
        private Collider _collider;
        private Rigidbody _rigidbody;
        private bool _originalProvidesContacts;
        private bool _hasContact;
        private bool _isMonitoring;

        public void StartMonitoring()
        {
            if (_isMonitoring)
                return;

            if (enabled == false)
            {
                throw new InvalidOperationException(
                    "Drone collision sensor must be enabled.");
            }

            _collider = GetComponent<Collider>();

            if (_collider == null || _collider.isTrigger)
                throw new InvalidOperationException(
                    "Drone collision sensor requires a solid collider.");

            _rigidbody = _collider.attachedRigidbody;

            if (_rigidbody == null)
            {
                throw new InvalidOperationException(
                    "Drone collision sensor requires an attached Rigidbody.");
            }

            _originalProvidesContacts = _collider.providesContacts;
            _isMonitoring = true;

            _collider.providesContacts = true;
            _hasContact = false;

            Physics.ContactEvent += HandlePhysicsContacts;
        }

        private void OnDisable()
        {
            StopMonitoring();
        }

        private void OnDestroy()
        {
            StopMonitoring();
        }

        public void StopMonitoring()
        {
            _hasContact = false;

            if (_isMonitoring == false)
                return;

            _isMonitoring = false;

            Physics.ContactEvent -= HandlePhysicsContacts;

            if (_collider != null)
                _collider.providesContacts = _originalProvidesContacts;

            _rigidbody = null;
        }

        public bool ConsumeContact()
        {
            bool hasContact = _hasContact;
            _hasContact = false;
            return hasContact;
        }

        public void ClearContact()
        {
            _hasContact = false;
        }

        private void HandlePhysicsContacts(
            PhysicsScene scene,
            NativeArray<ContactPairHeader>.ReadOnly pairHeaders)
        {
            if (_isMonitoring == false ||
                isActiveAndEnabled == false ||
                _collider == null)
                return;

            for (int i = 0; i < pairHeaders.Length; i++)
            {
                var header = pairHeaders[i];

                if (header.body != _rigidbody && header.otherBody != _rigidbody)
                    continue;

                for (int j = 0; j < header.pairCount; j++)
                {
                    var pair = header.GetContactPair(j);

                    if (!pair.isCollisionEnter && !pair.isCollisionStay)
                        continue;

                    if (pair.collider == _collider || pair.otherCollider == _collider)
                    {
                        _hasContact = true;
                        return;
                    }
                }
            }
        }
    }
}
