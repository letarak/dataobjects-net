﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.03.17

using System;
using Xtensive.Core;
using Xtensive.Core.Linq;
using Xtensive.Indexing;
using Xtensive.Storage.Model;

namespace Xtensive.Storage.Rse.Optimisation
{
  /// <summary>
  /// Extracter of <see cref="RangeSet{T}"/> from a boolean expression in disjunctive normal form.
  /// </summary>
  internal class RangeSetExtractor
  {
    private readonly ExtractingVisitor cnfVisitor;

    /// <summary>
    /// Extracts collection of <see cref="RangeSet{T}"/>.
    /// </summary>
    /// <param name="predicate">A boolean expression in disjunctive normal form.</param>
    /// <param name="info">A description of index to use for extracting of a <see cref="RangeSet{T}"/>.</param>
    /// <param name="primaryIdxRecordSetHeader">A header from a primary index</param>
    public RsExtractionResult Extract(NormalizedBooleanExpression predicate, IndexInfo info,
      RecordSetHeader primaryIdxRecordSetHeader)
    {
      ValidatePredicate(predicate);
      var result = new RsExtractionResult(info);
      foreach (var cnf in predicate) {
        result.AddPart(cnfVisitor.Extract((NormalizedBooleanExpression)cnf, info, primaryIdxRecordSetHeader));
      }
      return result;
    }

    private static void ValidatePredicate(NormalizedBooleanExpression predicate)
    {
      ArgumentValidator.EnsureArgumentNotNull(predicate, "predicate");
      if (predicate.NormalForm != NormalFormType.Disjunctive)
        throw new ArgumentException(String.Format(Resources.Strings.ExNormalizedExpressionMustHaveXForm,
                                    NormalFormType.Disjunctive), "predicate");
      if(!predicate.IsRoot)
        throw new ArgumentException(Resources.Strings.ExNormalizedExpressionMustBeRoot, "predicate");
    }

    public RangeSetExtractor(DomainModel domainModel)
    {
      cnfVisitor = new ExtractingVisitor(domainModel);
    }
  }
}
