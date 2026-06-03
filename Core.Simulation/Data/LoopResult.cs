namespace Core.Simulation.Data
{
    public sealed record LoopResult(
        LoopStatus Status,
        int Day,
        Money Cash,
        Money TargetCash,
        int Reputation,
        int ReputationFailureThreshold,
        string Reason
    )
    {
        public static LoopResult Active(int day, Money cash, Money targetCash, int reputation, int reputationFailureThreshold)
        {
            return new LoopResult(
                LoopStatus.Active,
                day,
                cash,
                targetCash,
                reputation,
                reputationFailureThreshold,
                "Loop in progress."
            );
        }
    }
}
