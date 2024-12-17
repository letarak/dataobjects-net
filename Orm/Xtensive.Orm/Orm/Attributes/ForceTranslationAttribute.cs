using System;
using JetBrains.Annotations;

namespace Xtensive.Orm;

/// <summary>
/// Attribute for specifying method which would be always translated inside select projection
/// </summary>
[Serializable]
[AttributeUsage(AttributeTargets.Method, Inherited = false)]
[MeansImplicitUse]
public sealed class ForceTranslationAttribute : Attribute
{
}