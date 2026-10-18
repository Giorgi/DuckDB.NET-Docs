# AGENTS.md

This file provides guidance to coding agents working with code in this repository.

This is the DocFX documentation site for [DuckDB.NET](https://github.com/Giorgi/DuckDB.NET), an ADO.NET provider and
low-level bindings for DuckDB. It is published at https://duckdb.net through Netlify. The library itself lives in a
separate repository, which must be checked out next to this one as `../DuckDB.NET`.

## Commands

```powershell
docfx metadata docfx.json           # Generate api/ from ../DuckDB.NET (needed once, and after API changes)
docfx docfx.json                    # Build the site into _site/
docfx serve _site --open-browser    # Serve the built site locally
dotnet build code                   # Compile the documentation samples
```

CI (`.github/workflows/docfx.yml`) pins docfx 2.78.5 and builds with `docfx --warningsAsErrors`. A warning that
passes locally fails the build there, so fix every DocFX warning: broken links, invalid xrefs, missing TOC entries.

## Publishing

Every push to `main` builds the site and deploys it to production; there is no staging step. The workflow also runs
for pull requests to `main`, but only to build: the deploy step runs only for a push or manual run on `main`.

CI checks out the **default branch of DuckDB.NET, which is `develop`**, and builds the API reference and the samples
against it. So the published API reference shows `develop`, which can be ahead of the released NuGet package.
A sample that uses an API not yet pushed to `develop` fails the build.

## Architecture

- **`docs/*.md`:** the handwritten pages. A new page must be added to `docs/toc.yml` to appear in the navigation.
  Each page starts with front matter holding a `description`, used as the page's meta description.
- **`code/`:** a compilable samples project (`Samples.csproj`) that holds the C# snippets shown in the pages.
- **`api/*.yml`:** API reference metadata, **generated** by `docfx metadata` from the sibling repository. It is
  gitignored. Never edit these files by hand.
- **`my-template/`:** a custom DocFX template applied on top of `default` and `modern` (see `template` in
  `docfx.json`). The CSS overrides are in `my-template/public/main.css`.
- **`toc.yml` (root):** the top-level navigation: Docs and API.
- **`images/`:** the only resource folder copied to the site.
- **`_site/`:** the build output, gitignored.

### Samples (`code/`)

- **Embedding:** a page includes a region of a sample file with `[!code-csharp[](../code/File.cs#Example)]`. Only the
  region is shown; the wrapping class, method and usings stay hidden. Most files have one region named `Example`.
  A file can have several named regions, for example `Map` and `Usage` in `MappedAppenderTypeMismatch.cs`.
- **Prefer a sample file to inline code in markdown.** CI compiles the project (`dotnet build code`), so a snippet
  that no longer matches the API fails the build instead of going stale.
- **Project settings:** `net8.0`, nullable reference types enabled, and warnings treated as errors. It references
  `../DuckDB.NET/DuckDB.NET.Data/Data.csproj` directly, plus Dapper and Octokit.
- **`Shared.cs`:** helper types that the snippets use.
- **`MappedAppenderCreateOverloads.cs`:** a listing of method signatures, shown as it is and excluded from
  compilation.

### Dependency on the DuckDB.NET repository

`docfx.json` generates the API metadata from `../DuckDB.NET` (`Bindings.csproj` and `Data.csproj`, `net8.0`).

- **A fresh clone has no `api/` folder.** Until `docfx metadata docfx.json` has run once, `docfx docfx.json` fails on
  every `xref:` link in the pages.
- **`docfx metadata` does not clean `api/`.** After an API change that removes types, delete the stale `api/*.yml`
  files (or the whole folder) before generating again.
- **Local builds use whatever is checked out in `../DuckDB.NET`**, so the local API reference and sample build can
  differ from CI, which uses `develop`.

## Writing pages

- Link to API members with `xref:`, for example
  `[UseStreamingMode](xref:DuckDB.NET.Data.DuckDBCommand.UseStreamingMode)`. A method that takes parameters needs
  their types in the xref, for example
  `xref:DuckDB.NET.Data.DuckDBCommand.ExecuteArrowBatchesAsync(System.Threading.CancellationToken)`.
- Line endings are mixed in the working tree: some pages use CRLF and some LF (`core.autocrlf` is on). Keep the
  endings a file already has. The pages have no byte-order mark.
