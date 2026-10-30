using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using Zenject;
using _Project.Develop.Runtime.Core.GameLoop.Application;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Combat.Targeting.Infrastructure;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Behaviour.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Targeting.Infrastructure;

namespace _Project.Develop.Editor.Enemies.KamikazeDrone
{
    [CustomEditor(typeof(DroneDetectionInstaller))]
    public sealed class DroneDetectionInstallerEditor : UnityEditor.Editor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();

            InspectorElement.FillDefaultInspector(root, serializedObject, this);

            var report = new Label();
            root.Add(report);

            RefreshReport(report);
            root.schedule.Execute(() => RefreshReport(report)).Every(200);

            return root;
        }

        private void RefreshReport(Label report)
        {
            if (target == null || !EditorApplication.isPlaying)
            {
                report.text = "\nDetection diagnostics: enter Play Mode.";
                return;
            }

            var installer = (DroneDetectionInstaller)target;
            var context =
                installer.GetComponentInParent<GameObjectContext>(true);

            if (context == null || context.Container == null)
            {
                report.text = "\nDrone context is not initialized.";
                return;
            }

            var scanner =
                context.Container.TryResolve<DroneTargetScanner>();
            var selector =
                context.Container.TryResolve<DroneTargetSelector>();    
            var registry =
                context.Container.TryResolve<CombatTargetRegistry>();
            var gameLoop =
                context.Container.TryResolve<GameLoopRegistry>();

            if (scanner == null || registry == null || gameLoop == null || selector == null)
            {
                report.text = "\nDetection dependencies are not available.";
                return;
            }

            bool registered = gameLoop.IsGameplayRegistered(scanner);
            
            ICombatTarget selected = selector.CurrentTarget;
            var behaviour = context.Container.TryResolve<DroneBehaviour>();
            var destination = behaviour?.AttackDestination;
            
            report.text =
                $"\nScanner registered in GameLoop: {registered}\n" +
                $"\nRegistered targets: {registry.Count}\n" +
                DescribeTargets(registry.Targets) +
                $"\n\nDetected targets: {scanner.DetectedTargets.Count}\n" +
                DescribeTargets(scanner.DetectedTargets) +
                "\n\nCurrent target: " +
                (selected != null ? DescribeTarget(selected) : "None") +
                "\nAttack destination: " +
                (destination.HasValue
                    ? destination.Value.ToString("F2")
                    : "None") +
                "\nAttack point reached: " +
                (behaviour != null && behaviour.HasReachedAttackPoint);
        }

        private static string DescribeTargets(
            IEnumerable<ICombatTarget> targets)
        {
            return string.Join("\n", targets.Select(DescribeTarget));
        }

        private static string DescribeTarget(ICombatTarget target)
        {
            string name = target is CombatTarget component && component != null
                ? component.name
                : "<unavailable object>";

            return $"{name} | {target.Kind} | ID: {target.TargetId}";
        }
    }
}
