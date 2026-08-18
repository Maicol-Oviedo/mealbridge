namespace MealBridge.Domain.Donations;

public static class DonationStatusTransitions
{
    public static bool IsAllowed(
        DonationStatus current,
        DonationStatus target) =>
        (current, target) switch
        {
            (DonationStatus.Available, DonationStatus.Cancelled) => true,
            (DonationStatus.Available, DonationStatus.Expired) => true,
            (DonationStatus.Claimed, DonationStatus.PickedUp) => true,
            (DonationStatus.Claimed, DonationStatus.Cancelled) => true,
            _ => false
        };
}
