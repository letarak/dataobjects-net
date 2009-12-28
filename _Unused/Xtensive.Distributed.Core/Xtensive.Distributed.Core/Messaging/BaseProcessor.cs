// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitry Voronov
// Created:    2007.10.01

using System;
using Xtensive.Messaging;

namespace Xtensive.Distributed.Core
{
  internal abstract class BaseProcessor: IMessageProcessor
  {
    protected SimpleElectionAlgorithm algorithm;

    public abstract void ProcessMessage(object message, Sender replySender);

    public void SetContext(object value)
    {
      algorithm = (SimpleElectionAlgorithm)value;
    }
  }
}