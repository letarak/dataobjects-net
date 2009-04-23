// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.16

using System;
using System.Reflection;
using Xtensive.Core.Collections;
using Xtensive.Core.Disposing;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Resources;

namespace Xtensive.Modelling.Comparison
{
  /// <summary>
  /// Holds current state of the <see cref="Upgrader"/>.
  /// </summary>
  public class UpgradeContext
  {
    /// <summary>
    /// Gets the upgrader this context is created for.
    /// </summary>
    public Upgrader Upgrader { get; private set; }

    /// <summary>
    /// Gets the parent upgrade context.
    /// </summary>
    public UpgradeContext Parent { get; private set; }

    /// <summary>
    /// Gets or sets the currently processed difference.
    /// </summary>
    public Difference Difference { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="UpgradeActionType.PreCondition"/> action sequence.
    /// </summary>
    public GroupingNodeAction PreConditions { get; set; }

    /// <summary>
    /// Gets or sets the current action sequence.
    /// </summary>
    public GroupingNodeAction Actions { get; set; }

    /// <summary>
    /// Gets or sets the current <see cref="UpgradeActionType.PostCondition"/> action sequence.
    /// </summary>
    public GroupingNodeAction PostConditions { get; set; }

    /// <summary>
    /// Gets or sets the currently processed property.
    /// </summary>
    public string Property { get; set; }

    /// <summary>
    /// Indicates whether node must be copied rather than processed as usual.
    /// </summary>
    public bool MustClone { get; set; }

    /// <summary>
    /// Gets or sets the type of the dependency root.
    /// </summary>
    public Type DependencyRootType { get; set; }

    /// <summary>
    /// Activates this instance.
    /// </summary>
    /// <returns>A disposable object deactivating it.</returns>
    /// <exception cref="InvalidOperationException">Invalid context activation sequence.</exception>
    public IDisposable Activate()
    {
      return Activate(true);
    }

    /// <summary>
    /// Merges the sequence of 
    /// <see cref="PreConditions"/>, <see cref="Actions"/> and <see cref="PostConditions"/>
    /// and returns the result.
    /// Sets all these properties to <see langword="null" />.
    /// </summary>
    /// <returns></returns>
    public GroupingNodeAction GetMergeActions()
    {
      if (PreConditions!=null && PreConditions.Actions.Count!=0) {
        Actions.Actions.Insert(0, PreConditions);
        PreConditions = null;
      }
      if (PostConditions!=null && PostConditions.Actions.Count!=0) {
        Actions.Actions.Add(PostConditions);
        PostConditions = null;
      }
      return Actions;
    }

    /// <summary>
    /// Activates this instance.
    /// </summary>
    /// <param name="safely">If <see langword="true" />,
    /// a check that <see cref="Parent"/> is active must be performed.</param>
    /// <returns>A disposable object deactivating it.</returns>
    /// <exception cref="InvalidOperationException">Invalid context activation sequence.</exception>
    public IDisposable Activate(bool safely)
    {
      if (safely && Upgrader.Context!=Parent)
        throw new InvalidOperationException(Strings.ExInvalidContextActivationSequence);
      var oldContext = Upgrader.Context;
      Upgrader.Context = this;
      return new Disposable<UpgradeContext, UpgradeContext>(this, oldContext,
        (isDisposing, _this, _oldContext) => {
          if (_this.Upgrader.Context!=_this)
            throw new InvalidOperationException(Strings.ExInvalidContextDeactivationSequence);
          _this.Upgrader.Context = _oldContext;
        });
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">No current Comparer.</exception>
    public UpgradeContext()
    {
      Upgrader = Upgrader.Current;
      if (Upgrader==null)
        throw new InvalidOperationException(Strings.ExNoCurrentUpgrader);
      Parent = Upgrader.Context;
      if (Parent==null) {
        PreConditions = new GroupingNodeAction();
        Actions = new GroupingNodeAction();
        PostConditions = new GroupingNodeAction();
        return;
      }

      Difference = Parent.Difference;
      Property = Parent.Property;
      MustClone = Parent.MustClone;
      DependencyRootType = Parent.DependencyRootType;

      PreConditions = Parent.PreConditions;
      Actions = Parent.Actions;
      PostConditions = Parent.PostConditions;
    }
  }
}