﻿// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.02

using System;
using System.Linq.Expressions;
using Xtensive.Core.Linq;
using Xtensive.Core.Reflection;

namespace Xtensive.Storage.Linq.Rewriters
{
  internal sealed class EntitySetAccessRewriter : ExpressionVisitor
  {
    public static Expression Rewrite(Expression e)
    {
      return new EntitySetAccessRewriter().Visit(e);
    }

    protected override Expression VisitUnknown(Expression e)
    {
      return e;
    }

    protected override Expression VisitMethodCall(MethodCallExpression mc)
    {
      if (!IsEntitySet(mc.Object))
        return base.VisitMethodCall(mc);
      if (mc.Method.Name == "Contains") {
        var method = WellKnownMembers.QueryableContains
          .MakeGenericMethod(mc.Object.Type.GetGenericArguments()[0]);
        return Expression.Call(method, Visit(mc.Object), Visit(mc.Arguments[0]));
      }
      throw new NotSupportedException(String.Format("'{0}' method isn't supported.", mc.Method.Name));
    }

    protected override Expression VisitMemberAccess(MemberExpression m)
    {
      if (!IsEntitySet(m.Expression))
        return base.VisitMemberAccess(m);
      if (m.Member.Name == "Count") {
        var method = WellKnownMembers.QueryableLongCount
          .MakeGenericMethod(m.Expression.Type.GetGenericArguments()[0]);
        return Expression.Call(method, Visit(m.Expression));
      }
      throw new NotSupportedException(IsEntitySet(m.Expression)
        ? String.Format("Can't access member of type 'EntitySet<>'.") 
        : String.Format("Can't access member '{0}'", m.Member.Name));
    }

    private static bool IsEntitySet(Expression expression)
    {
      return expression!=null && expression.Type.IsOfGenericType(typeof (EntitySet<>));
    }
  }
}