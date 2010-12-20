// Copyright (C) 2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2010.06.21

using System;
using System.Linq;
using NUnit.Framework;
using Xtensive.Storage.Upgrade;

namespace Xtensive.Storage.Tests.Issues.InconsistentDefaultDateTimeValuesModel1
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues.InconsistentDefaultDateTimeValuesModel2
{
  [HierarchyRoot]
  public class MyEntity : Entity
  {
    [Key, Field]
    public long Id { get; private set; }

    [Field]
    public DateTime Value { get; private set; }
  }
}

namespace Xtensive.Storage.Tests.Issues
{
  public class Issue0713_InconsistentDefaultDateTimeValues
  {
    public class Upgrader : UpgradeHandler
    {
      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      protected override void AddUpgradeHints(Core.Collections.ISet<UpgradeHint> hints)
      {
        hints.Add(new RenameTypeHint(typeof (InconsistentDefaultDateTimeValuesModel1.MyEntity).FullName, typeof (InconsistentDefaultDateTimeValuesModel2.MyEntity)));
      }
    }

    [TestFixtureSetUp]
    public void TestSetUp()
    {
      Require.ProviderIsNot(StorageProvider.Memory);
    }

    [Test]
    public void MainTest()
    {
      var configuration1 = DomainConfigurationFactory.Create();
      configuration1.Types.Register(typeof(InconsistentDefaultDateTimeValuesModel1.MyEntity));
      configuration1.UpgradeMode = DomainUpgradeMode.Recreate;

      using (var domain1 = Domain.Build(configuration1))
      using (Session.Open(domain1))
      using (var ts = Transaction.Open()) {
        new InconsistentDefaultDateTimeValuesModel1.MyEntity();
        ts.Complete();
      }

      var configuration2 = DomainConfigurationFactory.Create();
      configuration2.Types.Register(typeof(InconsistentDefaultDateTimeValuesModel2.MyEntity));
      configuration2.Types.Register(typeof(Upgrader));
      configuration2.UpgradeMode = DomainUpgradeMode.Perform;

      using (var domain2 = Domain.Build(configuration2))
      using (Session.Open(domain2))
      using (var ts = Transaction.Open()) {
        var count = Query.All<InconsistentDefaultDateTimeValuesModel2.MyEntity>().Count();
        Assert.AreEqual(1, count);
        Query.All<InconsistentDefaultDateTimeValuesModel2.MyEntity>().First(entity => entity.Value==DateTime.MinValue);
        ts.Complete();
      }
    }
  }
}