// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Kofman
// Created:    2008.08.27

using System;
using System.Reflection;
using NUnit.Framework;
using Xtensive.Core.Testing;
using Xtensive.Storage.Configuration;

namespace Xtensive.Storage.Tests.Storage.TranscationsTest
{
  public class TransactionsTest : AutoBuildTest
  {    
    [HierarchyRoot]
    public class Hexagon : Entity
    {
      [Field, KeyField]
      public int ID { get; private set; }

      [Field]
      public int Kwanza { get; set;}

      [Field]
      public Hexagon Babuka { get; set;}

      public void Wobble(int newKanza)
      {
        Kwanza = newKanza;
        throw new InvalidOperationException();
      }
    }    

    protected override DomainConfiguration BuildConfiguration()
    {
      DomainConfiguration config = base.BuildConfiguration();
      config.Types.Register(Assembly.GetExecutingAssembly(), "Xtensive.Storage.Tests.Storage.TranscationsTest");
      return config;
    }

    [Test]
    public void RollbackCreationTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;
        using (Transaction.Open()) {
          hexagon = new Hexagon();
        }
        using (Transaction.Open()) {
          Assert.IsTrue(hexagon.IsRemoved);
        }
        AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 15; });
      }
    }

    [Test]
    public void RollbackRemovingTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;
        using (var t = Transaction.Open()) {
          hexagon = new Hexagon {Kwanza = 36};
          t.Complete();
        }
        using (Transaction.Open()) {
          hexagon.Remove();
          AssertEx.ThrowsInvalidOperationException( delegate { hexagon.Kwanza = 20; });
          // rolling back removal
        }
        using (Transaction.Open()) {
          hexagon.Kwanza = 14;
          Assert.AreEqual(14, hexagon.Kwanza);
        }
      }
    }

    [Test]
    public void RollbackModifyingTest()
    {
      using (Domain.OpenSession()) {
        Hexagon hexagon;

        using (var t = Transaction.Open()) {
          hexagon = new Hexagon {Kwanza = 3};
          t.Complete();
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }

        using (Transaction.Open()) {
          hexagon.Kwanza = 11;
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          hexagon.Babuka = new Hexagon();
        }
        using (Transaction.Open()) {
          Assert.IsNull(hexagon.Babuka);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          hexagon.Kwanza = 12;
          Session.Current.Persist();
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
          Assert.AreEqual(PersistenceState.Synchronized, hexagon.PersistenceState);
        }
        using (Transaction.Open()) {
          try {
            hexagon.Wobble(18);
          }
          catch (InvalidOperationException) {
          }
        }
        using (Transaction.Open()) {
          Assert.AreEqual(3, hexagon.Kwanza);
        }
      } 
    }
  }
}