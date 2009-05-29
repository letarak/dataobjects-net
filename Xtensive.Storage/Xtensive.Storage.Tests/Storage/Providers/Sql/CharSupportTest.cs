// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2009.04.20

using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Tests.Storage.Providers.Sql.CharSupportTestModel;

namespace Xtensive.Storage.Tests.Storage.Providers.Sql.CharSupportTestModel
{
  [HierarchyRoot]
  class MyEntity : Entity
  {
    [Field, KeyField]
    public int Id {get; private set;}

    [Field]
    public char Char {get; set;}
  }
}

namespace Xtensive.Storage.Tests.Storage.Providers.Sql
{
  public class CharSupportTest : AutoBuildTest
  {
    private string charColumn;

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.Types.Register(typeof(MyEntity).Assembly, typeof(MyEntity).Namespace);
      return config;
    }

    public override void TestFixtureSetUp()
    {
      base.TestFixtureSetUp();

      charColumn = Domain.Model.Types[typeof(MyEntity)].Fields["Char"].Column.Name;

      using (Domain.OpenSession())
      using (var t = Transaction.Open()) {
        new MyEntity {Char = 'X'};
        new MyEntity {Char = 'Y'};
        new MyEntity {Char = 'Z'};
        t.Complete();
      }
    }

    [Test]
    public void SelectCharTest()
    {
      using (Domain.OpenSession())
      using (var transaction = Transaction.Open()) {
        var rs = GetRecordSet<MyEntity>();
        var result = rs
          .Select(rs.Header.IndexOf(charColumn))
          .Select(i => i.GetValueOrDefault<char>(0))
          .ToList();
        Assert.AreEqual(3, result.Count);
        Assert.IsTrue(result.Contains('X'));
        Assert.IsTrue(result.Contains('Y'));
        Assert.IsTrue(result.Contains('Z'));
        transaction.Complete();
      }
    }

    [Test]
    public void CharParameterTest()
    {
      using (Domain.OpenSession())
      using (var transaction = Transaction.Open()) {
        var y = 'Y';
        var rs = GetRecordSet<MyEntity>();
        var result = rs
          .Select(rs.Header.IndexOf(charColumn))
          .Filter(t => t.GetValueOrDefault<char>(0) == y)
          .Select(i => i.GetValueOrDefault<char>(0))
          .ToList();
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(y, result[0]);
        transaction.Complete();
      }
    }
  }
}