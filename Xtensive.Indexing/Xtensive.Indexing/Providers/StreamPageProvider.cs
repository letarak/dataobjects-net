// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Nick Svetlov
// Created:    2007.12.26

using System;
using System.IO;
using System.Threading;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Internals.DocTemplates;
using Xtensive.Core.IO;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Core.Threading;
using Xtensive.Indexing.BloomFilter;
using Xtensive.Indexing.Implementation;
using Xtensive.Indexing.Implementation.Interfaces;
using Xtensive.Indexing.Providers.Internals;
using Xtensive.Indexing.Resources;
using Xtensive.Core.Helpers;

namespace Xtensive.Indexing.Providers
{
  /// <summary>
  /// Serialize-and-read <see cref="Stream"/> page provider.
  /// </summary>
  /// <typeparam name="TKey">Key type.</typeparam>
  /// <typeparam name="TItem">Value type.</typeparam>
  public class StreamPageProvider<TKey, TItem> : IndexPageProviderBase<TKey, TItem>
  {
    private StreamProvider streamProvider;
    private readonly IValueSerializer serializer;
    private readonly ValueSerializer<long> offsetSerializer;
    private readonly WeakCache<IPageRef, Page<TKey, TItem>> pageCache;
    private readonly ReaderWriterLockSlim pageCacheLock = new ReaderWriterLockSlim();
    private bool descriptorPageIdentifierAssigned;
    private bool rootPageIdentifierAssigned;
    private readonly StreamSerializationHelper<TKey, TItem> serializeHelper;

    /// <inheritdoc/>
    public override ISerializationHelper<TKey, TItem> SerializationHelper
    {
      get { return serializeHelper; }
    }

    /// <inheritdoc/>
    public override IndexFeatures Features
    {
      get { return IndexFeatures.SerializeAndRead; }
    }

    /// <summary>
    /// Gets the stream provider.
    /// </summary>
    /// <value>The stream provider.</value>
    public StreamProvider StreamProvider
    {
      get { return streamProvider; }
    }

    #region PageCache access methods

    /// <inheritdoc/>
    public override void AddToCache(Page<TKey, TItem> page)
    {
      if (pageCache!=null) {
        LockCookie? cookie = pageCacheLock.BeginWrite();
        try {
          pageCache.Add(page);
        }
        finally {
          pageCacheLock.EndWrite(cookie);
        }
      }
    }

    /// <inheritdoc/>
    public override void RemoveFromCache(Page<TKey, TItem> page)
    {
      if (pageCache!=null) {
        LockCookie? cookie = pageCacheLock.BeginWrite();
        try {
          pageCache.Remove(page);
        }
        catch (Exception) {
          return;
        }
        finally {
          pageCacheLock.EndWrite(cookie);
        }
      }
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> GetFromCache(IPageRef pageRef)
    {
      if (pageCache!=null) {
        pageCacheLock.BeginRead();
        try {
          return pageCache[pageRef, true];
        }
        catch (Exception) {
          return null;
        }
        finally {
          pageCacheLock.EndRead();
        }
      }
      else {
        return null;
      }
    }

    #endregion

    /// <inheritdoc/>
    public override void AssignIdentifier(Page<TKey, TItem> page)
    {
      if (page is DescriptorPage<TKey, TItem>) {
        if (descriptorPageIdentifierAssigned)
          throw Exceptions.InternalError("Second DescriptorPage has been created.", Log.Instance);
        page.Identifier = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Descriptor);
        descriptorPageIdentifierAssigned = true;
      }
      else if (!rootPageIdentifierAssigned && (page is LeafPage<TKey, TItem>)) {
        page.Identifier = StreamPageRef<TKey, TItem>.Create((long) 0);
        rootPageIdentifierAssigned = true;
      }
      else
        page.Identifier = StreamPageRef<TKey, TItem>.Create(StreamPageRefType.Undefined);
    }

