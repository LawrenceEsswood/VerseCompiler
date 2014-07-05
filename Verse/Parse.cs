using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Verse
{
    public struct signiture
    {
        public String ID;
        public List<String> arguments;
        public bool[] copy;
        public bool copyReturn;
        public bool hasReturn;

        public String asC
        {
            get
            {
                String argList = "";
                for(int i =0; i < arguments.Count ; i++)
                    argList += (copy[i] ? "var " : "var* ") + arguments[i] + ((i == arguments.Count -1) ? "" : ", ");

                return (hasReturn ? (copyReturn ? "var " : "var* ") : "void ") + ID + " (" + argList + ")";
            }
        }
    }

    class uncompiledPoem
    {
        public signiture sig;
        public Queue<Token> tokens;
        public List<ParsedExp> exps;
        public Dictionary<String, int> variableTable;
        public Dictionary<String, int> labels;
        public int varCount = 0;
        public int tmpCount = 0;
    }

    struct LeftRightVal
    {
        public String left;
        public String right;
        public LeftRightVal(String l, String r)
        {
            this.left = l;
            this.right = r;
        }
    }

    abstract class ParsedExp
    {
        public abstract Action finalise(Parse parser, uncompiledPoem up);
        public abstract String finaliseC(Parse parser, uncompiledPoem up, signiture poemSig);

    }

    class DeclareExp : ParsedExp
    {
        String id;
        String val;
        public DeclareExp(String id, String val)
        {
            this.id = id;
            this.val = val;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            return new Declare(parser.resolveID(id, up),val);
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            return "var* " + id + " = " + (val == null ? "emptyVar();" : "assumeVar(\"" + val + "\");");
        }
    }

    class CallExp : ParsedExp
    {
        signiture poem;
        String[] toCopy;
        String ri;
        public CallExp(signiture p, String[] copyIDs, String returnID)
        {
            this.poem = p;
            this.toCopy = copyIDs;
            this.ri = returnID;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            int[] cop = (toCopy.Length == 0) ? null : new int[toCopy.Length];
            for(int i = 0; i < toCopy.Length ; i++) cop[i] = parser.resolveID(toCopy[i], up);
            return new Call(parser.resolveSig(poem),cop,parser.resolveID(ri, up));
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            String args = "";
            for (int i = 0; i < toCopy.Length; i++)
                args += (poem.copy[i] ? "varCopy(" + toCopy[i] + ")" : toCopy[i]) + (i == toCopy.Length - 1 ? "" : ", ");
            return (ri != null ? ri + " = " : "") + poem.ID + "(" + args + ");";
        }
    }

    class AssignExp : ParsedExp
    {
        String l, r;
        public AssignExp(String l, String r)
        {
            this.l = l;
            this.r = r;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            return new Assign(parser.resolveID(l, up), parser.resolveID(r, up));
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            return "*" + l + " = *" + r + ";";
        }
    }

    class ReturnExp : ParsedExp
    {
        String id;

        public ReturnExp(String id)
        {
            this.id = id;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            int index = (id == null) ? -1 : parser.resolveID(id, up);
            return new Return(index);
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            return "return " + (poemSig.copyReturn ? "varCopy(" + id + ")": id) + ";";
        }
    }

    class BranchIfExp : ParsedExp
    {
        String testable;
        String jumpLabel;
        Boolean invert;
        public BranchIfExp(String testable, String jumpLabel, Boolean invert)
        {
            this.testable = testable;
            this.jumpLabel = jumpLabel;
            this.invert = invert;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            return new BranchIf(parser.resolveLabel(jumpLabel, up), parser.resolveID(testable, up), invert);
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            return "if(" + (invert ? "!" : "") + "varTest(" + testable + ")) goto " + jumpLabel + ";";
        }
    }

    class BranchExp: ParsedExp
    {
        String jumpLabel;

        public BranchExp(String jumpLabel)
        {
            this.jumpLabel = jumpLabel;
        }

        public override Action finalise(Parse parser, uncompiledPoem up)
        {
            return new Branch(parser.resolveLabel(jumpLabel, up));
        }

        public override string finaliseC(Parse parser, uncompiledPoem up, signiture poemSig)
        {
            return "goto " + jumpLabel + ";";
        }
    }

    class Parse
    {
        private Lex lexer;

        public Parse(Lex lexer)
        {
            this.lexer = lexer;
        }

        Dictionary<signiture, Poem> sigResolve;

        public Poem resolveSig(signiture s)
        {
            return sigResolve[s];
        }

        public Anthology compile()
        {
            makeExps();

            sigResolve = new Dictionary<signiture, Poem>();

            foreach (InbuiltPoem p in InbuiltPoem.allInbuilt) sigResolve.Add(p.sig, p);

            int i = 0;
            Poem[] compiledPoems = new Poem[poems.Count];
            foreach (uncompiledPoem up in poems)
            {
                Poem p = new Poem(up.sig);
                compiledPoems[i++] = p;
                sigResolve.Add(up.sig, p);
               
            }
            i = 0;
            foreach (uncompiledPoem up in poems)
            {
                Poem p = compiledPoems[i++];
                Action[] acts = new Action[up.exps.Count];

                int j = 0;
                foreach (ParsedExp pe in up.exps)
                {
                    acts[j++] = pe.finalise(this, up);
                }

                p.setActions(acts, up.varCount);
            }
            
            Anthology a = new Anthology(compiledPoems[0], compiledPoems);
            return a;
        }

        public string compileC()
        {
            makeExps();

            String all = "#include poem.h\n#include inbuiltPoems.h\n\n";


            foreach (uncompiledPoem up in poems)
            {
                all += up.sig.asC + ";\n";
            }
            all += "\n";

            foreach (uncompiledPoem up in poems)
            {
                String[] lines = new String[up.exps.Count];
                int j = 0;
                foreach (ParsedExp pe in up.exps)
                {
                    lines[j++] = "  " + pe.finaliseC(this, up, up.sig);
                }

                foreach(KeyValuePair<String, int> pair in up.labels) 
                {
                    lines[pair.Value] = "  " + pair.Key + ": " + lines[pair.Value];
                }

                String tmpDef = "";
                for (int i = 0; i < up.tmpCount; i++) tmpDef += "  var* _tmp" + i + "; ";
                if (tmpDef != "") tmpDef += "\n";

                String body = String.Join("\n", lines);
                all += up.sig.asC + " {\n" + tmpDef + body + "\n}\n\n";
            }

            uncompiledPoem main = poems[0];
            String entrySig = "void main(int agrc, char** argv) {\n";
            String entryBody = "//TODO: call entry point with arguments, unwrap result and out it\n";

            all += entrySig + entryBody + "\n}";
            return all;
        }

        private void makeExps()
        {

            functionTable = new Dictionary<string, signiture>();
            poems = new List<uncompiledPoem>();

            while (!lexer.EOS)
            {
                Token t = lexer.peek();
                if (t.tokenType == TT.EOS) break;
                if (t.tokenType == TT.NewLine)
                {
                    lineNo++;
                    continue;
                }
                if (t.tokenType != TT.Tild) error("Poem title must begin with '~' symbol");

                uncompiledPoem cp = compilePoem();
                poems.Add(cp);
            }
            lineNo = 1;

            //inbuilt functions are added to the table
            functionTable.Add("SAY", InbuiltPoem.sayPoem.sig);
            functionTable.Add("READ", InbuiltPoem.readPoem.sig);
            functionTable.Add("DECLARE", InbuiltPoem.sayPoem.sig);
            functionTable.Add("ADD", InbuiltPoem.addPoem.sig);
            functionTable.Add("SUM", InbuiltPoem.addPoem.sig);
            functionTable.Add("JOIN", InbuiltPoem.addPoem.sig);
            functionTable.Add("TAKE", InbuiltPoem.subPoem.sig);
            functionTable.Add("MULTIPLY", InbuiltPoem.multPoem.sig);
            functionTable.Add("DIVIDE", InbuiltPoem.divPoem.sig);
            functionTable.Add("REMAIN", InbuiltPoem.modPoem.sig);
            functionTable.Add("BOTH", InbuiltPoem.addPoem.sig);
            functionTable.Add("EITHER", InbuiltPoem.orPoem.sig);
            functionTable.Add("EXCLUSIVELY", InbuiltPoem.xorPoem.sig);
            functionTable.Add("NOT", InbuiltPoem.notPoem.sig);
            functionTable.Add("SAME", InbuiltPoem.equalPoem.sig);
            functionTable.Add("FIRST", InbuiltPoem.hdPoem.sig);
            functionTable.Add("END", InbuiltPoem.tlPoem.sig);
            functionTable.Add("FOLLOW", InbuiltPoem.appendPoem.sig);
            functionTable.Add("EMPTY", InbuiltPoem.emptyPoem.sig);
            functionTable.Add("LESS", InbuiltPoem.lessPoem.sig);
            functionTable.Add("SMALLER", InbuiltPoem.lessPoem.sig);
            functionTable.Add("SMALL", InbuiltPoem.lessPoem.sig);
            functionTable.Add("MORE", InbuiltPoem.morePoem.sig);
            functionTable.Add("GREATER", InbuiltPoem.morePoem.sig);
            functionTable.Add("LARGER", InbuiltPoem.morePoem.sig);
            functionTable.Add("LARGE", InbuiltPoem.morePoem.sig);

            foreach (uncompiledPoem uncompiled in poems)
            {
                functionTable.Add(uncompiled.sig.ID, uncompiled.sig);
            }
            foreach (uncompiledPoem uncompiled in poems)
            {
                buildPoem(uncompiled);
            }
        }

        List<uncompiledPoem> poems;
        Dictionary<String, signiture> functionTable;
        List<Tuple<Word, LeftRightVal>> assignmentMatches;

        private void resetState()
        {
            assignmentMatches = new List<Tuple<Word, LeftRightVal>>();
        }

        int labelI = 0;
        public String newLabel()
        {
            String lbl = "label" + (labelI++);
            return lbl;
        }

        public int resolveLabel(String label, uncompiledPoem up)
        {
            return up.labels[label];
        }

        public int resolveID(String id, uncompiledPoem up)
        {
            if (id == null) return -1;
            if (up.variableTable.Keys.Contains(id)) return up.variableTable[id];
            int newId = up.varCount++;
            up.variableTable.Add(id, newId);
            return newId;
        }

        public String createTmp(uncompiledPoem up)
        {
            String s = "_tmp" + (up.tmpCount++);
            resolveID(s, up);
            return s;
        }

        public int resolveLiteral(uncompiledPoem up,String id)
        {
            bool needAssign = !up.variableTable.Keys.Contains(id);
            int index = resolveID(id, up);
            if (needAssign)
            {
                up.exps.Add(new DeclareExp(id, id.Remove(0, 1)));
            }
            return index;
        }

        private void buildPoem(uncompiledPoem up)
        {
            resetState();
            lineNo++;
            
            foreach (String s in up.sig.arguments)
            {
                resolveID(s, up);
            }

            while (up.tokens.Count != 0)
            {
                if (up.tokens.Peek().tokenType == TT.FullStop) error("Full stop found without matched if or while");
                if (up.tokens.Peek().tokenType == TT.SemiColon) error("Semi colon encountered without matching if");
                if (up.tokens.Peek().tokenType == TT.ExclamationMark)
                {
                    warning("Top level return found, nothing afterwards will execute. If this is the end of the poem ignore this warning!");
                    return;
                }
                buildLine(up);
            }

        }

        Word lastWord = null;
        int lineStartActionLabel;

        private void buildLine(uncompiledPoem up)
        {
            //FIXME error with . and ;

            Token t;
            lastWord = null;
            LeftRightVal lrv;
            lrv.left = null; lrv.right = null;

            lineStartActionLabel = up.exps.Count;
            while ((t = up.tokens.Peek()).tokenType == TT.Word || t.tokenType == TT.Literal)
            {
                up.tokens.Dequeue();
                if(t.tokenType == TT.Literal)
                {
                    lrv.right = "_" + t.wordV;
                    lastWord = null;
                    continue;
                }
                if (!Word.wordExists(t.wordV)) error("Word '" + t.wordV + "' cannot be found in dictionary");
                Word cword = new Word(t.wordV);
                if (functionTable.Keys.Contains(t.wordV))
                {
                    lastWord = cword;
                    lrv.right = buildFunction(up, t.wordV);
                }
                else if (up.variableTable.Keys.Contains(t.wordV))
                {
                    lrv.left = lrv.right = t.wordV;
                    lastWord = cword;
                }
                else
                {
                    if (lastWord != null && cword.alliterate(lastWord))
                    {
                        LeftRightVal tmpLRV = buildDecleration(up, cword);
                        lrv.left = tmpLRV.left;
                        if (tmpLRV.right != null) lrv.right = tmpLRV.right;
                    }
                }

            }

            if (lastWord != null)
            {
                foreach (Tuple<Word, LeftRightVal> lineEnding in assignmentMatches)
                {
                    if (lineEnding.Item1.primaryRyhme(lastWord))
                    {
                        assignmentMatches.Remove(lineEnding);
                        buildAssignment(up, lineEnding.Item2.left, lrv.right);
                        break;
                    }
                }
            }

            if(lastWord != null) assignmentMatches.Add(new Tuple<Word, LeftRightVal>(lastWord, lrv));

            if (t.tokenType == TT.ExclamationMark)
                buildReturn(up, lrv.right);

            if (t.tokenType == TT.FullStop || t.tokenType == TT.SemiColon || t.tokenType == TT.ExclamationMark) return;
            
            up.tokens.Dequeue();

            if (t.tokenType == TT.NewLine) lineNo++;
            else if (t.tokenType == TT.EOS) return;
            else if (t.tokenType == TT.QuestionMark) buildIF(up, lrv.right);
            else if (t.tokenType == TT.Colon) buildWhile(up, lrv.right);
        }

        private String buildFunction(uncompiledPoem up, String functionName)
        {


            signiture s = functionTable[functionName];
            int argsRemain = (s.arguments == null) ? 0 : s.arguments.Count;
            String[] indexs = new String[argsRemain];
            Token t;
            int index = 0;
            while (argsRemain != 0)
            {
                t = up.tokens.Dequeue();
                
                if (t.tokenType == TT.Word)
                {
                    lastWord = new Word(t.wordV);
                    if (functionTable.Keys.Contains(t.wordV))
                    {
                        indexs[index++] = buildFunction(up, t.wordV);
                    }
                    else
                    {
                        if (up.variableTable.Keys.Contains(t.wordV))
                        {
                            indexs[index++] = t.wordV;
                        }
                        else continue;
                    }
                }
                else if (t.tokenType == TT.Literal)
                {
                    resolveLiteral(up, "_" + t.wordV);
                    indexs[index++] = "_" + t.wordV;
                    lastWord = null;
                }
                else error("Was expecting more arguments after " + functionName + " call.");
                argsRemain--;
            }

            String returnTmp = s.hasReturn ? createTmp(up) : null;
            up.exps.Add(new CallExp(functionTable[functionName], indexs, returnTmp));
            return returnTmp;
        }

        private LeftRightVal buildDecleration(uncompiledPoem up, Word firstOccurence)
        {
            Queue<Word> toDeclare = new Queue<Word>();
            toDeclare.Enqueue(firstOccurence);
            Token t;
            lastWord = firstOccurence;
            while ((t = up.tokens.Peek()).tokenType == TT.Word)
            {
                Word cword = new Word(t.wordV);
                if (!cword.alliterate(firstOccurence)) break;
                toDeclare.Enqueue(cword);
                lastWord = cword;

                up.tokens.Dequeue();
            }

            String init = null;
            if ((t = up.tokens.Peek()).tokenType == TT.Literal)
            {
                lastWord = null;
                init = t.wordV;
                up.tokens.Dequeue();
            }

            String variableName = null;
            while (toDeclare.Count != 0)
            {
                Word w = toDeclare.Dequeue();
                variableName = w.ToString();
                if (up.variableTable.Keys.Contains(variableName)) error(variableName + " was already declared.");
                resolveID(variableName, up);
                up.exps.Add(new DeclareExp(variableName, init));
            }

            return new LeftRightVal(variableName, init);
        }

        private void buildAssignment(uncompiledPoem up, String left, String right)
        {
            if (left == null || right == null)
            {
                warning("Null assignment attempted. Did you mean this to rhyme?");
                return;
            }
            if (right.First() == '_') resolveLiteral(up, right);
            up.exps.Add(new AssignExp(left, right));
        }

        private void buildReturn(uncompiledPoem up, String lv)
        {
            up.exps.Add(new ReturnExp(lv));
        }

        private void buildIF(uncompiledPoem up, String rv)
        {
            if (rv.First() == '_') resolveLiteral(up, rv);
            String label = newLabel();
            up.exps.Add(new BranchIfExp(rv, label, true));
            Token t;

            do
            {
                buildLine(up);
                t = up.tokens.Peek();
            } while (t.tokenType != TT.FullStop && t.tokenType != TT.SemiColon && t.tokenType != TT.ExclamationMark);

            up.tokens.Dequeue();

            if (t.tokenType == TT.FullStop || t.tokenType == TT.ExclamationMark)
            {
                up.labels.Add(label, up.exps.Count);
            }
            else if(t.tokenType == TT.SemiColon)
            {
                String secondLabel = newLabel();
                up.exps.Add(new BranchExp(secondLabel));
                up.labels.Add(label, up.exps.Count);

                do
                {
                    buildLine(up);
                    t = up.tokens.Peek();
                } while (t.tokenType != TT.FullStop && t.tokenType != TT.ExclamationMark);
                
                up.tokens.Dequeue();

                up.labels.Add(secondLabel, up.exps.Count);
            }
            
        }

        private void buildWhile(uncompiledPoem up, String rv)
        {
            if (rv.First() == '_') resolveLiteral(up, rv);
            List<ParsedExp> whileBody = up.exps.GetRange(lineStartActionLabel, up.exps.Count - lineStartActionLabel);
            up.exps.RemoveRange(lineStartActionLabel, up.exps.Count - lineStartActionLabel);

            String endLabel = newLabel();
            String startLabel = newLabel();
            up.exps.Add(new BranchExp(endLabel));
            up.labels.Add(startLabel, up.exps.Count);
            Token t;

            do
            {
                buildLine(up);
                t = up.tokens.Peek();
            } while (t.tokenType != TT.FullStop && t.tokenType != TT.ExclamationMark);
            up.tokens.Dequeue();

            up.labels.Add(endLabel, up.exps.Count);
            up.exps.AddRange(whileBody);
            up.exps.Add(new BranchIfExp(rv, startLabel, false));
        }

        private uncompiledPoem compilePoem()
        {
            signiture sig = compilePoemTitle();
            lineNo++;
            Queue<Token> tokens = new Queue<Token>();
            Token t;

            while((t = lexer.peek()).tokenType != TT.Tild && t.tokenType != TT.EOS)
            {
                if (t.tokenType == TT.NewLine) lineNo++;
                tokens.Enqueue(t);
                lexer.next();
            }

            tokens.Enqueue(Token.EOS);
            uncompiledPoem up = new uncompiledPoem();
            up.sig = sig;
            up.tokens = tokens;
            up.exps = new List<ParsedExp>();
            up.variableTable = new Dictionary<string,int>();
            up.labels = new Dictionary<string,int>();
            up.varCount = 0;
            return up;
        }

        private signiture compilePoemTitle()
        {
            String ID;
            List<String> arguments = new List<String>();
            List<bool> copy = new List<bool>();
            signiture sig;
            Token t;
            while ((t = lexer.next()).tokenType == TT.Tild);

            sig.hasReturn = false;
            if (t.tokenType == TT.Pipe)
            {
                sig.hasReturn = true;
                t = lexer.next();
            }

            if (t.tokenType != TT.Word) error("Poem title may only consist of (at least one) words and leading/trailing '~' symbols");

            ID = t.wordV;
            sig.copyReturn = !t.captilised;

            while ((t = lexer.next()).tokenType == TT.Word)
            {
                arguments.Add(t.wordV);
                copy.Add(!t.captilised);
            }

            while ((t = lexer.next()).tokenType == TT.Tild || t.tokenType == TT.Pipe);
            if (t.tokenType != TT.NewLine) error("Poem title may only end with traling '~'");

            sig.ID = ID;
            sig.arguments = arguments;
            sig.copy = copy.ToArray();

            return sig;
        }

        int lineNo = 1;

        private void error(String desc)
        {
            throw new Exception("Error on line " + lineNo + ". " + desc);
        }

        private void warning(String desc)
        {
            Console.WriteLine("Warning on line " + lineNo + ": " + desc);
        }
    }
}
