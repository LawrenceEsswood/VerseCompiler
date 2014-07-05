using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
namespace Verse
{
    public enum TT
    {
        QuestionMark,
        ExclamationMark,
        SemiColon,
        FullStop,
        Comma,
        Colon,
        Tild,
        Pipe,
        NewLine,
        Literal,
        EOS,
        Word
    };

    public struct Token
    {
        public static Token QuestionMark = new Token(TT.QuestionMark);
        public static Token ExclamationMark = new Token(TT.ExclamationMark);
        public static Token SemiColon = new Token(TT.SemiColon);
        public static Token FullStop = new Token(TT.FullStop);
        public static Token Colon = new Token(TT.Colon);
        public static Token Tild = new Token(TT.Tild);
        public static Token NewLine = new Token(TT.NewLine);
        public static Token EOS = new Token(TT.EOS);
        public static Token Pipe = new Token(TT.Pipe);
        public static Token Comma = new Token(TT.Comma);

        public TT tokenType;
        public String wordV;
        public bool captilised;

        public Token(TT tt)
        {
            tokenType = tt;
            wordV = null;
            captilised = false;
        }

        public Token(TT tt, String w, bool upper)
        {
            tokenType = tt;
            captilised = Char.IsUpper(w[0]);
            wordV = upper ? w.ToUpper() : w;
        }
    }

    class Lex
    {


        public StreamReader reader;
        private Queue<Token> hold = new Queue<Token>();
        private Boolean eos = false;

        public Boolean EOS
        {
            get
            {
                return (hold.Count == 0) && eos;
            }
        }

        public Token peek()
        {
            while (hold.Count == 0) readNext();
            return hold.Peek();
        }

        public Token next()
        {
            while (hold.Count == 0) readNext();
            Token t = hold.Dequeue();
            return t;
        }

        public Lex(StreamReader reader)
        {
            this.reader = reader;
        }

