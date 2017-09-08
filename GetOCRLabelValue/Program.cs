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
                string txt = ReadFile();
                string remainTxt = txt;
                //find all the numbers after the label, get max
                string max = FindLabelValue(ref txt, "total");

                while (true)
                {
                    string num = FindLabelValue(ref txt, "total");
                    if (string.IsNullOrEmpty(num))
                        break;
                    if (Convert.ToDouble(num) > Convert.ToDouble(max))
                        max = num;
                }
                Console.WriteLine(max);
                Console.ReadKey();
            }

            private static string FindLabelValue(ref string txt, string labelName)
            {
                Label label = new Label();
                label.Name = labelName;
                int startIdx = txt.IndexOf(label.Name, StringComparison.CurrentCultureIgnoreCase);
                if (startIdx == -1)
                    return "";
                label.StartIdx = startIdx;
                label.EndIdx = startIdx + label.Name.Length;
                //find the spaces before label
                FindSpaces(txt, label);
                //get the line number of the label
                GetLineNumber(txt, label
                );
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
                        if (startIdx >= txt.Length)
                            break;
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
                            break;
                        int num = 0;
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
                    if (startIdx >= txt.Length)
                        break;
                    //get the number
                    Label number = new Label()
                    {
                        Name = txt.Substring(startIdx, endIdx - startIdx),
                        StartIdx = startIdx,
                        EndIdx = endIdx

                    };
                    FindSpaces(txt, number);
                    GetLineNumber(txt, number);
                    if (number.LineNumber > label.LineNumber + 1)
                        break;
                    CalculateCost(label, number);
                    numbers.Add(number);
                }
                Label result = numbers.OrderBy(n => n.Cost).FirstOrDefault();


                return result.Name;
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

                return File.ReadAllText("1.txt");
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
