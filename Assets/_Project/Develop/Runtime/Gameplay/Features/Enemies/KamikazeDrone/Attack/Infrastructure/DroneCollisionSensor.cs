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

        private void OnEnable()
        {
            _collider = GetComponent<Collider>();

            if (_collider == null || _collider.isTrigger)
                throw new InvalidOperationException(
                    "Drone collision sensor requires a solid collider.");

            _rigidbody = _collider.attachedRigidbody;
            _originalProvidesContacts = _collider.providesContacts;
            _collider.providesContacts = true;
            _hasContact = false;

            Physics.ContactEvent += HandlePhysicsContacts;
        }

        private void OnDisable()
        {
            Physics.ContactEvent -= HandlePhysicsContacts;
            _hasContact = false;

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
            if (!isActiveAndEnabled || _collider == null)
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
