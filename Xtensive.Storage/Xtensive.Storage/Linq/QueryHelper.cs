﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;
using Xtensive.Storage.Model;
using FieldInfo = Xtensive.Storage.Model.FieldInfo;

namespace Xtensive.Storage.Linq
{
  internal static class QueryHelper
  {
    public static Expression CreateEntityQuery(Type elementType)
    {
      var query = typeof(Query<>)
        .MakeGenericType(elementType)
        .InvokeMember("All", BindingFlags.Default | BindingFlags.GetProperty, null, null, null);
      return Expression.Constant(query, typeof(IQueryable<>).MakeGenericType(elementType));
    }

    public static Expression CreateEntitySetQuery(Expression ownerEntity, FieldInfo field)
    {
      if (!field.UnderlyingProperty.PropertyType.IsOfGenericType(typeof(EntitySet<>)))
        throw new ArgumentException();

      var elementType = field.UnderlyingProperty.PropertyType.GetGenericArguments()[0];

      if (field.Association.Multiplicity == Multiplicity.OneToMany) {
        var whereParameter = Expression.Parameter(elementType, "p");
        var whereExpression = Expression.Equal(
          Expression.Property(
            Expression.Property(whereParameter, field.Association.Reversed.OwnerField.Name),
            WellKnown.KeyFieldName),
          Expression.Property(
            ownerEntity,
            WellKnown.KeyFieldName)
          );
        return Expression.Call(
          WellKnownMembers.QueryableWhere.MakeGenericMethod(elementType),
          CreateEntityQuery(elementType),
          FastExpression.Lambda(whereExpression, whereParameter)
          );
      }

      var connectorType = field.Association.Master.AuxiliaryType.UnderlyingType;
      string master = "Master";
      string slave = "Slave";

      if (field.ReflectedType.UnderlyingType != field.Association.Master.TargetType.UnderlyingType) {
        var s = master;
        master = slave;
        slave = s;
      }
      var filterParameter = Expression.Parameter(connectorType, "t");
      var filterExpression = Expression.Equal(
        Expression.Property(
          Expression.Property(filterParameter, master),
          WellKnown.KeyFieldName),
        Expression.Property(
          ownerEntity,
          WellKnown.KeyFieldName)
        );

      var outerQuery = Expression.Call(
        WellKnownMembers.QueryableWhere.MakeGenericMethod(connectorType),
        CreateEntityQuery(connectorType),
        FastExpression.Lambda(filterExpression, filterParameter)
        );

      var outerSelectorParameter = Expression.Parameter(connectorType, "o");
      var outerSelector = FastExpression.Lambda(Expression.Property(outerSelectorParameter, slave), outerSelectorParameter);
      var innerSelectorParameter = Expression.Parameter(elementType, "i");
      var innerSelector = FastExpression.Lambda(innerSelectorParameter, innerSelectorParameter);
      var resultSelector = FastExpression.Lambda(innerSelectorParameter, outerSelectorParameter, innerSelectorParameter);

      return Expression.Call(typeof(Queryable), "Join", new[]
        {
          connectorType,
          elementType,
          outerSelector.Body.Type,
          resultSelector.Body.Type
        },
        outerQuery,
        CreateEntityQuery(elementType),
        outerSelector,
        innerSelector,
        resultSelector);
    }
  }
}
