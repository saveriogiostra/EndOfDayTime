# EndOfDayTime — Contesto sessione

## Cosa è questo progetto

Suite di package .NET che implementa il tipo `EndOfDayTime` — una struct che
rappresenta orari nel range 00:00–24:00 dove 24:00 è fine giornata (ISO 8601).
Repository: https://github.com/saveriogiostra/EndOfDayTime

## Struttura solution

EndOfDayTime/
├── Directory.Build.props ← metadati NuGet + Platform=Any CPU fix
├── Directory.Build.targets ← disabilita doc XML per test e samples
├── build.ps1 ← $env:Platform=""; dotnet build
├── test.ps1 ← $env:Platform=""; dotnet test
├── src/
│ ├── EndOfDayTime.Core/ ← netstandard2.0, zero dipendenze
│ ├── EndOfDayTime.EntityFramework/ ← net8.0, EF Core 8
│ ├── EndOfDayTime.Blazor/ ← net8.0, componente InputBase<T>
│ ├── EndOfDayTime.WinForms/ ← net8.0-windows
│ └── EndOfDayTime.Wpf/ ← net8.0-windows
├── tests/
│ ├── EndOfDayTime.Core.Tests/
│ ├── EndOfDayTime.EntityFramework.Tests/
│ ├── EndOfDayTime.Blazor.Tests/
│ ├── EndOfDayTime.WinForms.Tests/
│ └── EndOfDayTime.Wpf.Tests/
└── samples/
├── ConsoleApp/ ← COMPLETATO
├── AspNetApi/ ← COMPLETATO
├── WinFormsApp/ ← COMPLETATO
├── WpfApp/ ← DA FARE
└── BlazorApp/ ← DA FARE

## Stato attuale

- 100/100 test passati
- Zero warning di build
- README.md in ogni package src/
- Metadati NuGet configurati (versione 1.0.0, autore Saverio Giostra, company Lavika)
- NUGET_API_KEY già configurata come GitHub Secret
- build.ps1 e test.ps1 necessari perché la macchina ha Platform=MCD come
  variabile d'ambiente di sistema (HP laptop) che interferisce con MSBuild

## Dettagli tecnici importanti

### Alias namespace obbligatorio

Il namespace `EndOfDayTime` coincide con il tipo `EndOfDayTime` — in tutti i
file che usano il tipo bisogna usare l'alias:

```csharp
using EodtCore = EndOfDayTime.Core;
// poi usare EodtCore.EndOfDayTime
```

### EndOfDayTimeTextBox WinForms

Riscritto completamente con approccio digit-based:

- Campo privato `_digits` (stringa 0-4 cifre) come stato interno
- Il colon viene inserito automaticamente dopo la seconda cifra
- Gestisce correttamente: backspace, delete, selezione parziale, paste,
  inserimento in mezzo al testo
- `UpdateWidth()` usa `TextRenderer.MeasureText` (non CreateGraphics)
- `OnHandleCreated` per aggiornare larghezza dopo creazione finestra
- Auto-validazione quando `_digits.Length == 4`
- `IsValidChanged` event per abilitare/disabilitare pulsanti Save
- `ErrorProvider` integrato tramite `EndOfDayTimeErrorProviderExtensions.Attach()`

### EndOfDayTimeTextBox WPF

- DependencyProperty `TimeValue`
- RoutedEvent `TimeValueChanged`
- `EndOfDayTimeValidationRule` separata
- Metodo pubblico `Validate()`

### Sample WinFormsApp

Form con due `EndOfDayTimeTextBox` (start/end), `ErrorProvider`, pulsante Save
che calcola durata turno. Usa `IsValidChanged` per abilitare/disabilitare Save.
NOTA: c'era un label di debug aggiunto temporaneamente — verificare che sia
stato rimosso prima di continuare.

## Da fare in ordine

1. **Sample WpfApp** — finestra con due EndOfDayTimeTextBox, binding a ViewModel,
   salvataggio turno, calcolo durata
2. **Sample BlazorApp** — pagina con EditForm, EndOfDayTimeInput, validazione
3. **GitHub Actions** — workflow che fa build+test su ogni push,
   pubblica su NuGet al push di un tag v\*
4. **Prima pubblicazione** — tag v1.0.0 e verifica package su NuGet.org

## Processo di lavoro stabilito

- Procedere un passo alla volta
- Build dopo ogni modifica
- Test dopo ogni package completato
- Mandare file completi quando richiesto (non snippet parziali)
- Usare `dotnet new wpf` / `dotnet new blazorserver` per i sample
- Target sempre net8.0 (non net9.0 che è il default dei template)
- Dopo ogni `dotnet new` verificare il csproj e allineare a net8.0
