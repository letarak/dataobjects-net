// Copyright (C) 2009-2021 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Denis Krjuchkov
// Created:    2009.08.14

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Orm.Configuration;
using Xtensive.Sql;

namespace Xtensive.Orm.Providers
{
  public partial class StorageDriver
  {
    private sealed class InitializationSqlExtension
    {
      public string Script;
    }

    public void ApplyNodeConfiguration(SqlConnection connection, NodeConfiguration nodeConfiguration)
    {
      if (connection.State != ConnectionState.Closed
        && !nodeConfiguration.NodeId.Equals(WellKnown.DefaultNodeId, StringComparison.Ordinal)) {
        throw new InvalidOperationException(Strings.ExCannotApplyNodeConfigurationSettingsConnectionIsInUse);
      }

      if (nodeConfiguration.ConnectionInfo != null) {
        connection.ConnectionInfo = nodeConfiguration.ConnectionInfo;
      }

      if (!string.IsNullOrEmpty(nodeConfiguration.ConnectionInitializationSql)) {
        SetInitializationSql(connection, nodeConfiguration.ConnectionInitializationSql);
      }
    }

    public SqlConnection CreateConnection(Session session)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXCreatingConnection, session.ToStringSafely());

      SqlConnection connection;
      try {
        connection = underlyingDriver.CreateConnection();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }

      if (connectionAccessorFactories != null) {
        connection.AssignConnectionAccessors(
          CreateConnectionAccessorsFast(configuration.Types.DbConnectionAccessors));
      }

      var sessionConfiguration = GetConfiguration(session);
      connection.CommandTimeout = sessionConfiguration.DefaultCommandTimeout;
      var connectionInfo = GetConnectionInfo(session) ?? sessionConfiguration.ConnectionInfo;
      if (connectionInfo != null) {
        connection.ConnectionInfo = connectionInfo;
      }

      var connectionInitializationSql = GetInitializationSql(session) ?? configuration.ConnectionInitializationSql;
      if (!string.IsNullOrEmpty(connectionInitializationSql))
        SetInitializationSql(connection, connectionInitializationSql);

