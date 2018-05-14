using Peshitta.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Peshitta.Data.DB
{
    public partial class KitabDB
    {
        private string SignificantNumber(int v)
        {
            return (v % 10).ToString();
        }
        public static TextExpanded Decompress(Text inp,  IEnumerable<TextWords> tws, IReadOnlyDictionary<int, words> wordsCache, StringBuilder sb)
        {
            var t = new TextExpanded(inp);
            sb.Length = 0;
            // loop from 0 to 2 for respective text, footer, header decompression
            for (int cx = 2; cx >= 0; cx--)
            {
                sb.Length = 0;
                IEnumerable<TextWords> texts = null;
                if (cx == 0)
                {
                    texts = tws.Where(f => (f.IsFootNote ?? false) == false && (f.IsHeader ?? false) == false);
                }
                else if (cx == 1)
                {
                    texts = tws.Where(f => f.IsFootNote == true);
                }
                else if (cx == 2)
                {
                    texts = tws.Where(f => f.IsHeader == true);
                }

                foreach (var tw in texts)
                {

                    var wrd = wordsCache[tw.wordid];

                    bool qmarkDone = false;
                    bool LDQuoteDone = false, RDQuoteDone = false;
                    bool eqDone = false;
                    if (wrd != null)
                    {

                        //if (tw.PrefixAmp ==true && tw.Semicolon==true) //&amp; encoding
                        //{
                        //    sb.Append('&');
                        //}
                        //if (tw.PreSpace==true)
                        //{
                        //    sb.Append(' ');
                        //}
                        if (tw.LParentThesis == true)
                        {
                            sb.Append('(');
                        }
                        else if (tw.LBracket == true)
                        {
                            sb.Append('[');
                        }
                        if (tw.AddLT == true)
                        {
                            sb.Append('<');
                            if (tw.AddSlash == true)
                                sb.Append('/');
                        }
                        if (tw.LSQuote == true)
                        {
                            sb.Append('\'');
                        }
                        if (tw.LDQuote == true && LDQuoteDone == false)
                        {
                            sb.Append('"');
                        }

                        if (wrd.IsNumber == true)
                        {
                            sb.Append(wrd.number.Value);
                        }
                        else if (tw.IsAllCaps == true)
                        {
                            sb.Append(wrd.word.ToUpper());
                        }
                        else if (tw.IsCapitalized == true)
                        {
                            string w = wrd.word;
                            sb.Append(char.ToUpper(w[0]));
                            sb.Append(w, 1, w.Length - 1);
                        }
                        else
                        {
                            sb.Append(wrd.word);
                        }

                        if (tw.AddEqual == true && tw.QMark == true)
                        {
                            eqDone = true;
                            sb.Append('=');
                            if (tw.RDQuote == true)
                            {
                                sb.Append('"');
                                RDQuoteDone = true;
                            }
                            else if (tw.LDQuote == true)
                            {
                                sb.Append('"');
                                LDQuoteDone = true;
                            }
                            if (tw.QMark == true)
                            {
                                sb.Append('?');
                                qmarkDone = true;
                            }
                        }
                        if (tw.AddBang == true)
                        {
                            sb.Append('!');
                        }
                        if (tw.QMark == true && !qmarkDone)
                        {
                            sb.Append('?');
                            qmarkDone = true;
                        }
                        if (tw.RSQuote == true)
                        {
                            sb.Append('\'');
                        }
                        else if (tw.AddDot == true)
                        {
                            sb.Append('.');
                        }
                        if (tw.AddColon == true)
                        {
                            sb.Append(':');
                        }
                        //else if (tw.Semicolon == true)
                        //{
                        //    sb.Append(';');
                        //}
                        if (tw.AddEqual == true && !eqDone)
                        {
                            sb.Append('=');
                        }

                        if (tw.RDQuote == true && !RDQuoteDone)
                        {
                            sb.Append('"');
                        }
                        if (tw.AddSlashAfter == true)
                        {
                            sb.Append('/');
                        }
                        if (tw.RBracket == true)
                        {
                            sb.Append(']');
                        }
                        else if (tw.AddGT == true)
                        {
                            sb.Append('>');
                        }
                        else if (tw.RParentThesis == true)
                        {
                            sb.Append(')');
                        }

                        if (tw.AddComma == true)
                        {
                            sb.Append(',');
                        }
                        if (tw.AddSpace == true)
                        {
                            sb.Append(' ');
                        }
                        if (tw.AddHyphenMin == true)
                        {
                            sb.Append('-');
                        }
                        //if (tw.PrefixAmp == true && tw.Semicolon==false)
                        //{
                        //    sb.Append('&'); // for &p=1&c=10 etc.
                        //}

                    }
                }
                sb.Replace(Environment.NewLine, "");
                sb.Replace("\n", "");
                sb.Replace("\r", "");
                if (cx == 0)
                {
                    t.Content = sb.ToString();
                }
                else if (cx == 1)
                {
                    if (sb.Length > 0)
                    {
                        t.Remarks = sb.ToString();
                    }
                }
                else if (cx == 2)
                {
                    if (sb.Length > 0)
                    {
                        t.Header = sb.ToString();
                    }
                }
            };
            return t;
        }
    }
}
