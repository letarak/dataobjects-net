// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.02.09

using System;
using System.Data.Common;
using Xtensive.Storage.Providers.Sql.Resources;

namespace Xtensive.Storage.Providers.Sql
{
  public partial class SessionHandler
  {
    // Implementation of IDirectSqlService

    /// <inheritdoc/>
    DbConnection IDirectSqlService.Connection {
      get {
        var sqlConnection = Connection;
        return sqlConnection==null ? null : sqlConnection.UnderlyingConnection;
      }
    }

    /// <inheritdoc/>
    DbTransaction IDirectSqlService.Transaction {
      get {
        var sqlConnection = Connection;
        return sqlConnection==null ? null : sqlConnection.ActiveTransaction;
      }
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Connection is not open.</exception>
    DbCommand IDirectSqlService.CreateCommand()
    {
      var sqlConnection = Connection;
      if (sqlConnection==null)
        throw new InvalidOperationException(Strings.ExConnectionIsNotOpen);
      return sqlConnection.CreateCommand();
    }
  }
}