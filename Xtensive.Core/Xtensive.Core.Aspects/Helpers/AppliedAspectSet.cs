// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.07.17

using System;
using System.Collections.Generic;

namespace Xtensive.Core.Aspects.Helpers
{
  /// <summary>
  /// A helper class allowing to apply the particular aspect just once.
  /// </summary>
  public static class AppliedAspectSet
  {
    private static readonly Dictionary<Pair<Type, object>, object> aspects = 
      new Dictionary<Pair<Type, object>, object>();

    /// <summary>
    /// Adds a new aspect created by <paramref name="generator"/> 
    /// with the specified key to the set, if there is no aspect with the same key; 
    /// otherwise, does nothing.
    /// </summary>
    /// <typeparam name="T">The type of aspect to add.</typeparam>
    /// <param name="key">The key of aspect to add.</param>
    /// <param name="generator">The aspect generator.</param>
    /// <returns>A generated aspect, if aspect with the specified 
    /// <paramref name="key"/> was not found; 
    /// otherwise, <see langword="null" />.</returns>
    public static T Add<T>(object key, Func<T> generator)
      where T : class
    {
      var tType = typeof (T);
      var fullKey = new Pair<Type, object>(tType, key);
      lock (aspects) {
        object result;
        if (!aspects.TryGetValue(fullKey, out result))
          aspects.Add(fullKey, result = generator.Invoke());
        else
          result = null;
        return (T) result;
      }
    }

    /// <summary>
    /// Adds an <paramref name="aspect"/> with the specified key to the set, 
    /// if there is no aspect with the same key; 
    /// otherwise, combines the <paramref name="aspect"/>
    /// with the existing one using <paramref name="combiner"/>.
    /// </summary>
    /// <typeparam name="T">The type of aspect to add.</typeparam>
    /// <param name="key">The key of aspect to add.</param>
    /// <param name="aspect">The aspect to add.</param>
    /// <param name="combiner">The aspect combiner. 
    /// Its first argument is an existing aspect, that should be modified; 
    /// the second one is <paramref name="aspect"/>.</param>
    /// <returns>A generated aspect, if aspect with the specified 
    /// <paramref name="key"/> was not found; 
    /// otherwise, <see langword="null" />.</returns>
    public static T AddOrCombine<T>(object key, T aspect, Action<T,T> combiner)
      where T : class
    {
      var tType = typeof (T);
      var fullKey = new Pair<Type, object>(tType, key);
      lock (aspects) {
        object result;
        if (!aspects.TryGetValue(fullKey, out result))
          aspects.Add(fullKey, result = aspect);
        else {
          combiner.Invoke((T) result, aspect);
          result = null;
        }
        return (T) result;
      }
    }
  }
}