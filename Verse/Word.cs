using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Verse
{
    public class Word
    {
        public enum phoneme
        {
            AA,
            AE,
            AH,
            AO,
            AW,
            AY,
            B,
            CH,
            D,
            DH,
            EH,
            ER,
            EY,
            F,
            G,
            HH,
            IH,
            IY,
            JH,
            K,
            L,
            M,
            N ,
            NG,
            OW,
            OY,
            P,
            R,
            S,
            SH,
            T,
            TH,
            UH,
            UW,
            V,
            W,
            Y,
            Z,
            ZH
        };

        public enum stress
        {
            none = 0,
            primary = 1,
            secondy = 2
        };

        public struct sound
        {
            public phoneme phoneme;
            public stress stress;
            public sound(phoneme p, stress s)
            {
                this.phoneme = p;
                this.stress = s;
            }
            public sound(phoneme p)
            {
                this.phoneme = p;
                this.stress = stress.none;
            }

            public static sound fromString(String asString)
            {
                int stress = 0;
                Boolean zero,one,two;
                zero = asString.EndsWith("0");
                one = asString.EndsWith("1");
                two = asString.EndsWith("2");

                if (zero || one || two) asString = asString.Remove(asString.Length - 1, 1);
                if (one) stress = 1;
                if (two) stress = 2;

                return new sound((phoneme)Enum.Parse(typeof(phoneme),asString), (stress)stress);
            }

            public static Boolean operator ==(sound c1, sound c2)
            {
                return (c1.phoneme == c2.phoneme) && (c1.stress == c2.stress);
            }

            public static Boolean operator !=(sound c1, sound c2)
            {
                return !(c1 == c2);
            }
        }

        private List<List<sound>> sounds;
        String asString;

        public Word(String word)
        {
            word = word.ToUpper();
            asString = word;
           if(!ryhmeDict.Keys.Contains(word)) throw new Exception("No such word found");
           this.sounds = ryhmeDict[word];
        }

        public override string ToString()
        {
            return asString;
        }

        public String soundsLike
        {
            get
            {
                String res = "";
                int i = 0;
                foreach(List<sound> ss in this.sounds)
                {
                    foreach (sound s in ss)
                    {
                        res += s.phoneme + "_" + s.stress +" ";
                    }
                    if(i++ != this.sounds.Count -1) res += ",\n  ";
                }
                return res;
            }
        }

        public Boolean primaryRyhme(Word with)
        {
            foreach(List<sound> a in this.sounds)
                foreach(List<sound> b in with.sounds)
                    if(a.Last() == b.Last()) return true;
            return false;
        }

        public Boolean alliterate(Word with)
        {
            foreach (List<sound> a in this.sounds)
                foreach (List<sound> b in with.sounds)
                    if (alliterate(a, b)) return true;
            return false;
        }

        private static Boolean alliterate(List<sound> a, List<sound> b)
        {
            sound? ac = constanent(a);
            if (ac == null) return false;
            sound? bc = constanent(b);
            if (bc == null) return false;

            return ac == bc;
        }


        private static sound? constanent(List<sound> a)
        {
            sound? ac = null;
            foreach (sound s in a)
            {
                if (isConst(s))
                {
                    ac = s;
                    break;
                }
            }
            return ac;
        }

        private static Boolean isConst(sound s)
        {
            phoneme p = s.phoneme;
            return p == phoneme.B ||
                p == phoneme.CH ||
                p == phoneme.D ||
                p == phoneme.DH ||
                p == phoneme.F ||
                p == phoneme.G ||
                p == phoneme.HH ||
                p == phoneme.JH ||
                p == phoneme.K ||
                p == phoneme.L ||
                p == phoneme.M ||
                p == phoneme.N ||
                p == phoneme.NG ||
                p == phoneme.P ||
                p == phoneme.R ||
                p == phoneme.S ||
                p == phoneme.SH ||
                p == phoneme.T ||
                p == phoneme.TH ||
                p == phoneme.V ||
                p == phoneme.W ||
                p == phoneme.Y ||
                p == phoneme.Z ||
                p == phoneme.ZH;
        }

        public static Boolean wordExists(String word)
        {
            return ryhmeDict.Keys.Contains(word);
        }

        private static int x = 0;
        public static void buildRyhmeDict(StreamReader reader)
        {
            ryhmeDict = new Dictionary<string, List<List<sound>>>();

            String line;

            char[] space = new char[]{' '};

            while((line  = reader.ReadLine()) != null)
            {
                x++;
                if(line.StartsWith("#")) continue;
                String[] bits = line.Split(space);

                List<sound> soundsLike = new List<sound>();
                for (int i = 1; i < bits.Length; i++)
                {
                    if (bits[i] == "") continue;
                    soundsLike.Add(sound.fromString(bits[i]));
                }

                if(bits[0].EndsWith(")"))
                {
                    bits[0] = bits[0].Remove(bits[0].Length - 3, 3);
                }

                if (!ryhmeDict.Keys.Contains(bits[0]))
                    ryhmeDict.Add(bits[0],new List<List<sound>>());

                ryhmeDict[bits[0]].Add(soundsLike);
            }
        }

        private static Dictionary<String, List<List<sound>>> ryhmeDict;
    }
}
