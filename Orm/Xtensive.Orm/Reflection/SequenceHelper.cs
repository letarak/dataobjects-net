// Copyright (C) 2009-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexis Kochetov
// Created:    2009.04.17

using System;
using System.Collections.Generic;

namespace Xtensive.Reflection
{
  /// <summary>
  /// <see cref="IEnumerable{T}"/> helper methods.
  /// </summary>
  public static class SequenceHelper
  {
    /// <summary>
    /// Gets the generic type of <see cref="IEnumerable{T}"/> where <paramref name="elementType"/> is generic argument.
    /// </summary>
    /// <param name="elementType">Type of the element.</param>
    public static Type GetSequenceType(Type elementType)
    {
      return WellKnownInterfaces.EnumerableOfT.MakeGenericType(elementType);
    }

    /// <summary>
    /// Gets element type of the sequence.
    /// </summary>
    /// <param name="sequenceType">Type of the sequence.</param>
    public static Type GetElementType(Type sequenceType)
    {
      Type ienum = FindIEnumerable(sequenceType);
      return ienum == null
               ? sequenceType
               : ienum.GetGenericArguments()[0];
    }

    private static Type FindIEnumerable(Type sequenceType)
    {
      if (sequenceType == null || sequenceType == WellKnownTypes.String)
        return null;
      if (sequenceType.IsArray)
        return WellKnownInterfaces.EnumerableOfT.MakeGenericType(sequenceType.GetElementType());
      if (sequenceType.IsGenericType)
        foreach (Type arg in sequenceType.GetGenericArguments()) {
          Type enumerable = WellKnownInterfaces.EnumerableOfT.MakeGenericType(arg);
          if (enumerable.IsAssignableFrom(sequenceType))
            return enumerable;
        }
      Type[] interfaces = sequenceType.GetInterfaces();
      if (interfaces != null && interfaces.Length > 0)
        foreach (Type @interface in interfaces) {
          Type enumerable = FindIEnumerable(@interface);
          if (enumerable != null)
            return enumerable;
        }
      if (sequenceType.BaseType != null && sequenceType.BaseType != WellKnownTypes.Object)
        return FindIEnumerable(sequenceType.BaseType);
      return null;
    }
  }
}