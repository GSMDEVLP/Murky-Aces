using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Application;
using _Project.Develop.Runtime.Gameplay.Features.Enemies.KamikazeDrone.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Movement.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Turret.Domain;

static class GameplayStateChecks
{
    public static void Run(Action<string, Action> run)
    {
        run("Default motion snapshot is stationary and has no direction change", () => {
            var state = default(TankMotionState);
            Check(state.CurrentDirection == 0 && !state.IsChangingDirection &&
                state.PendingDirection == 0 && state.RemainingDirectionSwitchDelay == 0f);
        });
        run("Motion snapshot reports the sign of actual speed", () => {
            Check(new TankMotionState(-3f, 0f, false, 0, 0f).CurrentDirection == -1);
            Check(new TankMotionState(3f, 0f, false, -1, 0.5f).CurrentDirection == 1);
        });
        run("Motion snapshot rejects contradictory direction change state", () => {
            Throws<ArgumentException>(() => new TankMotionState(0f, 0f, false, 0, 1f));
            Throws<ArgumentException>(() => new TankMotionState(0f, 0f, true, 1, 0f));
            Throws<ArgumentOutOfRangeException>(() => new TankMotionState(0f, 0f, false, 2, 0f));
            Throws<ArgumentOutOfRangeException>(() => new TankMotionState(0f, 0f, false, -1, -1f));
        });
        run("Motion snapshot rejects non-finite measurements", () => {
            foreach (float value in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Throws<ArgumentOutOfRangeException>(() => new TankMotionState(value, 0f, false, 0, 0f));
                Throws<ArgumentOutOfRangeException>(() => new TankMotionState(0f, value, false, 0, 0f));
                Throws<ArgumentOutOfRangeException>(() => new TankMotionState(0f, 0f, false, 1, value));
            }
        });
        run("Turret snapshots remain stable when the mechanism advances", () => {
            var turret = Turret();
            var before = turret.State;
            turret.SetInput(1f, 1f);
            turret.Tick(0.5f, 0f);
            Near(before.CurrentYaw, 0f);
            Near(before.CurrentPitch, 0f);
            Near(turret.State.CurrentYaw, 30f);
            Near(turret.State.CurrentPitch, 15f);
        });
        run("Turret enforces pitch limits at initialization and while aiming", () => {
            var turret = new TurretMechanism(new TurretAimState(0f, 100f, 0f,
                TurretReferenceMode.HullRelative), 60f, 30f, -20f, 30f);
            Near(turret.State.CurrentPitch, 30f);
            turret.SetInput(0f, -1f);
            turret.Tick(10f, 0f);
            Near(turret.State.CurrentPitch, -20f);
            Near(turret.State.TargetPitch, -20f);
        });
        run("World-stabilized turret compensates for hull rotation", () => {
            var turret = new TurretMechanism(new TurretAimState(10f, 0f, 20f,
                TurretReferenceMode.WorldStabilized), 60f, 30f, -20f, 30f);
            turret.Tick(1f, 40f);
            Near(turret.State.WorldDirection, 30f);
            Near(turret.State.CurrentYaw, -10f);
        });
        run("Clearing turret input keeps its reached angles", () => {
            var turret = Turret();
            turret.SetInput(1f, 1f);
            turret.Tick(0.2f, 0f);
            var reached = turret.State;
            turret.ClearInput();
            turret.Tick(1f, 0f);
            Near(turret.State.CurrentYaw, reached.CurrentYaw);
            Near(turret.State.CurrentPitch, reached.CurrentPitch);
        });
        run("Switching turret reference mode keeps the current direction", () => {
            var turret = Turret();
            turret.SetInput(1f, 0f);
            turret.Tick(0.5f, 0f);
            var before = turret.State;
            turret.ToggleReferenceMode(10f);
            Check(turret.State.ReferenceMode == TurretReferenceMode.WorldStabilized);
            Near(turret.State.CurrentYaw, before.CurrentYaw);
            Near(turret.State.WorldDirection, before.CurrentYaw + 10f);
        });
        run("Turret rejects invalid angles and input without changing state", () => {
            Throws<ArgumentOutOfRangeException>(() => new TurretAimState(float.NaN, 0f, 0f, TurretReferenceMode.HullRelative));
            Throws<ArgumentOutOfRangeException>(() => new TurretAimState(0f, 0f, 0f, (TurretReferenceMode)99));
            var turret = Turret();
            Throws<ArgumentOutOfRangeException>(() => turret.SetInput(float.NaN, 0f));
            Throws<ArgumentOutOfRangeException>(() => turret.Tick(float.PositiveInfinity, 0f));
            Near(turret.State.CurrentYaw, 0f);
        });
        run("Drone snapshots remain stable when the patrol advances", () => {
            var drone = Drone();
            var before = drone.State;
            drone.Tick(0f, 0.5f);
            Near(before.Speed, 0f);
            Near(drone.State.Speed, 2f);
            Check(drone.State.Mode == KamikazeDroneMode.Patrolling);
        });
        run("Drone brakes before the leg ends and stops without overshooting", () => {
            var drone = Drone();
            const float step = 0.02f;
            for (int i = 0; i < 2000 && drone.State.Mode == KamikazeDroneMode.Patrolling; i++)
            {
                drone.Tick(drone.State.Speed * step, step);
                Check(drone.State.Speed >= 0f && drone.State.Speed <= 10f);
                Check(drone.State.DistanceTravelled <= 20.001f);
            }
            Check(drone.State.Mode == KamikazeDroneMode.Waiting);
            Near(drone.State.Speed, 0f);
        });
        run("Drone waits then starts a fresh leg with a bounded turn", () => {
            var drone = Drone();
            drone.Tick(20f, 0.1f);
            var waiting = drone.State;
            Check(waiting.Mode == KamikazeDroneMode.Waiting);
            drone.Tick(0f, 0.2f);
            Check(drone.State.Mode == KamikazeDroneMode.Waiting);
            Near(drone.State.PauseRemaining, 0.3f);
            drone.Tick(0f, 0.4f);
            Check(drone.State.Mode == KamikazeDroneMode.Patrolling);
            Near(drone.State.DistanceTravelled, 0f);
            Near(drone.State.PauseRemaining, 0f);
            Check(Math.Abs(drone.State.HeadingOffsetDegrees) <= 90f);
            Near(waiting.PauseRemaining, 0.5f);
        });
        run("Drone rejects invalid samples before altering its state", () => {
            var drone = Drone();
            Throws<ArgumentOutOfRangeException>(() => drone.Tick(float.NaN, 0.1f));
            Throws<ArgumentOutOfRangeException>(() => drone.Tick(0f, float.PositiveInfinity));
            Throws<ArgumentException>(() => new KamikazeDroneBehaviour(float.PositiveInfinity, 4f, 5f, 20f, 0.5f, new Random(1)));
            Near(drone.State.DistanceTravelled, 0f);
            Near(drone.State.Speed, 0f);
        });
        run("Drone snapshot rejects contradictory patrol state", () => {
            Throws<ArgumentException>(() => new KamikazeDroneState(KamikazeDroneMode.Waiting, 1f, 20f, 0.5f, 0f));
            Throws<ArgumentException>(() => new KamikazeDroneState(KamikazeDroneMode.Patrolling, 1f, 0f, 0.5f, 0f));
            Throws<ArgumentOutOfRangeException>(() => new KamikazeDroneState(KamikazeDroneMode.Patrolling, -1f, 0f, 0f, 0f));
        });
    }

    static TurretMechanism Turret() => new(new TurretAimState(0f, 0f, 0f,
        TurretReferenceMode.HullRelative), 60f, 30f, -20f, 30f);

    static KamikazeDroneBehaviour Drone() => new(10f, 4f, 5f, 20f, 0.5f, new Random(1));

    static void Near(float actual, float expected)
    {
        if (Math.Abs(actual - expected) > 0.001f)
            throw new Exception($"Expected {expected}, got {actual}.");
    }

    static void Check(bool value)
    {
        if (!value) throw new Exception("Assertion failed.");
    }

    static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new Exception("Expected " + typeof(T).Name);
    }
}
