using System;
using UnityEngine;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Application;

namespace _Project.Develop.Runtime.Gameplay.Features.Tank.Weapon.Infrastructure
{
    public sealed class RigidbodyGunLauncher : MonoBehaviour, IGunLauncher
    {
        private Transform _ownerRoot;

        public void InitializeOwner(Transform ownerRoot)
        {
            _ownerRoot = ownerRoot != null
                ? ownerRoot
                : throw new ArgumentNullException(nameof(ownerRoot));
        }

        public bool TryLaunch(in GunShotRequest request)
        {
            if (!isActiveAndEnabled || _ownerRoot == null)
                return false;

            var shell = request.Shell;

            if (shell == null || !shell.HasValidProjectileSetup)
                return false;

            if (!shell.ProjectilePrefab.activeSelf)
                return false;

            GameObject projectile = null;

            try
            {
                projectile = Instantiate(
                    shell.ProjectilePrefab,
                    request.MuzzlePose.position,
                    request.MuzzlePose.rotation);

                Rigidbody body = projectile.GetComponent<Rigidbody>();

                Collider[] colliders =
                    projectile.GetComponentsInChildren<Collider>(true);

                if (body == null || colliders.Length != 1)
                {
                    return Reject(
                        projectile,
                        "Projectile needs a root Rigidbody and one collider.");
                }

                Collider projectileCollider = colliders[0];

                bool supportedCollider =
                    projectileCollider is BoxCollider ||
                    projectileCollider is SphereCollider ||
                    projectileCollider is CapsuleCollider;

                if (!supportedCollider ||
                    !projectileCollider.enabled ||
                    !projectileCollider.gameObject.activeInHierarchy ||
                    projectileCollider.isTrigger ||
                    projectileCollider.attachedRigidbody != body)
                {
                    return Reject(
                        projectile,
                        "Projectile collider setup is invalid.");
                }

                body.isKinematic = false;
                body.mass = shell.ProjectileMass;
                body.useGravity = true;
                body.detectCollisions = true;
                body.interpolation = RigidbodyInterpolation.Interpolate;
                body.collisionDetectionMode =
                    CollisionDetectionMode.ContinuousDynamic;

                Collider[] ownerColliders =
                    _ownerRoot.GetComponentsInChildren<Collider>(true);

                foreach (Collider ownerCollider in ownerColliders)
                {
                    if (!ownerCollider.enabled ||
                        !ownerCollider.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    Physics.IgnoreCollision(
                        projectileCollider,
                        ownerCollider,
                        true);
                }

                Vector3 direction =
                    request.MuzzlePose.rotation * Vector3.forward;

                body.linearVelocity =
                    direction * shell.MuzzleSpeed +
                    request.InheritedTankVelocity;

                body.angularVelocity = Vector3.zero;

                Destroy(projectile, shell.ProjectileLifetime);
                return true;
            }
            catch (Exception exception)
            {
                RemoveFailedProjectile(projectile);
                Debug.LogException(exception, this);
                return false;
            }
        }

        private bool Reject(GameObject projectile, string reason)
        {
            RemoveFailedProjectile(projectile);
            Debug.LogWarning(reason, this);
            return false;
        }

        private static void RemoveFailedProjectile(GameObject projectile)
        {
            if (projectile == null)
                return;

            projectile.SetActive(false);
            Destroy(projectile);
        }
    }
}