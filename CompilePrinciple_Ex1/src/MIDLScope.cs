using System.Collections.Generic;

public enum IdentifierType
{
    All,
    Module,
    Struct,
    Declaration,
}

public class MIDLScope
{
    public string Name { get; set; } = "<default_scope>";
    public IDictionary<string, IdentifierType> Identifiers { get; private set; } = new Dictionary<string,IdentifierType>();
    public MIDLScope Parent { get; set; }
    public IDictionary<string, MIDLScope> ChildScopes { get; private set; } = new Dictionary<string, MIDLScope>();
    public string GetScopePrefix()
    {
        return Parent == null ? "" : Parent.GetScopePrefix() + $"{Name}::";
    }

    private bool SearchScopeNameInternal(string[] scopes, int startIndex, IdentifierType type)
    {
        if (startIndex == scopes.Length - 1)
        {
            string identifier = scopes[startIndex];
            if (!Identifiers.ContainsKey(identifier))
                return false;
            return type == IdentifierType.All ? true : Identifiers[identifier] == type;
        }
        string scope = scopes[startIndex];
        if (ChildScopes.ContainsKey(scope))
            return ChildScopes[scope].SearchScopeNameInternal(scopes, startIndex + 1, type);
        else
            return false;
    }

    public bool SearchScopeName(string scopeName, IdentifierType type = IdentifierType.All)
    {
        string[] scopes = scopeName.Split("::");
        return SearchScopeNameInternal(scopes, 0, type)
            || Parent == null ? false : Parent.SearchScopeNameInternal(scopes, 0, type);
    }

    public override string ToString() => $"Scope:{Name}";
}