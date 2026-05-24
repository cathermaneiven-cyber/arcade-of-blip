/// <summary>
/// Implement this interface on any component that can be interacted with.
/// The cursor system will call Interact() automatically when the player
/// clicks while in range and looking at a tagged object.
///
/// Example: DoorInteractable, LightSwitch, PickupItem, etc.
/// </summary>
public interface IInteractable
{
    void Interact();
}