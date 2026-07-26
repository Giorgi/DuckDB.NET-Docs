# DuckDB.NET-Docs

DuckDB.NET project documentation.

## Overview

This repository contains the documentation for the [DuckDB.NET project](https://github.com/Giorgi/DuckDB.NET/), which provides a .NET wrapper for the DuckDB embedded database.


## Getting Started

To get started with DuckDB.NET, visit the [official DuckDB.NET website](http://duckdb.net) for more information on the project.

## Building the docs

Building the full site requires a checkout of [DuckDB.NET](https://github.com/Giorgi/DuckDB.NET) as a sibling directory (`../DuckDB.NET`): the API reference metadata in `api/` is gitignored and must be generated from it with `docfx metadata docfx.json` before `docfx docfx.json` can resolve the API cross-references used throughout the docs.

