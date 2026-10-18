---
description: "Release notes for every DuckDB.NET version, grouped by release series, with the DuckDB version each release is built on."
---

# Release Notes

The release notes are grouped by series, one page for each minor version. DuckDB.NET ships as two packages that share a version number:

- **DuckDB.NET.Data** is the ADO.NET provider. Most applications use it, usually through `DuckDB.NET.Data.Full`.
- **DuckDB.NET.Bindings** holds the low-level bindings to the DuckDB C API.

Each release lists the DuckDB.NET.Data changes first. Changes that affect only the bindings appear under **Bindings**. The "Built on" line gives the version of DuckDB bundled in the `.Full` packages.

> [!NOTE]
> Before version 1.5.6.1, the release notes of a package often repeated the notes of the previous release. These pages list each change only in the release that introduced it. Versions with no changes of their own have only a "Built on" line.

## Release series

| Series | Latest version | Released | DuckDB versions |
|---|---|---|---|
| [1.5](release-notes/1.5.md) | [1.5.6.1](release-notes/1.5.md#1561) | 2026-10-15 | v1.5.0 – v1.5.6 |
| [1.4](release-notes/1.4.md) | [1.4.4](release-notes/1.4.md#144) | 2026-02-03 | v1.4.1 – v1.4.4 |
| [1.3](release-notes/1.3.md) | [1.3.2](release-notes/1.3.md#132) | 2025-07-08 | v1.3.0 – v1.3.2 |
| [1.2](release-notes/1.2.md) | [1.2.1](release-notes/1.2.md#121) | 2025-03-05 | v1.2.0 – v1.2.1 |
| [1.1](release-notes/1.1.md) | [1.1.3](release-notes/1.1.md#113) | 2024-11-07 | v1.1.0 – v1.1.3 |
| [1.0](release-notes/1.0.md) | [1.0.2](release-notes/1.0.md#102) | 2024-07-22 | v1.0.0 |
| [0.x](release-notes/0.x.md) | [0.10.3](release-notes/0.x.md#0103) | 2024-05-22 | v0.6.1 – v0.10.3 |
