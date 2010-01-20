// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.12.08

using System;
using System.Collections;
using System.Collections.Generic;
using Xtensive.Core.ObjectMapping.Model;
using Xtensive.Core.Resources;

namespace Xtensive.Core.ObjectMapping
{
  internal sealed class ObjectExtractor
  {
    private readonly MappingDescription mappingDescription;
    private Queue<object> referencedObjects;

    public void Extract(object root, Dictionary<object,object> resultContainer)
    {
      if (root == null)
        return;
      referencedObjects = new Queue<object>();
      InitializeExtraction(root);
      while (referencedObjects.Count > 0) {
        var current = referencedObjects.Dequeue();
        if (current == null)
          continue;
        var currentType = current.GetType();
        if (MappingHelper.IsTypePrimitive(currentType))
          continue;
        if (!currentType.IsValueType) {
          var key = mappingDescription.ExtractTargetKey(current);
          if (resultContainer.ContainsKey(key))
            continue;
          resultContainer.Add(key, current);
        }
        var description = mappingDescription.TargetTypes[currentType];
        foreach (var property in description.ComplexProperties.Values) {
          var value = property.SystemProperty.GetValue(current, null);
          if (value==null)
            continue;
          if (!property.IsCollection)
            referencedObjects.Enqueue(value);
          else
            foreach (var obj in (IEnumerable) value)
              referencedObjects.Enqueue(obj);
        }
      }
    }

    private void InitializeExtraction(object root)
    {
      var type = root.GetType();
      if (MappingHelper.IsCollection(type))
          foreach (var obj in (IEnumerable) root) {
            if (obj!=null && MappingHelper.IsCollection(obj.GetType()))
              throw new ArgumentException(Strings.ExNestedCollectionIsNotSupported, "root");
            referencedObjects.Enqueue(obj);
          }
      else
        referencedObjects.Enqueue(root);
    }


    // Constructors

    public ObjectExtractor(MappingDescription mappingDescription)
    {
      ArgumentValidator.EnsureArgumentNotNull(mappingDescription, "mappingDescription");

      this.mappingDescription = mappingDescription;
    }
  }
}