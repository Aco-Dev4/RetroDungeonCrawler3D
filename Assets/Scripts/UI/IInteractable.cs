public interface IInteractable
{
    int Priority { get; }     // higher = more important
    void Interact();
    void OnFocus();
    void OnUnfocus();
}
