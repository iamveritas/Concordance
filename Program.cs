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
        private int numberOfOccurances;
        private string tail;

        /// <summary>
        /// Ctor public
        /// </summary>
        public WordConcordance()
        {
            this.numberOfOccurances = 0;
            this.tail = string.Empty;
        }

        /// <summary>
        /// Gets many times the word occurs in input text
        /// </summary>
        public int NumberOfOccurances
        {
            get { return numberOfOccurances; }
        }

        /// <summary>
        /// Add an additional occurance in sentence represented by zero based index sentenceIndex
        /// </summary>
        /// <param name="sentenceIndex">zero based index for sentence</param>
        public void Add(int sentenceIndex)
        {
            if (string.IsNullOrEmpty(tail))
                tail = string.Format("{0}", sentenceIndex);
            else
                tail = string.Format("{0},{1}", tail, sentenceIndex);
            
            this.numberOfOccurances++;
        }

        /// <summary>
        /// Override the ToString to easily get the concordance string representation for this word
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{{{0}:{1}}}", this.numberOfOccurances, this.tail);
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
        private char[] workTokenExcessChars = new char[] { ' ', ',', ':', '-', '\'', '"', '(', ')', '{', '}', '[', ']' };

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
            // make sure we have an input text
            if (string.IsNullOrEmpty(inputText))
            {
                inputText = ReadInputTextFromConsole();
            }

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
                
                // process each word 
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
                        concordance[word].Add(sentenceIdx);
                    }
                }
            }

            // sort them alfabetically
            IOrderedEnumerable<KeyValuePair<string, WordConcordance>> concordanceKVP = concordance.OrderBy(s => s.Key);

            // list the resulted concordance
            foreach (KeyValuePair<string, WordConcordance> kvp in concordanceKVP)
                Console.WriteLine("{0}\t{1}", kvp.Key, kvp.Value.ToString());
        }

        /// <summary>
        /// this function gets a wordToken and it is cleaning it off of commas, spaces, column, semicolumn, etc
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        private string CleanWordToken(string wordToken)
        {
            string cleanedWord = wordToken.Trim(workTokenExcessChars);
            wordToken = cleanedWord.Trim(sentenceSeparators);
            cleanedWord = wordToken.Trim(wordSeparators);

            // the abreviations have to end in a dot
            if (cleanedWord.IndexOf('.') > 0)
                cleanedWord = string.Format("{0}.", cleanedWord);

            return cleanedWord.ToLowerInvariant();
        }

        /// <summary>
        /// This function reads input text from console and returns it
        /// </summary>
        /// <returns></returns>
        private string ReadInputTextFromConsole()
        {
            bool bKeepReading = true;
            StringBuilder inputBuildder = new StringBuilder(250);

            while (bKeepReading)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);

                // if ESC is pressed stop reading
                if (consoleKeyInfo.Key == ConsoleKey.Escape)
                    bKeepReading = false;
                else
                {
                    inputBuildder.Append(consoleKeyInfo.KeyChar);
                }
            }
            return inputBuildder.ToString();
        }
    }
}
