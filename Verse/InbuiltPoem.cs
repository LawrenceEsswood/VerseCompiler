using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
    class InbuiltPoem : Poem
    {
        protected InbuiltPoem(int nVars) : base(null, nVars) { }
        public static InbuiltPoem sayPoem = new SayPoem();
        public static InbuiltPoem readPoem = new ReadPoem();
        public static InbuiltPoem addPoem = new AddPoem();
        public static InbuiltPoem subPoem = new SubPoem();
        public static InbuiltPoem multPoem = new MultPoem();
        public static InbuiltPoem divPoem = new DivPoem();
        public static InbuiltPoem modPoem = new ModPoem();
        public static InbuiltPoem andPoem = new AndPoem();
        public static InbuiltPoem orPoem = new OrPoem();
        public static InbuiltPoem xorPoem = new XorPoem();
        public static InbuiltPoem notPoem = new NotPoem();
        public static InbuiltPoem equalPoem = new EqualPoem();
        public static List<InbuiltPoem> allInbuilt = new List<InbuiltPoem> { sayPoem, readPoem, addPoem, subPoem, multPoem, divPoem, modPoem, andPoem, orPoem, xorPoem, notPoem, equalPoem };
    }

    class EqualPoem : InbuiltPoem
    {
        public EqualPoem()
            : base(2)
        {
            this.sig.ID = "EQUAL";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return new Variable(Variable.equal(localStack[0], localStack[1]));
        }

    }

    class SayPoem : InbuiltPoem
    {
        public SayPoem()
            : base(1)
        {
            this.sig.ID = "SAY";
            this.sig.arguments = new List<string>() { "PHRASE" };
            this.sig.copy = new bool[] { false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = false;
        }

        public override Variable run(Variable[] localStack)
        {
            Console.WriteLine(localStack[0].asString());
            return null;
        }
    }

    class ReadPoem : InbuiltPoem
    {
        public ReadPoem()
            : base(0)
        {
            this.sig.ID = "READ";
            this.sig.arguments = null;
            this.sig.copy = null;
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.assumeType(Console.ReadLine());
        }
    }

    class AddPoem : InbuiltPoem
    {
        public AddPoem()
            : base(2)
        {
            this.sig.ID = "ADD";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.add(localStack[0], localStack[1]);
        }
    }

    class SubPoem : InbuiltPoem
    {
        public SubPoem()
            : base(2)
        {
            this.sig.ID = "SUB";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.subtract(localStack[0], localStack[1]);
        }
    }

    class DivPoem : InbuiltPoem
    {
        public DivPoem()
            : base(2)
        {
            this.sig.ID = "DIV";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.divide(localStack[0], localStack[1]);
        }
    }

    class MultPoem : InbuiltPoem
    {
        public MultPoem()
            : base(2)
        {
            this.sig.ID = "MULT";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.multiply(localStack[0], localStack[1]);
        }
    }

    class ModPoem : InbuiltPoem
    {
        public ModPoem()
            : base(2)
        {
            this.sig.ID = "MOD";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.mod(localStack[0], localStack[1]);
        }
    }

    class AndPoem : InbuiltPoem
    {
        public AndPoem()
            : base(2)
        {
            this.sig.ID = "AND";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.and(localStack[0], localStack[1]);
        }
    }

    class OrPoem : InbuiltPoem
    {
        public OrPoem()
            : base(2)
        {
            this.sig.ID = "OR";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.or(localStack[0], localStack[1]);
        }
    }

    class XorPoem : InbuiltPoem
    {
        public XorPoem()
            : base(2)
        {
            this.sig.ID = "XOR";
            this.sig.arguments = new List<string>() { "A", "B" };
            this.sig.copy = new bool[] { false, false };
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.xor(localStack[0], localStack[1]);
        }
    }

    class NotPoem : InbuiltPoem
    {
        public NotPoem()
            : base(1)
        {
            this.sig.ID = "NOT";
            this.sig.arguments = new List<string>() { "A"};
            this.sig.copy = new bool[] { false};
            this.sig.copyReturn = false;
            this.sig.hasReturn = true;
        }

        public override Variable run(Variable[] localStack)
        {
            return Variable.not(localStack[0]);
        }
    }
}
