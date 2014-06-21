using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
    abstract class Action
    {
        public abstract int run(Variable[] localStack, int line);
    }

    class Declare : Action
    {
        int index;
        String v;
        public override int run(Variable[] localStack, int line)
        {
            Variable var = (v != null) ? Variable.assumeType(v) : new Variable();
            localStack[index] = var;
            return line + 1;
        }

        public Declare(int index, String intialValue)
        {
            this.index = index;
            this.v = intialValue;
        }
    }

    class Assign : Action
    {
        int lindex;
        int rindex;

        public override int run(Variable[] localStack, int line)
        {
            localStack[lindex].set(localStack[rindex]);
            return line + 1;
        }

        public Assign(int l, int r)
        {
            lindex = l;
            rindex = r;
        }
    }

    class Call : Action
    {
        Poem toCall;
        int[] indexs;
        int rI;

        public override int run(Variable[] localStack, int line)
        {
            Variable[] newLocal = new Variable[toCall.nVars];
            if (indexs != null)
            {
                for (int i = 0; i < indexs.Length; i++)
                {
                    Variable v = localStack[indexs[i]];
                    if (toCall.copyArgs[i]) v = v.clone();
                    newLocal[i] = v;
                }
            }

            Variable ret = toCall.run(newLocal);

            if(rI != -1)
            {
                if(toCall.copyReturn) ret = ret.clone();
                localStack[rI] = ret;
            }

            return line + 1;
        }

        public Call(Poem call, int[] copyVars, int returnIndex)
        {
            this.toCall = call;
            this.indexs = copyVars;
            this.rI = returnIndex;
        }
    
    }

    class BranchIf : Action
    {
        int T;
        int index;
        Boolean invert;

        public override int run(Variable[] localStack, int line)
        {
            if (localStack[index].test() ^ invert)
            {
                return T;
            }
            else return line + 1;
        }

        public BranchIf (int gotoActionNumber, int varIndex, Boolean invert)
        {
            this.T = gotoActionNumber;
            this.index = varIndex;
            this.invert = invert;
        }
    }

    class Branch : Action
    {
        int T;
        public override int run(Variable[] localStack, int line)
        {
            return T;
        }

        public Branch(int gotoActionNumber)
        {
            this.T = gotoActionNumber;
        }
    }

    class Return : Action
    {
        int index;
        public override int run(Variable[] localStack, int line)
        {
            localStack[0] = (index == -1) ? null : localStack[index];
            return -1;  
        }

        public Return(int variableIndex)
        {
            this.index = variableIndex;
        }
    }


    class Poem
    {
        public signiture sig;
        Action[] actions;
        int nActions;
        public int nVars;

        public bool[] copyArgs
        {
            get { return sig.copy; }
        }
        public bool copyReturn
        {
            get { return sig.copyReturn; }
        }

        public Poem(Action[] actions, int nVars)
        {
            setActions(actions, nVars);
        }

        public Poem(int nVars, bool[] copy)
        {
            this.nVars = nVars;
            this.sig.copy = copy;
        }

        public Poem(signiture sig)
        {
            this.sig = sig;
        }

        public void setActions(Action[] actions, int nVars)
        {
            this.actions = actions;
            this.nActions = (actions == null) ? 0 : actions.Length;
            this.nVars = nVars;
        }


        public virtual Variable run(Variable[] localStack)
        {
            int lineNo = 0;
            while (lineNo != -1 && lineNo != nActions)
            {
                Action act = actions[lineNo];
                lineNo = act.run(localStack, lineNo);
            }
            if (sig.hasReturn)
                return copyReturn ? localStack[0].clone() : localStack[0];
            else return null;
        }

        
    }
}