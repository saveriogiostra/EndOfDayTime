# Changelog

All notable changes to the EndOfDayTime packages are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).
All packages are released together and share one version number.

## [2.0.0] - 2026-10-09

2.0.0 fixes the handling of `00:00`, which the UI controls used to treat as "empty",
and cleans up the parts of the API that could not be fixed without breaking changes.
See [Upgrading from 1.0.0](#upgrading-from-100) for what to change in your code.

### Breaking changes

#### All UI controls: `00:00` is a real value, `null` means empty

- **WinForms, WPF, WebForms:** `TimeValue` is now `EndOfDayTime?`. An empty field is
  `null`; `00:00` is midnight at the start of the day. In WinForms and WPF the
  `TimeValueChanged` event carries `EndOfDayTime?` as well.
- **Blazor:** `EndOfDayTimeInput` is now `EndOfDayTimeInput<TValue>`, where `TValue` is
  `EndOfDayTime` or `EndOfDayTime?`. Any other type throws `InvalidOperationException`.
- **ASP.NET Core:** `EndOfDayTimeInputFor` accepts `EndOfDayTime` and `EndOfDayTime?`
  properties (its signature is now `EndOfDayTimeInputFor<TModel, TResult>`), and the
  `value` parameter of `EndOfDayTimeInput` is `EndOfDayTime?`. A value of `00:00` is
  rendered as `00:00` instead of an empty field.

#### WinForms / WPF

- An empty field is now valid. `Validate()` only reports "Time is required" when the
  new `IsRequired` property is set (default `false`). `EndOfDayTimeValidationRule`
  (WPF) has the same property and the same default.
- `TimeValue` is `null` while the text is incomplete or invalid. Previously it kept the
  last valid value, so `12:3` or `25:00` still reported `12:30`.

#### WebForms

- `EndOfDayTimeTextBox` now derives from `TextBox`.
- `InputCssClass` is removed; use `CssClass`.
- Invalid posted text is kept in `Text` so the user can correct it. `IsValid` is `false`
  and `TimeValue` is `null`.
- The `net8.0` target is removed. It contained no usable control; the package now ships
  `lib/net48` only.

#### ASP.NET Core

- `EndOfDayTimeInputTagHelper` has a parameterless constructor; it no longer takes an
  `IHtmlGenerator`.
- `EndOfDayTimeHtmlHelperExtensions.BuildEndOfDayTimeInput` is no longer public.

#### Core

- `EndOfDayTime` is serialized to JSON as an `"HH:mm"` string without registering
  `EndOfDayTimeJsonConverter`. Previously it was written as an object unless the
  converter was added to the options, and that object could not be read back.
  Non-string JSON tokens now throw `JsonException`.
- `EndOfDayTime.TryParse` and `Parse` accept exactly `HH:mm` with ASCII digits
  (surrounding whitespace is still trimmed). Inputs such as `+5:30` or `1 :30`, which
  used to parse, are rejected.

### Added

- **Core:** `net8.0` target alongside `netstandard2.0`. On `net8.0` the package has no
  `System.Text.Json` dependency and the `TimeOnly` conversions are available:
  `ToTimeOnly()`, `FromTimeOnly()` and explicit operators in both directions. The
  conversion from `TimeOnly` discards seconds.
- **Entity Framework Core:** `UseEndOfDayTime()` for `ConfigureConventions`, the
  recommended way to map every `EndOfDayTime` and `EndOfDayTime?` property at once.
- **Entity Framework Core:** `HasEndOfDayTimeConverter()` overload for
  `EndOfDayTime?` properties.
- **WinForms / WPF:** `IsRequired` on `EndOfDayTimeTextBox`, and on
  `EndOfDayTimeValidationRule` in WPF.
- **WebForms:** the standard `TextBox` members (`CssClass`, `Enabled`, `Width`,
  `AutoPostBack`, ...) now work on `EndOfDayTimeTextBox`.

### Fixed

- **Entity Framework Core:** `ApplyEndOfDayTimeConverter()` found no properties on
  relational providers, so the model failed to build. It now also maps nullable
  properties.
- **ASP.NET Core:** `endofdaytime-input.js` was missing from the package, so the
  documented `_content/EndOfDayTime.AspNet/endofdaytime-input.js` URL returned 404.
- **WPF:** `TimeValueChanged` was not raised for changes made by typing.
- **WPF:** `EndOfDayTimeConverter.ConvertBack` wrote `00:00` to the binding source when
  the text could not be parsed. It now returns `DependencyProperty.UnsetValue`, or
  `null` for empty text when the target is nullable.
- **WPF:** `IsValid` is reset when typing resumes, and updating `TimeValue` from user
  input no longer replaces a binding on the property.
- **WinForms / WPF:** validation on lost focus also runs when the field is empty.
- **Entity Framework Core, Blazor:** the packages required the latest 8.0.x patch of
  their dependencies that happened to be current at build time. The minimum is now
  `8.0.0`.

### Upgrading from 1.0.0

1. **`TimeValue` is nullable.** Replace comparisons with `default` by a null check, and
   read the value with `.Value` or `??`:

   ```csharp
   // 1.0.0
   if (timeBox.TimeValue != default) Save(timeBox.TimeValue);

   // 2.0.0
   if (timeBox.TimeValue is { } time) Save(time);
   ```

   Handlers of `TimeValueChanged` in WinForms and WPF receive `EndOfDayTime?`.

2. **Required fields (WinForms / WPF).** If you relied on an empty field failing
   validation, set `IsRequired = true` on the control or on the validation rule.

3. **Blazor.** In markup `TValue` is inferred from `@bind-Value`, so most pages need no
   change. Bind to an `EndOfDayTime?` property if the field may be left empty. Code
   that names the component type must use `EndOfDayTimeInput<EndOfDayTime>` or
   `EndOfDayTimeInput<EndOfDayTime?>`.

4. **ASP.NET Core.** A non-nullable model property that was never set now renders
   `00:00`; make it `EndOfDayTime?` to render an empty field. Remove the
   `IHtmlGenerator` argument if you construct `EndOfDayTimeInputTagHelper` yourself,
   and replace calls to `BuildEndOfDayTimeInput` with `Html.EndOfDayTimeInput(...)`.

5. **WebForms.** Rename `InputCssClass` to `CssClass` in markup and code-behind.
   Projects that referenced the package from a `net8.0` project must drop the
   reference; the control only ever worked on .NET Framework 4.8.

6. **JSON.** Remove `options.Converters.Add(new EndOfDayTimeJsonConverter())` if you
   like; it is no longer needed. If you stored or exchanged JSON written by 1.0.0
   *without* the converter, those payloads contain an object rather than `"HH:mm"` and
   are rejected by 2.0.0.

7. **Entity Framework Core.** No change is required. To map all properties in one
   place, prefer:

   ```csharp
   protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
       => configurationBuilder.UseEndOfDayTime();
   ```

## [1.0.0] - 2026-05-28

Initial release.

[2.0.0]: https://github.com/saveriogiostra/EndOfDayTime/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/saveriogiostra/EndOfDayTime/releases/tag/v1.0.0
