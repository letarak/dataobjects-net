// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2008.09.23

using System;
using System.Data;
using System.Data.Common;
using Xtensive.Sql.Common;
using Xtensive.Storage.Providers.Sql.Mappings;

namespace Xtensive.Storage.Providers.MsSql
{
  public sealed class SqlValueTypeMapper : Sql.SqlValueTypeMapper
  {
    private static readonly long TicksPerMillisecond = TimeSpan.FromMilliseconds(1).Ticks;

    protected override void BuildTypeSubstitutes()
    {
      base.BuildTypeSubstitutes();
      var @int64 = DomainHandler.SqlDriver.ServerInfo.DataTypes.Int64;
      var @timespan = new RangeDataTypeInfo<TimeSpan>(SqlDataType.Int64, null);
      @timespan.Value = new ValueRange<TimeSpan>(TimeSpan.FromTicks(@int64.Value.MinValue), TimeSpan.FromTicks(@int64.Value.MaxValue));
      BuildDataTypeMapping(@timespan);
    }

    protected override DataTypeMapping CreateDataTypeMapping(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type == typeof(decimal)) {
        var oldTypeInfo = DomainHandler.SqlDriver.ServerInfo.DataTypes.Decimal;
        var oldMapping = base.CreateDataTypeMapping(oldTypeInfo);

        var newTypeInfo = new FractionalDataTypeInfo<decimal>(oldTypeInfo.SqlType, null);
        newTypeInfo.Scale = new ValueRange<short>(oldTypeInfo.Scale.MinValue, oldTypeInfo.Scale.MaxValue,
          (short) ((oldTypeInfo.Scale.MinValue + oldTypeInfo.Scale.MaxValue) / 2));
        newTypeInfo.Precision = oldTypeInfo.Precision;
        return new DataTypeMapping(
          newTypeInfo,
          oldMapping.DataReaderAccessor,
          oldMapping.DbType,
          oldMapping.ToSqlValue,
          oldMapping.FromSqlValue);
      }

      if (dataTypeInfo.Type == typeof(DateTime)) {
        RangeDataTypeInfo<DateTime> dti = DomainHandler.SqlDriver.ServerInfo.DataTypes.DateTime;
        DateTime min = dti.Value.MinValue;
        return new DataTypeMapping(dataTypeInfo, BuildDataReaderAccessor(dataTypeInfo), DbType.DateTime, value => (DateTime) value < min ? min : value, null);
      }

      if (dataTypeInfo.Type == typeof(TimeSpan))
        return new DataTypeMapping(
          dataTypeInfo,
          BuildDataReaderAccessor(dataTypeInfo),
          DbType.Int64,
          value => ((TimeSpan) value).Ticks / TicksPerMillisecond,
          value => TimeSpan.FromMilliseconds((long) value)
          );

      return base.CreateDataTypeMapping(dataTypeInfo);
    }

    protected override DbType GetDbType(DataTypeInfo dataTypeInfo)
    {
      Type type = dataTypeInfo.Type;
      TypeCode typeCode = Type.GetTypeCode(type);
      switch (typeCode) {
      case TypeCode.SByte:
        return DbType.Int16;
      case TypeCode.UInt16:
        return DbType.Int32;
      case TypeCode.UInt32:
        return DbType.Int64;
      case TypeCode.UInt64:
        return DbType.Decimal;
      }
      return base.GetDbType(dataTypeInfo);
    }

    protected override Func<DbDataReader, int, object> BuildDataReaderAccessor(DataTypeInfo dataTypeInfo)
    {
      if (dataTypeInfo.Type == typeof(TimeSpan))
        return (reader, fieldIndex) => reader.GetInt64(fieldIndex);
      return base.BuildDataReaderAccessor(dataTypeInfo);
    }
  }
}