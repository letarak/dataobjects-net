// Copyright (C) 2007 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Dmitri Maximov
// Created:    2007.08.03

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xtensive.Core;
using Xtensive.Core.Collections;
using Xtensive.Core.Helpers;
using Xtensive.Storage.Building.Definitions;
using Xtensive.Storage.Configuration.TypeRegistry;

namespace Xtensive.Storage.Configuration
{
  /// <summary>
  /// The configuration of the <see cref="Domain"/>.
  /// </summary>
  [Serializable]
  public class DomainConfiguration : ConfigurationBase,
    IEquatable<DomainConfiguration>
  {
    //    private ServiceRegistry services = new ServiceRegistry();
    private CollectionBaseSlim<Type> builders = new CollectionBaseSlim<Type>();
    private UrlInfo connectionInfo;
    private NamingConvention namingConvention;
    private Registry types = new Registry(new TypeProcessor());
    private int sessionPoolSize = 64;
    private string name = string.Empty;
    private SessionConfiguration session;

    /// <summary>
    /// Gets or sets configuration name.
    /// </summary>
    public string Name
    {
      get { return name; }
      set
      {
        ArgumentValidator.EnsureArgumentNotNull(value, "value");
        name = value;
      }
    }

    /// <summary>
    /// Gets or sets the size of the session pool. The default value is 64.
    /// </summary>
    public int SessionPoolSize
    {
      get { return sessionPoolSize; }
      set
      {
        this.EnsureNotLocked();
        sessionPoolSize = value;
      }
    }

    /// <summary>
    /// Gets or sets the naming convention.
    /// </summary>
    public NamingConvention NamingConvention
    {
      get { return namingConvention; }
      set
      {
        this.EnsureNotLocked();
        namingConvention = value;
      }
    }

    /// <summary>
    /// Gets the <see cref="ICollection{T}"/> of persistent <see cref="Type"/>s that are about to be 
    /// registered in <see cref="Domain"/>.
    /// </summary>
    public Registry Types
    {
      get { return types; }
    }

    /// <summary>
    /// Gets or sets the connection info.
    /// </summary>
    public UrlInfo ConnectionInfo
    {
      get { return connectionInfo; }
      set
      {
        this.EnsureNotLocked();
        connectionInfo = value;
      }
    }

    /// <summary>
    /// Gets the collection of custom <see cref="DomainModelDef"/> builders.
    /// </summary>
    public IList<Type> Builders
    {
      get { return builders; }
    }

    /// <summary>
    /// Gets default session configuration.
    /// </summary>
    public SessionConfiguration Session
    {
      get { return session; }
    }

    /// <summary>
    /// Locks the instance and (possible) all dependent objects.
    /// </summary>
    /// <param name="recursive"><see langword="True"/> if all dependent objects should be locked as well.</param>
    public override void Lock(bool recursive)
    {
      types.Lock(true);
      builders.Lock(true);
      session.Lock(true);
      //      services.Lock(true);
      base.Lock(recursive);
    }

    /// <inheritdoc/>
    public override void Validate()
    {
    }

    /// <inheritdoc/>
    protected override ConfigurationBase CreateClone()
    {
      return new DomainConfiguration();
    }

    /// <inheritdoc/>
    protected override void Clone(ConfigurationBase source)
    {
      base.Clone(source);
      var configuration = (DomainConfiguration) source;
      builders = new CollectionBase<Type>(configuration.Builders);
      connectionInfo = new UrlInfo(configuration.ConnectionInfo.ToString());
      namingConvention = (NamingConvention) configuration.NamingConvention.Clone();
      types = (Registry) configuration.Types.Clone();
      sessionPoolSize = configuration.SessionPoolSize;
      name = configuration.Name;
      session = configuration.Session;
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      if (ReferenceEquals(this, obj))
        return true;
      if (obj.GetType() != typeof(DomainConfiguration))
        return false;
      return Equals((DomainConfiguration)obj);
    }

    /// <inheritdoc/>
    public bool Equals(DomainConfiguration other)
    {
      if (ReferenceEquals(null, other))
        return false;
      if (ReferenceEquals(this, other))
        return true;
      return Equals(other.builders, builders) 
        && Equals(other.connectionInfo, connectionInfo) 
        && Equals(other.namingConvention, namingConvention) 
        && Equals(other.types, types) 
        && other.sessionPoolSize == sessionPoolSize 
        && Equals(other.name, name) && Equals(other.session, session);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
      unchecked {
        int result = builders.GetHashCodeRecursive();
        result = (result * 397) ^ (connectionInfo != null ? connectionInfo.GetHashCode() : 0);
        result = (result * 397) ^ (namingConvention != null ? namingConvention.GetHashCode() : 0);
        result = (result * 397) ^ types.GetHashCodeRecursive();
        result = (result * 397) ^ sessionPoolSize;
        result = (result * 397) ^ (name != null ? name.GetHashCode() : 0);
        result = (result * 397) ^ (session != null ? session.GetHashCode() : 0);
        return result;
      }
    }

    
    // Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class.
    /// </summary>
    /// <param name="connectionUrl">The string containing connection URL for <see cref="Domain"/>.</param>
    /// <exception cref="ArgumentNullException">When <paramref name="connectionUrl"/> is null or empty string.</exception>
    public DomainConfiguration(string connectionUrl)
      : this()
    {
      ArgumentValidator.EnsureArgumentNotNull(connectionUrl, "connectionUrl");
      connectionInfo = new UrlInfo(connectionUrl);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class.
    /// </summary>
    public DomainConfiguration()
    {
      namingConvention = new NamingConvention();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainConfiguration"/> class from specified <see cref="DomainElement"/>.
    /// </summary>
    /// <param name="element"><see cref="DomainElement"/> to initialize <see cref="DomainConfiguration"/> from.</param>
    internal DomainConfiguration(DomainElement element)
    {
      ArgumentValidator.EnsureArgumentNotNull(element, "element");
      name = element.Name;
      connectionInfo = element.ConnectionInfo;
      sessionPoolSize = element.SessionPoolSize;
      foreach (BuilderElement builder in element.Builders) {
        Type type = Type.GetType(builder.Type, true);
        Builders.Add(type);
      }
      foreach (TypeElement registry in element.Types) {
        Assembly assembly = Assembly.Load(registry.Assembly);
        if (registry.Namespace.IsNullOrEmpty())
          Types.Register(assembly);
        else
          Types.Register(assembly, registry.Namespace);
      }
      namingConvention = element.NamingConvention.AsNamingConvention();
      session = element.Session.AsSessionConfiguration();
    }
  }
}