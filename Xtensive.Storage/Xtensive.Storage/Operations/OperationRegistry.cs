// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2010.08.04

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage.Operations
{
  /// <summary>
  /// Operation registry for <see cref="Session"/> type.
  /// </summary>
  public sealed class OperationRegistry
  {
    private CompletableScope blockingScope;
    private bool isOperationRegistrationEnabled = true;
    private bool isUndoOperationRegistrationEnabled = true;
    private bool isSystemOperationRegistrationEnabled = true;
    private Deque<CompletableScope> scopes = new Deque<CompletableScope>();

    /// <summary>
    /// Gets the session this instance is bound to.
    /// </summary>
    internal Session Session { get; private set; }

    #region IsXxx properties

    /// <summary>
    /// Indicates whether operation logging is enabled.
    /// <see cref="IsSystemLogicOnly"/> and <see cref="SessionBound."/> implicitely turn this option off;
    /// <see cref="DisableOperationRegistration"/> does this explicitly.
    /// </summary>
    public bool IsRegistrationEnabled {
      get { return isOperationRegistrationEnabled || isUndoOperationRegistrationEnabled; }
    }

    /// <summary>
    /// Gets a value indicating whether system operation registration is enabled.
    /// </summary>
    public bool IsSystemOperationRegistrationEnabled {
      get { return isSystemOperationRegistrationEnabled; }
      internal set { isSystemOperationRegistrationEnabled = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this instance can register operation
    /// using <see cref="RegisterOperation"/> method.
    /// </summary>
    public bool CanRegisterOperation {
      get {
        var scope = scopes.TailOrDefault;
        return scope!=null && scope is OperationRegistrationScope;
      }
    }

    /// <summary>
    /// Gets a value indicating whether this instance is registering operation now,
    /// i.e. <see cref="BeginRegistration"/> method was incoked, but the
    /// scope isn't closed yet.
    /// </summary>
    public bool IsRegisteringOperation {
      get { return scopes.Count!=0; }
    }

    internal bool IsOutermostOperationRegistrationEnabled {
      get {
        return (
          OutermostOperationCompleted!=null || OutermostOperationStarting!=null || 
          NestedOperationCompleted!=null || NestedOperationStarting!=null
          ) && IsRegistrationEnabled;
      }
    }

    internal bool IsNestedOperationRegistrationEnabled {
      get { 
        return 
          (NestedOperationStarting!=null || NestedOperationCompleted!=null)
          && IsRegistrationEnabled; 
      }
    }

    internal bool IsOperationRegistrationEnabled {
      get {
        return (
          OutermostOperationCompleted!=null || OutermostOperationStarting!=null || 
          NestedOperationCompleted!=null || NestedOperationStarting!=null
          ) && isOperationRegistrationEnabled;
      }
    }

    internal bool IsUndoOperationRegistrationEnabled {
      get { return UndoOperation!=null && IsUndoOperationRegistrationEnabled; }
    }

    #endregion

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    public void RegisterOperation(Operation operation)
    {
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      if (scope.Operation!=null)
        throw new InvalidOperationException(Strings.ExOnlyOneOperationCanBeRegisteredInEachScope);
      operation.Type = scope.OperationType;
      operation.OuterOperation = scope.Parent==null ? null : scope.Parent.Operation;
      scope.Operation = operation;
    }

    public void OperationStarted()
    {
      if (!CanRegisterOperation)
        return;
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      var operation = scope.Operation;
      if (operation==null)
        throw new InvalidOperationException(Strings.ExOperationIsNotRegisteredYet);
      if (scope.IsOperationStarted)
        throw new InvalidOperationException(Strings.ExOperationStartedIsAlreadyCalledForThisOperation);
      if (operation.IsOutermost)
        NotifyOutermostOperationStarting(operation);
      else
        NotifyNestedOperationStarting(operation);
    }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operation">The operation to register.</param>
    public void RegisterUndoOperation(Operation operation)
    {
      var scope = GetCurrentOperationRegistrationScope();
      if (scope==null)
        return;
      if (scope.UndoOperations==null)
        scope.UndoOperations = new List<IOperation>();
      scope.UndoOperations.Add(operation);
      // Notifying...
      NotifyUndoOperation(operation);
    }

    /// <summary>
    /// Registers the entity identifier.
    /// </summary>
    /// <param name="key">The key of the entity to log the identifier for.</param>
    /// <param name="identifier">The entity identifier.
    /// <see langword="null" /> indicates identifier must be assigned automatically 
    /// as sequential number inside the current operation context.</param>
    public void RegisterEntityIdentifier(Key key, string identifier)
    {
      var scope = scopes.HeadOrDefault as OperationRegistrationScope;
      if (scope==null)
        return;
      scope.RegisterEntityIdentifier(key, identifier);
    }

    /// <summary>
    /// Temporarily disables undo operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    public IDisposable DisableUndoOperationRegistration()
    {
      if (!isUndoOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isUndoOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isUndoOperationRegistrationEnabled = previousState);
      isUndoOperationRegistrationEnabled = false;
      return result;
    }

    /// <summary>
    /// Temporarily disables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object enabling the logging back on its disposal.</returns>
    public IDisposable DisableSystemOperationRegistration()
    {
      if (!isSystemOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isSystemOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isSystemOperationRegistrationEnabled = previousState);
      isSystemOperationRegistrationEnabled = false;
      return result;
    }

    /// <summary>
    /// Temporarily enables system operation logging.
    /// </summary>
    /// <returns>An <see cref="IDisposable"/> object disabling the logging back on its disposal.</returns>
    public IDisposable EnableSystemOperationRegistration()
    {
      if (isSystemOperationRegistrationEnabled)
        return null;
      var result = new Disposable<OperationRegistry, bool>(this, isSystemOperationRegistrationEnabled,
        (disposing, _this, previousState) => _this.isSystemOperationRegistrationEnabled = previousState);
      isSystemOperationRegistrationEnabled = true;
      return result;
    }

    /// <summary>
    /// Registers the operation.
    /// </summary>
    /// <param name="operationType">Type of the operation.</param>
    /// <returns></returns>
    public CompletableScope BeginRegistration(OperationType operationType)
    {
      var currentScope = GetCurrentScope();
      if (currentScope == null) {
        if (!IsOutermostOperationRegistrationEnabled)
          return SetCurrentScope(blockingScope);
        else {
          if (((operationType & OperationType.System)==OperationType.System) && !IsSystemOperationRegistrationEnabled)
            return SetCurrentScope(blockingScope);
          else
            return SetCurrentScope(new OperationRegistrationScope(this, operationType));
        }
      }
      var currentOperationRegistrationScope = currentScope as OperationRegistrationScope;
      if (currentOperationRegistrationScope == null || !IsNestedOperationRegistrationEnabled)
        return SetCurrentScope(blockingScope);
      else {
        if (((operationType & OperationType.System)==OperationType.System) && !IsSystemOperationRegistrationEnabled)
          return SetCurrentScope(blockingScope);
        else
          return SetCurrentScope(new OperationRegistrationScope(this, operationType));
      }
    }

    internal void CloseOperationRegistrationScope(OperationRegistrationScope scope)
    {
      Operation operation = null;
      try {
        operation = (Operation) scope.Operation;
        if (operation == null)
          return;
        if (scope.PrecedingOperations!=null)
          operation.PrecedingOperations = new ReadOnlyList<IOperation>(scope.PrecedingOperations);
        if (scope.FollowingOperations!=null)
          operation.FollowingOperations = new ReadOnlyList<IOperation>(scope.FollowingOperations);
        if (scope.UndoOperations!=null)
          operation.UndoOperations = new ReadOnlyList<IOperation>(scope.UndoOperations);
        if (scope.KeyByIdentifier!=null)
          operation.IdentifiedEntities = new ReadOnlyDictionary<string, Key>(scope.KeyByIdentifier);
      }
      finally {
        RemoveCurrentScope(scope);
        if (operation != null) {
          // Adding it to parent scope's nested operations collection
          var parentScope = (OperationRegistrationScope) GetCurrentScope();
          if (parentScope != null) {
            if (!parentScope.IsOperationStarted) {
              if (parentScope.PrecedingOperations==null)
                parentScope.PrecedingOperations = new List<IOperation>();
              parentScope.PrecedingOperations.Add(operation);
            }
            else {
              if (parentScope.FollowingOperations==null)
                parentScope.FollowingOperations = new List<IOperation>();
              parentScope.FollowingOperations.Add(operation);
            }
          }
          // Notifying...
          if (operation.IsOutermost)
            NotifyOutermostOperationCompleted(operation, scope.IsCompleted);
          else
            NotifyNestedOperationCompleted(operation, scope.IsCompleted);
        }
      }
    }

    #region Events and notification methods

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is starting.
    /// </summary>
    public event EventHandler<OperationEventArgs> OutermostOperationStarting;

    /// <summary>
    /// Occurs when outermost <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs> OutermostOperationCompleted;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is starting.
    /// </summary>
    public event EventHandler<OperationEventArgs> NestedOperationStarting;

    /// <summary>
    /// Occurs when nested <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationCompletedEventArgs> NestedOperationCompleted;

    /// <summary>
    /// Occurs when undo <see cref="IOperation"/> is being registered.
    /// </summary>
    public event EventHandler<OperationEventArgs> UndoOperation;

    internal void NotifyOutermostOperationStarting(IOperation operation)
    {
      if (OutermostOperationStarting!=null && IsRegistrationEnabled)
        OutermostOperationStarting(this, new OperationEventArgs(operation));
    }

    internal void NotifyOutermostOperationCompleted(IOperation operation, bool isCompleted)
    {
      if (OutermostOperationCompleted!=null && IsRegistrationEnabled)
        OutermostOperationCompleted(this, new OperationCompletedEventArgs(operation, isCompleted));
    }

    internal void NotifyNestedOperationStarting(IOperation operation)
    {
      if (NestedOperationStarting!=null && IsRegistrationEnabled)
        NestedOperationStarting(this, new OperationEventArgs(operation));
    }

    internal void NotifyNestedOperationCompleted(IOperation operation, bool isCompleted)
    {
      if (NestedOperationCompleted!=null && IsRegistrationEnabled)
        NestedOperationCompleted(this, new OperationCompletedEventArgs(operation, isCompleted));
    }

    internal void NotifyUndoOperation(IOperation operation)
    {
      if (IsUndoOperationRegistrationEnabled)
        UndoOperation(this, new OperationEventArgs(operation));
    }

    #endregion

    #region Private \ internal methods

    internal CompletableScope GetCurrentScope()
    {
      return scopes.TailOrDefault;
    }

    private OperationRegistrationScope GetCurrentOperationRegistrationScope()
    {
      var scope = GetCurrentScope();
      if (scope==null)
        throw new InvalidOperationException(Strings.ExNoOperationRegistrationScope);
      return scope as OperationRegistrationScope;
    }

    internal CompletableScope SetCurrentScope(CompletableScope scope)
    {
      scopes.AddTail(scope);
      return scope;
    }

    internal void RemoveCurrentScope(CompletableScope scope)
    {
      if (scopes.TailOrDefault!=scope)
        throw new InvalidOperationException(Strings.ExInvalidScopeDisposalOrder);
      scopes.ExtractTail();
    }

    internal long GetNextIdentifier()
    {
      var scope = scopes.HeadOrDefault as OperationRegistrationScope;
      return scope==null ? -1 : scope.CurrentIdentifier++;
    }

    #endregion


    // Constructors

    internal OperationRegistry(Session session)
    {
      Session = session;
      blockingScope = new BlockingOperationRegistrationScope(this);
    }
  }
}