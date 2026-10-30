using System;
using UnityEngine;
using Zenject;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Core.Infrastructure;

namespace _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Attack.Infrastructure
{
    public sealed class DroneAttackInstaller : MonoInstaller
    {
        [SerializeField] private DroneDetonationBody _body;
        [SerializeField] private DroneCollisionSensor _collisionSensor;

        public override void InstallBindings()
        {
            var context = GetComponentInParent<GameObjectContext>(true);
            var root = context != null
                ? context.GetComponent<KamikazeDroneRoot>()
                : null;

            if (root == null ||
                _body == null ||
                _body.DroneRoot != root ||
                !_body.transform.IsChildOf(root.transform) ||
                _body.gameObject == root.gameObject)
            {
                throw new InvalidOperationException(
                    "Drone detonation body must be a child of its context root.");
            }

            Rigidbody rigidbody = root.GetComponent<Rigidbody>();
            Collider collider = _collisionSensor != null
                ? _collisionSensor.GetComponent<Collider>()
                : null;

            if (rigidbody == null ||
                collider == null ||
                collider.isTrigger ||
                collider.GetComponentInParent<Rigidbody>(true) != rigidbody ||
                !collider.transform.IsChildOf(root.transform))
            {
                throw new InvalidOperationException(
                    "Drone collision sensor must share a solid collider " +
                    "belonging to the drone Rigidbody.");
            }

            Container.Bind<IDroneDetonationBody>().FromInstance(_body).AsSingle();
            Container.Bind<DroneCollisionSensor>().FromInstance(_collisionSensor).AsSingle();
            Container.Bind<DroneAttack>().AsSingle();
            Container.Bind<IDroneRuntimeModule>().To<DroneAttackModule>().AsSingle();
        }
    }
}
