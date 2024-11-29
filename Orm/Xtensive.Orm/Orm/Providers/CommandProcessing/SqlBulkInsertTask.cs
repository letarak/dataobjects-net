using System;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Sql.Dml;
using Tuple = Xtensive.Tuples.Tuple;

namespace Xtensive.Orm.Providers;

/// <summary>
/// A bulk insert task
/// </summary>
public sealed class SqlBulkInsertTask : SqlTask
{
  /// <summary>
  /// Table
  /// </summary>
  public readonly SqlTableRef Table;

  /// <summary>
  /// A tuples that store changed column values for multi-record INSERT.
  /// <see cref="Tuple"/> should remain <see langword="null" />
  /// </summary>
  public readonly IReadOnlyList<Tuple> Tuples;

  /// <inheritdoc/>
  public override void ProcessWith(ISqlTaskProcessor processor, CommandProcessorContext context)
  {
    processor.ProcessTask(this, context);
  }

  // Constructors

  public SqlBulkInsertTask(SqlTableRef table, IReadOnlyList<Tuple> tuples)
  {
    Table = table;
    Tuples = tuples;
  }
}