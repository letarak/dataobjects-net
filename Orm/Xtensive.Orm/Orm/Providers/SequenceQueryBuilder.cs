﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.03.07

using System;
using Xtensive.Core;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Providers
{
  internal sealed class SequenceQueryBuilder
  {
    private readonly StorageDriver driver;
    private readonly bool hasSequences;
    private readonly bool hasBatches;
    private readonly bool hasInsertDefaultValues;
    private readonly SequenceQueryCompartment compartment;

    public SequenceQuery Build(SchemaNode generatorNode, long increment)
    {
      var sqlNext = hasSequences
        ? GetSequenceBasedNextImplementation(generatorNode, increment)
        : GetTableBasedNextImplementation(generatorNode);

      var requiresSeparateSession = !hasSequences;
      var batch = sqlNext as SqlBatch;
      if (batch==null || hasBatches)
        // There are batches or there is single statement, so we can run this as a single request
        return new SequenceQuery(driver.Compile(sqlNext).GetCommandText(), compartment);

      // No batches, so we must execute this manually
      return new SequenceQuery(
        driver.Compile((ISqlCompileUnit) batch[0]).GetCommandText(),
        driver.Compile((ISqlCompileUnit) batch[1]).GetCommandText(),
        compartment);
    }

    private ISqlCompileUnit GetSequenceBasedNextImplementation(SchemaNode generatorNode, long increment)
    {
      return SqlDml.Select(SqlDml.NextValue((Sequence) generatorNode, (int) increment));
    }

    private ISqlCompileUnit GetTableBasedNextImplementation(SchemaNode generatorNode)
    {
      var table = (Table) generatorNode;

      var idColumn = GetColumn(table, WellKnown.GeneratorColumnName);

      var tableRef = SqlDml.TableRef(table);
      var insert = SqlDml.Insert(tableRef);

      if (!hasInsertDefaultValues) {
        var fakeColumn = GetColumn(table, WellKnown.GeneratorFakeColumnName);
        insert.Values[tableRef[fakeColumn.Name]] = SqlDml.Null;
      }

      var result = SqlDml.Batch();
      result.Add(insert);
      result.Add(SqlDml.Select(SqlDml.LastAutoGeneratedId()));
      return result;
    }

    private static TableColumn GetColumn(Table table, string columnName)
    {
      var idColumn = table.TableColumns[columnName];
      if (idColumn==null)
        throw new InvalidOperationException(string.Format(
          Strings.ExColumnXIsNotFoundInTableY, columnName, table.Name));
      return idColumn;
    }

    public SequenceQueryBuilder(StorageDriver driver)
    {
      ArgumentValidator.EnsureArgumentNotNull(driver, "driver");

      this.driver = driver;

      var providerInfo = driver.ProviderInfo;

      hasSequences = providerInfo.Supports(ProviderFeatures.Sequences);
      hasBatches = providerInfo.Supports(ProviderFeatures.DmlBatches);
      hasInsertDefaultValues = providerInfo.Supports(ProviderFeatures.InsertDefaultValues);

      compartment = hasSequences || providerInfo.Supports(ProviderFeatures.TransactionalKeyGenerators)
        ? SequenceQueryCompartment.SameSession
        : SequenceQueryCompartment.SeparateSession;
    }
  }
}