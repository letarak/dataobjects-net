// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.01

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Internals;
using Xtensive.Storage.Model;
using Xtensive.Storage.ReferentialIntegrity;
using Xtensive.Storage.Resources;

namespace Xtensive.Storage
{
  /// <summary>
  /// Principal data objects about which information has to be managed. 
  /// It has a unique identity, independent existence, and forms the operational unit of consistency.
  /// Instance of <see cref="Entity"/> type can be referenced via <see cref="Key"/>.
  /// </summary>
  public abstract class Entity
    : Persistent,
      IEntity
  {
    private static readonly Dictionary<Type, Func<EntityData, Entity>> activators = new Dictionary<Type, Func<EntityData, Entity>>();
    private readonly EntityData data;

    #region Internal properties

    [DebuggerHidden]
    internal EntityData Data
    {
      get { return data; }
    }

    /// <exception cref="Exception">Property is already initialized.</exception>
    [Field]
    [DebuggerHidden]
    internal int TypeId
    {
      get { return GetValue<int>(Session.Domain.NameProvider.TypeId); }
      set
      {
        if (TypeId > 0)
          throw Exceptions.AlreadyInitialized(Session.Domain.NameProvider.TypeId);
        FieldInfo field = Type.Fields[Session.Domain.NameProvider.TypeId];
        field.GetAccessor<int>().SetValue(this, field, value);
      }
    }

    #endregion

    #region Properties: Key, Type, Tuple, PersistenceState

    /// <exception cref="Exception">Property is already initialized.</exception>
    [DebuggerHidden]
    public Key Key
    {
      get { return Data.Key; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    public override sealed TypeInfo Type
    {
      get { return Data.Type; }
    }

    /// <inheritdoc/>
    [DebuggerHidden]
    protected internal sealed override Tuple Tuple
    {
      get { return Data.Tuple; }
    }

    /// <summary>
    /// Gets persistence state of the entity.
    /// </summary>
    public PersistenceState PersistenceState
    {
      get { return Data.PersistenceState; }
      internal set
      {
        if (Data.PersistenceState == value)
          return;
        Data.PersistenceState = value;
        Session.DirtyItems.Register(Data);
      }
    }

    #endregion

    #region IIdentifier members

    /// <inheritdoc/>
    Key IIdentified<Key>.Identifier
    {
      get { return Key; }
    }

    /// <inheritdoc/>
    object IIdentified.Identifier
    {
      get { return Key; }
    }

    #endregion

    /// <summary>
    /// Removes the instance.
    /// </summary>
    public void Remove()
    {
      EnsureIsNotRemoved();
      Session.Persist();

      OnRemoving();
      ReferenceManager.ClearReferencesTo(this);
      PersistenceState = PersistenceState.Removed;
      OnRemoved();
    }

    #region Protected event-like methods

    /// <inheritdoc/>
    protected internal override sealed void OnCreating()
    {
      Session.IdentityMap.Add(Data);
      Session.DirtyItems.Register(Data);
      if (TypeId == 0)
        TypeId = Type.TypeId;
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnGettingValue(FieldInfo fieldInfo)
    {
      EnsureIsNotRemoved();
      EnsureIsFetched(fieldInfo);
    }

    /// <inheritdoc/>
    /// <exception cref="InvalidOperationException">Entity is removed.</exception>
    protected internal override sealed void OnSettingValue(FieldInfo fieldInfo)
    {
      EnsureIsNotRemoved();
    }

    /// <inheritdoc/>
    protected internal override sealed void OnSetValue(FieldInfo fieldInfo)
    {
      PersistenceState = PersistenceState.Modified;
    }

    protected virtual void OnRemoving()
    {
    }

    protected virtual void OnRemoved()
    {
    }

    #endregion

    #region Private \ internal methods

    internal static Entity Activate(Type type, EntityData data)
    {
      if (!activators.ContainsKey(type))
        throw new ArgumentException(String.Format("Type '{0}' was not registered for activation", type));
      return activators[type](data);
    }

    private void EnsureIsFetched(FieldInfo field)
    {
      if (Session.DirtyItems.GetItems(PersistenceState.New).Contains(Data))
        return;
      if (Data.Tuple.IsAvailable(field.MappingInfo.Offset))
        return;
      Fetcher.Fetch(Key, field);
    }

    private void EnsureIsNotRemoved()
    {
      if (PersistenceState==PersistenceState.Removed)
        throw new InvalidOperationException(Strings.ExEntityIsRemoved);
    }

    #endregion


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    protected Entity()
      : this(ArrayUtils<object>.EmptyArray)
    {
    }

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <param name="keyData">The values that will be used for key building.</param>
    /// <remarks>Use this type of constructor when you need to explicitly build key for this instance.</remarks>
    protected Entity(params object[] keyData)
    {
      TypeInfo type = Session.Domain.Model.Types[GetType()];
      Key key = Session.Domain.KeyManager.BuildPrimaryKey(type, keyData);
      DifferentialTuple tuple = new DifferentialTuple(Tuple.Create(type.TupleDescriptor));
      key.Tuple.Copy(tuple, 0);

      data = new EntityData(key, tuple, this);
      OnCreating();
    }

    /// <summary>
    /// <see cref="ClassDocTemplate()" copy="true"/>
    /// </summary>
    /// <param name="data">The initial data of this instance fetched from storage.</param>
    protected Entity(EntityData data)
      : base(data)
    {
      this.data = data;
      data.Entity = this;
    }
  }
}