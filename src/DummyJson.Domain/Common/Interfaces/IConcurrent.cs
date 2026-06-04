using System;

namespace DummyJson.Domain.Common.Interfaces;

/// <summary>
/// Interface for entities that support optimistic concurrency tracking.
/// </summary>
public interface IConcurrent
{
    /// <summary>
    /// Stamp used to track concurrency. Usually updated on every modification.
    /// </summary>
    Guid ConcurrencyStamp { get;  }
}
