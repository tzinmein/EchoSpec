# Changelog

> **Note:** EchoSpec uses annotated tags as releases, not GitHub Releases.

All notable changes to EchoSpec will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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

[1.0.0]: https://github.com/tzinmein/EchoSpec/releases/tag/v1.0.0
