using _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Application;
using _Project.Develop.Runtime.Gameplay.Features.Interaction.Domain;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Application;
using _Project.Develop.Runtime.Gameplay.Features.Tank.Exterior.Domain;

static class Program
{
    static int failures;
    static int passed;

    static int Main()
    {
        Run("Single Press executes directly once", () => {
            var f = new Fixture(); var a=f.Add(InteractionActionId.Primary);
            f.Frame(press:true,held:true); f.Frame(held:true);
            Check(a.Begins==1 && a.Completes==1 && !f.Model.IsChoosingAction);
        });
        Run("Single Hold completes only after required duration", () => {
            var f=new Fixture(); var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true,dt:0.3f); f.Frame(held:true,dt:0.3f);
            Check(f.Model.IsHolding && a.Completes==0);
            f.Frame(held:true,dt:0.5f); f.Frame(held:true);
            Check(!f.Model.IsHolding && a.Completes==1);
        });
        Run("Release cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Frame(released:true);
            Check(a.Cancels==1 && a.Completes==0 && !f.Model.IsHolding);
        });
        Run("Losing focus cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true); f.Finder.Target=null; f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Disabled target cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Target.IsActive=false;f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Disable and re-enable generation invalidates Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Target.Revision++;f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Action replacement with same ID cannot inherit Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Target.Actions.Clear();
            var replacement=f.Add(InteractionActionId.Primary,hold:true);f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && replacement.Completes==0);
        });
        Run("Action revision change cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);a.Version++;f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Action provider lifecycle change cancels Hold on a composite target", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);a.Generation++;f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Changing Hold meaning cannot complete another action", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);a.Prompt=InteractionPromts.Exit;f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Changing duration cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);a.Duration=0.2f;f.Frame(held:true);
            Check(a.Cancels==1 && a.Completes==0);
        });
        Run("Multiple actions open menu without execution", () => {
            var f=MenuFixture(out var enter,out var remove);f.Frame(press:true,held:true);
            Check(f.Model.IsChoosingAction && f.Input.MenuOpen && enter.Begins==0 && remove.Begins==0);
        });
        Run("Opening key must be released before confirmation", () => {
            var f=MenuFixture(out var enter,out var remove);f.Frame(press:true,held:true);
            f.Frame(press:true,held:true,dt:3);
            Check(enter.Begins==0 && remove.Begins==0 && f.Model.IsChoosingAction);
        });
        Run("Selected Press executes without opening another menu", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(released:true);f.Frame(nav:1);
            f.Frame(press:true,held:true);
            Check(remove.Completes==1 && enter.Begins==0 && !f.Model.IsChoosingAction && !f.Input.MenuOpen);
        });
        Run("Selected Hold progresses on same target with multiple actions", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(released:true);f.Frame(press:true,held:true,dt:0.4f);
            Check(f.Model.IsHolding && !f.Model.IsChoosingAction && enter.Completes==0);
            f.Frame(held:true,dt:0.7f);
            Check(enter.Completes==1 && remove.Completes==0 && !f.Model.IsHolding);
        });
        Run("Cancel closes menu and restores player control", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(cancel:true);
            Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen && enter.Begins==0 && remove.Begins==0);
        });
        Run("Losing target closes menu without executing", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Finder.Target=null;f.Frame();
            Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen && enter.Begins==0);
        });
        Run("Disappearing menu option never triggers remaining option", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(released:true);
            remove.Available=false;f.Frame(press:true,held:true);
            Check(!f.Model.IsChoosingAction && enter.Begins==0 && remove.Begins==0);
        });
        Run("Changing selected object revision invalidates menu", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(released:true);remove.Version++;
            f.Frame(press:true,held:true);
            Check(!f.Model.IsChoosingAction && remove.Begins==0 && enter.Begins==0);
        });
        Run("Input context transition cancels menu", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Input.ContextRevision++;f.Frame();
            Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen);
        });
        Run("Action provider lifecycle change invalidates menu", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Frame(released:true);remove.Generation++;
            f.Frame(press:true,held:true);
            Check(!f.Model.IsChoosingAction && remove.Begins==0 && enter.Begins==0);
        });
        Run("Gameplay disabling cancels Hold", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Control.SetGameplayEnabled(false);
            Check(a.Cancels==1 && !f.Model.IsHolding && !f.Model.HasFocusedTarget);
            f.Frame(held:true,dt:5);
            Check(a.Cancels==1 && a.Completes==0 && !f.Model.HasFocusedTarget);
        });
        Run("Gameplay disabling releases menu input lock", () => {
            var f=MenuFixture(out var enter,out var remove);
            f.Frame(press:true,held:true);f.Control.SetGameplayEnabled(false);
            Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen);
        });
        Run("Scope transition cancels active interaction", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Control.TryRestrictTargetScope(new DenyScope());
            Check(a.Cancels==1 && !f.Model.IsHolding && !f.Model.HasFocusedTarget);
            f.Frame(held:true,dt:2);
            Check(a.Cancels==1 && a.Completes==0 && !f.Model.HasFocusedTarget);
        });
        Run("Control configured before construction is respected", () => {
            var input=new Input();var actor=new Actor();var target=new Target();
            var action=new FakeAction{Id=InteractionActionId.Primary};target.Actions.Add(action);
            var control=new InteractionControl();control.SetGameplayEnabled(false);
            using var model=new PlayerInteraction(input,new Finder{Target=target},new InteractionContext(actor),control);
            input.Frame(true,true,false,0,false,false);model.Tick(1);
            Check(!model.IsGameplayEnabled && !model.HasFocusedTarget && action.Begins==0);
            control.TryRestrictTargetScope(new DenyScope());control.SetGameplayEnabled(true);model.Tick(1);
            Check(model.IsGameplayEnabled && !model.HasFocusedTarget && action.Begins==0);
        });
        Run("Unchanged control settings do not interrupt Hold", () => {
            var f=new Fixture();var scope=new AllowScope();Check(f.Control.TryRestrictTargetScope(scope));
            var a=f.Add(InteractionActionId.Primary,hold:true);f.Frame(press:true,held:true);
            f.Control.SetGameplayEnabled(true);Check(f.Control.TryRestrictTargetScope(scope));
            Check(!f.Control.TryRestrictTargetScope(null));
            Check(f.Model.IsHolding && a.Cancels==0);
            f.Frame(held:true,dt:1);Check(a.Completes==1);
        });
        Run("Clearing an active scope cancels Hold immediately", () => {
            var f=new Fixture();f.Control.TryRestrictTargetScope(new AllowScope());
            var a=f.Add(InteractionActionId.Primary,hold:true);f.Frame(press:true,held:true);
            f.Control.ClearTargetScope();f.Control.ClearTargetScope();
            Check(a.Cancels==1 && !f.Model.IsHolding && !f.Model.HasFocusedTarget);
        });
        Run("Interaction resumes after control is restored", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Control.SetGameplayEnabled(false);f.Control.SetGameplayEnabled(false);
            f.Control.SetGameplayEnabled(true);f.Frame(released:true);
            f.Frame(press:true,held:true,dt:1.1f);
            Check(a.Cancels==1 && a.Completes==1 && f.Model.IsGameplayEnabled);
        });
        Run("Interaction controls belong to individual players", () => {
            var f=MenuFixture(out var enter,out var remove);var other=new Fixture(f.Target,actorId:2);
            f.Frame(press:true,held:true);other.Frame(press:true,held:true);
            f.Control.SetGameplayEnabled(false);
            Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen);
            Check(other.Model.IsChoosingAction && other.Input.MenuOpen && other.Model.IsGameplayEnabled);
        });
        Run("Control disable and restore during Begin invalidates the action", () => {
            foreach(bool hold in new[]{false,true})
            {
                var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold);
                a.OnBegin=()=>{f.Control.SetGameplayEnabled(false);f.Control.SetGameplayEnabled(true);};
                f.Frame(press:true,held:true,dt:2);
                Check(a.Completes==0 && a.Cancels==1 && !f.Model.IsHolding && !f.Model.HasFocusedTarget);
            }
        });
        Run("Scope replacement during Begin invalidates the action even when allowed", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);
            a.OnBegin=()=>f.Control.TryRestrictTargetScope(new AllowScope());
            f.Frame(press:true,held:true);
            Check(a.Completes==0 && a.Cancels==1);
        });
        Run("Failed Begin cannot restore focus after control disables gameplay", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);a.AllowBegin=false;
            a.OnBegin=()=>f.Control.SetGameplayEnabled(false);
            f.Frame(press:true,held:true);
            Check(!f.Model.IsGameplayEnabled && !f.Model.HasFocusedTarget && a.Completes==0);
        });
        Run("Disposing interaction cancels Hold once and stops further execution", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Frame(press:true,held:true);f.Model.Dispose();f.Model.Dispose();
            f.Control.SetGameplayEnabled(false);f.Control.SetGameplayEnabled(true);
            f.Frame(press:true,held:true,dt:5);
            Check(a.Cancels==1 && a.Begins==1 && a.Completes==0 && !f.Model.IsGameplayEnabled && !f.Model.HasFocusedTarget);
        });
        Run("Disposing interaction releases the menu input lock", () => {
            var f=MenuFixture(out var enter,out var remove);f.Frame(press:true,held:true);
            f.Model.Dispose();Check(!f.Model.IsChoosingAction && !f.Input.MenuOpen);
        });
        Run("Failed Begin never completes", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);a.AllowBegin=false;
            f.Frame(press:true,held:true);Check(a.Completes==0 && !f.Model.IsHolding);
        });
        Run("Completion may disable gameplay reentrantly", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);
            a.OnComplete=()=>f.Control.SetGameplayEnabled(false);
            f.Frame(press:true,held:true);
            Check(a.Completes==1 && !f.Model.IsGameplayEnabled && !f.Model.HasFocusedTarget);
        });
        Run("Press cannot complete after Begin disables its target", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);
            a.OnBegin=()=>f.Target.IsActive=false;
            f.Frame(press:true,held:true);
            Check(a.Completes==0 && a.Cancels==1);
        });
        Run("Press cannot complete after Begin changes provider lifecycle", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary);
            a.OnBegin=()=>a.Generation++;
            f.Frame(press:true,held:true);
            Check(a.Completes==0 && a.Cancels==1);
        });
        Run("Menus belong to individual players", () => {
            var f=MenuFixture(out var enter,out var remove);
            var other=new Fixture(f.Target,actorId:2);
            f.Frame(press:true,held:true);other.Frame(press:true,held:true);
            f.Frame(released:true,nav:1);
            Check(f.Model.SelectedActionIndex==1 && other.Model.SelectedActionIndex==0);
            f.Frame(cancel:true);
            Check(other.Model.IsChoosingAction && other.Input.MenuOpen);
        });
        Run("Duplicate action IDs are rejected", () => {
            var f=new Fixture();f.Add(InteractionActionId.Primary);f.Add(InteractionActionId.Primary);
            Throws<InvalidOperationException>(()=>f.Frame());
        });
        Run("Non-finite Hold durations are rejected", () => {
            Throws<ArgumentOutOfRangeException>(()=>InteractionInfo.Hold(InteractionPromts.Hold,float.NaN));
            Throws<ArgumentOutOfRangeException>(()=>InteractionInfo.Hold(InteractionPromts.Hold,float.PositiveInfinity));
        });
        Run("Drop cancels active interaction before releasing item", () => {
            var f=new Fixture();var a=f.Add(InteractionActionId.Primary,hold:true);
            f.Actor.HeldItem=new CarryItem { IsHeld=true };
            f.Frame(press:true,held:true);f.Frame(drop:true);
            Check(a.Cancels==1 && f.Actor.HeldItem==null && !f.Model.IsHolding);
        });
        Run("Installation failure restores attachment and hand ownership", () => {
            var actor=new Actor();var item=new CarryItem{IsHeld=true};actor.HeldItem=item;actor.AllowRelease=false;
            var attachment=new ExteriorAttachmentState(ExteriorMountType.TurretHatch,true);
            var slot=new ExteriorMountSlotState(ExteriorMountType.TurretHatch);
            bool restored=false;
            var command=new ExteriorInstallationCommand(actor,item,attachment,slot,()=>true,()=>{},()=>{restored=true;return true;});
            Check(!command.TryExecute() && restored && !attachment.IsInstalled && !slot.IsOccupied && ReferenceEquals(actor.HeldItem,item));
            Check(!command.TryExecute());
        });
        Run("Removal failure rolls back mounting", () => {
            var actor=new Actor{AllowReceive=false};var item=new CarryItem();
            var attachment=new ExteriorAttachmentState(ExteriorMountType.TurretHatch,true);
            var slot=new ExteriorMountSlotState(ExteriorMountType.TurretHatch);Check(attachment.TryInstall(slot));
            bool restored=false;
            var command=new ExteriorRemovalCommand(actor,item,attachment,slot,()=>true,()=>{},()=>{restored=true;return true;});
            Check(!command.TryExecute() && restored && attachment.IsInstalled && ReferenceEquals(slot.Attachment,attachment));
        });
        Run("Welded attachment cannot be removed", () => {
            var actor=new Actor();var item=new CarryItem();
            var attachment=new ExteriorAttachmentState(ExteriorMountType.TurretHatch,true);
            var slot=new ExteriorMountSlotState(ExteriorMountType.TurretHatch);Check(attachment.TryInstall(slot));
            var weld=new ExteriorWeldingCommand(attachment,slot,()=>true);Check(weld.TryExecute() && !weld.TryExecute());
            var removal=new ExteriorRemovalCommand(actor,item,attachment,slot,()=>true,()=>{},()=>true);
            Check(!removal.TryExecute() && attachment.IsSecured);
        });
        Run("Stale welding command cannot weld a reinstalled item", () => {
            var attachment=new ExteriorAttachmentState(ExteriorMountType.TurretHatch,true);
            var slot=new ExteriorMountSlotState(ExteriorMountType.TurretHatch);Check(attachment.TryInstall(slot));
            var weld=new ExteriorWeldingCommand(attachment,slot,()=>true);
            Check(attachment.TryRemove(slot) && attachment.TryInstall(slot));
            Check(!weld.TryExecute() && !attachment.IsSecured);
        });
        GameplayStateChecks.Run(Run);
        ActorDependencyChecks.Run(Run);
        Console.WriteLine($"RESULT: {passed} passed, {failures} failed.");
        return failures==0?0:1;
    }

    static Fixture MenuFixture(out FakeAction enter,out FakeAction remove)
    {
        var f=new Fixture();enter=f.Add(InteractionActionId.Primary,hold:true);remove=f.Add(InteractionActionId.Remove);
        return f;
    }
    static void Run(string name,Action test)
    {
        try {test();passed++;Console.WriteLine("PASS "+name);}
        catch(Exception e) {failures++;Console.WriteLine("FAIL "+name+": "+e.Message);}
    }
    static void Check(bool value) {if(!value)throw new Exception("Assertion failed.");}
    static void Throws<T>(Action test) where T:Exception
    {
        try{test();}catch(T){return;}throw new Exception("Expected "+typeof(T).Name);
    }
    sealed class Fixture
    {
        public readonly Input Input=new();
        public readonly Actor Actor;
        public readonly Target Target;
        public readonly Finder Finder;
        public readonly InteractionControl Control = new();
        public readonly PlayerInteraction Model;
        public Fixture(Target target=null,ulong actorId=1)
        {
            Actor=new Actor{Id=actorId};Target=target??new Target();Finder=new Finder{Target=Target};
            Model=new PlayerInteraction(Input,Finder,new InteractionContext(Actor),Control);
        }
        public FakeAction Add(InteractionActionId id,bool hold=false)
        {
            var a=new FakeAction{Id=id,Hold=hold};Target.Actions.Add(a);return a;
        }
        public void Frame(bool press=false,bool held=false,bool released=false,int nav=0,bool cancel=false,bool drop=false,float dt=0.1f)
        {
            Input.Frame(press,held,released,nav,cancel,drop);Model.Tick(dt);
        }
    }
    sealed class Input:IInteractionInput
    {
        bool suppressed;
        public long ContextRevision{get;set;}
        public bool InteractPressedThisFrame{get;private set;}
        public bool InteractHeld{get;private set;}
        public bool InteractReleasedThisFrame{get;private set;}
        public bool DropPressedThisFrame{get;private set;}
        public int InteractionMenuNavigation{get;private set;}
        public bool InteractionMenuCancelPressed{get;private set;}
        public bool MenuOpen{get;private set;}
        public void SetInteractionMenuOpen(bool value)=>MenuOpen=value;
        public void SuppressInteractionUntilReleased()
        {
            suppressed=true;ContextRevision++;InteractPressedThisFrame=InteractHeld=InteractReleasedThisFrame=false;
        }
        public void Frame(bool press,bool held,bool released,int nav,bool cancel,bool drop)
        {
            InteractPressedThisFrame=!suppressed&&press;InteractHeld=!suppressed&&held;InteractReleasedThisFrame=!suppressed&&released;
            if(suppressed&&!held)suppressed=false;
            InteractionMenuNavigation=nav;InteractionMenuCancelPressed=cancel;DropPressedThisFrame=drop;
        }
    }
    sealed class Actor:IInteractionActor,IPickupReceiver
    {
        public ulong Id{get;set;}=1;
        public IHoldableItem HeldItem{get;set;}
        public bool IsOccupied=>HeldItem!=null;
        public bool AllowReceive=true,AllowRelease=true;
        public bool TryReceive(IHoldableItem item)
        {
            if(!AllowReceive||IsOccupied)return false;
            HeldItem=item;return item.TryHold(Id,null);
        }
        public bool TryDrop(){if(HeldItem==null)return false;HeldItem=null;return true;}
        public bool TryReleaseHeldItem(IHoldableItem expected)
        {
            if(!AllowRelease||!ReferenceEquals(HeldItem,expected))return false;
            expected.TryEndCarry(Id);HeldItem=null;return true;
        }
    }
    sealed class CarryItem:IHoldableItem
    {
        public bool IsHeld{get;set;}
        public bool TryHold(ulong id,UnityEngine.Transform anchor){IsHeld=true;return true;}
        public bool TryDrop(ulong id,UnityEngine.Vector3 position,UnityEngine.Vector3 impulse){IsHeld=false;return true;}
        public bool TryEndCarry(ulong id){IsHeld=false;return true;}
    }
    sealed class Target:IInteractionTarget
    {
        public bool IsActive{get;set;}=true;
        public long Revision{get;set;}
        public readonly List<IInteractionAction> Actions=new();
        public void CollectActions(in InteractionContext context,List<IInteractionAction> actions)=>actions.AddRange(Actions);
    }
    sealed class Finder:IInteractionTargetFinder
    {
        public IInteractionTarget Target;
        public bool TryFindTarget(IInteractionScope scope,in InteractionContext context,out IInteractionTarget target)
        {
            target=Target;
            return target!=null&&target.IsActive&&(scope==null||scope.Allows(target));
        }
    }
    sealed class DenyScope:IInteractionScope {public bool Allows(IInteractionTarget target)=>false;}
    sealed class AllowScope:IInteractionScope {public bool Allows(IInteractionTarget target)=>true;}
    sealed class FakeAction:IInteractionAction
    {
        public InteractionActionId Id{get;set;}
        public InteractionPromts Prompt=InteractionPromts.Enter;
        public bool Available=true,Hold,AllowBegin=true;
        public float Duration=1;
        public long Version;
        public long Generation;
        public long Revision=>Generation;
        public int Begins,Completes,Cancels;
        public Action OnComplete;
        public Action OnBegin;
        public InteractionInfo GetInteractionInfo(in InteractionContext context)=>Hold
            ?InteractionInfo.Hold(Prompt,Duration,Available,Version):InteractionInfo.Press(Prompt,Available,Version);
        public bool Begin(in InteractionContext context){Begins++;OnBegin?.Invoke();return Available&&AllowBegin;}
        public void Complete(in InteractionContext context){Completes++;OnComplete?.Invoke();}
        public void Cancel(in InteractionContext context,InteractionCancelReason reason)=>Cancels++;
    }
}
