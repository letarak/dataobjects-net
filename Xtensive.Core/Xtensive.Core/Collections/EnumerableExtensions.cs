// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.05.16

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xtensive.Core.Comparison;

namespace Xtensive.Core.Collections
{
  /// <summary>
  /// <see cref="IEnumerable{T}"/> related extension methods.
  /// </summary>
  public static class EnumerableExtensions
  {
    /// <summary>
    /// Calculates hash code of <see cref="IEnumerable"/> by XOR hash codes of it's items.
    /// </summary>
    ///<param name="items">Enumerable to calculate hash for.</param>
    /// <typeparam name="TItem">The type of item.</typeparam>
    ///<returns>Hash code, calculated by enumerable items. If enumerable is null or empty returns 0.</returns>
    public static int GetHashCodeRecursive<TItem>(this IEnumerable<TItem> items)
    {
      if (items==null) 
        return 0;
      return items.Aggregate(0, (previousValue, item) => previousValue ^ item.GetHashCode());
    }


    /// <summary>
    /// Indicates whether enumerable is empty or not 
    /// by attempting to cast it to <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> and <see cref="IQueryable{T}"/>.
    /// May return false negative response.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to check.</param>
    /// <returns><see langword="True"/> if collection is definitely <see langword="null"/> or empty;
    /// otherwise, <see langword="false"/>.</returns>
    public static bool IsNullOrEmpty<TItem>(this IEnumerable<TItem> items)
    {
      if (items==null)
        return true;
      long? count = items.TryGetCount();
      if (!count.HasValue)
        return false;
      return count.GetValueOrDefault()==0;
    }

    /// <summary>
    /// Gets the count of items (as <see cref="long"/>) of <see cref="IEnumerable{T}"/>, if it is actually
    /// <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> or <see cref="IQueryable{T}"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to get count of.</param>
    /// <returns>The count of items, if it's possible to get it;
    /// otherwise, <see langword="null"/>.</returns>
    public static long? TryGetLongCount<TItem>(this IEnumerable<TItem> items)
    {
      {
        var c1 = items as ICollection<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as ICountable<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as IQueryable<TItem>;
        if (c1 != null)
          return c1.LongCount();
      }
      return null;
    }

    /// <summary>
    /// Gets the count of items of <see cref="IEnumerable{T}"/>, if it is actually
    /// <see cref="ICollection{T}"/>, <see cref="ICountable{TItem}"/> or <see cref="IQueryable{T}"/>.
    /// Otherwise returns <see langword="null"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">Items to get count of.</param>
    /// <returns>The count of items, if it's possible to get it;
    /// otherwise, <see langword="null"/>.</returns>
    public static int? TryGetCount<TItem>(this IEnumerable<TItem> items)
    {
      {
        var c1 = items as ICollection<TItem>;
        if (c1 != null)
          return c1.Count;
      }
      {
        var c1 = items as ICountable<TItem>;
        if (c1 != null)
          return (int)c1.Count;
      }
      {
        var c1 = items as IQueryable<TItem>;
        if (c1 != null)
          return (int)c1.LongCount();
      }
      return null;
    }

    /// <summary>
    /// Converts the elements of <paramref name="source"/> sequence 
    /// using specified <paramref name="converter"/>.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <typeparam name="TNewItem">The type of item to convert to.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="converter">A delegate that converts each element.</param>
    /// <returns>A sequence of converted elements.</returns>
    public static IEnumerable<TNewItem> Convert<TItem, TNewItem>(this IEnumerable<TItem> source, Converter<TItem, TNewItem> converter)
    {
      ArgumentValidator.EnsureArgumentNotNull(converter, "converter");
      foreach (TItem item in source)
        yield return converter(item);
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to comma-delimited string.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>Comma-delimited string containing string representation 
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToCommaDelimitedString<TItem>(this IEnumerable<TItem> source)
    {
      return ToDelimitedString((IEnumerable) source, ", ");
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to comma-delimited string.
    /// </summary>
    /// <param name="source">The sequence to convert.</param>
    /// <returns>Comma delimited string combining string representations
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToCommaDelimitedString(this IEnumerable source)
    {
      return source.ToDelimitedString(", ");
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to a delimited string.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="delimiter">The delimiter.</param>
    /// <returns>A delimited string combining string representations
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToDelimitedString<TItem>(this IEnumerable<TItem> source, string delimiter)
    {
      if (source==null)
        return String.Empty;
      StringBuilder sb = new StringBuilder();
      bool prefixWithComma = false;
      foreach (var item in source) {
        if (prefixWithComma)
          sb.Append(delimiter);
        sb.Append(item.ToString());
        prefixWithComma = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Converts the <paramref name="source"/> to delimited string.
    /// </summary>
    /// <param name="source">The sequence to convert.</param>
    /// <param name="separator">The delimiter.</param>
    /// <returns>Delimited string containing string representation 
    /// of all the items from <paramref name="source"/>.</returns>
    public static string ToDelimitedString(this IEnumerable source, string separator)
    {
      if (source==null)
        return String.Empty;
      StringBuilder sb = new StringBuilder();
      bool prefixWithComma = false;
      foreach (object item in source) {
        if (prefixWithComma)
          sb.Append(separator);
        sb.Append(item.ToString());
        prefixWithComma = true;
      }
      return sb.ToString();
    }

    /// <summary>
    /// Determines whether this <see cref="IEnumerable{T}"/> equals to another, 
    /// i.e. contains the same items in the same order.
    /// </summary>
    /// <typeparam name="TItem">The type of item.</typeparam>
    /// <param name="items">This <see cref="IEnumerable"/>.</param>
    /// <param name="other">The <see cref="IEnumerable"/> to compare with.</param>
    /// <returns>
    /// <see langword="true"/> if this <see cref="IEnumerable{T}"/> equals to the specified <see cref="IEnumerable{T}"/>; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool EqualsTo<TItem>(this IEnumerable<TItem> items, IEnumerable<TItem> other)
    {     
      long? thisCount = items.TryGetLongCount();
      if (thisCount.HasValue) {
        long? otherCount = other.TryGetCount();
        if (otherCount.HasValue && otherCount!=thisCount)
          return false;
      }           
      IEnumerator<TItem> enumerator = items.GetEnumerator();

      foreach (var item in other) {
        if (!AdvancedComparerStruct<TItem>.System.Equals(enumerator.Current, item))
          return false;
        enumerator.MoveNext();
      }
      return true;
    }
  }
}