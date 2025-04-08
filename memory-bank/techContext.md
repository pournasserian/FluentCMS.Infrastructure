# Tech Context: FluentCMS Plugin System

This document details the technologies and development environment for the FluentCMS plugin system.

## Technologies

* **.NET:** The plugin system will be built using .NET.
* **ASP.NET Core:** The plugin system will integrate with ASP.NET Core.
* **C#:** Plugins will be developed using C#.

## Development Setup

Plugins will be developed as separate class library projects targeting the same .NET framework as the main application.

## Dependencies

Plugins can have their own dependencies, which will be managed through NuGet.
