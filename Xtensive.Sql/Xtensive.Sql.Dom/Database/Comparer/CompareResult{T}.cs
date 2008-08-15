// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Aleksey Gamzov
// Created:    2008.08.15

using Xtensive.Core;
using Xtensive.Core.Helpers;

namespace Xtensive.Sql.Dom.Database.Comparer
{
  /// <summary>
  /// Base class for compare results with original and new values inside.
  /// </summary>
  public abstract class CompareResult<T> : CompareResult
  {
    private T originalValue;
    private T newValue;

    /// <summary>
    /// Gets new value.
    /// </summary>
    public T NewValue
    {
      get { return newValue; }
      internal set
      {
        this.EnsureNotLocked();
        newValue = value;
      }
    }

    /// <summary>
    /// Gets original value.
    /// </summary>
    public T OriginalValue
    {
      get { return originalValue; }
      internal set
      {
        this.EnsureNotLocked(); originalValue = value;
      }
    }

    /// <inheritdoc/>
    public override void Lock(bool recursive)
    {
      base.Lock(recursive);
      if (recursive) {
        (originalValue as ILockable).LockSafely(recursive);
        (newValue as ILockable).LockSafely(recursive);
      }
    }
  }
}