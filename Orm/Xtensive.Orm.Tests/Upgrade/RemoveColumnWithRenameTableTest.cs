﻿// Copyright (C) 2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Denis Krjuchkov
// Created:    2012.07.05

using System;
using NUnit.Framework;
using Xtensive.Orm.Upgrade;
using V1=Xtensive.Orm.Tests.Upgrade.RemoveColumnWithRenameTableTestModel.Version1;
using V2=Xtensive.Orm.Tests.Upgrade.RemoveColumnWithRenameTableTestModel.Version2;

namespace Xtensive.Orm.Tests.Upgrade
{
  namespace RemoveColumnWithRenameTableTestModel.Version1
  {
    [HierarchyRoot]
    public class EntityToRemove : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    [HierarchyRoot]
    public class EntityWithFieldToRemove : Entity
    {
      [Field, Key]
      public long Id { get; private set; }

      [Field]
      public EntityToRemove Ref { get; set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "1";
      }
    }
  }

  namespace RemoveColumnWithRenameTableTestModel.Version2
  {
    [HierarchyRoot]
    public class EntityWithRemovedField : Entity
    {
      [Field, Key]
      public long Id { get; private set; }
    }

    public class Upgrader : UpgradeHandler
    {
      protected override string DetectAssemblyVersion()
      {
        return "2";
      }

      public override bool CanUpgradeFrom(string oldVersion)
      {
        return true;
      }

      protected override void AddUpgradeHints(Collections.ISet<UpgradeHint> hints)
      {
        hints.Add(new RenameTypeHint(typeof (Version1.EntityWithFieldToRemove).FullName, typeof (EntityWithRemovedField)));
        hints.Add(new RemoveTypeHint(typeof (Version1.EntityToRemove).FullName));
        hints.Add(new RemoveFieldHint(typeof (Version1.EntityWithFieldToRemove).FullName, "Ref"));
      }
    }
  }

  [TestFixture]
  public class RemoveColumnWithRenameTableTest
  {
    private Domain BuildDomain(Type sampleType, DomainUpgradeMode mode)
    {
      var configuration = DomainConfigurationFactory.Create();
      configuration.UpgradeMode = mode;
      configuration.Types.Register(sampleType.Assembly, sampleType.Namespace);
      return Domain.Build(configuration);
    }

    [Test]
    public void MainTest()
    {
      using (var domain = BuildDomain(typeof (V1.Upgrader), DomainUpgradeMode.Recreate))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        new V1.EntityWithFieldToRemove {Ref = new V1.EntityToRemove()};
        tx.Complete();
      }

      using (var domain = BuildDomain(typeof (V2.Upgrader), DomainUpgradeMode.PerformSafely))
      using (var session = domain.OpenSession())
      using (var tx = session.OpenTransaction()) {
        var count = session.Query.All<V2.EntityWithRemovedField>().Count();
        Assert.That(count, Is.EqualTo(1));
        tx.Complete();
      }
    }
  }
}