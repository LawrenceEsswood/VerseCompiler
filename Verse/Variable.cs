using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Verse
{
    enum types : byte
    {
        type_string,
        type_int,
        type_float,
        type_bool
    };

    [StructLayout(LayoutKind.Explicit)] 
    struct val
    {
        [FieldOffset(0)]
        public int intV;
        [FieldOffset(0)]
        public float floatV;
        [FieldOffset(0)]
        public bool boolV;
        [FieldOffset(4)]
        public String strV;
    }

    class Variable
    {
        val value;
        types type;
        public Variable()
        {
            value.strV = "";
            type = types.type_string;
        }

        public Variable(String v)
        {
            value.strV = v;
            type = types.type_string;
        }

        public Variable(int x)
        {
            value.intV = x;
            type = types.type_int;
        }

        public Variable(float x)
        {
            value.floatV = x;
            type = types.type_float;
        }

        public Variable(bool b)
        {
            value.boolV = b;
            type = types.type_bool;
        }

        public Variable(Variable v)
        {
            this.value = v.value;
            this.type = v.type;
        }

        public static Variable assumeType(String v)
        {
            String upV = v.ToUpper();
            int x; float y;
            if (upV == "TRUE" || upV == "FALSE")
                return new Variable(upV == "TRUE");
            else if(int.TryParse(v,out x))
                return new Variable(x);
            else if (float.TryParse(v, out y)) 
                return(new Variable(y));
            else 
                return (new Variable(v));
        }

        public Variable clone()
        {
            return new Variable(this);
        }

        public Boolean test()
        {
            return (type == types.type_bool && value.boolV) ||
                (type == types.type_int && value.intV != 0) ||
                (type == types.type_float && value.floatV != 0)
                || (type == types.type_string && value.strV.Length != 0);
        }

        public String asString()
        {
            switch (type)
            {
                case types.type_bool: return value.boolV.ToString();
                case types.type_float: return value.floatV.ToString();
                case types.type_int: return value.intV.ToString();
                case types.type_string: return value.strV;
                default: throw new Exception("Variable typing exception");
            }
        }

        public static Variable add(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV + b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV + b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV + b.value.intV);
            else if (a.type == types.type_float && b.type == types.type_float) return new Variable(a.value.floatV + b.value.floatV);
            else return new Variable(a.asString() + b.asString());
        }

        public static Variable subtract(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV - b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV - b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV - b.value.intV);
            else throw new Exception("Cannot substract strings or bools");
        }

        public static Variable multiply(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV * b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV * b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV * b.value.intV);
            else throw new Exception("Cannot multiply strings or bools");
        }

        public static Variable divide(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV / b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV / b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV / b.value.intV);
            else throw new Exception("Cannot divide strings or bools");
        }

        public static Variable mod(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV % b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV % b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV % b.value.intV);
            else throw new Exception("Cannot mod strings or bools");
        }

        public static Variable and(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV & b.value.intV);
            else if (a.type == types.type_bool && b.type == types.type_bool) return new Variable(a.value.boolV && b.value.boolV);
            else throw new Exception("And is only defined for int,int and bool,bool");
        }

        public static Variable or(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV | b.value.intV);
            else if (a.type == types.type_bool && b.type == types.type_bool) return new Variable(a.value.boolV || b.value.boolV);
            else throw new Exception("Or is only defined for int,int and bool,bool");
        }

        public static Variable xor(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV ^ b.value.intV);
            else if (a.type == types.type_bool && b.type == types.type_bool) return new Variable(a.value.boolV != b.value.boolV);
            else throw new Exception("Xor is only defined for int,int and bool,bool");
        }

        public static Variable not(Variable a)
        {
            if (a.type == types.type_int) return new Variable(~a.value.intV);
            else if (a.type == types.type_bool) return new Variable(!a.value.boolV);
            else throw new Exception("Not is only defined for int and bool");
        }
    }
}
