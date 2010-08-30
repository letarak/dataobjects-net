// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.08.13

using System;
using Xtensive.Core.Internals.DocTemplates;

namespace Xtensive.Storage
{
  /// <summary>
  /// Marks persistent property as a part of version.
  /// </summary>
  [Serializable]
  [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
  public sealed class VersionAttribute : StorageAttribute
  {
    /// <summary>
    /// Gets the version check mode for the persistent property.
    /// </summary>
    public VersionMode Mode { get; private set; }

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <remarks><see cref="Mode"/> is set to <see cref="VersionMode.Auto"/>.</remarks>
    public VersionAttribute()
      : this(VersionMode.Auto)
    {}

    /// <summary>
    ///   <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="mode">The version check mode.</param>
    public VersionAttribute(VersionMode mode)
    {
      Mode = mode;
    }
  }
}