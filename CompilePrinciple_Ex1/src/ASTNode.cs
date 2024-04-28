using System;
using System.Collections.Generic;
using Antlr4.Runtime;

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
    public IToken Start { get; set; }
    public IToken Stop { get; set; }
    public abstract override string ToString();
    public abstract string ToCppCode(int level);
    public static string Indent(int level)
    {
        string ans = "";
        for (int i = 0; i < level; i++)
            ans += '\t';
        return ans;
    }

    public class Specification : ASTNode
    {
        public override string ToString() => $"Specification[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level)
        {
            string ans = "";
            foreach (var child in Childs)
                ans += child.ToCppCode(level) + "\n";
            return ans;
        }
    }

    public abstract class Scope : ASTNode
    {
        public string ID { get; set; }
    }

    public class Struct : Scope
    {
        public override string ToString() => $"Struct_{ID}[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level)
        {
            string indent = Indent(level);
            string ans = $"{indent}typedef struct {ID}\n{indent}{{\n";
            foreach (var child in Childs)
                ans += child.ToCppCode(level + 1);
            ans += $"{indent}}}{ID};\n";
            return ans;
        }
    }

    public class Module : Scope
    {
        public override string ToString() => $"Module_{ID}[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level)
        {
            string indent = Indent(level);
            string ans = $"{indent}namespace {ID}\n{indent}{{\n";
            foreach (var child in Childs)
                ans += child.ToCppCode(level + 1);
            ans += $"{indent}}}\n";
            return ans;
        }
    }

    public class Member : ASTNode
    {
        public string TypeText { get; set; }
        public override string ToString() => $"Member_Type({TypeText})[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level)
        {
            string ans = $"{Indent(level)}{CppTypeText()} {Childs[0].ToCppCode(level)}";
            for (int i = 1; i < Childs.Count; i++)
                ans += "," + Childs[i].ToCppCode(level);
            ans += ";\n";
            return ans;
        }
        private string CppTypeText()
        {
            switch(TypeText)
            {
                case "int16":
                    return "short";
                case "long":
                case "int32":
                    return "int";
                case "longlong":
                case "int64":
                    return "long long";
                case "unsignedshort":
                case "uint16":
                    return "unsigned short";
                case "unsignedlong":
                case "uint32":
                    return "unsigned int";
                case "unsignedlonglong":
                case "uint64":
                    return "unsigned long";
                case "longdouble":
                    return "long double";
                case "boolean":
                    return "bool";
            }
            return TypeText;
        }
    }

    public class Declarator : ASTNode
    {
        public string ID { get; set; }
        public bool IsArray { get; set; }
        public override string ToString() => $"{(!IsArray ? "Variable" : "Array")}_{ID}[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level)
        {
            if (!IsArray)
            {
                if (Childs.Count == 0)
                    return ID;
                else
                    return $"{ID} = {Childs[0].ToCppCode(level)}";
            }

            string ans = $"{ID}[{Childs[0].ToCppCode(level)}]";
            if (Childs.Count > 1)
                ans += " = [";
            for (int i = 1; i < Childs.Count - 1; i++)
                ans += Childs[i].ToCppCode(level) + ", ";
            ans += Childs[Childs.Count - 1].ToCppCode(level) + "]";
            return ans;
            
        }
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
        public override string ToCppCode(int level)
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
                    return $"{Childs[0].ToCppCode(level)} {ToString()} {Childs[1].ToCppCode(level)}";
                case Op.Positive:
                case Op.Negtive:
                case Op.Invert:
                    return $"{ToString()}{Childs[0].ToCppCode(level)}";
                case Op.Undefined:
                default:
                    return "";
            }
        }
    }

    public class Literal : Expression
    {
        private ConstantType _type;
        public override ConstantType Type => _type;
        public Literal(ConstantType type) => _type = type;
        public string Text { get; set; }
        public override string ToString() => $"({Type}):{Text}[{Start.Line}:{Start.Column}~{Stop.Line}:{Stop.Column}]";
        public override string ToCppCode(int level) => Text;
    }
}