using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;

public class ASTGenerator : MIDLBaseVisitor<ASTNode>
{
    public override ASTNode VisitSpecification([NotNull] MIDLParser.SpecificationContext context)
    {
        ASTNode specification = new ASTNode.Specification();
        foreach (var definition in context.definition())
        {
            var child = VisitDefinition(definition);
            child.Parent = specification;
            specification.Childs.Add(child);
        }
        specification.Start = context.Start;
        specification.Stop = context.Stop;
        return specification;
    }

    public override ASTNode VisitDefinition([NotNull] MIDLParser.DefinitionContext context)
    {
        if (context.type_decl() != null)
            return VisitType_decl(context.type_decl());
        else if (context.module() != null)
            return VisitModule(context.module());
        return null;
    }

    public override ASTNode VisitModule([NotNull] MIDLParser.ModuleContext context)
    {
        ASTNode.Module module = new ASTNode.Module();
        module.ID = context.ID().GetText();
        foreach (var definition in context.definition())
        {
            var child = VisitDefinition(definition);
            child.Parent = module;
            module.Childs.Add(child);
        }
        module.Start = context.Start;
        module.Stop = context.Stop;
        return module;
    }

    public override ASTNode VisitType_decl([NotNull] MIDLParser.Type_declContext context)
    {
        if (context.ID() != null)
        {
            ASTNode.Struct @struct = new ASTNode.Struct();
            @struct.ID = context.ID().GetText();
            return @struct;
        }
        else if (context.struct_type() != null)
        {
            return VisitStruct_type(context.struct_type());
        }
        return null;
    }

    public override ASTNode VisitStruct_type([NotNull] MIDLParser.Struct_typeContext context)
    {
        ASTNode.Struct @struct = new ASTNode.Struct();
        @struct.ID = context.ID().GetText();
        var member_list = context.member_list();
        var type_spec = member_list.type_spec();
        var declarators = member_list.declarators();
        int len = type_spec.Length;
        for (int i = 0; i < len; i++)
        {
            ASTNode.Member member = new ASTNode.Member();
            member.TypeText = type_spec[i].GetText();
            member.Start = type_spec[i].Start;
            member.Stop = declarators[i].Stop;
            foreach (var declarator in declarators[i].declarator())
            {
                var child = VisitDeclarator(declarator);
                child.Parent = member;
                member.Childs.Add(child);
            }

            member.Parent = @struct;
            @struct.Childs.Add(member);
        }
        @struct.Start = context.Start;
        @struct.Stop = context.Stop;
        return @struct;
    }

    public override ASTNode VisitDeclarator([NotNull] MIDLParser.DeclaratorContext context)
    {
        ASTNode.Declarator declarator = new ASTNode.Declarator();
        declarator.Start = context.Start;
        declarator.Stop = context.Stop;
        if (context.simple_declarator() != null)
        {
            var simple = context.simple_declarator();
            declarator.ID = simple.ID().GetText();
            if(simple.or_expr() != null)
            {
                var child = VisitOr_expr(simple.or_expr());
                child.Parent = declarator;
                declarator.Childs.Add(child);
            }
        }
        else if (context.array_declarator() != null)
        {
            var array = context.array_declarator();
            declarator.IsArray = true;
            declarator.ID = array.ID().GetText();
            var child = VisitOr_expr(array.or_expr());
            child.Parent = declarator;
            declarator.Childs.Add(child);
            if(array.exp_list() != null)
            {
                foreach (var or_expr in array.exp_list().or_expr())
                {
                    child = VisitOr_expr(or_expr);
                    child.Parent = declarator;
                    declarator.Childs.Add(child);
                }
            }
        }
        return declarator;
    }

