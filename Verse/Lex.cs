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
                        if (v != "") addWordToQueue(v);
                        String value = "";
                        char c;
                        while ((c = getChar()) != '"') value += c;
                        hold.Enqueue(new Token(TT.Literal, value, false));
                        return;
                    case'~':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.Tild);
                        return;
                    case '|':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.Pipe);
                        return;
                    case '?':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.QuestionMark);
                        return;
                    case ';':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.SemiColon);
                        return;
                    case '.':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.FullStop);
                        return;
                    case ':':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.Colon);
                        return;
                    case '!':
                        if (v != "") addWordToQueue(v);
                        hold.Enqueue(Token.ExclamationMark);
                        return;
                    case ' ':
                        if (v != "")
                        {
                            addWordToQueue(v);
                            return;
                        } else break;
                    case '\r': case '\n':
                        
                        if (x == '\r' && getChar() != '\n') throw new Exception("What the absolute fuck is this. Use a proper text editor.");

                        if (v != "") addWordToQueue(v);
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

        private void addWordToQueue(String wordV)
        {
            String upper = wordV.ToUpper();
            if (upper == "TRUE" || upper == "FALSE") hold.Enqueue(new Token(TT.Literal, wordV, false));
            else if (!shortWords.Contains(upper)) hold.Enqueue(new Token(TT.Word, wordV, true));
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
