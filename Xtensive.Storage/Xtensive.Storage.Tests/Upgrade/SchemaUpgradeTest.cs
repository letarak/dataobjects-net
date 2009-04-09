// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2009.04.09

using System;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Storage.Attributes;
using Xtensive.Storage.Configuration;
using Xtensive.Storage.Configuration.TypeRegistry;
using Xtensive.Storage.Internals;

namespace Xtensive.Storage.Tests.Upgrade.Model_1
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Name  { get; set;}

    [Field] 
    public Address Address { get; set;}
  }

  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Address : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string City { get; set;}
  }
}

namespace Xtensive.Storage.Tests.Upgrade.Model_2
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Person : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string Name  { get; set;}

    [Field] 
    public string City { get; set;}

    [Field]
    [Obsolete]
    public Recycled_2.Address Address { get; set;}
  }

  public class UpgraderToVersion_2 : ISchemaUpgrader
  {
    public string AssemblyName
    {
      get { return "Xtensive.Storage.Tests.Upgrade.Model"; }
    }

    public string SourceVersion
    {
      get { return "1"; }
    }

    public string ResultVersion
    {
      get { return "2"; }
    }

    public void RunUpgradeScript()
    {
      foreach (Person person in Query<Person>.All) {
        person.City = person.Address.City;
      }
    }

    public void RegisterRecycledTypes(Registry typeRegistry)
    {
      Assembly thisAssembly = Assembly.GetExecutingAssembly();
      typeRegistry.Register(thisAssembly, typeof(Recycled_2.Address).Namespace);
    }

    public UpgraderToVersion_2()
    {}
  }
}

namespace Xtensive.Storage.Tests.Upgrade.Recycled_2
{
  [HierarchyRoot(typeof (KeyGenerator), "ID")]
  public class Address : Entity
  {
    [Field]
    public int ID { get; private set; }

    [Field]
    public string City { get; set;}
  }
}

namespace Xtensive.Storage.Tests.Upgrade
{
  [TestFixture]
  public class SchemaUpgradeTest
  {
    public DomainConfiguration GetConfiguration(Type persistentType)
    {
      DomainConfiguration configuration = DomainConfigurationFactory.Create();
      configuration.Types.Register(Assembly.GetExecutingAssembly(), persistentType.Namespace);
      configuration.ModelAssembliesManagerType = typeof (TestModelAssembliesManager);
      return configuration;
    }

    [Test]
    public void MainTest()
    {
      BuildFirstModel();
      Upgrade();
      BuildSecondDomain();
    }

    private void Upgrade()
    {
      var configuration = GetConfiguration(typeof (Model_2.Person));
      SchemaUpgradeHelper upgradeHelper = new SchemaUpgradeHelper(configuration);
      bool isActual = upgradeHelper.IsSchemaActual();
      Assert.IsFalse(isActual);
      upgradeHelper.UpgradeSchema();      
    }


    private void BuildFirstModel()
    {
      DomainConfiguration configuration = GetConfiguration(typeof(Model_1.Person));
      Domain domain = Domain.Build(configuration);
      using (domain.OpenSession()) {
        using (var transactionScope = Transaction.Open()) {
          new Model_1.Person
            {
              Address = new Model_1.Address {City = "Mumbai"},
              Name = "Gaurav"
            };
          new Model_1.Person
            {
              Address = new Model_1.Address {City = "Delhi"},
              Name = "Mihir"
            };
          transactionScope.Complete();
        }
      }
    }

    private void BuildSecondDomain()
    {
      DomainConfiguration configuration = GetConfiguration(typeof(Model_2.Person));
      Domain domain = Domain.Build(configuration);
      using (domain.OpenSession()) {
        using (Transaction.Open()) {
          foreach (var person in Query<Model_2.Person>.All) {
            if (person.Name=="Gauvar")
              Assert.AreEqual("Mumbai", person.City);
            else
              Assert.AreEqual("Delhi", person.City);
          }
        }
      }
    }
  }
}
