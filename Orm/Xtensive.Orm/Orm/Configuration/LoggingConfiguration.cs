// Copyright (C) 2013-2020 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Kulakov
// Created:    2013.09.27

using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using Xtensive.Core;
using Xtensive.Orm.Configuration.Internals;
using ConfigurationSection = Xtensive.Orm.Configuration.Elements.ConfigurationSection;

namespace Xtensive.Orm.Configuration
{
  /// <summary>
  /// Configuration of logging
  /// </summary>
  public sealed class LoggingConfiguration
  {
    /// <summary>
    /// Gets or sets external provider. Provider's name specified as assembly qualified name.
    /// </summary>
    public string Provider { get; set; }

    /// <summary>
    /// Gets or sets list of <see cref="LogConfiguration"/>
    /// </summary>
    public IList<LogConfiguration> Logs { get; set; }

    /// <summary>
    /// Loads logging configuration from the default configuration section.
    /// </summary>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load()
    {
      return Load(WellKnown.DefaultConfigurationSection);
    }

    /// <summary>
    /// Loads logging configuration from the specified configuration section.
    /// </summary>
    /// <param name="sectionName">Name of configuration section.</param>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load(string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, nameof(sectionName));

      var section = (ConfigurationSection)ConfigurationManager.GetSection(sectionName);
      if (section==null)
        throw new InvalidOperationException(string.Format(Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      var configuration = section.Logging.ToNative();
      return configuration;
    }

    /// <summary>
    /// Loads logging configuration from the default configuration section.
    /// </summary>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load(System.Configuration.Configuration configuration)
    {
      return Load(configuration, WellKnown.DefaultConfigurationSection);
    }

    /// <summary>
    /// Loads logging configuration from the specified configuration section.
    /// </summary>
    /// <param name="configuration">A <see cref="System.Configuration.Configuration"/>
    /// instance to load from.</param>
    /// <param name="sectionName">Name of configuration section.</param>
    /// <returns>Loaded configuration.</returns>
    public static LoggingConfiguration Load(System.Configuration.Configuration configuration, string sectionName)
    {
      ArgumentValidator.EnsureArgumentNotNull(configuration, nameof(configuration));
      ArgumentValidator.EnsureArgumentNotNullOrEmpty(sectionName, nameof(sectionName));

      var section = (ConfigurationSection) configuration.GetSection(sectionName);
      if (section==null)
        throw new InvalidOperationException(string.Format(Strings.ExSectionIsNotFoundInApplicationConfigurationFile, sectionName));
      var loggingConfiguration = section.Logging.ToNative();
      return loggingConfiguration;
    }

    /// <summary>
    /// Loads logging configuration from the specified configuration section.
    /// </summary>
    /// <param name="configurationSection">Root configuration section where logging configuration is placed.</param>
    /// <returns>Logging configuration.</returns>
    /// <exception cref="InvalidOperationException">The logging section is not found.</exception>
    public static LoggingConfiguration Load(IConfigurationSection configurationSection)
    {
      return new LoggingConfigurationReader().Read(configurationSection);
    }

    /// <summary>
    /// Loads logging configuration from default section.
    /// </summary>
    /// <param name="configurationRoot">Root of configuration.</param>
    /// <returns>Read configuration.</returns>
    /// <exception cref="InvalidOperationException">The logging section is not found.</exception>
    public static LoggingConfiguration Load(IConfigurationRoot configurationRoot)
    {
      return new LoggingConfigurationReader().Read(configurationRoot);
    }

    /// <summary>
    /// Loads logging configuration from specific section.
    /// </summary>
    /// <param name="configurationRoot">Root of configuration.</param>
    /// <param name="sectionName">Section name where logging configuration is defined.</param>
    /// <returns>Read configuration.</returns>
    /// <exception cref="InvalidOperationException">The logging section is not found.</exception>
    public static LoggingConfiguration Load(IConfigurationRoot configurationRoot, string sectionName)
    {
      return new LoggingConfigurationReader().Read(configurationRoot, sectionName);
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    public LoggingConfiguration()
    {
      Logs = new List<LogConfiguration>();
    }

    /// <summary>
    /// Creates instance of this class.
    /// </summary>
    /// <param name="provider">External provider for logging. Provider's name specified as assembly qualified name.</param>
    public LoggingConfiguration(string provider)
    {
      Provider = provider;
      Logs = new List<LogConfiguration>();
    }
  }
}
