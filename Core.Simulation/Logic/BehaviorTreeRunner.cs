using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public static class BehaviorTreeRunner
    {
        public static void UpdateAgent(ref AIState agent, ref Money globalEconomy)
        {
            switch (agent.CurrentBehavior)
            {
                case 0: // Idle
                    if (agent.Wallet < Money.FromUnits(100))
                        agent.CurrentBehavior = 1; // Transition to Work
                    break;

                case 1: // Work
                    var wage = Money.FromUnits(10);
                    agent.Wallet += wage;
                    globalEconomy -= wage;
                    agent.CurrentBehavior = 0; // Transition to Idle
                    break;
            }
        }
    }
}
