// Copyright (C) 2008-2024 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
// Created by: Alexey Gamzov
// Created:    2008.08.07

using System;
using System.Configuration;
using Xtensive.Core;

namespace Xtensive.Orm.Configuration.Elements
{
  /// <summary>
  /// <see cref="NamingConvention"/> configuration element within a configuration file.
  /// </summary>
  public class NamingConventionElement : ConfigurationElement
  {
    private const string LetterCasePolicyElementName = "letterCasePolicy";
    private const string NamespacePolicyElementName = "namespacePolicy";
    private const string NamingRulesElementName = "namingRules";
    private const string NamespaceSynonymsElementName = "namespaceSynonyms";

    /// <summary>
    /// <see cref="NamingConvention.LetterCasePolicy" />
    /// </summary>
    [ConfigurationProperty(LetterCasePolicyElementName, IsRequired = false, IsKey = false, DefaultValue = "Default")]
    public string LetterCasePolicy
    {
      get { return (string) this[LetterCasePolicyElementName]; }
      set { this[LetterCasePolicyElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamespacePolicy" />
    /// </summary>
    [ConfigurationProperty(NamespacePolicyElementName, IsRequired = false, IsKey = false, DefaultValue = "Default")]
    public string NamespacePolicy
    {
      get { return (string) this[NamespacePolicyElementName]; }
      set { this[NamespacePolicyElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamingRules" />
    /// </summary>
    [ConfigurationProperty(NamingRulesElementName, IsRequired = false, IsKey = false, DefaultValue = "Default")]
    public string NamingRules
    {
      get { return (string) this[NamingRulesElementName]; }
      set { this[NamingRulesElementName] = value; }
    }

    /// <summary>
    /// <see cref="NamingConvention.NamespaceSynonyms" />
    /// </summary>
    [ConfigurationProperty(NamespaceSynonymsElementName, IsRequired = false, IsKey = false)]
    [ConfigurationCollection(typeof (ConfigurationCollection<NamespaceSynonymElement>), AddItemName = "synonym")]
    public ConfigurationCollection<NamespaceSynonymElement> NamespaceSynonyms
    {
      get { return (ConfigurationCollection<NamespaceSynonymElement>) this[NamespaceSynonymsElementName]; }
    }

    /// <summary>
    /// Converts the element to a native configuration object it corresponds to - 
    /// i.e. to a <see cref="NamingConvention"/> object.
    /// </summary>
    /// <returns>The result of conversion.</returns>
    public NamingConvention ToNative()
    {
      var result = new NamingConvention{
          LetterCasePolicy = (LetterCasePolicy) Enum.Parse(typeof(LetterCasePolicy), LetterCasePolicy, true),
          NamespacePolicy = (NamespacePolicy) Enum.Parse(typeof(NamespacePolicy), NamespacePolicy, true),
          NamingRules = (NamingRules) Enum.Parse(typeof(NamingRules), NamingRules, true)
        };
      foreach (var namespaceSynonym in NamespaceSynonyms)
        result.NamespaceSynonyms.Add(namespaceSynonym.Namespace, namespaceSynonym.Synonym);
      return result;
    }
  }
}