# Project Guide: SQL Persistence

## Purpose
Define SQL Server and EF Core persistence standards.

## Standards
- Use Entity Framework Core for persistence access.
- Keep migrations source-controlled and reviewable.
- Follow naming conventions for tables/columns/keys.
- Keep persistence concerns in Infrastructure layer.
- Favor explicit query patterns for readability and performance.

## Boundaries
- Domain and Application should depend on abstractions, not EF internals.
- Avoid leaking persistence models beyond infrastructure boundaries.
