public abstract class SystemType
{
    public static readonly SystemType SHORT = new SystemType.Int16() { Name = "short" };
    public static readonly SystemType INT16 = new SystemType.Int16() { Name = "int16" };
    public static readonly SystemType LONG = new SystemType.Int32() { Name = "long" };
    public static readonly SystemType INT32 = new SystemType.Int32() { Name = "int32" };
    public static readonly SystemType LONG_LONG = new SystemType.Int64() { Name = "longlong" };
    public static readonly SystemType INT64 = new SystemType.Int64() { Name = "int64" };
    public static readonly SystemType UNSIGNED_SHORT = new SystemType.Uint16() { Name = "unsignedshort" };
    public static readonly SystemType UINT16 = new SystemType.Uint16() { Name = "uint16" };
    public static readonly SystemType UNSIGNED_LONG = new SystemType.Uint32() { Name = "unsignedlong" };
    public static readonly SystemType UINT32 = new SystemType.Uint32() { Name = "uint32" };
    public static readonly SystemType UNSIGNED_LONG_LONG = new SystemType.Uint64() { Name = "unsignedlonglong" };
    public static readonly SystemType UINT64 = new SystemType.Uint64() { Name = "uint64" };
    public static readonly SystemType FLOAT = new SystemType.Float() { Name = "float" };
    public static readonly SystemType DOUBLE = new SystemType.Double() { Name = "double" };
    public static readonly SystemType LONG_DOUBLE = new SystemType.LongDouble() { Name = "longdouble" };
    public static readonly SystemType CHAR = new SystemType.Char() { Name = "char" };
    public static readonly SystemType STRING = new SystemType.String() { Name = "string" };
    public static readonly SystemType BOOLEAN = new SystemType.Boolean() { Name = "boolean" };

    public string Name { get; private set; }
    public abstract bool Accept(string value);
    public abstract bool Accept(ConstantType type);

    class Int16 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Int16.TryParse(value, out _);
        }

        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
                
    }
    class Int32 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Int32.TryParse(value, out _);
        }

        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
    }

    class Int64 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Int64.TryParse(value, out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
    }

    class Uint16 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.UInt16.TryParse(value, out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
    }

    class Uint32 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.UInt32.TryParse(value, out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
    }

    class Uint64 : SystemType
    {
        public override bool Accept(string value)
        {
            return System.UInt64.TryParse(value, out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Integer;
        }
    }

    class Float : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Single.TryParse(value.TrimEnd('f'), out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Float;
        }
    }

    class Double : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Double.TryParse(value.TrimEnd('d'), out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Float;
        }
    }

    class LongDouble : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Decimal.TryParse(value.TrimEnd('d'), out _);
        }
        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Float;
        }
    }

    class Char : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Char.TryParse(value.Trim('\''), out _);
        }

        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Char;
        }
    }

    class String : SystemType
    {
        public override bool Accept(string value)
        {
            return true;
        }

        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.String;
        }
    }
    class Boolean : SystemType
    {
        public override bool Accept(string value)
        {
            return System.Boolean.TryParse(value, out _);
        }

        public override bool Accept(ConstantType type)
        {
            return type == ConstantType.Boolean;
        }
    }

    public class Custom : SystemType
    {
        public Custom(string typename) => Name = typename;
        public override bool Accept(ConstantType type) => false;
        public override bool Accept(string value) => false;
    }
}