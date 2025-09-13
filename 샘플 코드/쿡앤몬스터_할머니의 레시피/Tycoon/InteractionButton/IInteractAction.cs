using UnityEngine;
using UnityEngine.UI;

public interface IInteractAction
{
    bool CanExecute(SpriteRenderer plate);
    void Execute();
}

public abstract class UIInteractActionBase : MonoBehaviour, IInteractAction
{
    public abstract bool CanExecute(SpriteRenderer plate);
    public abstract void Execute();
}
