// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexey Kochetov
// Created:    2008.07.11

using System.Collections.Generic;
using Xtensive.Core.Tuples;
using Xtensive.Storage.Model;
using Xtensive.Storage.Rse;
using Xtensive.Storage.Rse.Providers;

namespace Xtensive.Storage.Providers.Sql.Providers
{
  public class SqlProvider : ProviderImplementation
  {
    public override IEnumerator<Tuple> GetEnumerator()
    {
      throw new System.NotImplementedException();
    }


    // Constructor

    public SqlProvider(RecordHeader header)
      : base(header)
    {
    }
  }
}