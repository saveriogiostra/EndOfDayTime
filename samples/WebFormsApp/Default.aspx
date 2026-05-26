<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs"
Inherits="WebFormsApp.Default" %>
<!DOCTYPE html>
<html>
  <head runat="server">
    <meta charset="utf-8" />
    <title>EndOfDayTime — WebForms Sample</title>
    <link
      rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"
    />
  </head>
  <body>
    <form id="form1" runat="server">
      <div class="container mt-5" style="max-width: 480px">
        <h1 class="mb-1">Timesheet</h1>
        <p class="text-muted mb-4">
          Inserisci gli orari di inizio e fine turno (00:00–24:00).
        </p>

        <div class="mb-3">
          <label class="form-label" for="eodtStart">Inizio turno</label><br />
          <eodt:EndOfDayTimeTextBox ID="eodtStart" runat="server" />
        </div>

        <div class="mb-3">
          <label class="form-label" for="eodtEnd">Fine turno</label><br />
          <eodt:EndOfDayTimeTextBox ID="eodtEnd" runat="server" />
        </div>

        <asp:Button
          ID="btnSubmit"
          runat="server"
          Text="Calcola durata"
          CssClass="btn btn-primary"
          OnClick="btnSubmit_Click"
        />

        <asp:Panel
          ID="pnlError"
          runat="server"
          Visible="false"
          CssClass="mt-4 alert alert-danger"
        >
          <asp:Label ID="lblError" runat="server" />
        </asp:Panel>

        <asp:Panel
          ID="pnlResult"
          runat="server"
          Visible="false"
          CssClass="mt-4 p-3 border rounded bg-light"
        >
          <asp:Label ID="lblResult" runat="server" />
        </asp:Panel>
      </div>
    </form>
  </body>
</html>
