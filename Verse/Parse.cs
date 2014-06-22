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
    }

    struct uncompiledPoem
    {
        public signiture sig;
        public Queue<Token> tokens;
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
        public abstract Action finalise(Parse parser);
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

        public override Action finalise(Parse parser)
        {
            return new Declare(parser.resolveID(id),val);
        }
    }

    class CallExp : ParsedExp
    {
        Poem poem;
        String[] toCopy;
        String ri;
        public CallExp(Poem p, String[] copyIDs, String returnID)
        {
            this.poem = p;
            this.toCopy = copyIDs;
            this.ri = returnID;
        }

        public override Action finalise(Parse parser)
        {

            int[] cop = (toCopy.Length == 0) ? null : new int[toCopy.Length];
            for(int i = 0; i < toCopy.Length ; i++) cop[i] = parser.resolveID(toCopy[i]);
            return new Call(poem,cop,parser.resolveID(ri));
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

        public override Action finalise(Parse parser)
        {
            return new Assign(parser.resolveID(l), parser.resolveID(r));
        }
    }

    class ReturnExp : ParsedExp
    {
        String id;

        public ReturnExp(String id)
        {
            this.id = id;
        }

        public override Action finalise(Parse parser)
        {
            int index = (id == null) ? -1 : parser.resolveID(id);
            return new Return(index);
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

        public override Action finalise(Parse parser)
        {
            return new BranchIf(parser.resolveLabel(jumpLabel), parser.resolveLiteral(testable), invert);
        }
    }

    class BranchExp: ParsedExp
    {
        String jumpLabel;

        public BranchExp(String jumpLabel)
        {
            this.jumpLabel = jumpLabel;
        }

        public override Action finalise(Parse parser)
        {
            return new Branch(parser.resolveLabel(jumpLabel));
        }
    }

    class Parse
    {
        private Lex lexer;

        public Parse(Lex lexer)
        {
            this.lexer = lexer;
        }

        public Anthology compile()
        {

            functionTable = new Dictionary<string, Poem>();
            List<uncompiledPoem> poems = new List<uncompiledPoem>();

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
            Poem[] compiledPoems = new Poem[poems.Count];
            int i = 0;

            //inbuilt functions are added to the table
            functionTable.Add("SAY", InbuiltPoem.sayPoem);
            functionTable.Add("READ", InbuiltPoem.readPoem);
            functionTable.Add("DECLARE", InbuiltPoem.sayPoem);
            functionTable.Add("ADD", InbuiltPoem.addPoem);
            functionTable.Add("SUM", InbuiltPoem.addPoem);
            functionTable.Add("JOIN", InbuiltPoem.addPoem);
            functionTable.Add("TAKE", InbuiltPoem.subPoem);
            functionTable.Add("MULTIPLY", InbuiltPoem.multPoem);
            functionTable.Add("DIVIDE", InbuiltPoem.divPoem);
            functionTable.Add("REMAIN", InbuiltPoem.modPoem);
            functionTable.Add("BOTH", InbuiltPoem.addPoem);
            functionTable.Add("EITHER", InbuiltPoem.orPoem);
            functionTable.Add("EXCLUSIVELY", InbuiltPoem.xorPoem);
            functionTable.Add("NOT", InbuiltPoem.notPoem);
            functionTable.Add("SAME", InbuiltPoem.equalPoem);

            foreach (uncompiledPoem uncompiled in poems)
            {
                functionTable.Add(uncompiled.sig.ID,compiledPoems[i++] = new Poem(uncompiled.sig));
            }
            i = 0;
            foreach (uncompiledPoem uncompiled in poems)
            {
                buildPoem(compiledPoems[i++], uncompiled);
            }

            Anthology a = new Anthology(compiledPoems[0],compiledPoems);
            return a;
            
        }

        Dictionary<String, Poem> functionTable;
        HashSet<String> allVars;
        Dictionary<String, int> variableTable;
        List<Tuple<Word, LeftRightVal>> assignmentMatches;
        List<ParsedExp> exps;
        Dictionary<String, int> labels;
        private int varCount;



        private void resetState()
        {
            allVars = new HashSet<string>();
            variableTable = new Dictionary<string, int>();
            assignmentMatches = new List<Tuple<Word, LeftRightVal>>();
            exps = new List<ParsedExp>();
            labels = new Dictionary<string, int>();
            varCount = 0;
        }

        int labelI = 0;
        public String newLabel()
        {
            String lbl = "label" + (labelI++);
            return lbl;
        }
        public int resolveLabel(String label)
        {
            return labels[label];
        }

        public int resolveID(String id)
        {
            if (id == null) return -1;
            if (variableTable.Keys.Contains(id)) return variableTable[id];
            int newId = varCount++;
            variableTable.Add(id, newId);
            return newId;
        }

        int tmpCount = 0;

        public String createTmp()
        {
            String s = "_" + (tmpCount++);
            resolveID(s);
            return s;
        }

        public int resolveLiteral(String id)
        {
            bool needAssign = !variableTable.Keys.Contains(id);
            int index = resolveID(id);
            if (needAssign)
            {
                exps.Add(new DeclareExp(id, id.Remove(0, 1)));
            }
            return index;
        }

        private void buildPoem(Poem p, uncompiledPoem up)
        {
            resetState();
            lineNo++;
            
            foreach (String s in up.sig.arguments)
            {
                allVars.Add(s);
                resolveID(s);
            }

            while (up.tokens.Count != 0)
            {
                if (up.tokens.Peek().tokenType == TT.FullStop) error("Full stop found without matched if or while");
                if (up.tokens.Peek().tokenType == TT.SemiColon) error("Semi colon encountered without matching if");
                buildLine(p, up);
            }

            Action[] acts = new Action[exps.Count];
            int i = 0;
            foreach (ParsedExp pe in exps)
            {
                acts[i++] = pe.finalise(this);
            }

            p.setActions(acts, varCount);
        }

        Word lastWord = null;
        int lineStartActionLabel;

        private void buildLine(Poem p, uncompiledPoem up)
        {
            //FIXME error with . and ;

            Token t;
            lastWord = null;
            LeftRightVal lrv;
            lrv.left = null; lrv.right = null;

            lineStartActionLabel = exps.Count;
            while ((t = up.tokens.Peek()).tokenType == TT.Word || t.tokenType == TT.Literal)
            {
                up.tokens.Dequeue();
                if(t.tokenType == TT.Literal)
                {
                    lrv.right = "$" + t.wordV;
                    lastWord = null;
                    continue;
                }
                if (!Word.wordExists(t.wordV)) error("Word '" + t.wordV + "' cannot be found in dictionary");
                Word cword = new Word(t.wordV);
                if (functionTable.Keys.Contains(t.wordV))
                {
                    lrv.right = buildFunction(p, up, t.wordV);
                    //FIXME should we be setting ladtWord to null?
                }
                else if (allVars.Contains(t.wordV))
                {
                    lrv.left = lrv.right = t.wordV;
                    lastWord = cword;
                }
                else
                {
                    if (lastWord != null && cword.alliterate(lastWord))
                    {
                        LeftRightVal tmpLRV = buildDecleration(p, up, cword);
                        allVars.Add(cword.ToString());
                        lrv.left = tmpLRV.left;
                        if (tmpLRV.right != null) lrv.right = tmpLRV.right;
                    }
                    lastWord = cword;
                }

            }

            if (lastWord != null)
            {
                foreach (Tuple<Word, LeftRightVal> lineEnding in assignmentMatches)
                {
                    if (lineEnding.Item1.primaryRyhme(lastWord))
                    {
                        assignmentMatches.Remove(lineEnding);
                        buildAssignment(p, up, lineEnding.Item2.left, lrv.right);
                        break;
                    }
                }
            }

            if(lastWord != null) assignmentMatches.Add(new Tuple<Word, LeftRightVal>(lastWord, lrv));

            if (t.tokenType == TT.FullStop || t.tokenType == TT.SemiColon) return;
            
            up.tokens.Dequeue();

            if (t.tokenType == TT.NewLine) lineNo++;
            else if (t.tokenType == TT.EOS) return;
            else if (t.tokenType == TT.QuestionMark) buildIF(p, up, lrv.right);
            else if (t.tokenType == TT.ExclamationMark) buildReturn(p, up, lrv.right);
            else if (t.tokenType == TT.Colon) buildWhile(p, up, lrv.right);
        }

        private String buildFunction(Poem p, uncompiledPoem up, String functionName)
        {


            signiture s = functionTable[functionName].sig;
            int argsRemain = s.arguments.Count;
            String[] indexs = new String[argsRemain];
            Token t;
            int index = 0;
            while (argsRemain != 0)
            {
                t = up.tokens.Dequeue();
                
                if (t.tokenType == TT.Word)
                {
                    if (functionTable.Keys.Contains(t.wordV))
                    {
                        indexs[index++] = buildFunction(p, up, t.wordV);
                    }
                    else
                    {
                        if (variableTable.Keys.Contains(t.wordV))
                        {
                            indexs[index++] = t.wordV;
                            lastWord = new Word(t.wordV);
                        }
                        else continue;
                    }
                }
                else if (t.tokenType == TT.Literal)
                {
                    resolveLiteral("$" + t.wordV);
                    indexs[index++] = "$" + t.wordV;
                    lastWord = null;
                }
                else error("Was expecting more arguments after " + functionName + " call.");
                argsRemain--;
            }

            String returnTmp = s.hasReturn ? createTmp() : null;
            exps.Add(new CallExp(functionTable[functionName], indexs, returnTmp));
            return returnTmp;
        }

        private LeftRightVal buildDecleration(Poem p, uncompiledPoem up, Word firstOccurence)
        {
            Queue<Word> toDeclare = new Queue<Word>();
            toDeclare.Enqueue(firstOccurence);
            Token t;
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
                if (variableTable.Keys.Contains(variableName)) error(variableName + " was already declared.");
                resolveID(variableName);
                exps.Add(new DeclareExp(variableName, init));
            }

            return new LeftRightVal(variableName, init);
        }

        private void buildAssignment(Poem p, uncompiledPoem up, String left, String right)
        {
            if (right.First() == '$') resolveLiteral(right);
            exps.Add(new AssignExp(left, right));
        }

        private void buildReturn(Poem p, uncompiledPoem up, String lv)
        {
            exps.Add(new ReturnExp(lv));
        }

        private void buildIF(Poem p, uncompiledPoem up, String rv)
        {
            if (rv.First() == '$') resolveLiteral(rv);
            String label = newLabel();
            exps.Add(new BranchIfExp(rv, label, true));
            Token t;

            do
            {
                buildLine(p, up);
                t = up.tokens.Peek();
            } while (t.tokenType != TT.FullStop && t.tokenType != TT.SemiColon);

            up.tokens.Dequeue();

            if (t.tokenType == TT.FullStop)
            {
                labels.Add(label, exps.Count);
            }
            else if(t.tokenType == TT.SemiColon)
            {
                String secondLabel = newLabel();
                exps.Add(new BranchExp(secondLabel));
                labels.Add(label, exps.Count);

                do
                {
                    buildLine(p, up);
                    t = up.tokens.Peek();
                } while (t.tokenType != TT.FullStop);
                
                up.tokens.Dequeue();

                labels.Add(secondLabel, exps.Count);
            }
            
        }

        private void buildWhile(Poem p, uncompiledPoem up, String rv)
        {
            if (rv.First() == '$') resolveLiteral(rv);
            List<ParsedExp> whileBody = exps.GetRange(lineStartActionLabel, exps.Count - lineStartActionLabel);
            exps.RemoveRange(lineStartActionLabel, exps.Count - lineStartActionLabel);

            String endLabel = newLabel();
            String startLabel = newLabel();
            exps.Add(new BranchExp(endLabel));
            labels.Add(startLabel, exps.Count);
            Token t;

            do
            {
                buildLine(p, up);
                t = up.tokens.Peek();
            } while (t.tokenType != TT.FullStop);
            up.tokens.Dequeue();

            labels.Add(endLabel, exps.Count);
            exps.AddRange(whileBody);
            exps.Add(new BranchIfExp(rv, startLabel, false));
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
            uncompiledPoem up;
            up.sig = sig;
            up.tokens = tokens;

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
    }
}
