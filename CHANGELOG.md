# Changelog

> **Note:** EchoSpec uses annotated tags as releases, not GitHub Releases.

All notable changes to EchoSpec will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [2.0.0] - Unreleased

### Breaking

- Target framework is **net10.0 only** (removed `net8.0`).
- `BuildWith` / `GenerateWith` no longer return `object`. Use:
  - `string BuildWith(ITableRenderer)` / `string GenerateWith(data, IReportRenderer)`
  - `IReadOnlyDictionary<string, string> BuildWith(params ITableRenderer[])` /
    `GenerateWith(data, params IReportRenderer[])`
- Removed `ReportFormat.Both`. Use `ReportFormat.All`, or `GenerateWith` /
  `BuildWith` for a subset of formats. `Generate` defaults to `All`.
- Removed `AddSection(console, markdown)` (HTML fell back to markdown text).
  Use `AddSection(plain)` or `AddSection(console, markdown, html)`.
- `AddTable` takes `Action<TableBuilder, IEnumerable<T>>` (no `return table`).
- Builders and built-in renderers are `sealed`; `ReportOutput` is a `sealed record`.
- `TableBuilder.Build(ReportFormat.All)` throws `ArgumentOutOfRangeException`
  (1.x `Build(Both)` silently produced Markdown).
- Empty tables (headers, no rows) now render header/separator across Console, Markdown, and HTML
  (Console/HTML previously returned `""`).
- Duplicate renderer `Name` values throw instead of silently overwriting dictionary entries.
- Rows are padded/truncated to the header width by default (`StrictRowWidths` optional).

### Added

- `ReportFormat.Html` and `ReportFormat.All` (replaces `Both` for multi-format reports).
- `ReportOutput.HtmlText` and `SaveHtml`.
- `TableBuilder.Build(ReportFormat.Html)` for table fragments.
- Typed single-renderer `BuildWith` / `GenerateWith` overloads.
- `AddSection(contentFormatter)` — shared plain text for all renderers.
- `AddSection(console, markdown, html)` — per-format section content.
- `TableBuilder.StrictRowWidths` and `AddRow(params string[])`.
- Markdown cell escaping for `|` and newlines.

### Changed

- Multi-format report generation prepares each section once (tables/formatters run a single time).
- HTML escaping uses a single-pass `StringBuilder` write.
- Console cell padding writes directly into the `StringBuilder`.

### Fixed

- Custom sections can target HTML without forcing markdown-flavored text through `EscapeHtml`.

## [1.0.0] - 2025-01-09

### Added

- Initial release of EchoSpec
- `ITableRenderer` and `IReportRenderer` interfaces for extensible output formats
- `ConsoleRenderer` for box-drawing character console output
- `MarkdownRenderer` for GitHub-compatible markdown tables
- `HtmlRenderer` for styled HTML output with embedded CSS
- `TableBuilder` for fluent table construction
- `ReportBuilder<T>` for generic, type-safe report generation
- `BuildWith(params ITableRenderer[])` - support for multiple renderers in single call
- `GenerateWith(params IReportRenderer[])` - support for multiple renderers in single call
- Column alignment support (Left, Center, Right)
- Zero external dependencies
- Full XML documentation
- Comprehensive README with examples

### Features

- Multi-format output generation (Console, Markdown, HTML, custom)
- Beautiful formatting with box-drawing characters
- Styled HTML output with responsive design
- Fully generic architecture with `ReportBuilder<T>`
- Extensible renderer pattern for custom formats
- Type-safe with C# generics and LINQ
- Fluent API for readable code
- Single or multiple renderer output support

[2.0.0]: https://github.com/tzinmein/EchoSpec/releases/tag/v2.0.0
[1.0.0]: https://github.com/tzinmein/EchoSpec/releases/tag/v1.0.0
