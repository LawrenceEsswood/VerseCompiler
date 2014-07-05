using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Verse
{


    enum types : byte
    {
        type_function = 0,
        type_bool = 1,
        type_string = 2,
        type_float = 3,
        type_int = 4,
        type_list = 5,
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
        public Object refV;

        public String strV
        {
            get {return (String)refV;}
            set {refV = value;}
        }
        public ListNode ndV
        {
            get {return (ListNode)refV;}
            set { refV = value;}
        }
        public Poem pmV
        {
            get {return (Poem)refV;}
            set { refV = value; }
        }
    }

    class ListNode
    {
        public Variable value;
        public ListNode next;

        public ListNode(Variable value)
        {
            this.value = value;
            this.next = null;
        }
    }   

    class Variable
    {
        val value;
        types type;
        public Variable()
        {
            value.intV = 0;
            type = types.type_int;
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

        public Variable(Poem p)
        {
            value.pmV = p;
            type = types.type_function;
        }

        public Variable(ListNode nd)
        {
            value.ndV = nd;
            type = types.type_list;
        }

        public Variable(Variable v)
        {
            this.value = v.value;
            this.type = v.type;
        }

        public void set(Variable v)
        {
            this.value = v.value;
            this.type = v.type;
        }

        public Poem getPoem()
        {
            if (this.type != types.type_function) throw new Exception("Cannot call a type " + this.type);
            return this.value.pmV;
        }
        public static Variable nodeOf(Variable v)
        {
            Variable newv = new Variable();
            newv.type = types.type_list;
            newv.value.ndV = (v != null) ? new ListNode(v) : null;
            return newv;
        }

        public Variable hd()
        {
            return this.value.ndV.value;
        }

        public ListNode tl()
        {
            return this.value.ndV.next;
        }

        public static void append(Variable v1, Variable v2)
        {
            ListNode nd = v1.value.ndV;
            while (nd.next != null) nd = nd.next;
            nd.next = v2.value.ndV;
        }

        public void setTl(Variable v)
        {
            this.value.ndV.next = v.value.ndV;
        }

        public void setTl(ListNode nd)
        {
            this.value.ndV.next = nd;
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

        public static int listLength(ListNode nd)
        {
            int length = 0;
            while(nd != null) { nd = nd.next ; length++;}
            return length;
        }
        
        public int compLength()
        {
            switch(this.type)
            {
                case types.type_bool: return value.boolV ? 1 : 0;
                case types.type_int: return value.intV;
                case types.type_list: return listLength(value.ndV);
                case types.type_string: return value.strV.Length;
                default: throw new Exception("Variable typing exception");
            }
        }

        public bool lessThan(Variable v)
        {
            if (this.type == types.type_float && v.type == types.type_float) return this.value.floatV < v.value.floatV;
            return this.compLength() < v.compLength();
        }

        public bool moreThan(Variable v)
        {
            if (this.type == types.type_float && v.type == types.type_float) return this.value.floatV > v.value.floatV;
            else return this.compLength() > v.compLength();
        }

        public bool test()
        {
            return (type == types.type_list && value.ndV != null) ||
                (type == types.type_bool && value.boolV) ||
                (type == types.type_int && value.intV != 0) ||
                (type == types.type_float && value.floatV != 0)
                || (type == types.type_string && value.strV.Length != 0);
        }

        public static bool equalLst(ListNode l1, ListNode l2)
        {
            return (l1 == null && l2 == null) || (l1 != null && l2 != null && equal(l1.value, l2.value) && equalLst(l1.next, l2.next));
        }

        public static bool equal(Variable v1, Variable v2)
        {
            if (v1.type == types.type_function && v2.type == types.type_function) return v1.value.pmV == v2.value.pmV;
            else if (v1.type == types.type_function || v2.type == types.type_function) throw new Exception("Variable typing exception");

            if(v2.type < v1.type) { Variable tmp = v1; v1 = v2; v2 = tmp;}

            switch (v1.type)
            {
                case types.type_bool: return v2.test() == v1.value.boolV;
                case types.type_string: return v2.asString() == v1.value.strV;
                case types.type_float: return (v2.type == types.type_int && (float)v2.value.intV == v1.value.floatV) || (v2.type == types.type_list && listLength(v2.value.ndV) == v1.value.floatV) || (v2.type == types.type_float && v2.value.floatV == v1.value.floatV);
                case types.type_int: return (v2.type == types.type_int && v2.value.intV == v1.value.intV) || (v2.type == types.type_list && listLength(v2.value.ndV) == v1.value.intV);
                case types.type_list: return (equalLst(v1.value.ndV, v2.value.ndV));

                default: throw new Exception("Variable typing exception");
            }
        }

        public static String listAsString(ListNode nd)
        {
            String str = "[";
            while (nd != null)
            {
                str += nd.value.asString();
                if (nd.next != null) str += ", ";
                                nd = nd.next;
            }
            str += "]";
            return str;
        }
        public String asString()
        {
            switch (type)
            {
                case types.type_bool: return value.boolV.ToString();
                case types.type_float: return value.floatV.ToString();
                case types.type_int: return value.intV.ToString();
                case types.type_string: return value.strV;
                case types.type_list: return listAsString(value.ndV);
                case types.type_function: return value.pmV.sig.ID;
                default: throw new Exception("Variable typing exception");
            }
        }

        public static Variable add(Variable a, Variable b)
        {
            if (a.type == types.type_function || a.type == types.type_function) throw new Exception("Variable typing exception");

            if (a.type == types.type_list || b.type == types.type_list)
            {
                if (b.type == types.type_list)
                {
                    Variable v = Variable.nodeOf(a);
                    v.setTl(b);
                    return v;
                }
                else
                {
                    Variable v = Variable.nodeOf(b);
                    v.setTl(a);
                    return v;
                }
            }
            else if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV + b.value.intV);
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
            else throw new Exception("Cannot substract strings or bools or functions");
        }

        public static Variable multiply(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV * b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV * b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV * b.value.intV);
            else throw new Exception("Cannot multiply strings or bools or functions");
        }

        public static Variable divide(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV / b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV / b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV / b.value.intV);
            else throw new Exception("Cannot divide strings or bools or functions");
        }

        public static Variable mod(Variable a, Variable b)
        {
            if (a.type == types.type_int && b.type == types.type_int) return new Variable(a.value.intV % b.value.intV);
            else if (a.type == types.type_int && b.type == types.type_float) return new Variable(a.value.intV % b.value.floatV);
            else if (a.type == types.type_float && b.type == types.type_int) return new Variable(a.value.floatV % b.value.intV);
            else throw new Exception("Cannot mod strings or bools or functions");
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
