  // Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.29

using System;
using System.Collections.Generic;
using System.Reflection;
using Xtensive.Core.Arithmetic;
using Xtensive.Core.Tuples;
using Xtensive.Sql.Common;
using Xtensive.Sql.Dom;
using Xtensive.Sql.Dom.Database;
using Xtensive.Sql.Dom.Dml;
using Xtensive.Storage.Building;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Providers.Sql;
using SqlFactory = Xtensive.Sql.Dom.Sql;

namespace Xtensive.Storage.Providers.MsSql
{
  [Serializable]
  public class DefaultGenerator : Storage.Generator
  {
    private Table generatorTable;
    private SqlDataType dataType;
    private Schema schema;
    private Func<IEnumerable<Tuple>> getNextMany;
    private SqlBatch query;

    protected override Tuple NextOne()
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      SqlBatch batch = SqlFactory.Batch();
      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(generatorTable));
      batch.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), dataType));
      batch.Add(select);

      using (Handlers.DomainHandler.OpenSession(SessionType.System)) {
        var handler = (SessionHandler)Handlers.SessionHandler;
        using (var command = new SqlCommand(handler.Connection)) {
          command.Transaction = handler.Transaction;
          command.Statement = batch;
          command.Prepare();
          var id = command.ExecuteScalar();
          result.SetValue(0, id);
        }
      }
      return result;
    }

    protected override IEnumerable<Tuple> NextMany()
    {
//      var result = new List<Tuple>();
//      SqlBatch batch = SqlFactory.Batch();
//      var i = SqlFactory.Variable("i", SqlDataType.Int16);
//      var temp = schema.CreateTemporaryTable(string.Format("Temp_{0}", Hierarchy.MappingName));
//      temp.IsGlobal = false;
//      temp.CreateColumn("ID", new SqlValueType(dataType));
//      batch.Add(SqlFactory.Create(temp));
//      batch.Add(i.Declare());
//      batch.Add(SqlFactory.Assign(i, 0));
//      var sqlWhile = SqlFactory.While(SqlFactory.LessThan(i, CacheSize));
//      var body = SqlFactory.Batch();
//      body.Add(SqlFactory.Assign(i, SqlFactory.Add(i, 1)));
//      body.Add(SqlFactory.Insert(SqlFactory.TableRef(generatorTable)));
//      var tempRef = SqlFactory.TableRef(temp);
//      var tempInsert = SqlFactory.Insert(tempRef);
//      tempInsert.Values[tempRef.Columns[0]] = SqlFactory.FunctionCall("SCOPE_IDENTITY");
//      body.Add(tempInsert);
//      sqlWhile.Statement = body;
//      batch.Add(sqlWhile);
//      SqlSelect select = SqlFactory.Select(SqlFactory.TableRef(temp));
//      select.Columns.Add(SqlFactory.Asterisk);
//      batch.Add(select);
//      batch.Add(SqlFactory.Drop(temp));
//      schema.Tables.Remove(temp);
//  
//      SqlQueryRequest request = new SqlQueryRequest(batch, Hierarchy.KeyTupleDescriptor);
//      using (Handlers.DomainHandler.OpenSession(SessionType.System)) {
//        var handler = (SessionHandler)Handlers.SessionHandler;
//        handler.DomainHandler.Compile(request);
//        using (var e = handler.Execute(request)) {
//          while (e.MoveNext())
//            result.Add(e.Current);
//        }
//      }
//      return result;
      return getNextMany();
    }

    // ReSharper disable UnusedPrivateMember
    private IEnumerable<Tuple> NextManyInternal<TFieldType>()
    // ReSharper restore UnusedPrivateMember
    {
      Tuple result = Tuple.Create(Hierarchy.KeyTupleDescriptor);
      
      using (Handlers.DomainHandler.OpenSession(SessionType.System)) {
        var handler = (SessionHandler)Handlers.SessionHandler;
        using (var command = new SqlCommand(handler.Connection)) {
          command.Transaction = handler.Transaction;
          command.Statement = query;
          command.Prepare();
          var id = command.ExecuteScalar();
          result.SetValue(0, id);
        }
      }
      var resultList = new List<Tuple>(CacheSize);
      Arithmetic<TFieldType> ar = Arithmetic<TFieldType>.Default;
      for (int i = 0; i < CacheSize; i++) {
        Tuple item = Tuple.Create(Hierarchy.KeyTupleDescriptor);
        item.SetValue(0, result.GetValue<TFieldType>(0));
        result.SetValue(0, ar.Subtract(result.GetValue<TFieldType>(0), ar.One));
        resultList.Add(item);
      }
      for (int i = resultList.Count - 1; i >= 0; i--)
        yield return resultList[i];
    }

    public override void Initialize()
    {
      base.Initialize();

      var sessionHandler = (SessionHandler)BuildingContext.Current.SystemSessionHandler;
      var keyColumn = Hierarchy.Columns[0];
      var domainHandler = (DomainHandler)Handlers.DomainHandler;
      schema = domainHandler.Schema;
      generatorTable = schema.CreateTable(Hierarchy.MappingName);
      if (keyColumn.ValueType == typeof(int))
        dataType = SqlDataType.Int32;
      else if (keyColumn.ValueType == typeof(uint))
        dataType = SqlDataType.UInt32;
      else if (keyColumn.ValueType == typeof(long))
        dataType = SqlDataType.Int64;
      else
        dataType = SqlDataType.UInt64;
      var column = generatorTable.CreateColumn("ID", new SqlValueType(dataType));
      var increment = CacheSize == 0 ? 1: CacheSize;
      column.SequenceDescriptor = new SequenceDescriptor(column, increment, increment);
      SqlBatch batch = SqlFactory.Batch();
      batch.Add(SqlFactory.Create(generatorTable));
      using (var command = new SqlCommand(sessionHandler.Connection)) {
        command.Transaction = sessionHandler.Transaction;
        command.Statement = batch;
        command.Prepare();
        command.ExecuteNonQuery();
      }

      Type fieldType = Hierarchy.KeyTupleDescriptor[0];
      var method = typeof(DefaultGenerator)
        .GetMethod("NextManyInternal", BindingFlags.Instance | BindingFlags.NonPublic)
        .MakeGenericMethod(new[] { fieldType });
      getNextMany = (Func<IEnumerable<Tuple>>)Delegate.CreateDelegate(typeof(Func<IEnumerable<Tuple>>), this, method);
      query = SqlFactory.Batch();
      SqlInsert insert = SqlFactory.Insert(SqlFactory.TableRef(generatorTable));
      query.Add(insert);
      SqlSelect select = SqlFactory.Select();
      select.Columns.Add(SqlFactory.Cast(SqlFactory.FunctionCall("SCOPE_IDENTITY"), dataType));
      query.Add(select);
    }
  }
}