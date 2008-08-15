// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.08.28

using System;
using System.Collections.Generic;
using Xtensive.Core;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Resources;

namespace Xtensive.Indexing
{
  partial class Index<TKey, TItem>
  {
    internal SeekResultPointer<IndexPointer<TKey, TItem>> InternalSeek(DataPage<TKey, TItem> page, Ray<IEntire<TKey>> ray)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(ray);
      int index = result.Pointer;
      SeekResultType resultType = result.ResultType;
      if (leafPage!=null) {
        if (resultType==SeekResultType.Default) {
          if (ray.Direction==Direction.Positive) {
            if (leafPage.RightPageRef!=null) {
              leafPage = leafPage.RightPage;
              resultType = SeekResultType.Nearest;
              index = 0;
            }
          }
          else {
            if (leafPage.LeftPageRef!=null) {
              leafPage = leafPage.LeftPage;
              resultType = SeekResultType.Nearest;
              index = leafPage.CurrentSize;
            }
          }
        }
        return new SeekResultPointer<IndexPointer<TKey, TItem>>(
          resultType, new IndexPointer<TKey, TItem>(leafPage, index));
      }

      return InternalSeek(innerPage.GetPage(index), ray);
    }

    internal SeekResultPointer<IndexPointer<TKey, TItem>> InternalSeek(DataPage<TKey, TItem> page, TKey key)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(key);
      int index = result.Pointer;
      SeekResultType resultType = result.ResultType;
      if (leafPage!=null) {
        if (resultType==SeekResultType.Default && leafPage.RightPageRef!=null) {
          leafPage = leafPage.RightPage;
          resultType = SeekResultType.Nearest;
          index = 0;
        }
        return new SeekResultPointer<IndexPointer<TKey, TItem>>(
          resultType, 
          new IndexPointer<TKey, TItem>(leafPage, index));
      }