    public override ASTNode VisitOr_expr([NotNull] MIDLParser.Or_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        var xor_exprs = context.xor_expr();
        for (int i = xor_exprs.Length - 1; i >= 0; i--)
        {
            ASTNode.Expression xorExpr = VisitXor_expr(xor_exprs[i]) as ASTNode.Expression;
            expressions.Push(xorExpr);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = ASTNode.Expression.Op.Or;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitXor_expr([NotNull] MIDLParser.Xor_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        var and_exprs = context.and_expr();
        for (int i = and_exprs.Length - 1; i >= 0; i--)
        {
            ASTNode.Expression andexpr = VisitAnd_expr(and_exprs[i]) as ASTNode.Expression;
            expressions.Push(andexpr);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = ASTNode.Expression.Op.Xor;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitAnd_expr([NotNull] MIDLParser.And_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        var shift_exprs = context.shift_expr();
        for (int i = shift_exprs.Length - 1; i >= 0; i--)
        {
            ASTNode.Expression shiftexpr = VisitShift_expr(shift_exprs[i]) as ASTNode.Expression;
            expressions.Push(shiftexpr);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = ASTNode.Expression.Op.And;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitShift_expr([NotNull] MIDLParser.Shift_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        for (int i = context.ChildCount - 1; i >= 0; i--)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode)
            {
                ASTNode.Expression.Op op = ASTNode.Expression.Op.Undefined;
                if (child.GetText() == "<<")
                    op = ASTNode.Expression.Op.LeftShift;
                else if (child.GetText() == ">>")
                    op = ASTNode.Expression.Op.RightShift;
                expressions.Push(new ASTNode.Expression()
                {
                    Operator = op
                });
            }
            else if (child is MIDLParser.Add_exprContext add_expr)
                expressions.Push(VisitAdd_expr(add_expr) as ASTNode.Expression);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var op = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = op.Operator;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitAdd_expr([NotNull] MIDLParser.Add_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        for (int i = context.ChildCount - 1; i >= 0; i--)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode)
            {
                ASTNode.Expression.Op op = ASTNode.Expression.Op.Undefined;
                if (child.GetText() == "+")
                    op = ASTNode.Expression.Op.Add;
                else if (child.GetText() == "-")
                    op = ASTNode.Expression.Op.Sub;
                expressions.Push(new ASTNode.Expression()
                {
                    Operator = op
                });
            }
            else if (child is MIDLParser.Mult_exprContext mult_expr)
                expressions.Push(VisitMult_expr(mult_expr) as ASTNode.Expression);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var op = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = op.Operator;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitMult_expr([NotNull] MIDLParser.Mult_exprContext context)
    {
        Stack<ASTNode.Expression> expressions = new Stack<ASTNode.Expression>();
        for (int i = context.ChildCount - 1; i >= 0; i--)
        {
            var child = context.GetChild(i);
            if (child is ITerminalNode)
            {
                ASTNode.Expression.Op op = ASTNode.Expression.Op.Undefined;
                if (child.GetText() == "*")
                    op = ASTNode.Expression.Op.Multi;
                else if (child.GetText() == "/")
                    op = ASTNode.Expression.Op.Div;
                else if (child.GetText() == "%")
                    op = ASTNode.Expression.Op.Mod;
                expressions.Push(new ASTNode.Expression()
                {
                    Operator = op
                });
            }
            else if (child is MIDLParser.Unary_exprContext unary_exp)
                expressions.Push(VisitUnary_expr(unary_exp) as ASTNode.Expression);
        }

        while (expressions.Count > 1)
        {
            var a = expressions.Pop();
            var op = expressions.Pop();
            var b = expressions.Pop();
            ASTNode.Expression expression = new ASTNode.Expression();
            expression.Operator = op.Operator;
            expression.Childs.Add(a);
            expression.Childs.Add(b);
            a.Parent = expression;
            b.Parent = expression;

            expression.Start = a.Start;
            expression.Stop = b.Stop;
            expressions.Push(expression);
        }

        return expressions.Peek();
    }

    public override ASTNode VisitUnary_expr([NotNull] MIDLParser.Unary_exprContext context)
    {
        if (context.ChildCount > 1)
        {
            ASTNode.Expression expression = new ASTNode.Expression();
            string opText = context.GetChild(0).GetText();
            if (opText == "+")
                expression.Operator = ASTNode.Expression.Op.Positive;
            else if (opText == "-")
                expression.Operator = ASTNode.Expression.Op.Negtive;
            else if (opText == "~")
                expression.Operator = ASTNode.Expression.Op.Invert;

            var child = VisitLiteral(context.literal());
            child.Parent = expression;
            expression.Childs.Add(child);
            expression.Start = context.Start;
            expression.Stop = context.Stop;
            return expression;
        }
        else
            return VisitLiteral(context.literal());
    }

    public override ASTNode VisitLiteral([NotNull] MIDLParser.LiteralContext context)
    {
        ConstantType type = ConstantType.Never;
        if (context.INTEGER() != null)
            type = ConstantType.Integer;
        else if (context.FLOATING_PT() != null)
            type = ConstantType.Float;
        else if (context.CHAR() != null)
            type = ConstantType.Char;
        else if (context.STRING() != null)
            type = ConstantType.String;
        else if (context.BOOLEAN() != null)
            type = ConstantType.Boolean;
        return new ASTNode.Literal(type)
        {
            Text = context.GetText(),
            Start = context.Start,
            Stop = context.Stop,
        }; 
    }
}