// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2009.10.06

using System;


namespace Xtensive.Diagnostics
{
  /// <summary>
  /// Default log implementation (see <see cref="LogImplementationBase"/>).
  /// </summary>
  [Serializable]
  public class LogImplementation : LogImplementationBase
  {
    /// <summary>
    /// Initializes new instance of this type.
    /// </summary>
    /// <param name="realLog">Real log to wrap.</param>
    public LogImplementation(IRealLog realLog)
      : base(realLog)
    {
    }
  }
}