        private void readNext()
        {
            if (eos) return;
            String v = "";


            while (!reader.EndOfStream)
            {
                char x = getChar();
                
                switch (x)
                {
                    case '"':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        String value = "";
                        char c;
                        while ((c = getChar()) != '"') value += c;
                        hold.Enqueue(new Token(TT.Literal, value, false));
                        return;
                    case'~':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.Tild);
                        return;
                    case '|':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.Pipe);
                        return;
                    case '?':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.QuestionMark);
                        return;
                    case ';':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.SemiColon);
                        return;
                    case '.':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.FullStop);
                        return;
                    case ':':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.Colon);
                        return;
                    case '!':
                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.ExclamationMark);
                        return;
                    case ' ':case ',':
                        if (v != "")
                        {
                            addWordToQueue(v);
                            return;
                        } else break;
                    case '\r': case '\n':
                        
                        if (x == '\r' && getChar() != '\n') throw new Exception("What the absolute fuck is this. Use a proper text editor.");

                        if (v != "" || workingOnNumber) addWordToQueue(v);
                        if (workingOnNumber) addWordToQueue("");
                        hold.Enqueue(Token.NewLine);
                        return;
                    default:
                        v += x.ToString();
                        break;
                }
            }

            if (v != "") hold.Enqueue(new Token(TT.Word, v, true));
            hold.Enqueue(Token.EOS);
            eos = true;
        }

        enum mods
        {
            BREAK = 0,
            HUNDRED = 1, 
            THOUSAND = 2,
            MILLION = 3,
            BILLION = 4,
            START = 5,
        }


        int[] counts = new int[4];

        int lastNumericalSequence = 0;
        mods ascendingMod = mods.BREAK;

        mods topMod = mods.BREAK;

        bool workingOnNumber = false;

        private void resentNumbers()
        {
            for (int i = 0; i < 4; i++) counts[i] = 0;
            lastNumericalSequence = 0;
            ascendingMod = mods.BREAK;
            topMod = mods.BREAK;
            workingOnNumber = false;
        }

        private void addWordToQueue(String wordV)
        {
            String upper = wordV.ToUpper();

            if(shortWords.Contains(upper)) return;

            int n = intFromString(upper);

            if (n != -1)
            {
                workingOnNumber = true;

                if (ascendingMod != mods.BREAK)
                {
                    if (counts[(int)ascendingMod - 1] != 0) throw new Exception(ascendingMod + " already specified");
                    counts[(int)ascendingMod - 1] = lastNumericalSequence;
                    lastNumericalSequence = 0;
                    ascendingMod = mods.BREAK;
                }

                if (lastNumericalSequence != 0 && (n >= 10 || n == 0 || lastNumericalSequence < 20)) throw new Exception("Only digits can follow tens");
                lastNumericalSequence += n;

                return;
            }
            else
            {
                mods mod = getModFromString(upper);
                if (mod != mods.BREAK && !workingOnNumber) throw new Exception("Cannot start number with modifier");

                if (workingOnNumber)
                {
                    if (mod != mods.BREAK &&  mod <= ascendingMod) throw new Exception("encountered " + mod + " after " + ascendingMod + ". Quantifers must strictly ascend.");
                    
                        //this is the greatest mod so far, everything before applies to it
                        for (int i = 0; i < (int)mod - 1; i++)
                        {
                            lastNumericalSequence += counts[i];
                            counts[i] = 0;
                        }
                        topMod = mod;

                    switch (mod)
                    {
                        case mods.BREAK: // we are done with the number here,TODO: do any sanity checks
                            for (int i = 0; i < 4; i++) lastNumericalSequence += counts[i];
                            hold.Enqueue(new Token(TT.Literal, lastNumericalSequence.ToString(), false));
                            resentNumbers();
                            break;
                        case mods.HUNDRED:
                            
                            lastNumericalSequence *= 100;
                            ascendingMod = mod;
                            return;
                        case mods.THOUSAND:
                            
                            lastNumericalSequence *= 1000;
                            ascendingMod = mod;
                            return;
                        case mods.MILLION:
                            
                            lastNumericalSequence *= 1000000;
                            ascendingMod = mod;
                            return;
                        case mods.BILLION:
                            lastNumericalSequence *= 1000000000;
                            ascendingMod = mod;
                            return;
                    }

                }
            }
            if (wordV == "") return;
            int x;
            float y;
            if (upper == "TRUE" || upper == "FALSE") hold.Enqueue(new Token(TT.Literal, wordV, false));
            else if (int.TryParse(wordV, out x)) hold.Enqueue(new Token(TT.Literal, wordV, false));
            else if (float.TryParse(wordV, out y)) hold.Enqueue(new Token(TT.Literal, wordV, false));
            else hold.Enqueue(new Token(TT.Word, wordV, true));
        }

        private static mods getModFromString(String s)
        {
            switch (s)
            {
                case "HUNDRED": return mods.HUNDRED;
                case "THOUSAND": return mods.THOUSAND;
                case "MILLION": return mods.MILLION;
                case "BILLION": return mods.BILLION;
                default: return mods.BREAK;
            }
        }
        private static int intFromString(String s)
        {
            switch (s)
            {
                case "ZER0": return 0;
                case "ONE": return 1;
                case "TWO": return 2;
                case "THREE": return 3;
                case "FOUR": return 4;
                case "FIVE": return 5;
                case "SIX": return 6;
                case "SEVEN": return 7;
                case "EIGHT": return 8;
                case "NINE": return 9;
                case "TEN": return 10;
                case "ELEVEN": return 11;
                case "TWELVE": return 12;
                case "THIRTEEN": return 13;
                case "FOURTEEN": return 14;
                case "FIFTEEN": return 15;
                case "SIXTEEN": return 16;
                case "SEVENTEEN": return 17;
                case "EIGHTEEN": return 18;
                case "NINETEEN": return 19;
                case "TWENTY": return 20;
                case "THIRTY": return 30;
                case "FOURTY": return 40;
                case "FIFTY": return 50;
                case "SIXTY": return 60;
                case "SEVENTY": return 70;
                case "EIGHTY": return 80;
                case "NINETY": return 90;
                default: return -1;
            }

        }
        public static string[] shortWords = new string[] { "IN", "OF", "THE", "A", "AT", "AM", "I", "ODE", "TO", "MY", "HER", "HIS", "IS", "AND" };

        private char? charHold = null;
        private char getChar()
        {
            if (charHold == null) return (char)reader.Read();
            else
            {
                char h = charHold.Value;
                charHold = null;
                return h;
            }
        }

        private void pushChar(char h)
        {
            charHold = h;
        }
    }
}
