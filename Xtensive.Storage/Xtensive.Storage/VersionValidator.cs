// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.11.10

using System;
using System.Collections.Generic;
using Xtensive.Core;
using System.Linq;
using Xtensive.Core.Aspects;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Core.Tuples.Transform;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.Resources;
using Xtensive.Storage.Rse;

namespace Xtensive.Storage
{
  /// <summary>
  /// An attachable service validating versions inside the specified <see cref="Session"/>.
  /// </summary>
  [Infrastructure]
  public sealed class VersionValidator : SessionBound, 
    IDisposable
  {
    private HashSet<Key> processed = new HashSet<Key>();
    private Dictionary<Key, VersionInfo> knownVersions;
    private Dictionary<Key, VersionInfo> queuedVersions;
    private Dictionary<Key, QueryTask> fetchVersionTasks;
    private readonly Func<Key, VersionInfo> expectedVersionProvider;
    private bool isAttached;

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ValidateVersion(Key key, VersionInfo version)
    {
      var expectedVersion = expectedVersionProvider.Invoke(key);
      if (expectedVersion.IsVoid)
        return true;
      else
        return expectedVersion==version;
    }

    /// <summary>
    /// Validates the <paramref name="version"/>
    /// for the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key to validate version for.</param>
    /// <param name="version">The version to validate.</param>
    /// <param name="throwOnFailure">Indicates whether <see cref="InvalidOperationException"/>
    /// must be thrown on validation failure.</param>
    /// <returns>
    /// <see langword="True"/>, if validation passes successfully;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="VersionConflictException">Version conflict is detected.</exception>
    public bool ValidateVersion(Key key, VersionInfo version, bool throwOnFailure)
    {
      var result = ValidateVersion(key, version);
      if (throwOnFailure && !result)
        throw new VersionConflictException(string.Format(
          Strings.ExVersionOfEntityWithKeyXDiffersFromTheExpectedOne, key));
      return result;
    }

    #region Valiadtor logic

    private void EnqueueVersionValidation(Entity entity)
    {
      if (entity.Type.VersionInfoTupleExtractor==null
        || queuedVersions.ContainsKey(entity.Key)
        || processed.Contains(entity.Key))
        return;
      VersionInfo version;
      if (knownVersions.TryGetValue(entity.Key, out version))
        queuedVersions.Add(entity.Key, version);
      else {
        var queryTask = CreateFetchVersionTask(entity.Key);
        Session.RegisterDelayedQuery(queryTask);
        fetchVersionTasks.Add(entity.Key, queryTask);
      }
    }

    private void CreateFetchVersionTasks()
    {
      var registry = Session.EntityChangeRegistry;
      foreach (var item in registry.GetItems(PersistenceState.New))
        processed.Add(item.Key);
      foreach (var item in registry.GetItems(PersistenceState.Modified)) {
        EnqueueVersionValidation(item.Entity);
        processed.Add(item.Key);
      }
      foreach (var item in registry.GetItems(PersistenceState.Removed)) {
        EnqueueVersionValidation(item.Entity);
        processed.Add(item.Key);
      }

//      var changedItem =
//        registry.GetItems(PersistenceState.Modified)
//          .Concat(registry.GetItems(PersistenceState.Removed))
//          .Where(item => item.Type.VersionExtractor!=null
//            && !processed.Contains(item.Key));
//      foreach (var item in changedItem) {
//        if (queuedVersions.ContainsKey(item.Key)
//          || fetchVersionTasks.ContainsKey(item.Key))
//          continue;
//        VersionInfo version;
//        if (actualVersions.TryGetValue(item.Key, out version))
//          queuedVersions.Add(item.Key, version);
//        else {
//          var queryTask = CreateFetchVersionTask(item.Key);
//          Session.RegisterDelayedQuery(queryTask);
//          fetchVersionTasks.Add(item.Key, queryTask);
//        }
//      }
    }

    private QueryTask CreateFetchVersionTask(Key key)
    {
      var type = key.Type;
      var provider = type.Indexes.PrimaryIndex.ToRecordSet().Seek(key.Value).Provider;
      var execProvider = Session.CompilationContext.Compile(provider);
      return new QueryTask(execProvider, null);
    }

    private static VersionInfo FetchVersion(TypeInfo type, Tuple state)
    {
      if (state==null)
        return new VersionInfo();
      var versionTuple = type.VersionInfoTupleExtractor.Apply(TupleTransformType.Tuple, state);
      return new VersionInfo(versionTuple);
    }

    #endregion

    #region Event handlers

    private void OnTransactionOpened(object sender, TransactionEventArgs e)
    {
      processed = new HashSet<Key>();
      knownVersions = new Dictionary<Key, VersionInfo>();
    }

    private void OnTransactionClosed(object sender, TransactionEventArgs e)
    {
      processed = null;
      knownVersions = null;
    }

    private void OnEntityVersionInfoChanging(object sender, EntityVersionInfoChangedEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnEntityRemoving(object sender, EntityEventArgs e)
    {
      OnEntityChanging(e.Entity);
    }

    private void OnEntityChanging(Entity entity)
    {
      // "return" here means "we can't rely on Entity.VersionInfo,
      // and so it must be fetched on Session.Persist, or
      // there is nothing to validate"
      if (entity.PersistenceState==PersistenceState.New)
        return;
      if (entity.Type.VersionInfoTupleExtractor==null)
        return;
      if (entity.State.IsStale && !entity.Type.HasVersionRoots)
        return;
      // Here we know the actual version is stored in VersionInfo
      if (knownVersions.ContainsKey(entity.Key))
        return;
      if (processed.Contains(entity.Key))
        return;
      knownVersions.Add(entity.Key, entity.VersionInfo);
    }

    private void OnPersisting(object sender, EventArgs e)
    {
      queuedVersions = new Dictionary<Key, VersionInfo>();
      fetchVersionTasks = new Dictionary<Key, QueryTask>();
      CreateFetchVersionTasks();
      if (fetchVersionTasks.Count > 0)
        Session.Handler.ExecuteQueryTasks(fetchVersionTasks.Values, true);
    }

    private void OnPersisted(object sender, EventArgs e)
    {
      if (fetchVersionTasks.Count > 0)
        foreach (var task in fetchVersionTasks) {
          var key = task.Key;
          var version = FetchVersion(task.Key.Type, task.Value.Result.FirstOrDefault());
          queuedVersions.Add(key, version);
        }
      foreach (var pair in queuedVersions)
        ValidateVersion(pair.Key, pair.Value, true);
    }

    #endregion

    #region AttachEventHandlers \ DetachEventHandlers methods

    private void AttachEventHandlers()
    {
      if (isAttached)
        throw new InvalidOperationException(Strings.ExTheServiceIsAlreadyAttachedToSession);
      isAttached = true;
      try {
        Session.TransactionOpened += OnTransactionOpened;
        Session.TransactionCommitted += OnTransactionClosed;
        Session.TransactionRollbacked += OnTransactionClosed;
        Session.EntityVersionInfoChanging += OnEntityVersionInfoChanging;
        Session.EntityRemoving += OnEntityRemoving;
        Session.Persisting += OnPersisting;
        Session.Persisted += OnPersisted;
      }
      catch {
        DetachEventHandlers();
        throw;
      }
    }

    private void DetachEventHandlers()
    {
      if (isAttached) {
        isAttached = false;
        Session.TransactionOpened -= OnTransactionOpened;
        Session.TransactionCommitted -= OnTransactionClosed;
        Session.TransactionRollbacked -= OnTransactionClosed;
        Session.EntityVersionInfoChanging -= OnEntityVersionInfoChanging;
        Session.EntityRemoving -= OnEntityRemoving;
        Session.Persisting -= OnPersisting;
        Session.Persisted -= OnPersisted;
      }
    }

    #endregion

    #region Attach methods (factory methods)

    /// <summary>
    /// Attaches the validator to the current session.
    /// </summary>
    /// <param name="expectedVersions">The set containing expected versions.</param>
    /// <returns>
    /// A newly created <see cref="VersionValidator"/> attached
    /// to the current session.
    /// </returns>
    public static VersionValidator Attach(VersionSet expectedVersions)
    {
      return Attach(Session.Demand(), expectedVersions);
    }

    /// <summary>
    /// Attaches the validator to the specified session.
    /// </summary>
    /// <param name="session">The session to attach validator to.</param>
    /// <param name="expectedVersions">The set containing expected versions.</param>
    /// <returns>
    /// A newly created <see cref="VersionValidator"/> attached
    /// to the specified <paramref name="session"/>.
    /// </returns>
    public static VersionValidator Attach(Session session, VersionSet expectedVersions)
    {
      return new VersionValidator(session, expectedVersions.Get);
    }

    /// <summary>
    /// Attaches the validator to the current session.
    /// </summary>
    /// <param name="expectedVersionProvider">The expected version provider.</param>
    /// <returns>A newly created <see cref="VersionValidator"/> attached
    /// to the current session.</returns>
    public static VersionValidator Attach(Func<Key, VersionInfo> expectedVersionProvider)
    {
      return Attach(Session.Demand(), expectedVersionProvider);
    }

    /// <summary>
    /// Attaches the validator to the specified session.
    /// </summary>
    /// <param name="session">The session to attach validator to.</param>
    /// <param name="expectedVersionProvider">The expected version provider.</param>
    /// <returns>A newly created <see cref="VersionValidator"/> attached
    /// to the specified <paramref name="session"/>.</returns>
    public static VersionValidator Attach(Session session, Func<Key, VersionInfo> expectedVersionProvider)
    {
      return new VersionValidator(session, expectedVersionProvider);
    }

    #endregion


    // Constructors

    /// <exception cref="InvalidOperationException">Session is persisting the changes.</exception>
    private VersionValidator(Session session, Func<Key, VersionInfo> expectedVersionProvider)
      : base(session)
    {
      ArgumentValidator.EnsureArgumentNotNull(expectedVersionProvider, "expectedVersionProvider");
      if (session.IsPersisting)
        throw new InvalidOperationException(
          Strings.ExServiceCanNotBeAttachedToSessionWhileItIsPersistingTheChanges);

      this.expectedVersionProvider = expectedVersionProvider;
      AttachEventHandlers();
    }
  
    // Dispose
    
    /// <see cref="DisposableDocTemplate.Dispose()" copy="true"/>
    public void Dispose()
    {
      DetachEventHandlers();
    }
  }
}