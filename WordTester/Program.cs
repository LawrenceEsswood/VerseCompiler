using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using System.IO;

namespace WordTester
{
    class Program
    {
        static void Main(string[] args)
        {
            
            Word.buildRyhmeDict(new StreamReader(new MemoryStream(Verse.Properties.Resources.c06d)));

            if (args.Length == 0)
            {
                Console.WriteLine("Manual entry mode.");
                while (true)
                {
                    Console.WriteLine("Enter word 1");
                    String w1 = Console.ReadLine().ToUpper();
                    Console.WriteLine("Enter word 2");
                    String w2 = Console.ReadLine().ToUpper();
                    diagnoseWords(w1, w2);
                }
            }
            else if (args.Length == 2)
            {
                Console.WriteLine("Command line mode");
                diagnoseWords(args[0], args[1]);
            }
            else Console.WriteLine("Usage: [word1 word2]");
            Console.ReadKey();
        }

        static void diagnoseWords(String w1, String w2)
        {
            if (!Word.wordExists(w1)) { Console.WriteLine("Word " + w1 + " is not in the dictionary"); return; }
            if (!Word.wordExists(w2)) { Console.WriteLine("Word " + w2 + " is not in the dictionary"); return; }
            Word word1 = new Word(w1);
            Word word2 = new Word(w2);

            Console.WriteLine("Sound word 1 " + word1.soundsLike);
            Console.WriteLine("Sound word 2 " + word2.soundsLike);

            Console.WriteLine("Primary Rhyme: " + word1.primaryRyhme(word2));
            Console.WriteLine("Alliterate: " + word1.alliterate(word2));
        }
    }
}
