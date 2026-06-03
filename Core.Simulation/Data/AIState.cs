namespace Core.Simulation.Data
{
    public struct AIState
    {
        public int EntityId;
        public int CurrentBehavior; // 0=Idle, 1=Work, 2=Move
        public IntVector2 TargetPos;
        public Money Wallet;
    }
}
