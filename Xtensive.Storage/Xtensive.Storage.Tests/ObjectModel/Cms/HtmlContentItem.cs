// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Ilyin
// Created:    2009.09.15

using System;
using Xtensive.Storage;

namespace Xtensive.Storage.Tests.ObjectModel.Cms
{
  public abstract class HtmlContentItem
    : ContentItem
  {
    [Field]
    public string HeaderFileID { get; set;}

    [Field]
    public string BodyFileID { get; set;}

    [Field]
    public DateTime? LastModificationDate { get; set;}

    [Field, Association(OnTargetRemove = OnRemoveAction.Clear)]
    public EntitySet<ContentReference> References { get; private set;}

    public virtual void NotifyModified()
    {
      LastModificationDate = DateTime.Now;
    }
  }
}