using System.Collections.Generic;
using System.Linq;
using System;
using Core.Simulation.Data;

namespace Core.Simulation.Logic
{
    public sealed class EmployeeManager
    {
        public List<EmployeeProfile> Employees { get; } = new();
        public List<EmployeeCandidate> Candidates { get; } = new();
        private int _nextCandidateId = 1;

        public int AverageMorale => Employees.Count == 0 ? 50 : Employees.Sum(e => e.Morale) / Employees.Count;
        public Money TotalSalary => Employees.Aggregate(Money.Zero, (sum, employee) => sum + employee.Salary);
        public int StockerEfficiency => GetRoleEfficiency(EmployeeRole.Stocker);
        public int CashierEfficiency => GetRoleEfficiency(EmployeeRole.Cashier);
        public int ManagerEfficiency => GetRoleEfficiency(EmployeeRole.Manager);
        public int SalesAssociateEfficiency => GetRoleEfficiency(EmployeeRole.SalesAssociate);
        public int SecurityEfficiency => GetRoleEfficiency(EmployeeRole.Security);
        public int DailyRestockCapacity => StockerEfficiency == 0 ? 0 : Math.Max(8, StockerEfficiency * 2);
        public int CheckoutCapacity => CashierEfficiency == 0 ? 4 : Math.Max(6, CashierEfficiency / 2);
        public int CustomerServiceDemandBasisPoints => 10_000 + SalesAssociateEfficiency * 35 + ManagerEfficiency * 10;
        public int QueuePressureMitigationBasisPoints => Math.Max(5_000, 10_000 - SecurityEfficiency * 40);
        public int MoraleProtection => ManagerEfficiency / 20;

        public void AddEmployee(EmployeeProfile employee)
        {
            Employees.Add(employee);
        }

        public EmployeeProfile CreateEmployee(string name, EmployeeRole role)
        {
            return role switch
            {
                EmployeeRole.Manager => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 78, Money.FromUnits(95), 78),
                EmployeeRole.Cashier => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 70, Money.FromUnits(60), 74),
                EmployeeRole.Stocker => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 66, Money.FromUnits(55), 70),
                EmployeeRole.SalesAssociate => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 68, Money.FromUnits(58), 72),
                EmployeeRole.Security => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 62, Money.FromUnits(50), 68),
                _ => new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), 65, Money.FromUnits(55), 70)
            };
        }

        public void RefreshCandidates(int currentDay)
        {
            Candidates.Clear();

            AddCandidate("Iris", EmployeeRole.Cashier, 64, 72, currentDay + 7);
            AddCandidate("Dorian", EmployeeRole.Stocker, 70, 68, currentDay + 7);
            AddCandidate("Selene", EmployeeRole.SalesAssociate, 73, 76, currentDay + 7);

            if (currentDay >= 8)
                AddCandidate("Vera", EmployeeRole.Security, 66, 70, currentDay + 7);
            if (currentDay >= 15)
                AddCandidate("Omar", EmployeeRole.Manager, 82, 80, currentDay + 7);
        }

        public void RemoveExpiredCandidates(int currentDay)
        {
            Candidates.RemoveAll(candidate => candidate.AvailableUntilDay < currentDay);
        }

        public bool HireCandidate(int candidateId, GameManager game)
        {
            var candidate = Candidates.Find(c => c.Id == candidateId);
            if (candidate == null)
                return false;

            if (!game.HireEmployee(candidate.Profile))
                return false;

            Candidates.Remove(candidate);
            return true;
        }

        public void LoadCandidates(IEnumerable<EmployeeCandidate> candidates)
        {
            Candidates.Clear();
            Candidates.AddRange(candidates);
            _nextCandidateId = Candidates.Count == 0 ? 1 : Candidates.Max(c => c.Id) + 1;
        }

        public void ClearEmployees()
        {
            Employees.Clear();
        }

        public void LoadEmployees(IEnumerable<EmployeeProfile> employees)
        {
            Employees.Clear();
            Employees.AddRange(employees);
        }

        public bool RemoveEmployeeByName(string employeeName)
        {
            var employee = Employees.Find(e => e.Name == employeeName);
            if (employee == null) return false;
            Employees.Remove(employee);
            return true;
        }

        public void AdjustMorale(int delta)
        {
            foreach (var employee in Employees)
                employee.Morale = Math.Clamp(employee.Morale + delta, 0, 100);
        }

        public void UpdateDaily(GameManager game)
        {
            foreach (var employee in Employees)
            {
                if (game.Events.HasRecentEvent(GameEventType.EmployeeStrike))
                {
                    employee.Morale = System.Math.Max(0, employee.Morale - Math.Max(3, 10 - MoraleProtection));
                }
                else
                {
                    employee.Morale = System.Math.Min(100, employee.Morale + 1);
                }

                employee.Efficiency = System.Math.Clamp(employee.Morale / 10 + 50, 10, 100);
            }

            game.Economy.RecordPayrollExpense(game.GetAdjustedPayrollCost(TotalSalary));
        }

        public EmployeeOperations GetOperations()
        {
            return new EmployeeOperations(
                CountRole(EmployeeRole.Manager),
                CountRole(EmployeeRole.Cashier),
                CountRole(EmployeeRole.Stocker),
                CountRole(EmployeeRole.SalesAssociate),
                CountRole(EmployeeRole.Security),
                DailyRestockCapacity,
                CheckoutCapacity,
                CustomerServiceDemandBasisPoints,
                QueuePressureMitigationBasisPoints,
                MoraleProtection
            );
        }

        public int CountRole(EmployeeRole role)
        {
            return Employees.Count(e => e.RoleType == role);
        }

        private int GetRoleEfficiency(EmployeeRole role)
        {
            var matching = Employees.Where(e => e.RoleType == role).ToList();

            return matching.Count == 0 ? 0 : matching.Sum(e => e.Efficiency) / matching.Count;
        }

        private void AddCandidate(string name, EmployeeRole role, int efficiency, int morale, int availableUntilDay)
        {
            Money salary = role switch
            {
                EmployeeRole.Manager => Money.FromUnits(105),
                EmployeeRole.Cashier => Money.FromUnits(65),
                EmployeeRole.Stocker => Money.FromUnits(60),
                EmployeeRole.SalesAssociate => Money.FromUnits(62),
                EmployeeRole.Security => Money.FromUnits(55),
                _ => Money.FromUnits(60)
            };

            Candidates.Add(new EmployeeCandidate(
                _nextCandidateId++,
                new EmployeeProfile(name, EmployeeRoleCatalog.ToDisplayName(role), efficiency, salary, morale),
                availableUntilDay
            ));
        }
    }
}
