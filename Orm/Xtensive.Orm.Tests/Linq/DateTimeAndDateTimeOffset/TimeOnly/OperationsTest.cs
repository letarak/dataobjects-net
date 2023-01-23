// Copyright (C) 2023 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.

#if NET6_0_OR_GREATER //DO_DATEONLY

using System;
using NUnit.Framework;
using Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.Model;

namespace Xtensive.Orm.Tests.Linq.DateTimeAndDateTimeOffset.TimeOnlys
{
  public class OperationsTest : DateTimeBaseTest
  {
    [Test]
    public void AddHoursTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddHours(1) == FirstTimeOnly.AddHours(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddHours(-2) == FirstMillisecondTimeOnly.AddHours(-2));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddHours(33) == NullableTimeOnly.AddHours(33));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddHours(1) == FirstTimeOnly.AddHours(2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddHours(-1) == FirstMillisecondTimeOnly.AddHours(-2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddHours(33) == NullableTimeOnly.AddHours(44));
      });
    }

    [Test]
    public void AddMinutesTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddMinutes(1) == FirstTimeOnly.AddMinutes(1));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddMinutes(-2) == FirstMillisecondTimeOnly.AddMinutes(-2));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddMinutes(33) == NullableTimeOnly.AddMinutes(33));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.AddMinutes(1) == FirstTimeOnly.AddMinutes(2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.AddMinutes(-1) == FirstMillisecondTimeOnly.AddMinutes(-2));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.AddMinutes(33) == NullableTimeOnly.AddMinutes(44));
      });
    }

    [Test]
    public void AddTimeSpanTest()
    {
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Add(FirstOffset) == FirstTimeOnly.Add(FirstOffset));
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Add(SecondOffset) == FirstMillisecondTimeOnly.Add(SecondOffset));
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Add(FirstOffset) == NullableTimeOnly.Add(FirstOffset));

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly.Add(FirstOffset) == FirstTimeOnly.Add(WrongOffset));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly.Add(SecondOffset) == FirstMillisecondTimeOnly.Add(WrongOffset));
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly.Value.Add(FirstOffset) == NullableTimeOnly.Add(WrongOffset));
      });
    }

    [Test]
    public void MinusTimeOnlyTest()
    {
      Require.ProviderIsNot(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - SecondTimeOnly == FirstTimeOnly - SecondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - SecondTimeOnly == FirstMillisecondTimeOnly - SecondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - SecondTimeOnly == NullableTimeOnly - SecondTimeOnly);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - SecondTimeOnly == FirstTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - SecondTimeOnly == FirstMillisecondTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - SecondTimeOnly == NullableTimeOnly - WrongTimeOnly);

      });
    }

    [Test]
    public void MysqlMinisDateTimeTest()
    {
      Require.ProviderIs(StorageProvider.MySql);
      ExecuteInsideSession((s) => {
        var firstTimeOnly = FirstTimeOnly.FixTimeOnlyForProvider(StorageProviderInfo.Instance);
        var firstMillisecondTimeOnly = FirstMillisecondTimeOnly.FixTimeOnlyForProvider(StorageProviderInfo.Instance);
        var secondTimeOnly = SecondTimeOnly.FixTimeOnlyForProvider(StorageProviderInfo.Instance);
        var nullableTimeOnly = NullableTimeOnly.FixTimeOnlyForProvider(StorageProviderInfo.Instance);

        RunTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - secondTimeOnly == firstTimeOnly - secondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - secondTimeOnly == firstMillisecondTimeOnly - secondTimeOnly);
        RunTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - secondTimeOnly == NullableTimeOnly - secondTimeOnly);

        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.TimeOnly - secondTimeOnly == secondTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.MillisecondTimeOnly - secondTimeOnly == firstMillisecondTimeOnly - WrongTimeOnly);
        RunWrongTest<SingleTimeOnlyEntity>(s, c => c.NullableTimeOnly - secondTimeOnly == nullableTimeOnly - WrongTimeOnly);
      });
    }
  }
}
#endif