      return connection;
    }

    public void OpenConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXOpeningConnectionY, session.ToStringSafely(), connection.ConnectionInfo);

      var script = connection.Extensions.Get<InitializationSqlExtension>()?.Script;

      try {
        if (!string.IsNullOrEmpty(script)) {
          connection.OpenAndInitialize(script);
        }
        else {
          connection.Open();
        }
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public Task OpenConnectionAsync(Session session, SqlConnection connection)
    {
      return OpenConnectionAsync(session, connection, CancellationToken.None);
    }

    public async Task OpenConnectionAsync(Session session, SqlConnection connection, CancellationToken cancellationToken)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXOpeningConnectionY, session.ToStringSafely(), connection.ConnectionInfo);

      var script = connection.Extensions.Get<InitializationSqlExtension>()?.Script;

      try {
        if (!string.IsNullOrEmpty(script)) {
          await connection.OpenAndInitializeAsync(script, cancellationToken).ConfigureAwait(false);
        }
        else {
          await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        }
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void EnsureConnectionIsOpen(Session session, SqlConnection connection)
    {
      if (connection.State != ConnectionState.Open) {
        OpenConnection(session, connection);
      }
    }

    public async Task EnsureConnectionIsOpenAsync(Session session, SqlConnection connection)
    {
      if (connection.State != ConnectionState.Open) {
        await OpenConnectionAsync(session, connection).ConfigureAwait(false);
      }
    }

    public void CloseConnection(Session session, SqlConnection connection)
    {
      if (connection.State!=ConnectionState.Open)
        return;

      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXClosingConnectionY, session.ToStringSafely(), connection.ConnectionInfo);

      try {
        connection.Close();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void DisposeConnection(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXDisposingConnection, session.ToStringSafely());

      try {
        connection.Dispose();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void BeginTransaction(Session session, SqlConnection connection, IsolationLevel? isolationLevel)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXBeginningTransactionWithYIsolationLevel, session.ToStringSafely(), isolationLevel);

      if (isolationLevel==null)
        isolationLevel = IsolationLevelConverter.Convert(GetConfiguration(session).DefaultIsolationLevel);

      try {
        connection.BeginTransaction(isolationLevel.Value);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void CommitTransaction(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXCommitTransaction, session.ToStringSafely());

      try {
        connection.Commit();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void RollbackTransaction(Session session, SqlConnection connection)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXRollbackTransaction, session.ToStringSafely());

      try {
        connection.Rollback();
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void MakeSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXMakeSavepointY, session.ToStringSafely(), name);

      if (!hasSavepoints)
        return; // Driver does not support savepoints, so let's fail later (on rollback)

      try {
        connection.MakeSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    public void RollbackToSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXRollbackToSavepointY, session.ToStringSafely(), name);

      if (!hasSavepoints)
        throw new NotSupportedException(Strings.ExCurrentStorageProviderDoesNotSupportSavepoints);

      try {
        connection.RollbackToSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }
 
    public void ReleaseSavepoint(Session session, SqlConnection connection, string name)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXReleaseSavepointY, session.ToStringSafely(), name);

      try {
        connection.ReleaseSavepoint(name);
      }
      catch (Exception exception) {
        throw ExceptionBuilder.BuildException(exception);
      }
    }

    #region Sync Execute methods

    public int ExecuteNonQuery(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteNonQuery());
    }

    public object ExecuteScalar(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteScalar());
    }

    public DbDataReader ExecuteReader(Session session, DbCommand command)
    {
      return ExecuteCommand(session, command, c => c.ExecuteReader());
    }

    #endregion

    #region Async Execute methods

    public Task<int> ExecuteNonQueryAsync(Session session, DbCommand command)
    {
      return ExecuteCommandAsync(session, command, CancellationToken.None,
        (c, ct) => c.ExecuteNonQueryAsync(ct));
    }

    public Task<int> ExecuteNonQueryAsync(Session session, DbCommand command, CancellationToken cancellationToken)
    {
      return ExecuteCommandAsync(session, command, cancellationToken,
        (c, ct) => c.ExecuteNonQueryAsync(ct));
    }

    public Task<object> ExecuteScalarAsync(Session session, DbCommand command)
    {
      return ExecuteCommandAsync(session, command, CancellationToken.None,
        (c, ct) => c.ExecuteScalarAsync(ct));
    }

    public Task<object> ExecuteScalarAsync(Session session, DbCommand command, CancellationToken cancellationToken)
    {
      return ExecuteCommandAsync(session, command, cancellationToken,
        (c, ct) => c.ExecuteScalarAsync(ct));
    }

    public Task<DbDataReader> ExecuteReaderAsync(Session session, DbCommand command)
    {
      return ExecuteCommandAsync(session, command, CancellationToken.None,
        (c, ct) => c.ExecuteReaderAsync(ct));
    }

    public Task<DbDataReader> ExecuteReaderAsync(Session session, DbCommand command, CancellationToken cancellationToken)
    {
      return ExecuteCommandAsync(session, command, cancellationToken, 
        (c, ct) => c.ExecuteReaderAsync(ct));
    }

    #endregion

    private TResult ExecuteCommand<TResult>(Session session, DbCommand command, Func<DbCommand, TResult> action)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXQueryY, session.ToStringSafely(), command.ToHumanReadableString());

      session?.Events.NotifyDbCommandExecuting(command);

      TResult result;
      try {
        result = action.Invoke(command);
      }
      catch (Exception exception) {
        var wrapped = ExceptionBuilder.BuildException(exception, command.ToHumanReadableString());
        session?.Events.NotifyDbCommandExecuted(command, wrapped);
        throw wrapped;
      }

      session?.Events.NotifyDbCommandExecuted(command);

      return result;
    }

    private async Task<TResult> ExecuteCommandAsync<TResult>(Session session, DbCommand command, CancellationToken cancellationToken, Func<DbCommand, CancellationToken, Task<TResult>> action)
    {
      if (isLoggingEnabled)
        SqlLog.Info(Strings.LogSessionXQueryY, session.ToStringSafely(), command.ToHumanReadableString());

      cancellationToken.ThrowIfCancellationRequested();
      session?.Events.NotifyDbCommandExecuting(command);

      TResult result;
      try {
        result = await action(command, cancellationToken).ConfigureAwait(false);
      }
      catch (OperationCanceledException) {
        throw;
      }
      catch (Exception exception) {
        var wrapped = ExceptionBuilder.BuildException(exception, command.ToHumanReadableString());
        session?.Events.NotifyDbCommandExecuted(command, wrapped);
        throw wrapped;
      }

      session?.Events.NotifyDbCommandExecuted(command);

      return result;
    }

    private void SetInitializationSql(SqlConnection connection, string script)
    {
      var extension = connection.Extensions.Get<InitializationSqlExtension>();
      if (extension==null) {
        extension = new InitializationSqlExtension();
        connection.Extensions.Set(extension);
      }
      extension.Script = script;
    }

    private SessionConfiguration GetConfiguration(Session session)
    {
      return session != null ? session.Configuration : configuration.Sessions.System;
    }

    private ConnectionInfo GetConnectionInfo(Session session)
    {
      return session == null
        ? null
        : session.GetStorageNodeInternal()?.Configuration.ConnectionInfo
          ?? session.Configuration.ConnectionInfo;
    }

    private string GetInitializationSql(Session session)
    {
      return session == null || session.GetStorageNodeInternal() == null
        || string.IsNullOrEmpty(session.GetStorageNodeInternal().Configuration.ConnectionInitializationSql)
        ? null
        : session.StorageNode.Configuration.ConnectionInitializationSql;
    }
  }
}