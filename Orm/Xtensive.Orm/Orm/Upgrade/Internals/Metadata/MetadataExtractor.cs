﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.02.16

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xtensive.Core;
using Xtensive.Orm.Providers;
using Xtensive.Sql;
using Xtensive.Sql.Dml;
using Xtensive.Sql.Model;

namespace Xtensive.Orm.Upgrade
{
  internal sealed class MetadataExtractor
  {
    private readonly MetadataMapping mapping;
    private readonly ISqlExecutor executor;

    public void ExtractTypes(MetadataSet output, SqlExtractionTask task)
    {
      var types = new List<TypeMetadata>();
      ExtractTypes(types, task);
      output.Types.AddRange(types);
    }

    public async Task ExtractTypesAsync(MetadataSet output, SqlExtractionTask task, CancellationToken token = default)
    {
      var types = new List<TypeMetadata>();
      await ExtractTypesAsync(types, task, token).ConfigureAwait(false);
      output.Types.AddRange(types);
    }

    public void ExtractAssemblies(MetadataSet output, SqlExtractionTask task)
    {
      var assemblies = new List<AssemblyMetadata>();
      ExtractAssemblies(assemblies, task);
      output.Assemblies.AddRange(assemblies);
    }

    public async Task ExtractAssembliesAsync(MetadataSet output, SqlExtractionTask task,
      CancellationToken token = default)
    {
      var assemblies = new List<AssemblyMetadata>();
      await ExtractAssembliesAsync(assemblies, task, token).ConfigureAwait(false);
      output.Assemblies.AddRange(assemblies);
    }

    public void ExtractExtensions(MetadataSet output, SqlExtractionTask task)
    {
      var extensions = new List<ExtensionMetadata>();
      ExtractExtensions(extensions, task);
      output.Extensions.AddRange(extensions);
    }

    public async Task ExtractExtensionsAsync(MetadataSet output, SqlExtractionTask task,
      CancellationToken token = default)
    {
      var extensions = new List<ExtensionMetadata>();
      await ExtractExtensionsAsync(extensions, task, token).ConfigureAwait(false);
      output.Extensions.AddRange(extensions);
    }

    #region Private / internal methods

    private void ExtractAssemblies(ICollection<AssemblyMetadata> output, SqlExtractionTask task)
    {
      var query = CreateQuery(mapping.Assembly, task, mapping.AssemblyName, mapping.AssemblyVersion);
      ExecuteQuery(output, query, ParseAssembly);
    }

    private Task ExtractAssembliesAsync(ICollection<AssemblyMetadata> output, SqlExtractionTask task,
      CancellationToken token)
    {
      var query = CreateQuery(mapping.Assembly, task, mapping.AssemblyName, mapping.AssemblyVersion);
      return ExecuteQueryAsync(output, query, ParseAssembly, token);
    }

    private void ExtractTypes(ICollection<TypeMetadata> output, SqlExtractionTask task)
    {
      var query = CreateQuery(mapping.Type, task, mapping.TypeId, mapping.TypeName);
      ExecuteQuery(output, query, ParseType);
    }

    private Task ExtractTypesAsync(ICollection<TypeMetadata> output, SqlExtractionTask task, CancellationToken token)
    {
      var query = CreateQuery(mapping.Type, task, mapping.TypeId, mapping.TypeName);
      return ExecuteQueryAsync(output, query, ParseType, token);
    }

    private void ExtractExtensions(ICollection<ExtensionMetadata> output, SqlExtractionTask task)
    {
      var query = CreateQuery(mapping.Extension, task, mapping.ExtensionName, mapping.ExtensionText);
      ExecuteQuery(output, query, ParseExtension);
    }

    private Task ExtractExtensionsAsync(ICollection<ExtensionMetadata> output, SqlExtractionTask task,
      CancellationToken token)
    {
      var query = CreateQuery(mapping.Extension, task, mapping.ExtensionName, mapping.ExtensionText);
      return ExecuteQueryAsync(output, query, ParseExtension, token);
    }

    private ExtensionMetadata ParseExtension(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var text = ReadString(reader, 1);
      return new ExtensionMetadata(name, text);
    }

    private AssemblyMetadata ParseAssembly(DbDataReader reader)
    {
      var name = ReadString(reader, 0);
      var version = ReadString(reader, 1);
      return new AssemblyMetadata(name, version);
    }

    private TypeMetadata ParseType(DbDataReader reader)
    {
      var id = ReadInt(reader, 0);
      var name = ReadString(reader, 1);
      return new TypeMetadata(id, name);
    }

    private void ExecuteQuery<T>(ICollection<T> output, ISqlCompileUnit query, Func<DbDataReader, T> parser)
    {
      using var command = executor.ExecuteReader(query);
      var reader = command.Reader;
      while (reader.Read()) {
        output.Add(parser.Invoke(reader));
      }
    }
 
    private async Task ExecuteQueryAsync<T>(ICollection<T> output, ISqlCompileUnit query, Func<DbDataReader, T> parser,
      CancellationToken token)
    {
      var command = await executor.ExecuteReaderAsync(query, token).ConfigureAwait(false);
      await using (command.ConfigureAwait(false)) {
        var reader = command.Reader;
        while (await reader.ReadAsync(token).ConfigureAwait(false)) {
          output.Add(parser.Invoke(reader));
        }
      }
    }

    private static SqlSelect CreateQuery(string tableName, SqlExtractionTask task, params string[] columnNames)
    {
      var catalog = new Catalog(task.Catalog);
      var schema = catalog.CreateSchema(task.Schema);
      var table = schema.CreateTable(tableName);
      foreach (var column in columnNames) {
        table.CreateColumn(column);
      }

      var tableRef = SqlDml.TableRef(table);
      var select = SqlDml.Select(tableRef);
      var columnRefs = columnNames
        .Select((c, i) => SqlDml.ColumnRef(tableRef.Columns[i], c));
      foreach (var columnRef in columnRefs) {
        select.Columns.Add(columnRef);
      }

      return select;
    }

    private string ReadString(DbDataReader reader, int index) =>
      reader.IsDBNull(index) ? null : (string) mapping.StringMapping.ReadValue(reader, index);

    private int ReadInt(DbDataReader reader, int index) =>
      (int) mapping.IntMapping.ReadValue(reader, index);

    #endregion

    // Constructors

    public MetadataExtractor(MetadataMapping mapping, ISqlExecutor executor)
    {
      ArgumentValidator.EnsureArgumentNotNull(mapping, nameof(mapping));
      ArgumentValidator.EnsureArgumentNotNull(executor, nameof(executor));

      this.mapping = mapping;
      this.executor = executor;
    }
  }
}