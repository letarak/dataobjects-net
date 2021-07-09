// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.12.27

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Orm;

namespace Xtensive.Sql
{
  /// <summary>
  /// Configuration for <see cref="SqlDriver"/>.
  /// </summary>
  public sealed class SqlDriverConfiguration
  {
    /// <summary>
    /// Gets or sets forced server version.
    /// </summary>
    public string ForcedServerVersion { get; set; }

    /// <summary>
    /// Gets or sets connection initialization SQL script.
    /// </summary>
    public string ConnectionInitializationSql { get; set; }

    /// <summary>
    /// Gets or sets a value indicating that connection should be checked before actual usage.
    /// </summary>
    public bool EnsureConnectionIsAlive { get; set; }

    /// <summary>
    /// Gets connection handlers that should be notified about connection events.
    /// </summary>
    public IReadOnlyCollection<IConnectionHandler> ConnectionHandlers { get; private set; }

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <returns>Clone of this instance.</returns>
    public SqlDriverConfiguration Clone()
    {
      // no deep cloning
      var interceptors = (ConnectionHandlers.Count == 0)
        ? Array.Empty<IConnectionHandler>()
        : ConnectionHandlers.ToArray(ConnectionHandlers.Count);

      return new SqlDriverConfiguration {
        ForcedServerVersion = ForcedServerVersion,
        ConnectionInitializationSql = ConnectionInitializationSql,
        EnsureConnectionIsAlive = EnsureConnectionIsAlive,
        ConnectionHandlers = interceptors
      };
    }

    /// <summary>
    /// Creates new instance of this type.
    /// </summary>
    public SqlDriverConfiguration()
    {
      ConnectionHandlers = Array.Empty<IConnectionHandler>();
    }

    public SqlDriverConfiguration(IReadOnlyCollection<IConnectionHandler> connectionInterceptors)
    {
      ConnectionHandlers = connectionInterceptors;
    }
  }
}