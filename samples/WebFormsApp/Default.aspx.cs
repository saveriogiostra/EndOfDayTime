using System;

namespace WebFormsApp
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            pnlResult.Visible = false;
            pnlError.Visible = false;

            if (!eodtStart.IsValid || !eodtEnd.IsValid)
            {
                ShowError("Inserire orari validi nel formato HH:mm (00:00–24:00).");
                return;
            }

            var start = eodtStart.TimeValue;
            var end   = eodtEnd.TimeValue;

            if (end <= start)
            {
                ShowError("L'orario di fine turno deve essere successivo all'orario di inizio.");
                return;
            }

            var duration = end - start; // operatore - restituisce TimeSpan
            var hours    = (int)duration.TotalHours;
            var minutes  = duration.Minutes;

            pnlResult.Visible = true;
            lblResult.Text =
                $"Inizio: <strong>{start}</strong> &nbsp;&ndash;&nbsp; " +
                $"Fine: <strong>{end}</strong><br />" +
                $"Durata: <strong>{hours}h {minutes:D2}m</strong>";
        }

        private void ShowError(string message)
        {
            pnlError.Visible = true;
            lblError.Text    = message;
        }
    }
}