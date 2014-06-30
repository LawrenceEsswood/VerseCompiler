using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Verse
{
    class Anthology
    {
        Poem[] poems;
        Poem main;

        public Anthology(Poem main, Poem[] poems)
        {
            this.main = main;
            this.poems = poems;
        }

        public Variable run(Variable[] args)
        {

                int[] copyVals = null;
                bool[] doCopy = null;
                if (args != null)
                {
                    copyVals = new int[args.Length];
                    doCopy = new bool[args.Length];
                    for (int i = 0; i < args.Length; i++)
                    {
                        doCopy[i] = false;
                        copyVals[i] = i;
                    }
                }
                else args = new Variable[1];

                Action a = new Call(main, copyVals, 0);
                a.run(args, 0);
                return args[0];

        }
        
    }
}