      return InternalSeek(innerPage.GetPage(index), key);
    }

    private TItem InternalGetItem(DataPage<TKey, TItem> page, TKey key)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(key);
      if (innerPage!=null)
        return InternalGetItem(innerPage.GetPage(result.Pointer), key);
      if (result.ResultType!=SeekResultType.Exact)
        throw new KeyNotFoundException();
      return leafPage[result.Pointer];
    }

    private bool InternalContainsKey(DataPage<TKey, TItem> page, TKey key)
    {
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(key);
      if (innerPage!=null)
        return InternalContainsKey(innerPage.GetPage(result.Pointer), key);
      return result.ResultType==SeekResultType.Exact;
    }

    private DataPage<TKey, TItem> InternalAdd(DataPage<TKey, TItem> page, TKey key, TItem item)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(key);
      
      if (innerPage != null) {
        DataPage<TKey, TItem> insertedPage = InternalAdd(innerPage.GetPage(result.Pointer), key, item);
        if (insertedPage==null) {
          innerPage.AddToMeasures(item);
          return null;
        }
        TKey lowestKey = insertedPage.Key;
        if (page.CurrentSize < PageSize) {
          innerPage.Insert(result.Pointer + 1, lowestKey, insertedPage.Identifier);
          innerPage.AddToMeasures(item);
          return null;
        }

        InnerPage<TKey, TItem> rightPage = innerPage.Split().AsInnerPage;
        if (result.Pointer < PageSize/2) {
          innerPage.Insert(result.Pointer + 1, lowestKey, insertedPage.Identifier);
          innerPage.AddToMeasures(insertedPage);
        }
        else {
          rightPage.Insert(result.Pointer - PageSize / 2, lowestKey, insertedPage.Identifier);
          rightPage.AddToMeasures(insertedPage);
        }
        return rightPage;
      }
      else {
        // Leaf page.
        if (result.ResultType == SeekResultType.Exact)
          throw new InvalidOperationException(Strings.ExItemWithTheSameKeyHasBeenAdded);

        if (leafPage.CurrentSize < PageSize) {
          leafPage.Insert(result.Pointer, item);
          leafPage.AddToMeasures(item);
          return null;
        }
        LeafPage<TKey, TItem> rightPage = leafPage.Split().AsLeafPage;

        if (leafPage.RightPageRef==null)
          RightmostPageRef = rightPage.Identifier;

        if (result.Pointer < PageSize / 2) {
          leafPage.Insert(result.Pointer, item);
          leafPage.AddToMeasures(item);
        }
        else {
          rightPage.Insert(result.Pointer - PageSize / 2, item);
          rightPage.AddToMeasures(item);
        }
        return rightPage;
      }
    }

    /// <returns>Replaced item.</returns>
    private TItem InternalReplace(DataPage<TKey, TItem> page, TItem item)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      TKey key = KeyExtractor(item);
      SeekResultPointer<int> result = page.Seek(key);
      if (innerPage!=null) {
        TItem replacedItem = InternalReplace(innerPage.GetPage(result.Pointer), item);
        innerPage.SubtractFromMeasures(replacedItem);
        innerPage.AddToMeasures(item);
        return replacedItem;
      }
      else {
        if (result.ResultType!=SeekResultType.Exact)
          throw new ArgumentOutOfRangeException("item", "Specified key could not be found.");
        TItem replacedItem = leafPage[result.Pointer];
        leafPage[result.Pointer] = item;
        leafPage.SubtractFromMeasures(replacedItem);
        leafPage.AddToMeasures(item);
        return replacedItem;
      }
    }

    private bool InternalRemove(DataPage<TKey, TItem> page, TKey key, out TItem item)
    {
      LeafPage<TKey, TItem> leafPage = page.AsLeafPage;
      InnerPage<TKey, TItem> innerPage = page.AsInnerPage;
      SeekResultPointer<int> result = page.Seek(key);

      if (innerPage!=null) {
        DataPage<TKey, TItem> childPage = innerPage.GetPage(result.Pointer);
        bool find = InternalRemove(childPage, key, out item);
        if (childPage.CurrentSize < PageSize / 2)
          MergePages(innerPage, result.Pointer, childPage);
        if (find)
          innerPage.SubtractFromMeasures(item);
        return find;
      }
      else {
        if (result.ResultType!=SeekResultType.Exact) {
          item = default(TItem);
          return false;
        }
        item = leafPage[result.Pointer];
        leafPage.SubtractFromMeasures(item);
        leafPage.Remove(result.Pointer);

        return true;
      }
    }

    private void MergePages(InnerPage<TKey, TItem> page, int index, DataPage<TKey, TItem> childPage)
    {
      bool isPrevious;
      DataPage<TKey, TItem> previous = null;
      DataPage<TKey, TItem> next = null;
      int size = page.CurrentSize;

      if (size==0) {
        // page parameter is RootPage; we should replace RootPage with childPage;
        RootPageRef = childPage;
        return;
      }

      if (index==-1) {
        isPrevious = false;
        next = page.GetPage(index + 1);
      }
      else if (index==size - 1) {
        isPrevious = true;
        previous = page.GetPage(index - 1);
      }
      else {
        previous = page.GetPage(index - 1);
        next = page.GetPage(index + 1);
        isPrevious = previous.CurrentSize < next.CurrentSize;
      }

      if (isPrevious) {
        if (previous.Merge(childPage))
          page.Remove(index);
        else
          page[index] = new KeyValuePair<TKey, IPageRef>(childPage.Key, childPage.Identifier);
      }
      else {
        if (childPage.Merge(next))
          page.Remove(index + 1);
        else
          page[index + 1] = new KeyValuePair<TKey, IPageRef>(next.Key, next.Identifier);
      }
    }

    private void ChangeRootPage(DataPage<TKey, TItem> newInnerPage)
    {
      DataPage<TKey, TItem> oldRoot = RootPage;
      RootPageRef = new InnerPage<TKey, TItem>(provider);
      InnerPage<TKey, TItem> rootPage = RootPage.AsInnerPage;
      rootPage.Insert(-1, default(TKey), oldRoot.Identifier);
      rootPage.Insert(0, newInnerPage.Key, newInnerPage.Identifier);
      if (HasMeasures) {
        rootPage.AddToMeasures(oldRoot);
        rootPage.AddToMeasures(newInnerPage);
      }
    }
  }
}