    /// <inheritdoc/>
    public override Page<TKey, TItem> Resolve(IPageRef identifier)
    {
      if (identifier==null)
        return null;
      Page<TKey, TItem> page = identifier as Page<TKey, TItem>;
      if (page!=null) // Cached page
        return page;
      StreamPageRef<TKey, TItem> streamPageRef = (StreamPageRef<TKey, TItem>) identifier;
      if (!streamPageRef.IsDefined)
        throw Exceptions.InternalError(String.Format("Undefined {0}.", streamPageRef), Log.Instance);
      page = GetFromCache(identifier);
      if (page==null) {
        try {
          page = Deserialize(streamPageRef);
          if (page==null)
            throw Exceptions.InternalError(String.Format("StreamPageRef {0} points to null page.", streamPageRef), Log.Instance);
          page.Provider = this;
          page.Identifier = identifier;
          pageCacheLock.ExecuteWriter(delegate { pageCache.Add(page); });
        }
        catch (Exception e) {
          Log.Error(Strings.ExCantDeserializeIndexPage, streamPageRef, e);
          throw;
        }
      }
      return page;
    }

    /// <inheritdoc/>
    public override void Flush()
    {
      throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
    }

    /// <inheritdoc/>
    public override void Clear()
    {
      throw new NotSupportedException(Strings.ExIndexPageProviderDoesntSupportWrite);
    }

    private Page<TKey, TItem> Deserialize(StreamPageRef<TKey, TItem> pageRef)
    {
      if (pageRef==null)
        return null;
      if (!pageRef.IsDefined)
        throw Exceptions.InternalError(String.Format("Undefined {0}.", pageRef), Log.Instance);
      long offset = pageRef.Offset;
      if (offset < 0)
        return null;
      Stream stream = streamProvider.GetStream();
      try {
        stream.Seek(offset, SeekOrigin.Begin);
        Page<TKey, TItem> page = (Page<TKey, TItem>) serializer.Deserialize(stream);
        LeafPage<TKey, TItem> leafPage = page as LeafPage<TKey, TItem>;
        if (leafPage!=null)
          leafPage.RightPageRef = StreamPageRef<TKey, TItem>.Create(offsetSerializer.Deserialize(stream));
        return page;
      }
      finally {
        streamProvider.ReleaseStream(stream);
      }
    }

    private DescriptorPage<TKey, TItem> DeserializeDescriptorPage()
    {
      Stream stream = streamProvider.GetStream();
      try {
        // This is DescriptorPage, so its actual offset is the last serialized number in the stream
        stream.Seek(-StreamSerializationHelper<TKey, TItem>.OffsetLength, SeekOrigin.End);
        long offset = offsetSerializer.Deserialize(stream);
        stream.Seek(offset, SeekOrigin.Begin);
        DescriptorPage<TKey, TItem> descriptorPage = (DescriptorPage<TKey, TItem>) serializer.Deserialize(stream);

        if (descriptorPage.Configuration.UseBloomFilter)
          descriptorPage.BloomFilter = new MemoryBloomFilter<TKey>(stream);

        return descriptorPage;
      }
      finally {
        streamProvider.ReleaseStream(stream);
      }
    }

    /// <inheritdoc/>
    public override void Initialize()
    {
      if (IsInitialized)
        throw new InvalidOperationException(Strings.ExIndexIsAlreadyInitialized);
      if (streamProvider.FileExists) {
        DescriptorPage<TKey, TItem> descriptorPage = DeserializeDescriptorPage();
        descriptorPage.Provider = this;
        index.DescriptorPage = descriptorPage;
      }
      BaseInitialize();
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
      streamProvider.DisposeSafely();
      streamProvider = null;
    }


    // Constructors

    /// <summary>
    /// <see cref="ClassDocTemplate.Ctor" copy="true" />
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <param name="cacheSize">Page cache size (in pages).</param>
    public StreamPageProvider(string fileName, int cacheSize)
    {
      ArgumentValidator.EnsureArgumentNotNull(fileName, "fileName");
      ArgumentValidator.EnsureArgumentIsInRange(cacheSize, 0, int.MaxValue, "cacheSize");
      streamProvider = new StreamProvider(fileName);
      serializer = ValueSerializationScope.CurrentSerializer; // BinarySerializer by default
      offsetSerializer = ValueSerializer<long>.Default;
      serializeHelper = new StreamSerializationHelper<TKey, TItem>(serializer, offsetSerializer);
      if (cacheSize > 0) {
        pageCache =
          new WeakCache<IPageRef, Page<TKey, TItem>>(
            cacheSize,
            value => value.Identifier,
            value => 1);
      }
    }
  }
}