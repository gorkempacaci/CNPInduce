using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using CNP.Language;
using CNP.Helper.EagerLinq;
using CNP.Helper;

namespace CNP.Language
{
  public class ProjectionMap : IReadOnlyDictionary<NameVar, NameVar>
  {
    private Dictionary<NameVar, NameVar> dict;

    public ProjectionMap(params (string, string)[] tups)
        : this(tups.ToDictionary(x => new NameVar(x.Item1), y => new NameVar(y.Item2)))
    {
    }

    public ProjectionMap(params (NameVar, NameVar)[] tups)
        : this(tups.ToDictionary(x => x.Item1, x => x.Item2))
    {
    }

    public ProjectionMap(IEnumerable<KeyValuePair<string, string>> args)
        : this(args.ToDictionary(x => new NameVar(x.Key), y => new NameVar(y.Value)))
    {

    }

    public ProjectionMap(IEnumerable<KeyValuePair<NameVar, NameVar>> args)
        : this(args.ToDictionary(x => x.Key, x => x.Value))
    {

    }

    //TODO: assert that the image is a set. (can't produce id using proj)
    public ProjectionMap(IDictionary<NameVar, NameVar> dic)
    {
      dict = new Dictionary<NameVar, NameVar>(dic);
    }

    public ProjectionMap Clone(TermReferenceDictionary plannedParenthood)
    {
      return new ProjectionMap(dict.ToDictionary(kvp => kvp.Key.Clone(plannedParenthood), kvp => kvp.Value.Clone(plannedParenthood)));
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
