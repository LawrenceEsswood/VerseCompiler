using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Verse
{
    class Program
    {
        static void Main(string[] args)
        {
            //this is a change
            Word.buildRyhmeDict(new StreamReader(new MemoryStream(Verse.Properties.Resources.c06d)));

            String programPath = args.Length == 0 ? "C:\\Users\\Lawrence Esswood\\Documents\\Visual Studio 2010\\Projects\\Verse\\Verse\\test.anth" : args[0];

            StreamReader sr = new StreamReader(programPath);
            Lex lexer = new Lex((StreamReader)sr);
            Parse parser = new Parse(lexer);

            //TODO: compiler flag for this
            //printC(parser);
            interpret(parser);

            Console.ReadKey();
        }

        static void interpret(Parse parser)
        {
            
            Anthology anth = parser.compile();

            //TODO parse args?
            Variable v = anth.run(null);
            if (v != null) Console.WriteLine(v.asString());

            Console.WriteLine("end.");
        }

        static void printC(Parse parser)
        {
            Console.Write(parser.compileC());
        }

    }
}
