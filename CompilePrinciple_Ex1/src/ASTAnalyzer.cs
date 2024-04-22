using System;
using System.Collections.Generic;
using System.Security;

public class ASTAnalyzer
{
    public HashSet<string> TypeNames = new HashSet<string>()
    {
        "short", "int16",
        "long", "int32",
        "longlong", "int64",
        "unsignedshort", "uint16",
        "unsignedlong", "uint32",
        "unsignedlonglong", "uint64",
        "char", "string",
        "boolean",
        "float", "double", "longdouble",
    };

    private Dictionary<ConstantType, HashSet<string>> AcceptableConstantType = new Dictionary<ConstantType, HashSet<string>>()
    {
        {ConstantType.Never, new HashSet<string>()},
        {ConstantType.Integer,
            new HashSet<string>() {
            "short", "int16",
            "long", "int32",
            "longlong", "int64",
            "unsignedshort", "uint16",
            "unsignedlong", "uint32",
            "unsignedlonglong", "uint64",
            }
        },
        {ConstantType.Float,
            new HashSet<string>()
            {
                "float", "double", "longdouble",
            }
        },
        {ConstantType.Char, new HashSet<string>(){"char"}},
        {ConstantType.String, new HashSet<string>{"string"}},
        {ConstantType.Boolean, new HashSet<string>{"boolean"}},
    };

    private MIDLScope SearchScope(ASTNode tree)
    {
        Stack<(MIDLScope ParentScope, ASTNode Node)> stack = new Stack<(MIDLScope ParentScope, ASTNode Node)>();
        MIDLScope rootScope = new MIDLScope();
        for (int i = tree.Childs.Count - 1; i >= 0; i--)
            stack.Push((rootScope, tree.Childs[i]));
        while (stack.Count > 0)
        {
            (MIDLScope parentScope, ASTNode node) = stack.Pop();
            if (node is ASTNode.Module module)
            {
                if (parentScope.Identifiers.ContainsKey(module.ID))
                {
                    Console.WriteLine($"The identifier {module.ID} is already defined in scope {parentScope.Name}");
                    continue;
                }
                parentScope.Identifiers.Add(module.ID, IdentifierType.Module);
                var moduleScope = new MIDLScope() { Name = module.ID };
                parentScope.ChildScopes.Add(module.ID, moduleScope);
                moduleScope.Parent = parentScope;

                for (int i = node.Childs.Count - 1; i >= 0; i--)
                    stack.Push((moduleScope, node.Childs[i]));
            }
            else if (node is ASTNode.Struct @struct)
            {
                if (parentScope.Identifiers.ContainsKey(@struct.ID))
                {
                    Console.WriteLine($"The identifier {@struct.ID} is already defined in scope {parentScope.Name}");
                    continue;
                }
                parentScope.Identifiers.Add(@struct.ID, IdentifierType.Struct);
                var typeName = parentScope.GetScopePrefix() + @struct.ID;
                TypeNames.Add(typeName);
                var structScope = new MIDLScope() { Name = @struct.ID };
                parentScope.ChildScopes.Add(@struct.ID, structScope);
                structScope.Parent = parentScope;

                for (int i = node.Childs.Count - 1; i >= 0; i--)
                    stack.Push((structScope, node.Childs[i]));
            }
        }
        return rootScope;
    }

    private void SearchDeclaration(MIDLScope root, ASTNode tree)
    {
        Stack<(MIDLScope ParentScope, ASTNode Node)> stack = new Stack<(MIDLScope ParentScope, ASTNode Node)>();
        for (int i = tree.Childs.Count - 1; i >= 0; i--)
            stack.Push((root, tree.Childs[i]));
        while (stack.Count > 0)
        {
            (MIDLScope parentScope, ASTNode node) = stack.Pop();
            if (node is ASTNode.Scope scope)
            {
                var tmp = parentScope.ChildScopes[scope.ID];
                for (int i = node.Childs.Count - 1; i >= 0; i--)
                    stack.Push((tmp, node.Childs[i]));
            }
            else if (node is ASTNode.Member member)
            {
                if(!TypeNames.Contains(member.TypeText)
                    && !parentScope.SearchScopeName(member.TypeText, IdentifierType.Struct))
                {
                    Console.WriteLine($"Type {member.TypeText} is not defined yet.");
                    continue;
                }
                foreach(var child in member.Childs)
                {
                    var declarator = (ASTNode.Declarator)child;
                    if (parentScope.Identifiers.ContainsKey(declarator.ID))
                    {
                        Console.WriteLine($"The identifier {declarator.ID} is already defined in scope {parentScope.Name}");
                        continue;
                    }
                    parentScope.Identifiers.Add(declarator.ID, IdentifierType.Declaration);
                    if(declarator.Childs.Count > 0)
                    {
                        if (!declarator.IsArray)
                        {
                            ASTNode.Expression expression = (ASTNode.Expression)declarator.Childs[0];
                            if (!AcceptableConstantType[expression.Type].Contains(member.TypeText))
                            {
                                Console.WriteLine($"Constant type {expression.Type} cannot be assigned to type {member.TypeText}).");
                                continue;
                            }
                            // TODO: Check the range of the constant value.
                        }
                        else
                        {
                            ASTNode.Expression expression = (ASTNode.Expression)declarator.Childs[0];
                            if (expression.Type != ConstantType.Integer)
                            {
                                Console.WriteLine($"The array length must be an integer number. {expression.Type} is provided.");
                                continue;
                            }
                            for(int i = 1; i < declarator.Childs.Count; i++)
                            {
                                expression = (ASTNode.Expression)declarator.Childs[i];
                                if (!AcceptableConstantType[expression.Type].Contains(member.TypeText))
                                {
                                    Console.WriteLine($"Constant type {expression.Type} cannot be assigned to type {member.TypeText}).");
                                    continue;
                                }

                                // TODO: Check the range of constant value.
                            }
                        }
                    }
                }
            }
        }
    }

    public void Start(ASTNode node)
    {
        MIDLScope root = SearchScope(node);
        SearchDeclaration(root, node);
    }
}
