using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using EodtCore = EndOfDayTime.Core;

namespace EndOfDayTime.Sample.Wpf
{
    public class ShiftViewModel : INotifyPropertyChanged
    {
        // ── Fields ───────────────────────────────────────────────────────

        private EodtCore.EndOfDayTime _start;
        private EodtCore.EndOfDayTime _end;
        private string _resultMessage = "Enter a shift and click Save.";
        private Brush _resultColor = Brushes.Black;

        // ── Properties ───────────────────────────────────────────────────

        public EodtCore.EndOfDayTime Start
        {
            get => _start;
            set { _start = value; OnPropertyChanged(); }
        }

        public EodtCore.EndOfDayTime End
        {
            get => _end;
            set { _end = value; OnPropertyChanged(); }
        }

        public string ResultMessage
        {
            get => _resultMessage;
            set { _resultMessage = value; OnPropertyChanged(); }
        }

        public Brush ResultColor
        {
            get => _resultColor;
            set { _resultColor = value; OnPropertyChanged(); }
        }

        // ── Command ──────────────────────────────────────────────────────

        public ICommand SaveCommand { get; }

        public ShiftViewModel()
        {
            SaveCommand = new RelayCommand(Save);
        }

        private void Save()
        {
            if (Start == default)
            {
                ResultMessage = "Please enter a start time.";
                ResultColor = Brushes.Red;
                return;
            }

            if (End == default)
            {
                ResultMessage = "Please enter an end time.";
                ResultColor = Brushes.Red;
                return;
            }

            if (End <= Start)
            {
                ResultMessage = "End time must be after start time.";
                ResultColor = Brushes.Red;
                return;
            }

            var duration = End - Start;
            ResultMessage = $"Shift saved!\nStart: {Start}\nEnd: {End}\nDuration: {duration.TotalHours:F1}h";
            ResultColor = Brushes.Green;
        }

        // ── INotifyPropertyChanged ────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // ── RelayCommand ──────────────────────────────────────────────────────

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        public RelayCommand(Action execute) => _execute = execute;

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute();

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}