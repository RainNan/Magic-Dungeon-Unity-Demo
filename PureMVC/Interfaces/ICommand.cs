using UnityEngine.Playables;

public interface ICommand
{
    void Execute(INotification notification);
}