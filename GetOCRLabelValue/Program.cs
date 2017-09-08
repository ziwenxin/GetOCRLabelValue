using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOCRLabelValue
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    namespace ConsoleApp1
    {
        class Program
        {
            static void Main(string[] args)
            {
                string label = "Customer account";
                string txt = ReadFile();
                string remainTxt = txt;
                //find all the numbers after the label, get max
                Label max = FindLabelValue(ref txt, label);

                while (true)
                {
                    Label num = FindLabelValue(ref txt, label);
                    if (num == null || max == null)
                        break;
                    if (Convert.ToDouble(num.Name) > Convert.ToDouble(max.Name))
                        max = num;
                }

                GetFullValue(txt, max);
                Console.WriteLine(max.Name);
                Console.ReadKey();
            }

            private static void GetFullValue(string txt, Label max)
            {
                int startIdx = max.StartIdx;
                int endIdx = max.EndIdx;
                //get all the parts of the result
                while (true)
                {
                    if (startIdx < 0 || txt[startIdx] == ' '||Environment.NewLine.Contains(txt[startIdx]))
                        break;
                    startIdx--;

                }
                while (true)
                {
                    if (endIdx > txt.Length || txt[endIdx] == ' ' || Environment.NewLine.Contains(txt[endIdx]))
                        break;
                    endIdx++;
                }
                max.StartIdx = startIdx+1;
                max.EndIdx = endIdx - 1;
                max.Name = txt.Substring(max.StartIdx, max.EndIdx - max.StartIdx);
            }

            private static Label FindLabelValue(ref string txt, string labelName)
            {
                //set the searching label
                Label label = new Label();
                label.Name = labelName;
                int startIdx = txt.IndexOf(label.Name, StringComparison.CurrentCultureIgnoreCase);
                if (startIdx == -1)
                    return null;
                label.StartIdx = startIdx;
                label.EndIdx = startIdx + label.Name.Length;
                //find the spaces before label
                FindSpaces(txt, label);
                //get the line number of the label
                GetLineNumber(txt, label);
                int lineCounter = 0;
                //replace the found label with *
                StringBuilder sb = new StringBuilder(txt);
                for (int i = 0; i < labelName.Length; i++)
                {
                    sb[startIdx++] = '*';
                }
                txt = sb.ToString();
                int endIdx = startIdx;

                List<Label> numbers = new List<Label>();
                //find all the numbers before the end of next line
                while (true)
                {
                    startIdx = endIdx + 1;
                    //found the first num after label
                    while (true)
                    {
                        //Console.WriteLine("$" + txt[startIdx] + "$");
                        if (startIdx >= txt.Length)
                            goto Finished;
                        //count the lines
                        if (Environment.NewLine.Contains(txt[startIdx]))
                            lineCounter++;
                        if (char.IsNumber(txt[startIdx]))
                        {
                            endIdx = startIdx + 1;
                            break;
                        }

                        startIdx++;
                    }
                    string[] puncuations = { ",", "." };
                    //find the end idx
                    while (true)
                    {

                        if (endIdx >= txt.Length)
                            goto Finished;
                        string character = txt[endIdx].ToString();
                        //it is a part of the amount
                        if (puncuations.Any(c => c == character))
                        {
                            endIdx++;
                            continue;
                        }

                        if (!char.IsNumber(txt[endIdx]))
                            break;
                        endIdx++;
                    }
                    //get the number
                    Label number = new Label()
                    {
                        Name = txt.Substring(startIdx, endIdx - startIdx),
                        StartIdx = startIdx,
                        EndIdx = endIdx

                    };
                    FindSpaces(txt, number);
                    GetLineNumber(txt, number);
                    CalculateCost(label, number);
                    numbers.Add(number);
                }
                Finished:
                Label result = numbers.OrderBy(n => n.Cost).FirstOrDefault();

                if (result != null)
                    return result;

                return null;
            }

            private static void CalculateCost(Label label, Label number)
            {

                number.Cost += Math.Abs(number.PreSpaces - label.PreSpaces);
                number.Cost += Math.Abs(number.PostSpaces - label.PostSpaces);
                number.Cost /= 2;
                number.Cost *= (number.LineNumber - label.LineNumber + 1) * 1.1;
            }

            private static void FindSpaces(string txt, Label label)
            {
                //get the string before the label,find the spaces after new line
                string searchStr = txt.Substring(0, label.StartIdx);
                int idx = searchStr.LastIndexOf(Environment.NewLine, StringComparison.Ordinal);
                label.PreSpaces = label.StartIdx - idx;
                //get the string after the label, find the spaces before new line
                searchStr = txt.Substring(label.StartIdx, txt.Length - label.StartIdx);
                idx = searchStr.IndexOf(Environment.NewLine, StringComparison.Ordinal);
                label.PostSpaces = idx - label.Name.Length;
            }

            static string ReadFile()
            {

                return File.ReadAllText("2.txt");
            }

            static void GetLineNumber(string txt, Label label)
            {
                string preTxt = txt.Substring(0, label.StartIdx);
                label.LineNumber = Regex.Matches(preTxt, Environment.NewLine).Count;
            }
        }

        class Label
        {
            public string Name { get; set; }
            public int LineNumber { get; set; }
            public int StartIdx { get; set; }
            public int EndIdx { get; set; }
            public int PreSpaces { get; set; }
            public int PostSpaces { get; set; }
            public double Cost { get; set; }

        }
    }
}
