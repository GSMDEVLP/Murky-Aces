using _Project.Develop.Runtime.Gameplay.Features.Input.Domain;

namespace _Project.Develop.Runtime.Gameplay.Features.Input.Abstractions
{
    public interface ICommonActionsReader
    {
        InputMapId MapId { get; }

        bool IsActive { get; }

        CommonInputSnapshot ReadCommonActions();
    }
}