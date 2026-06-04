using System;

namespace DummyJson.Domain.Common.Primitives;

/// <summary>
/// Centralized ID generation utility.
/// </summary>
public static class IdGenerator
{
    /// <summary>
    /// Generates a new time-sortable Guid (UUIDv7).
    /// </summary>
    public static Guid NewId() => Guid.CreateVersion7();
    
    public static Guid NewGuid() => Guid.NewGuid();
}
