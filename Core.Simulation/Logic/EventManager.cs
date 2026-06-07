using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class EventManager
    {
        private readonly DeterministicRandom _rng;
        private GameEvent? _activeEvent;
        private EventDecision? _pendingDecision;

        public EventManager(int seed)
        {
            _rng = new DeterministicRandom(seed);
        }

        public void InjectEvents(GameManager game)
        {
            if (_activeEvent != null)
            {
                _activeEvent.TickDay();
                if (!_activeEvent.IsActive)
                {
                    _activeEvent = null;
                }
                return;
            }

            if (_rng.ChanceBasisPoints(4_500))
            {
                int roll = _rng.Next(0, 8);
                switch (roll)
                {
                    case 0:
                        SetEvent(new GameEvent(GameEventType.DemandSpike, "Retro interest has spiked.", 10, 3));
                        game.Customers.ApplyReputationEvent(game, 5, ReputationChangeSource.MinorEvent);
                        break;
                    case 1:
                        SetEvent(new GameEvent(GameEventType.SupplierDelay, "A supplier shipment is delayed.", 15, 2));
                        game.Suppliers.ApplySupplyShock(game, 5);
                        break;
                    case 2:
                        SetEvent(new GameEvent(GameEventType.ReputationCrisis, "A negative review has shaken customer trust.", -10, 4));
                        game.Customers.ApplyReputationEvent(game, -10, ReputationChangeSource.MajorEvent);
                        break;
                    case 3:
                        SetEvent(new GameEvent(GameEventType.EmployeeStrike, "Employees demand better conditions.", -15, 3));
                        game.Customers.ApplyReputationEvent(game, -5, ReputationChangeSource.MajorEvent);
                        break;
                    case 4:
                        SetEvent(new GameEvent(GameEventType.HolidayRush, "A retro holiday rush is underway.", 20, 3));
                        break;
                    case 5:
                        SetEvent(new GameEvent(GameEventType.ImportPressure, "EUR/RON pressure increased import costs.", -8, 4));
                        game.ApplyImportPressure(11_800);
                        break;
                    case 6:
                        SetEvent(new GameEvent(GameEventType.DefectiveProductComplaint, "A customer reports a defective console.", -6, 2));
                        game.Customers.ApplyReputationEvent(game, -2, ReputationChangeSource.MinorEvent);
                        break;
                    case 7:
                        SetEvent(new GameEvent(GameEventType.RefundRequest, "A refund request is waiting for a decision.", -4, 2));
                        break;
                }
            }
        }

        public bool TriggerDebugEvent(GameManager game)
        {
            if (_activeEvent != null && _pendingDecision != null)
                return false;

            SetEvent(new GameEvent(GameEventType.DefectiveProductComplaint, "Customer complaint: defective console needs handling.", -5, 2));
            game.Customers.ApplyReputationEvent(game, -2, ReputationChangeSource.MinorEvent);
            return true;
        }

        public bool ResolveDecision(GameManager game, int option)
        {
            if (_pendingDecision == null)
                return false;

            switch (_pendingDecision.Type)
            {
                case GameEventType.DemandSpike:
                    if (option == 0)
                        game.Economy.RecordExpense(Money.FromUnits(80));
                    else
                        game.Customers.ApplyReputationEvent(game, -2, ReputationChangeSource.MinorEvent);
                    break;
                case GameEventType.SupplierDelay:
                    if (option == 0)
                        game.Economy.RecordExpense(Money.FromUnits(120));
                    else if (game.Suppliers.Suppliers.Count > 0)
                        game.Suppliers.Suppliers[0].Reliability = System.Math.Max(0, game.Suppliers.Suppliers[0].Reliability - 8);
                    break;
                case GameEventType.ReputationCrisis:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(100));
                        game.Customers.ApplyReputationEvent(game, 8, ReputationChangeSource.MajorEvent);
                    }
                    else
                        game.Customers.ApplyReputationEvent(game, -4, ReputationChangeSource.MajorEvent);
                    break;
                case GameEventType.EmployeeStrike:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(120));
                        game.Employees.AdjustMorale(10);
                    }
                    else
                        game.Employees.AdjustMorale(-8);
                    break;
                case GameEventType.HolidayRush:
                     if (option == 0)
                        game.Economy.RecordExpense(Money.FromUnits(90));
                    else
                        game.Customers.ApplyReputationEvent(game, -3, ReputationChangeSource.MinorEvent);
                    break;
                case GameEventType.ImportPressure:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(75));
                        game.ApplyImportPressure(10_600);
                        game.RecordSupportEventEffect("Absorbed part of the import-cost shock.");
                    }
                    else
                    {
                        game.ApplyImportPressure(12_400);
                        game.Customers.ApplyReputationEvent(game, -2, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Passed import pressure to customers.");
                    }
                    break;
                case GameEventType.DefectiveProductComplaint:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(140));
                        game.Customers.ApplyReputationEvent(game, 6, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Full refund protected trust.");
                    }
                    else
                    {
                        game.Economy.RecordExpense(Money.FromUnits(55));
                        game.Customers.ApplyReputationEvent(game, 2, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Store credit contained the complaint.");
                    }
                    break;
                case GameEventType.RefundRequest:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(90));
                        game.Customers.ApplyReputationEvent(game, 4, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Refund request accepted.");
                    }
                    else
                    {
                        game.Customers.ApplyReputationEvent(game, -5, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Refund denied; reputation suffered.");
                    }
                    break;
                case GameEventType.WarrantyDispute:
                    if (option == 0)
                    {
                        game.Economy.RecordExpense(Money.FromUnits(65));
                        game.Suppliers.ApplySupplyShock(game, 3);
                        game.RecordSupportEventEffect("Investigated warranty issue with supplier.");
                    }
                    else
                    {
                        game.Customers.ApplyReputationEvent(game, -3, ReputationChangeSource.MinorEvent);
                        game.RecordSupportEventEffect("Warranty dispute left unresolved.");
                    }
                    break;
            }

            _pendingDecision = null;
            return true;
        }

        public bool HasRecentEvent(GameEventType eventType)
        {
            return _activeEvent?.Type == eventType;
        }

        public int GetDemandModifierBasisPoints()
        {
            return _activeEvent?.Type switch
            {
                GameEventType.DemandSpike => 13_500,
                GameEventType.HolidayRush => 15_000,
                GameEventType.SupplierDelay => 9_000,
                GameEventType.ReputationCrisis => 8_500,
                GameEventType.EmployeeStrike => 8_000,
                GameEventType.ImportPressure => 9_500,
                GameEventType.DefectiveProductComplaint => 9_200,
                GameEventType.RefundRequest => 9_500,
                GameEventType.WarrantyDispute => 9_000,
                _ => 10_000
            };
        }

        public GameEvent? CurrentEvent => _activeEvent;
        public EventDecision? CurrentDecision => _pendingDecision;

        public void RestoreEvent(GameEvent? gameEvent, bool hasPendingDecision)
        {
            _activeEvent = gameEvent;
            _pendingDecision = gameEvent != null && hasPendingDecision ? CreateDecision(gameEvent) : null;
        }

        private void SetEvent(GameEvent gameEvent)
        {
            _activeEvent = gameEvent;
            _pendingDecision = CreateDecision(gameEvent);
        }

        private static EventDecision? CreateDecision(GameEvent gameEvent)
        {
            return gameEvent.Type switch
            {
                GameEventType.DemandSpike => new EventDecision(gameEvent.Type, "Demand is spiking. How do you handle the rush?", "Run a small promotion", "Keep cash"),
                GameEventType.SupplierDelay => new EventDecision(gameEvent.Type, "A shipment is delayed. How do you respond?", "Pay for priority freight", "Accept the delay"),
                GameEventType.ReputationCrisis => new EventDecision(gameEvent.Type, "A bad review is spreading.", "Issue refunds", "Ignore it"),
                GameEventType.EmployeeStrike => new EventDecision(gameEvent.Type, "Staff are unhappy.", "Pay a morale bonus", "Push through"),
                GameEventType.HolidayRush => new EventDecision(gameEvent.Type, "Holiday traffic is rising.", "Prepare extra service", "Save cash"),
                GameEventType.ImportPressure => new EventDecision(gameEvent.Type, "EUR/RON pressure is raising import costs.", "Absorb cost", "Raise prices later"),
                GameEventType.DefectiveProductComplaint => new EventDecision(gameEvent.Type, "Defective console complaint.", "Full refund", "Store credit"),
                GameEventType.RefundRequest => new EventDecision(gameEvent.Type, "Customer asks for a refund.", "Refund", "Deny"),
                GameEventType.WarrantyDispute => new EventDecision(gameEvent.Type, "Warranty dispute on a collector item.", "Investigate supplier", "Delay response"),
                _ => null
            };
        }
    }
}
