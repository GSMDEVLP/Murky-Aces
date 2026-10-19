namespace _Project.Develop.Runtime.Gameplay.Features.Interaction.Abstractions
{
    public interface IInteractionScope
    {
        bool Allows(IInteractionTarget target);
    }
}