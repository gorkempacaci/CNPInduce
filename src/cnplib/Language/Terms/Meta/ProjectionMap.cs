using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Language;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{
  /// <summary>
  /// Maps source argument names to target argument names.
  /// in a program like proj(id, {a:u, b:v}), represents the {a:u, b:v}. Therefore keys belong to the inner expression and values to the outer one.
  /// </summary>
  public class ProjectionMap : IReadOnlyDictionary<NameVar, NameVar>
  {
    private Dictionary<NameVar, NameVar> dict;

    public ProjectionMap(params (NameVar, NameVar)[] pairs)
      : this(pairs.ToDictionary(x => x.Item1, x => x.Item2)) { }

    public ProjectionMap(IEnumerable<(NameVar, NameVar)> pairs)
      : this(pairs.ToDictionary(x => x.Item1, x => x.Item2)) { }

    public ProjectionMap(IEnumerable<KeyValuePair<NameVar, NameVar>> args)
      : this(args.ToDictionary(x => x.Key, x => x.Value)) { }

    public ProjectionMap(IDictionary<NameVar, NameVar> dic)
    {
      dict = new Dictionary<NameVar, NameVar>(dic);
#if DEBUG
      // assert that the co-domain of projections is a set
      if (!dict.Values.IsSet())
        throw new ArgumentException("Projection cannot map twice to the same argument name." + dic.ToMappingString());
#endif

    }

    public ProjectionMap Clone(TermReferenceDictionary plannedParenthood)
    {
      return new ProjectionMap(dict.ToDictionary(kvp => kvp.Key.Clone(plannedParenthood), kvp => kvp.Value.Clone(plannedParenthood)));
    }

    public override string ToString()
    {
      return "{" + string.Join(", ", this.Select(nn => nn.Key + ":" + nn.Value)) + "}";
    }

    #region Delegate to this.dict
    public NameVar this[NameVar key] => dict[key];
    public IEnumerable<NameVar> Keys => dict.Keys;
    public IEnumerable<NameVar> Values => dict.Values;
    public int Count => dict.Count;
    public bool ContainsKey(NameVar key) => dict.ContainsKey(key);
    public IEnumerator<KeyValuePair<NameVar, NameVar>> GetEnumerator() => dict.GetEnumerator();
    public bool TryGetValue(NameVar key, [MaybeNullWhen(false)] out NameVar value) => dict.TryGetValue(key, out value);
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)dict).GetEnumerator();
    #endregion
  }
}
