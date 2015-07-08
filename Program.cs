using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Concordance
{
    // this class encapsulates a concordance for one single word; e.g. {6:0,0,0,1,1,2}
    class WordConcordance
    {
        private List<int> positions;

        /// <summary>
        /// Ctor public
        /// </summary>
        public WordConcordance()
        {
            this.positions = new List<int>(7);
        }

        /// <summary>
        /// Gets many times the word occurs in input text
        /// </summary>
        public int NumberOfOccurances
        {
            get { return this.positions.Count(); }
        }

        /// <summary>
        /// Add an additional occurance in sentence represented by zero based index sentenceIndex
        /// </summary>
        /// <param name="sentenceIndex">zero based index for sentence</param>
        public void Add(int sentenceIndex)
        {
            this.positions.Add(sentenceIndex);
        }

        /// <summary>
        /// Override the ToString to easily get the concordance string representation for this word
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string stringRepresentation = string.Format("{0}:", this.positions.Count());
            for (int idx = 0; idx < positions.Count - 1; idx++ )
            {
                stringRepresentation += string.Format("{0},", positions[idx]);
            }
            if (positions.Count - 1 >= 0)
                stringRepresentation += string.Format("{0}", positions[positions.Count - 1]);
            return string.Format("{{{0}}}", stringRepresentation);
        }
    }

    class Program
    {
        private string inputText = "A \"concordance\" is an alphabetical list of the words present in a text with a count of how\n\r" +
                            "often each word appears and citations of where each word appears in the text (e.g., page\n\r" +
                            "number). Write a program -- in the programming language of your choice -- that will\n\r" +
                            "generate a concordance of an arbitrary text document written in English: the text can be\n\r" +
                            "read from stdin, and the program should output the concordance to stdout or a file. For\n\r" +
                            "each word, it should print the count and the sorted list of citations, in this case the\n\r" +
                            "zero-indexed sentence number in which that word occurs. You may assume that the input\n\r" +
                            "contains only spaces, newlines, standard English letters, and standard English punctuation\n\r" +
                            "marks.";

        private char[] sentenceSeparators = new char[] { '.', '!', '?', ';' };
        private char[] wordSeparators = new char[] { ' ', '\r', '\n' };
        private char[] wordTokenExcessChars = new char[] { ' ', ',', ':', '-', '\'', '"', '(', ')', '{', '}', '[', ']' };

        // this abreviations dictionary has to be completed
        private string[] wordAbreviations = new string[] { "ex.", "e.g.", "i.e"};

        Dictionary<string, WordConcordance> concordance = new Dictionary<string, WordConcordance>(100);

        static void Main(string[] args)
        {
            try
            {
                Concordance.Program prog = new Concordance.Program();
                prog.Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.ToString());
            }
        }

        public void Start()
        {
            InitializeInputText();

            // INVARIANT: we have input text initialized (one way or the other)

            // break the text in sentences
            // first make sure the abreviations are not confusing the sentence parser; encode all abreviations
            // N.B. if char | is used in english texts then it has to be replaced with a different char which is not in english texts
            foreach (string abreviation in wordAbreviations)
                inputText = inputText.Replace(abreviation, abreviation.Replace('.', '|'));

            // find all sentences
            string[] sentences = inputText.Split(sentenceSeparators);

            // break each sentence in words
            for (int sentenceIdx=0; sentenceIdx<sentences.Length; sentenceIdx++)
            {
                // decode the encoded abreviations
                foreach (string abreviation in wordAbreviations)
                    sentences[sentenceIdx] = sentences[sentenceIdx].Replace(abreviation.Replace('.', '|'), abreviation);

                string[] words = sentences[sentenceIdx].Split(wordSeparators);
                
                // process each word and add to its own concordance
                foreach (string wordToken in words)
                {
                    // INVARIANT: we are in sentence identified by zero based index sentenceIdx 
                    // INVARIANT: we are processing word wordToken

                    string word = CleanWordToken(wordToken);

                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        if (!concordance.Keys.Contains(word))
                        {
                            WordConcordance wordConcordance = new WordConcordance();
                            concordance.Add(word, wordConcordance);
                        }
                        // update word concordance with this new instance found
                        concordance[word].Add(sentenceIdx);
                    }
                }
            }

            // sort all words alfabetically
            IOrderedEnumerable<KeyValuePair<string, WordConcordance>> concordanceKVP = concordance.OrderBy(s => s.Key);

            // list the resulted concordance
            Console.WriteLine("\nThe Concordance Computed is:\n");
            foreach (KeyValuePair<string, WordConcordance> kvp in concordanceKVP)
                Console.WriteLine("{0}\t\t\t{1}", kvp.Key, kvp.Value.ToString());
        }

        /// <summary>
        /// this function gets a wordToken and it is cleaning it off of commas, spaces, column, semicolumn, etc
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private string CleanWordToken(string wordToken)
        {
            string cleanedWord = wordToken.Trim(wordTokenExcessChars);
            wordToken = cleanedWord.Trim(sentenceSeparators);
            cleanedWord = wordToken.Trim(wordSeparators);

            // the abreviations have to end in a dot
            if (cleanedWord.IndexOf('.') > 0)
                cleanedWord = string.Format("{0}.", cleanedWord);

            return cleanedWord.ToLowerInvariant();
        }

        private void InitializeInputText()
        {
            // ask the user if it wants to enter the text manually or see the program in action 
            // using the statically linked input text 
            Console.WriteLine("Do you want to:\n\t(1) enter input text manually or\n\t(2) use the hardcoded input text demo?\n\ttype 1 or 2 followed by ENTER");
            string lineRead = Console.ReadLine();

            if (lineRead.Equals("1"))
                inputText = null;
            else
            {
                Console.WriteLine("You chose to use the statically linked demo input text below:\n");
                Console.WriteLine(inputText);
            }

            // make sure we have an input text
            if (string.IsNullOrEmpty(inputText))
            {
                inputText = ReadInputTextFromConsole();
            }
        }
        /// <summary>
        /// This function reads input text from console and returns it
        /// </summary>
        /// <returns></returns>
        private string ReadInputTextFromConsole()
        {
            bool bKeepReading = true;
            StringBuilder inputBuildder = new StringBuilder(250);

            Console.WriteLine("Please type the text to generate the concordance for. Press ESC when you are done typing.\n");

            while (bKeepReading)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(false);

                // if ESC is pressed stop reading
                if (consoleKeyInfo.Key == ConsoleKey.Escape)
                {
                    bKeepReading = false;
                    // delete the ESC char which was just printed on the console
                    Console.SetCursorPosition(Console.CursorLeft - 1 >= 0 ? Console.CursorLeft - 1 : 0, Console.CursorTop);
                    Console.WriteLine(" ");
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    inputBuildder.Append("\n\r");
                }
                else if (consoleKeyInfo.Key == ConsoleKey.Backspace)
                {
                    // delete the last char by overwritting it with space 
                    Console.Write(" ");
                    if (Console.CursorLeft - 1 >= 0 && inputBuildder.ToString().Length - 1 >= 0)
                    {
                        inputBuildder.Remove(inputBuildder.ToString().Length - 1, 1);
                    }
                    Console.SetCursorPosition(Console.CursorLeft - 1 >= 0 ? Console.CursorLeft - 1 : 0, Console.CursorTop); 
                }
                else
                {
                    inputBuildder.Append(consoleKeyInfo.KeyChar);
                }
            }
            return inputBuildder.ToString();
        }
    }
}
