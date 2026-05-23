using System;
using System.Windows.Forms;
using EodtCore = EndOfDayTime.Core;
using EndOfDayTime.WinForms;

namespace EndOfDayTime.Sample.WinForms
{
    public partial class Form1 : Form
    {
        private readonly EndOfDayTimeTextBox _startBox;
        private readonly EndOfDayTimeTextBox _endBox;
        private readonly ErrorProvider _errorProvider;
        private readonly Button _saveButton;
        private readonly Label _resultLabel;

        public Form1()
        {
            InitializeComponent();
            Text = "EndOfDayTime WinForms Sample";
            ClientSize = new System.Drawing.Size(400, 280);

            // ── Labels ───────────────────────────────────────────────────
            var lblStart = new Label { Text = "Shift start:", Left = 20, Top = 20, Width = 100 };
            var lblEnd = new Label { Text = "Shift end:", Left = 20, Top = 60, Width = 100 };

            // ── EndOfDayTimeTextBox controls ─────────────────────────────
            _startBox = new EndOfDayTimeTextBox { Left = 130, Top = 17, Width = 100 };
            _endBox = new EndOfDayTimeTextBox { Left = 130, Top = 57, Width = 100 };

            // ── ErrorProvider ─────────────────────────────────────────────
            _errorProvider = new ErrorProvider();
            _errorProvider.Attach(_startBox);
            _errorProvider.Attach(_endBox);

            // ── Save button ───────────────────────────────────────────────
            _saveButton = new Button { Text = "Save shift", Left = 130, Top = 100, Width = 100 };
            _saveButton.Click += OnSave;

            // ── Result label ──────────────────────────────────────────────
            _resultLabel = new Label
            {
                Left = 20,
                Top = 150,
                Width = 360,
                Height = 80,
                Text = "Enter a shift and click Save."
            };

            // ── IsValidChanged — enable Save only when both fields valid ──
            _startBox.IsValidChanged += (s, e) => UpdateSaveButton();
            _endBox.IsValidChanged += (s, e) => UpdateSaveButton();

            var debugLabel = new Label { Left = 20, Top = 230, Width = 360 };
            Controls.Add(debugLabel);

            _startBox.TextChanged += (s, e) => debugLabel.Text = $"Text='{_startBox.Text}' Len={_startBox.Text.Length}";

            Controls.AddRange(new Control[]
            {
                lblStart, lblEnd,
                _startBox, _endBox,
                _saveButton, _resultLabel
            });
        }

        private void UpdateSaveButton()
        {
            _saveButton.Enabled = _startBox.IsValid && _endBox.IsValid;
        }

        private void OnSave(object? sender, EventArgs e)
        {
            var startValid = _startBox.Validate();
            var endValid = _endBox.Validate();

            if (!startValid || !endValid)
            {
                _resultLabel.Text = "Please fix the errors before saving.";
                return;
            }

            var start = _startBox.TimeValue;
            var end = _endBox.TimeValue;

            if (end <= start)
            {
                _resultLabel.Text = "End time must be after start time.";
                return;
            }

            var duration = end - start;
            _resultLabel.Text =
                $"Shift saved!\n" +
                $"Start:    {start}\n" +
                $"End:      {end}\n" +
                $"Duration: {duration.TotalHours:F1}h";
        }
    }
}