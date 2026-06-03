using System.ComponentModel;
using System.Runtime.CompilerServices;
using Core.Simulation.Data;

namespace Client.Architecture.MVVM
{
    public class EconomyViewModel : INotifyPropertyChanged
    {
        private Money _cachedMoney = Money.Zero;
        private string _moneyString = "0.00 lei";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string MoneyString
        {
            get => _moneyString;
            private set
            {
                if (_moneyString != value)
                {
                    _moneyString = value;
                    OnPropertyChanged();
                }
            }
        }

        // Called by Godot's _Process loop (Main Thread) inside TickManager
        public void Update(Money currentSimMoney)
        {
            // Money uses overloaded == from Phase 3: zero garbage, pure integer compare
            if (_cachedMoney != currentSimMoney)
            {
                _cachedMoney = currentSimMoney;
                // Only allocate a new string when the money actually changes
                MoneyString = _cachedMoney.ToString();
            }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
