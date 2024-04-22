using System;
using System.Collections.Generic;

public enum ConstantType
{
    Never,
    Integer,
    Float,
    Char,
    String,
    Boolean
}

public abstract class ASTNode
{
    public ASTNode Parent { get; set; }
    public IList<ASTNode> Childs { get; private set; } = new List<ASTNode>();
    public abstract override string ToString();

    public class Specification : ASTNode
    {
        public override string ToString() => "Specification";
    }

    public abstract class Scope : ASTNode
    {
        public string ID { get; set; }
    }

    public class Struct : Scope
    {
        public override string ToString() => $"Struct_{ID}";
    }

    public class Module : Scope
    {
        public override string ToString() => $"Module_{ID}";
    }

    public class Member : ASTNode
    {
        public string TypeText { get; set; }
        public override string ToString() => $"Member_Type({TypeText})";
    }

    public class Declarator : ASTNode
    {
        public string ID { get; set; }
        public bool IsArray { get; set; }
        public override string ToString() => $"{(!IsArray ? "Variable" : "Array")}_{ID}";
    }

    public class Expression : ASTNode
    {
        public enum Op
        {
            Undefined,
            Or,
            Xor,
            And,
            LeftShift,
            RightShift,
            Add,
            Sub,
            Multi,
            Div,
            Mod,
            Positive,
            Negtive,
            Invert
        };
        public virtual ConstantType Type
        {
            get
            {
                switch (Operator)
                {
                    case Op.Or:
                    case Op.Xor:
                    case Op.And:
                    case Op.LeftShift:
                    case Op.RightShift:
                    case Op.Add:
                    case Op.Sub:
                    case Op.Multi:
                    case Op.Div:
                    case Op.Mod:
                        var left = (Expression)Childs[0];
                        var right = (Expression)Childs[1];
                        var leftType = left.Type;
                        var rightType = right.Type;
                        if(leftType != rightType)
                        {
                            Console.WriteLine($"The operator {this} cannot used between type {left.Type} and type {right.Type}.");
                            return ConstantType.Never;
                        }
                        return left.Type;
                    case Op.Positive:
                    case Op.Negtive:
                    case Op.Invert:
                        var child = (Expression)Childs[0];
                        return child.Type;
                    case Op.Undefined:
                    default:
                        return ConstantType.Never;
                }
            }
        }
        public Op Operator { get; set; }
        public override string ToString()
        {
            switch (Operator)
            {
                case Op.Or:
                    return "|";
                case Op.Xor:
                    return "^";
                case Op.And:
                    return "&";
                case Op.LeftShift:
                    return "<<";
                case Op.RightShift:
                    return ">>";
                case Op.Add:
                    return "+";
                case Op.Sub:
                    return "-";
                case Op.Multi:
                    return "*";
                case Op.Div:
                    return "/";
                case Op.Mod:
                    return "%";
                case Op.Positive:
                    return "+";
                case Op.Negtive:
                    return "-";
                case Op.Invert:
                    return "~";
                case Op.Undefined:
                default:
                    return "Undefined";
            }
        }
    }

    public class Literal : Expression
    {
        private ConstantType _type;
        public override ConstantType Type => _type;
        public Literal(ConstantType type) => _type = type;
        public string Text { get; set; }
        public override string ToString() => $"({Type}):{Text}";
    }
}