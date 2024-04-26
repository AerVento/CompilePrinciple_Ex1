using System;
using System.Collections.Generic;

public class ASTAnalyzer
{
    public HashSet<SystemType> Types = new HashSet<SystemType>()
    {
        SystemType.SHORT, SystemType.INT16,
        SystemType.LONG, SystemType.INT32,
        SystemType.LONG_LONG, SystemType.INT64,
        SystemType.UNSIGNED_SHORT, SystemType.UINT16,
        SystemType.UNSIGNED_LONG, SystemType.UINT32,
        SystemType.UNSIGNED_LONG_LONG, SystemType.UINT64,
        SystemType.FLOAT, SystemType.DOUBLE, SystemType.LONG_DOUBLE,
        SystemType.CHAR,
        SystemType.STRING,
        SystemType.BOOLEAN
    };

    public SystemType GetType(string typename)
    {
        foreach (var type in Types)
            if (type.Name == typename)
                return type;
        return null;
    }


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
                    Print(module.Start.Line, module.Start.Column,
                        $"The identifier \"{module.ID}\" is already defined in scope \"{parentScope.Name}\"."
                        );
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
                    Print(@struct.Start.Line, @struct.Start.Column,
                        $"The identifier \"{@struct.ID}\" is already defined in scope \"{parentScope.Name}\"."
                        );
                    continue;
                }
                parentScope.Identifiers.Add(@struct.ID, IdentifierType.Struct);
                var typeName = parentScope.GetScopePrefix() + @struct.ID;
                Types.Add(new SystemType.Custom(typeName));
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
                var type = GetType(member.TypeText);
                if(type == null && !parentScope.SearchScopeName(member.TypeText, IdentifierType.Struct))
                {
                    Print(member.Start.Line, member.Start.Column,
                        $"Type \"{member.TypeText}\" is not defined yet."
                        );
                    continue;
                }
                foreach(var child in member.Childs)
                {
                    var declarator = (ASTNode.Declarator)child;
                    if (parentScope.Identifiers.ContainsKey(declarator.ID))
                    {
                        Print(declarator.Start.Line, declarator.Start.Column,
                            $"The identifier \"{declarator.ID}\" is already defined in scope \"{parentScope.Name}\"."
                            );
                        continue;
                    }
                    parentScope.Identifiers.Add(declarator.ID, IdentifierType.Declaration);


                    if (!declarator.IsArray)
                    {
                        if (declarator.Childs.Count == 0)
                            continue;

                        ASTNode.Expression expression = (ASTNode.Expression)declarator.Childs[0];
                        if (!type.Accept(expression.Type))
                        {
                            Print(expression.Start.Line, expression.Start.Column,
                                $"Constant type \"{expression.Type}\" cannot be assigned to type \"{member.TypeText}\"."
                                );
                            continue;
                        }
                        else if(expression is ASTNode.Literal literal && !type.Accept(literal.Text))
                        {
                            Print(expression.Start.Line, expression.Start.Column,
                                $"Value \"{literal.Text}\" cannot be assigned to type \"{member.TypeText}\". ");
                            continue;
                        }
                    }
                    else
                    {
                        ASTNode.Expression expression = (ASTNode.Expression)declarator.Childs[0];
                        if (expression.Type != ConstantType.Integer)
                        {
                            Print(expression.Start.Line, expression.Start.Column,
                                $"The array length must be an integer number. \"{expression.Type}\" is provided."
                                );
                            continue;
                        }
                        for (int i = 1; i < declarator.Childs.Count; i++)
                        {
                            expression = (ASTNode.Expression)declarator.Childs[i];
                            if (!type.Accept(expression.Type))
                            {
                                Print(expression.Start.Line, expression.Start.Column,
                                    $"Constant type \"{expression.Type}\" cannot be assigned to type \"{member.TypeText}\"."
                                    );
                                continue;
                            }
                            else if (expression is ASTNode.Literal literal && !type.Accept(literal.Text))
                            {
                                Print(expression.Start.Line, expression.Start.Column,
                                    $"Value \"{literal.Text}\" cannot be assigned to type \"{member.TypeText}\".");
                                continue;
                            }
                        }
                    }
                }
            }
        }
    }

    private void Print(int line, int col, string msg)
    {
        Console.WriteLine($"[Line {line}:{col}]:{msg}");
    }

    public void Start(ASTNode node)
    {
        MIDLScope root = SearchScope(node);
        SearchDeclaration(root, node);
    }
}
