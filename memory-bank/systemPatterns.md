# System Patterns: FluentCMS Plugin System

This document outlines the architectural patterns and key technical decisions for the FluentCMS plugin system.

## Plugin Architecture

The plugin system will use an assembly-based approach, where each plugin is a separate DLL.  Plugins will be loaded dynamically at runtime and integrate with the main application through dependency injection.

## Dependency Injection

Plugin services will be registered with the main application's DI container. This allows plugins to access core application services and for the application to access plugin services.

## API Integration

Plugins can define API controllers that are automatically registered with the main application's routing system.

## Background Tasks

Plugins can implement background services that run within the main application's process.

## Inter-plugin Communication

An event bus will facilitate communication between plugins. Plugins can publish and subscribe to events, enabling loose coupling and flexibility.
