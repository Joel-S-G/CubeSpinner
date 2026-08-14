using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Playfair_Cipher
{
    internal class Program
    {
        static Random r = new Random();
        public struct myArrayQueue //used to store special characters removed from the plaintext before encryption. NOT circular
        {
            private int[] positions;
            private char[] characters;
            private int lastIndex;
            private int firstIndex;

            public myArrayQueue()
            {
                positions = new int[3000]; //stores -1 for empty
                characters = new char[3000]; //stores x for empty
                lastIndex = 0;
                firstIndex = 0;
            }

            public bool isEmpty() { return lastIndex == firstIndex; }
            public bool isFull() { return lastIndex == 3000; } 
            public void Enqueue(int position, char character)
            {
                if (isFull()) { throw new Exception("tried to insert data to full arraylist, check input length"); } //error checking

                positions[lastIndex] = position; //store the data
                characters[lastIndex] = character;

                lastIndex++; //increment lastIndex so the next data item can be stored

            }

            public Tuple<int, char> Dequeue()
            {
                if (isEmpty()) { throw new Exception("tried to remove item from an empty queue"); }

                Tuple<int, char> charPosition = new Tuple<int, char>(positions[firstIndex], characters[firstIndex]); //get the element in question
                positions[firstIndex] = -1; //empty the space where it was
                characters[firstIndex] = 'x';

                firstIndex++; //reduce the size of the queue

                return charPosition;
            }

            public void Reset() //doesn't actually remove any data but tells the structure that it is empty so it begins to overwrite old data
            {
                firstIndex = 0;
                lastIndex = 0;
            }

            public myArrayQueue Clone()
            {
                myArrayQueue newQueue = new myArrayQueue();
                newQueue.positions = (int[])this.positions.Clone();
                newQueue.characters = (char[])this.characters.Clone();
                newQueue.lastIndex = this.lastIndex;
                newQueue.firstIndex = this.firstIndex;

                return newQueue;
            }

        }

        public struct myDict //used for ngram analysis
        {
            //note: the largest amount of data this will need to store is 26^4 quadgrams, which is 456976 or approximately 457000
            //i can hash the words by treating each letter as a number from 0-25
            private double[] dictionary; //the values
            private int n; //the length of word being stored

            public myDict(int n)
            {
                this.n = n;
                dictionary = new double[(int)Math.Pow(26, n)];

                for (int i = 0; i < dictionary.Length; i++)
                {
                    dictionary[i] = -15;
                }
            }

            public void SetValue(string word, double value) //used for both adding new things and updating old things
            {
                dictionary[Hash(word)] = value; 
            }

            public double getValue(string word)
            {
                return dictionary[Hash(word)];
            }

            private int Hash(string word) //this improves access time because I know where in the array to look as opposed to storing 2 arrays where I would have to search one for the key
            { 
                int hash = 0;
                int i = n-1;
                foreach(char c in word)
                {
                    hash += (c-'A') * (int)Math.Pow(26, i);
                    i--;
                }
                return hash;
            }

            public double getByIndex(int index) { return dictionary[index]; }

            public void SetByIndex(int index, double value) {  dictionary[index] = value; }

        }

        public abstract class NGramAnalysis
        {
            protected myDict NGrams;
            protected abstract string filePath { get; }

            protected abstract int n { get; }

            protected NGramAnalysis()
            {
                NGrams = new myDict(n);

                string[] data = File.ReadAllLines(filePath); //read all quadgrams individually

                long sum = 0; //this is going to be a very large number

                foreach (string ngram in data) //read all data into dictionary
                {
                    string[] components = ngram.Split(' '); //split into word and value

                    NGrams.SetValue(components[0], double.Parse(components[1])); //load into the dictionary

                    sum += int.Parse(components[1]); //add to the total
                }


                foreach(string ngram in data)
                {
                    string[] components = ngram.Split(' ');
                    NGrams.SetValue(components[0], Math.Log(double.Parse(components[1])/(double)sum)); //replace each value with ln(old value/sum of values)
                }

            }

            public double GetScore(string plaintext) //scores closer to 0 are better
            {
                double score = 0;

                plaintext = plaintext.ToUpper();

                if( plaintext.Length < n)
                {
                    throw new Exception("Tried to score text with insufficient length");
                }

                for (int i = 0; i <= plaintext.Length-n; i++) //stops at LEN-n so you don't run out of text
                {
                    score += NGrams.getValue(plaintext.Substring(i, n)); //get the score of the n letters at position i
                }

                return score;
            }
        }
        
        public class QuadgramAnalysis : NGramAnalysis
        {
            protected override string filePath => "Data/quadgrams.txt";
            protected override int n => 4;
        }
        public class TrigramAnalysis : NGramAnalysis
        {
            protected override string filePath => "Data/trigrams.txt";
            protected override int n => 3;
        }
        public class BigramAnalysis : NGramAnalysis
        {
            protected override string filePath => "Data/bigrams.txt";
            protected override int n => 2;
        }
        

        public class Key
        {
            private string key {  get; set; }

            public Key(string? key = null)
            {
                if (key is not null)
                {
                    this.key = key;
                }
                else //this case is only used if you are starting to guess an unknown key
                {
                    this.key = "ABCDEFGHIKLMNOPQRSTUVWXYZ";
                }
            }

            public Key Clone()
            {
                Key newKey = new Key(this.key);
                return newKey;
            }

            public void Randomise() //fisher-yates shuffle
            {
                
                char[] keyArray = this.key.ToCharArray();
                char temp;
                int j;

                for(int i = 24; i > 0; i--) //this assumes that the key is 25 characters long
                {
                    j = r.Next(0, i + 1);

                    temp = keyArray[i];

                    keyArray[i] = keyArray[j];
                    keyArray[j] = temp;
                }
                this.key = new string(keyArray);
            }

            public void Output() //outputs the key in a human readable form
            {
                int index = 0;
                for(int i = 0; i < 5;  i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        Console.Write(key[index] + " ");
                        index++;
                    }
                    Console.WriteLine();
                }
            }

            public void Centre() //"centres" the key so that A is the first character (makes it easy to check for duplicate keys)
            {
                char[] keyArray = key.ToCharArray();
                int rowshift = (5-((int)Math.Floor((double)keyArray.IndexOf('A')/5)))%5; //how many times do you have to translate the matrix up to get A in the top row
                int colShift = (5-(keyArray.IndexOf('A') % 5)) % 5; //how many times do you have to translate the matrix left to get A in the leftmost column

                char[] temp = new char[25];
                int newRow;
                int newCol;

                for (int row = 0; row < 5; row++)
                {
                    for (int col = 0; col < 5; col++)
                    {
                        newRow = (row + rowshift) % 5;
                        newCol = (col + colShift) % 5;

                        temp[newRow*5 + newCol] = keyArray[row*5 + col];
                    }
                }

                key = new string(temp);
            }

            public void Mutate() //at random, swaps two elements, rows or columns 
            {
                
                int temp = r.Next(100); //>75 swaps elements, >85 swaps rows, >95 swaps cols, otherwise reverse the key
                int i = 0;
                int j = 0;
                char[] keyArray = key.ToCharArray();
                char tempChar;//i could reuse int temp here and do ASCII conversions but I don't want to

                switch(temp)
                {
                    case <75: //by far the simplest
                        i = r.Next(25); //which characters are we swapping
                        j= r.Next(25);

                        if (i==j) { j = (j+1)%25; } //make sure you arent swapping the same character

                        tempChar = keyArray[i];
                        keyArray[i] = keyArray[j];
                        keyArray[j] = tempChar;

                        break;

                    case <85: //rows
                        i = r.Next(5);
                        j = r.Next(5);

                        if (i == j) { j = (j + 1) % 5; }

                        for (int col = 0; col <5; col++)
                        {
                            tempChar = keyArray[i * 5 + col];
                            keyArray[i*5+col] = keyArray[j*5+col];
                            keyArray[j * 5 + col] = tempChar;
                        }

                        break;

                    case <95: //cols
                        i = r.Next(5);
                        j = r.Next(5);

                        if (i == j) { j = (j + 1) % 5; }

                        for (int row = 0; row < 5; row++)
                        {
                            tempChar = keyArray[row * 5 + i];
                            keyArray[row * 5 + i] = keyArray[row * 5 + j];
                            keyArray[row*5 + j] = tempChar; 
                        }

                        break;
                    default: //reverse the key
                        Array.Reverse(keyArray);
                        break;
                }
                
                key = new string(keyArray);
            }

            private string SwapDigraphs(string text, int encrypt) //encrypt = 1 for encryption, -1 for decryption
            {
                string pair = "";
                int row0;
                int row1;
                int col0;
                int col1;

                row0 = key.IndexOf(text[0]) / 5;
                row1 = key.IndexOf(text[1]) / 5;
                col0 = key.IndexOf(text[0]) % 5;
                col1 = key.IndexOf(text[1]) % 5;

                if (col0 == col1) 
                {
                    pair = key[((row0+encrypt+5)%5) * 5 + col0].ToString() + key[((row1+encrypt+5)%5) * 5 + col1].ToString(); //shift down by one
                }
                else if (row0 == row1)
                {
                    pair = key[row0 * 5 + (col0 + encrypt + 5   ) % 5].ToString() + key[row1 * 5 + (col1 + encrypt + 5) % 5].ToString(); //shift right by one
                }
                else
                {
                    pair = key[row0 * 5 + col1].ToString() + key[row1 * 5 + col0].ToString(); //rectangle case
                }


                return pair;
            }

            public string Encrypt(string text) //does what it says on the lid
            {
                string ciphertext = "";

                for (int i = 0; i < text.Length; i+=2)
                {
                    ciphertext = string.Concat(ciphertext, SwapDigraphs(string.Concat(text[i], text[i+1]),1));
                }

                return ciphertext;
            }
            public string Decrypt(string ciphertext) //undoes what it says on the lid
            {
                string plaintext = "";

                for (int i = 0; i < ciphertext.Length; i+=2)
                {
                    plaintext = string.Concat(plaintext, SwapDigraphs(string.Concat(ciphertext[i], ciphertext[i+1]),-1));
                }

                return plaintext;
            }
        }

        public class Cipher
        {
            public string? ciphertext;
            public string? plaintext;
            private Key key;
            private myArrayQueue specialChars;

            public Cipher(string? ciphertext = null, string? plaintext = null, string? key = null) //you do not pass anything into the cipher unless it is being handled by the simulated annealing
            {
                specialChars = new myArrayQueue();
                if (ciphertext is not null) { this.ciphertext = ciphertext; }
                if (plaintext is not null) { this.plaintext = plaintext; }
                this.key = new Key(key); //this works even if string key is null
            }

            public Cipher Clone()
            {
                Cipher newCipher = new Cipher();
                newCipher.ciphertext = this.ciphertext;
                newCipher.plaintext = this.plaintext;
                newCipher.key = this.key.Clone(); //pass a copy not a reference
                newCipher.specialChars = this.specialChars.Clone();

                return newCipher;
            }

            public void SetKey(string key) //creates a new Key and overwrites the old one
            {
                string alphabet = "ABCDEFGHIKLMNOPQRSTUVWXYZ";

                key = key.ToUpper();
                string validKey = "";

                if (!Regex.IsMatch(key, @"^ [A - IK - Z] +$"))
                {
                    Console.WriteLine("The key contains invalid characters (non alphabetical or J). These will be removed");
                }

                if (key.Distinct().Count() != key.Length)
                {
                    Console.WriteLine("The key contains duplicate characters. These will be removed.");
                }

                if (key.Length != 25)
                {
                    Console.WriteLine("The key should be 25 characters long. Extra characters will be removed and missing characters will be filled in.");
                }

                foreach(char c in key) //grab all valid bits from the users key
                {
                    if (Regex.IsMatch(c.ToString(), @"^[A-IK-Z]+$") && !validKey.Contains(c)) //check its a valid character and not already in the key
                    {
                        validKey += c;
                    }
                }

                if (key.Length != 25) //if the key is the wrong length (the user did not input enough characters)
                {
                    foreach(char c in alphabet) 
                    {
                        if (!validKey.Contains(c)) //add all missing chars in alphabetical order
                        {
                            validKey += c;
                        }
                    }
                }

                Key newKey = new Key(validKey);

                this.key = null;

                this.key = newKey;

                Console.WriteLine("New key created");
                this.key.Output();
                Console.WriteLine();
                Console.WriteLine("Press ENTER to continue.");
                Console.ReadLine();
                Console.Clear();
            }

            public void SetPlainText(string plaintext)
            {
                this.specialChars.Reset(); 

                string cleantext = "";
                plaintext = plaintext.ToUpper();
                int pos = 0;

                bool jFlag = false; //has the user inputted text containing J

                foreach(char c in plaintext)
                {
                    if (c == 'J') //cipher can't handle J
                    {
                        cleantext += 'I';
                        jFlag = true;
                    }
                    else if(Regex.IsMatch(c.ToString(), @"^[A-Z]$")) //check if its an alphabetical character (J is already excluded)
                    {
                        cleantext += c;
                    }
                    else //c is a special character
                    {
                        specialChars.Enqueue(pos, c);
                    }
                    pos++;
                }

                //let the user know whats up
                if (jFlag) {
                    Console.WriteLine("We found occurrences of the letter J in your plaintext. Due to the nature of the Playfair Cipher, these have been removed and replaced with the letter I.");
                    Console.WriteLine();
                    Console.WriteLine("Press ENTER to continue.");
                    Console.ReadLine();
                    Console.Clear();
                }


                string paddedText = "";

                for (int i = 0; i < cleantext.Length; i+=2) //split up consecutive duplicate chars (eg "HELLO -> HELXLO" but HLLEO -> "HL LE OX" because LL is not a pair)
                {
                    paddedText += cleantext[i];

                    if (i+1 >=  cleantext.Length) { break; } //because I add 2, if cleantext.length is odd then I get indexOOB without this line

                    if (cleantext[i] == cleantext[i + 1])
                    {
                        paddedText += 'X';
                        i--;
                    }
                    else
                    {
                        paddedText += cleantext[i + 1];
                    }
                    
                }

                if (paddedText.Length%2 == 1) { paddedText += 'X'; } //pad text if not even in length
                this.plaintext = paddedText;
            }

            public void MutateKey() { this.key.Mutate(); }

            public void RandomiseKey() { this.key.Randomise(); }

            public void SetCipherText(string ciphertext) //takes user input until they enter valid ciphertext
            {
                bool valid = false;

                while (!valid) { 

                    this.specialChars.Reset();


                    if (ciphertext == "CANCEL")
                    {
                        Console.WriteLine("Cancelling change to ciphertext");
                        break;
                    }


                    valid = true;

                    bool flag = false;
                    string cleantext = "";
                    ciphertext = ciphertext.ToUpper();
                    int pos = 0;

                    foreach (char c in ciphertext) //remove any invalid chars
                    {
                        if (c == 'J') //cipher can't handle J
                        {
                            valid = false;
                            flag = true;
                            break;
                        }
                        else if (Regex.IsMatch(c.ToString(), @"^[A-Z]$")) //check if its an alphabetical character (J is already excluded)
                        {
                            cleantext += c;
                        }
                        else //c is a special character
                        {
                            specialChars.Enqueue(pos, c);
                        }
                        pos++;
                    }

                    if (flag) //text contained J
                    {
                        Console.WriteLine("Your ciphertext contains the letter J. With this implementation of the Playfair cipher, that is not possible. Are you sure you are entering ciphertext?");
                        Console.WriteLine();
                        Console.WriteLine("Please enter your ciphertext (or 'cancel' to cancel):");
                        ciphertext = Console.ReadLine().ToUpper();
                        Console.Clear();
                        continue;
                    }
                
                    

                    if (cleantext.Length % 2 == 1) //divine punishment unto stupid users
                    {
                        Console.WriteLine("The text you entered is an odd length. All ciphertext must be even in length. Please try again.");
                        Console.WriteLine();
                        Console.WriteLine("Enter your ciphertext, or 'cancel' to cancel:");
                        ciphertext = Console.ReadLine().ToUpper();
                        valid = false;
                        Console.Clear();
                        continue;
                    }

                    for (int i = 0; i < cleantext.Length; i+=2) //this only breaks if length is odd, which at this point it cannot be.
                    {
                        if (cleantext[i] == cleantext[i+1]) //check no repeat characters.
                        {
                            Console.WriteLine("Your ciphertext contains a repeated digraph. This will result in an error in decryption, as the cipher cannot handle this input.");
                            Console.WriteLine($"Characters are identical in positions {i} and {i+1}, both containing the character {ciphertext[i]}.");
                            Console.WriteLine();
                            Console.WriteLine("Enter your ciphertext, or 'cancel' to cancel:");
                            ciphertext = Console.ReadLine().ToUpper();
                            valid = false;
                            flag = true;
                            Console.Clear();
                            break;
                        }
                    }

                    if (flag) { continue; }

                    this.ciphertext = cleantext;
                }

            }

            public string GetPlainText() { return plaintext; } //can return null (check this)
            public string GetCipherText() { return ciphertext; } //can return null

            public void DisplayKey() { this.key.Output(); }

            public void CentreKey() { this.key.Centre(); }

            public string Encrypt()
            {
                if (this.plaintext is null)
                {
                    throw new Exception("Tried to encrypt without plaintext");
                }

                string decision = "n";
                Console.WriteLine("Do you want to put special characters back in your ciphertext? y/N");
                decision = Console.ReadLine().ToLower();
                
                if (decision == "y" && specialChars.isEmpty()) //divine punishment unto stupid users
                {
                    Console.WriteLine("No special characters are stored. Try re-entering your plaintext if there should be special characters in memory.");
                    decision = "n";
                }
                Console.Clear();

                string ciphertext = key.Encrypt(plaintext); //do the actual encryption (plaintext should have been cleaned upon entry)
                

                if (decision == "y")
                {
                    while (!specialChars.isEmpty()) //run until there are no more special characters
                    {
                        Tuple<int, char> nextChar = specialChars.Dequeue();

                        string firstHalf = ciphertext.Substring(0, nextChar.Item1); //get the half of the text before the character
                        string lastHalf = ciphertext.Substring(nextChar.Item1, ciphertext.Length - nextChar.Item1); //get the half of the text after the character

                        ciphertext = string.Concat(firstHalf, nextChar.Item2, lastHalf); //put the character in the text
                    }
                    specialChars.Reset(); //leave it "ready for use"
                    Console.WriteLine("Special characters returned to ciphertext.");
                }

                return ciphertext;
            }

            public string Decrypt(bool human) //takes input of whether or not a human is calling this function (for simulated annealing purposes)
            {
                if (this.ciphertext is null)
                {
                    throw new Exception("Tried to decrypt with null ciphertext");
                }


                string decision = "n";
                if (human)
                {
                    Console.WriteLine("Do you want to put special characters back in your plaintext? y/N");
                    decision = Console.ReadLine().ToLower();

                    if (decision == "y" && specialChars.isEmpty()) //divine punishment unto stupid users
                    {
                        Console.WriteLine("No special characters are stored. Try re-entering your ciphertext if you were expecting special characters in memory.");
                        decision = "n";
                    }
                    Console.Clear();
                }

                string plaintext = key.Decrypt(ciphertext);
                

                if (decision == "y") //is skipped if not human
                {
                    while (!specialChars.isEmpty()) //run until there are no more special characters
                    {
                        Tuple<int, char> nextChar = specialChars.Dequeue();

                        string firstHalf = plaintext.Substring(0, nextChar.Item1); //get the half of the text before the character
                        string lastHalf = plaintext.Substring(nextChar.Item1, plaintext.Length - nextChar.Item1); //get the half of the text after the character

                        plaintext = string.Concat(firstHalf, nextChar.Item2, lastHalf); //put the character in the text
                    }
                    specialChars.Reset(); //leave it "ready for use"
                    Console.WriteLine("Special characters returned to plaintext.");
                }

                return plaintext;
            }
        }

        public struct annealingSettings
        {
            public bool notifyNewBest {  get; set; }
            public bool notifyNewGlobalBest { get; set; }
            public bool notifyAcceptance { get; set;  }
            public bool notifyRestart {  get; set; }
            public bool showKey { get; set; }
            public bool showDecrypted {  get; set; }
            public double T {  get; set; }
            public double cooling {  get; set; }
            public int iterations { get; set;  }
            public int restarts { get; set; }
            public int analysisType { get; set; } //0 for bigram, 1 for trigram, 2 for quadgram
            public annealingSettings() //my default settings
            { 
                notifyNewBest = false;
                notifyNewGlobalBest = true;
                notifyAcceptance = false;
                notifyRestart = true;
                showKey = false;
                showDecrypted = false;
                T = 20;
                cooling = 0.00005;
                iterations = 50000;
                restarts = 50;
                analysisType = 2;
            }

        }

        static void Main(string[] args) 
        {
            Cipher cipher = new Cipher();
            NGramAnalysis banalysis = new BigramAnalysis();
            NGramAnalysis tanalysis = new TrigramAnalysis();
            NGramAnalysis qanalysis = new QuadgramAnalysis();

            annealingSettings settings = new annealingSettings();

            string decision = "";
            bool inMenu = true;

            while(true) //main menu
            {
                Console.WriteLine("Welcome to the Playfair Cipher tool");
                Console.WriteLine();
                Console.WriteLine("Type:");
                Console.WriteLine("1 for cipher functions");
                Console.WriteLine("2 for settings");
                Console.WriteLine("3 to quit");
                decision = Console.ReadLine();

                Console.Clear();

                switch(decision) //evil nest pile of doom and despair
                {
                    case "1":
                        inMenu = true;
                        while (inMenu) //cipher functions menu
                        {
                            Console.WriteLine("Cipher functions");
                            Console.WriteLine();
                            Console.WriteLine("Type:");
                            Console.WriteLine("1 to set the key");
                            Console.WriteLine("2 to set plaintext");
                            Console.WriteLine("3 to set ciphertext");
                            Console.WriteLine("4 to encrypt the plaintext");
                            Console.WriteLine("5 to decrypt the ciphertext");
                            Console.WriteLine("6 to randomise the key");
                            Console.WriteLine("7 to crack the cipher via simulated annealing");
                            Console.WriteLine("8 to see the fitness of the plaintext");
                            Console.WriteLine("9 to centre the key");
                            Console.WriteLine("0 to return to the main menu");
                            decision = Console.ReadLine();
                            Console.Clear();

                            switch(decision)
                            {
                                case "1": //set key
                                    Console.WriteLine("You are setting the key. If you input something invalid, the software will modify your input to form a valid key.");
                                    Console.WriteLine("Enter your key, or write 'cancel' to cancel");
                                    decision = Console.ReadLine().ToUpper(); //using this as string storage 
                                    Console.Clear();

                                    if (decision == "CANCEL") { break; }
                                    else
                                    {
                                        cipher.SetKey(decision);
                                    }

                                    break;
                                case "2": //set plaintext
                                    Console.WriteLine("You are setting the plaintext. If you enter something invalid, the program will remove invalid characters");
                                    Console.WriteLine("You will have the option to store non alphabetical characters you input, for later");
                                    Console.WriteLine("Padding may occur if necessary, with the character 'X'");
                                    Console.WriteLine();
                                    Console.WriteLine("Enter your plaintext, or 'cancel' to cancel");
                                    decision = Console.ReadLine().ToUpper();
                                    Console.Clear();

                                    if (decision == "CANCEL") { break; }
                                    else
                                    {
                                        cipher.SetPlainText(decision);
                                    }

                                    Console.WriteLine("The new plaintext is:");
                                    Console.WriteLine(cipher.GetPlainText());
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();

                                    break;
                                case "3": //set ciphertext
                                    Console.WriteLine("You are setting the ciphertext. You may be required to repeat your input if it is invalid.");
                                    Console.WriteLine("The ciphertext must be even in length and must not contain repeat digraphs");
                                    Console.WriteLine("Ciphertext CAN contain non alphabetical characters, which you will have the option to preserve");
                                    Console.WriteLine();
                                    Console.WriteLine("Enter your ciphertext, or 'cancel' to cancel");
                                    decision = Console.ReadLine().ToUpper();
                                    Console.Clear();

                                    if (decision == "CANCEL") { break; }
                                    else
                                    {
                                        cipher.SetCipherText(decision);
                                    }

                                    Console.WriteLine("The new ciphertext is:");
                                    Console.WriteLine();
                                    Console.WriteLine(cipher.GetCipherText());
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();

                                    break;
                                case "4": //encrypt
                                    if (cipher.GetPlainText() is null)
                                    {
                                        Console.WriteLine("Cannot encrypt null plaintext. Please input plaintext and try again.");
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();
                                        break;
                                    }

                                    Console.WriteLine(cipher.Encrypt());
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();
                                    break;
                                case "5": //decrypt

                                    if (cipher.GetCipherText() is null)
                                    {
                                        Console.WriteLine("Cannot decrypt null ciphertext. Please input ciphertext and try again.");
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();
                                        break;
                                    }

                                    Console.WriteLine(cipher.Decrypt(true));
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();
                                    break;
                                case "6": //randomise
                                    cipher.RandomiseKey();
                                    Console.WriteLine("Key randomised. New key:");
                                    cipher.DisplayKey();
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();
                                    break;
                                case "7": //annealing
                                    if (cipher.GetCipherText() is null)
                                    {
                                        Console.WriteLine("The software cannot crack null ciphertext. Please enter ciphertext then try again.");
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();
                                    }
                                    else
                                    {
                                        if (cipher.GetCipherText().Length < 200)
                                        {
                                            Console.WriteLine("WARNING: your ciphertext is quite short. Simulated annealing may not be able to correctly decrypt this.");
                                            Console.WriteLine();
                                            Console.WriteLine("Press ENTER to continue.");
                                            Console.ReadLine();
                                            Console.Clear();
                                        }
                                        if (cipher.GetCipherText().Length > 2000)
                                        {
                                            Console.WriteLine("Your ciphertext is very long. It may take a while to crack.");
                                            Console.WriteLine();
                                            Console.WriteLine("Press ENTER to continue.");
                                            Console.ReadLine();
                                            Console.Clear();
                                        }

                                        Console.WriteLine("Your settings will be passed to the annealing function.");
                                        Console.WriteLine("The key currently set in your cipher will be overwritten and replaced with the correct key for the ciphertext.");
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();

                                        cipher = SimulatedAnnealing(cipher, settings);

                                        Console.Clear(); //clear anything that was written by annealing (it can cause a visual mess)

                                        Console.WriteLine("Simulated annealing is complete");
                                        Console.WriteLine("Here is the plaintext: ");
                                        Console.WriteLine(cipher.Decrypt(false));
                                        Console.WriteLine();
                                        Console.WriteLine();
                                        Console.WriteLine("Here is the valid key:");
                                        cipher.DisplayKey();
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();
                                    }
                                    break;
                                case "8": //fitness

                                    if (cipher.GetPlainText() is null)
                                    {
                                        Console.WriteLine("Cannot evaluate fitness of null plaintext. Please set plaintext and try again.");
                                        Console.WriteLine();
                                        Console.WriteLine("Press ENTER to continue.");
                                        Console.ReadLine();
                                        Console.Clear();
                                        break;
                                    }
                                    inMenu = true;

                                    while (inMenu)
                                    {
                                        Console.WriteLine("The closer to 0 the fitness is, the better.");
                                        Console.WriteLine("The magnitude of the fitness is related more closely to the length of the text than its actual fitness, but this does not affect annealing.");
                                        Console.WriteLine("Select fitness function: ");
                                        Console.WriteLine("Type: ");
                                        Console.WriteLine("1 for bigram frequency analysis");
                                        Console.WriteLine("2 for trigram frequency analysis");
                                        Console.WriteLine("3 for quadgram frequency analysis");
                                        Console.WriteLine("4 to cancel");

                                        decision = Console.ReadLine();
                                        Console.Clear();

                                        inMenu = false;
                                        switch(decision)
                                        {
                                            case "1":
                                                Console.WriteLine("The fitness of the plaintext given by bigram frequency analysis is:");
                                                Console.WriteLine(banalysis.GetScore(cipher.GetPlainText()));
                                                Console.WriteLine();
                                                Console.WriteLine("Press ENTER to continue.");
                                                Console.ReadLine();
                                                Console.Clear();
                                                break;
                                            case "2":
                                                Console.WriteLine("The fitness of the plaintext given by trigram frequency analysis is:");
                                                Console.WriteLine(tanalysis.GetScore(cipher.GetPlainText()));
                                                Console.WriteLine();
                                                Console.WriteLine("Press ENTER to continue.");
                                                Console.ReadLine();
                                                Console.Clear();
                                                break;
                                            case "3":
                                                Console.WriteLine("The fitness of the plaintext given by quadgram frequency analysis is:");
                                                Console.WriteLine(qanalysis.GetScore(cipher.GetPlainText()));
                                                Console.WriteLine();
                                                Console.WriteLine("Press ENTER to continue.");
                                                Console.ReadLine();
                                                Console.Clear();
                                                break;
                                            case "4":
                                                break;
                                            default:
                                                inMenu = true;
                                                Console.WriteLine("Please input a valid option");
                                                Console.WriteLine();
                                                Console.WriteLine("Press ENTER to continue.");
                                                Console.ReadLine();
                                                Console.Clear();
                                                break;
                                        }
                                    }
                                    inMenu = true;
                                    break;
                                case "9": //centre
                                    cipher.CentreKey();
                                    Console.WriteLine("The centred key is mathematically identical to before, but oriented using the keys translational symmetry to put A in position 0");
                                    Console.WriteLine("Here is the centred key: ");
                                    Console.WriteLine();
                                    cipher.DisplayKey();
                                    Console.WriteLine();
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();
                                    break;
                                case "0": //return
                                    inMenu = false;
                                    break;
                                default:
                                    Console.WriteLine("That isn't a valid option");
                                    break;
                            }

                        }
                        break;

                    case "2":

                        inMenu = true;

                        while (inMenu)
                        {
                            Console.WriteLine("Settings");
                            Console.WriteLine("");
                            Console.WriteLine("Type:");
                            Console.WriteLine();
                            Console.WriteLine("1 for analysis settings");
                            Console.WriteLine("2 for output settings");
                            Console.WriteLine("3 for annealing settings");
                            Console.WriteLine("4 to return to the main menu");

                            decision = Console.ReadLine();
                            Console.Clear();

                            switch(decision)
                            {
                                case "1": //analysis
                                    Console.WriteLine("Analysis settings. Please note that quadgram analysis has been shown to be optimal by testing");
                                    Console.WriteLine();
                                    Console.WriteLine("Type:");
                                    Console.WriteLine("1 for bigram analysis");
                                    Console.WriteLine("2 for trigram analysis");
                                    Console.WriteLine("3 for quadgram analysis");
                                    Console.WriteLine("anything else to cancel");
                                    decision = Console.ReadLine();
                                    Console.Clear();

                                    switch(decision)
                                    {
                                        case "1":
                                            settings.analysisType = 0;
                                            break;
                                        case "2":
                                            settings.analysisType = 1;
                                            break;
                                        case "3":
                                            settings.analysisType = 2;
                                            break;
                                        default:
                                            break;
                                    }

                                    break;
                                case "2": //notifications
                                    Console.WriteLine("Output settings.");
                                    Console.WriteLine("Please note that frequent outputs will significantly reduce performance.");
                                    Console.WriteLine();
                                    Console.WriteLine("Type:");
                                    Console.WriteLine("1 for outputs when a new best score is found");
                                    Console.WriteLine("2 for outputs when a new global best score is found");
                                    Console.WriteLine("3 for outputs when a new score is accepted");
                                    Console.WriteLine("4 for outputs when the process restarts");
                                    Console.WriteLine("5 to output the current plaintext with notifications");
                                    Console.WriteLine("6 to output the current key with notifications");
                                    Console.WriteLine("anything else to cancel");

                                    decision = Console.ReadLine();
                                    Console.Clear();

                                    switch(decision)
                                    {
                                        case "1":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("notify when a new best score is found.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.notifyNewBest = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.notifyNewBest = false;
                                            }

                                            break;
                                        case "2":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("notify when a new global best score is found.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.notifyNewGlobalBest = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.notifyNewGlobalBest = false;
                                            }
                                            break;
                                        case "3":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("notify when a score is accepted.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.notifyAcceptance = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.notifyAcceptance = false;
                                            }
                                            break;
                                        case "4":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("notify when the process restarts.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.notifyRestart = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.notifyRestart = false;
                                            }
                                            break;
                                        case "5":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("output the current plaintext when other information is displayed.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.showDecrypted = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.showDecrypted = false;
                                            }
                                            break;
                                        case "6":
                                            Console.Write("The software currently does ");
                                            if (!settings.notifyNewBest) { Console.Write("not "); }
                                            Console.WriteLine("show the key when other information is displayed.");
                                            Console.WriteLine("Type:");
                                            Console.WriteLine("1 to turn ON output for this category");
                                            Console.WriteLine("2 to turn OFF output for this category");
                                            Console.WriteLine("anything else to cancel");
                                            decision = Console.ReadLine();
                                            Console.Clear();

                                            if (decision == "1")
                                            {
                                                settings.showKey = true;
                                            }
                                            if (decision == "2")
                                            {
                                                settings.showKey = false;
                                            }
                                            break;
                                    }

                                    break;
                                case "3": //annealing
                                    Console.WriteLine("Annealing settings.");
                                    Console.WriteLine("WARNING: these are very easy to break. I believe I have found the optimal settings, changing these settings may prevent the code from working as intended.");
                                    Console.WriteLine();
                                    Console.WriteLine("Type:");
                                    Console.WriteLine("1 to change initial temperature");
                                    Console.WriteLine("2 to change cooling");
                                    Console.WriteLine("3 to change number of iterations");
                                    Console.WriteLine("4 to change number of restarts");
                                    Console.WriteLine("anything else to cancel");
                                    decision = Console.ReadLine();
                                    Console.Clear();

                                    bool validInput = false;

                                    switch(decision)
                                    {
                                        case "1":

                                            while(!validInput)
                                            {
                                                Console.WriteLine("The current initial temperature is " + settings.T);
                                                Console.WriteLine("Enter a positive real number to overwrite, or 0 to cancel");
                                                decision = Console.ReadLine();
                                                Console.Clear();

                                                double temp = 10; //temp as in temporary not temperature
                                                if (!double.TryParse(decision, out temp) || temp < 0)
                                                {
                                                    Console.WriteLine("Please input a valid number");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    continue;
                                                }
                                                else if (temp == 0)
                                                {
                                                    validInput = true;
                                                    continue;
                                                }
                                                else
                                                {
                                                    settings.T = temp;
                                                    Console.WriteLine("Initial temperature updated");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    validInput = true;
                                                    continue;
                                                }

                                            }

                                            break;
                                        case "2":
                                            while (!validInput)
                                            {
                                                Console.WriteLine("The current cooling is " + settings.cooling);
                                                Console.WriteLine("Enter a real number between 0 and 1 (exclusive) to overwrite, or 0 to cancel");
                                                decision = Console.ReadLine();
                                                Console.Clear();

                                                double temp = 10; //temp as in temporary not temperature
                                                if (!double.TryParse(decision, out temp) || temp < 0 || temp >= 1)
                                                {
                                                    Console.WriteLine("Please input a valid number");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    continue;
                                                }
                                                else if (temp == 0)
                                                {
                                                    validInput = true;
                                                    continue;
                                                }
                                                else
                                                {
                                                    settings.cooling = temp;
                                                    Console.WriteLine("Cooling updated");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    validInput = true;
                                                    continue;
                                                }

                                            }
                                            break;
                                        case "3":
                                            while (!validInput)
                                            {
                                                Console.WriteLine("The current iteration count is " + settings.iterations);
                                                Console.WriteLine("Enter a positive integer to overwrite, or 0 to cancel");
                                                decision = Console.ReadLine();
                                                Console.Clear();

                                                int temp = 10; //temp as in temporary not temperature
                                                if (!int.TryParse(decision, out temp) || temp < 0)
                                                {
                                                    Console.WriteLine("Please input a valid number");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    continue;
                                                }
                                                else if (temp == 0)
                                                {
                                                    validInput = true;
                                                    continue;
                                                }
                                                else
                                                {
                                                    settings.iterations = temp;
                                                    Console.WriteLine("iteration count updated");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    validInput = true;
                                                    continue;
                                                }

                                            }
                                            break;
                                        case "4":
                                            while (!validInput)
                                            {
                                                Console.WriteLine("The current restart number is " + settings.restarts);
                                                Console.WriteLine("Enter a positive integer to overwrite, or 0 to cancel");
                                                decision = Console.ReadLine();
                                                Console.Clear();

                                                int temp = 10; //temp as in temporary not temperature
                                                if (!int.TryParse(decision, out temp) || temp < 0)
                                                {
                                                    Console.WriteLine("Please input a valid number");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    continue;
                                                }
                                                else if (temp == 0)
                                                {
                                                    validInput = true;
                                                    continue;
                                                }
                                                else
                                                {
                                                    settings.restarts = temp;
                                                    Console.WriteLine("iteration count updated");
                                                    Console.WriteLine();
                                                    Console.WriteLine("Press ENTER to continue.");
                                                    Console.ReadLine();
                                                    validInput = true;
                                                    continue;
                                                }

                                            }
                                            break;
                                    }
                                    Console.Clear();
                                    break;
                                case "4":
                                    inMenu = false;
                                    break;
                                default:
                                    Console.WriteLine("Please enter a valid option");
                                    Console.WriteLine();
                                    Console.WriteLine("Press ENTER to continue.");
                                    Console.ReadLine();
                                    Console.Clear();
                                    break;
                            }
                        }
                        inMenu = true;
                        break;

                    case "3":
                        Environment.Exit(0);
                        break;
                    default:
                        Console.WriteLine("That isn't a valid option");
                        Console.WriteLine("Press ENTER to continue");
                        Console.ReadLine();
                        Console.Clear();
                        break;
                }
            }
        }
        
        static Cipher SimulatedAnnealing(Cipher initial, annealingSettings settings)
        {
            NGramAnalysis analysis;

            switch(settings.analysisType)
            {
                case 0:
                    analysis = new BigramAnalysis();
                    break;
                case 1:
                    analysis = new TrigramAnalysis();
                    break;
                case 2:
                    analysis = new QuadgramAnalysis();
                    break;
                default:
                    throw new Exception("invalid analysis type");
            }
            
            
            
            Cipher globalBestCipher = initial.Clone();

            
            double globalBestScore = analysis.GetScore(initial.Decrypt(false));


            for (int restarts = 0; restarts < settings.restarts; restarts++)
            {
                Cipher bestCipher = initial.Clone();
                bestCipher.RandomiseKey();
                Cipher currentCipher = bestCipher.Clone();

                double bestScore = analysis.GetScore(bestCipher.Decrypt(false));
                double currentScore = bestScore;

                double T = settings.T; //the temperature
                double cooling = settings.cooling; //quite slow cooling
                int iterations = settings.iterations;
                int lastImprov = 0;



                for (int it = 0; it < iterations; it++)
                {
                    Cipher newCipher = currentCipher.Clone();
                    globalBestCipher.MutateKey();

                    string plaintext = globalBestCipher.Decrypt(false);
                    double newScore = analysis.GetScore(plaintext);

                    double change = newScore - currentScore;

                    if (change > 0 || r.NextDouble() < Math.Exp(change / T))
                    {
                        currentCipher = globalBestCipher.Clone();
                        currentScore = newScore;

                        if (settings.notifyAcceptance)
                        {
                            Console.WriteLine($"New key accepted. New score: {newScore}, previous score: {bestScore}. Iteration {it} res {restarts}");
                            if (settings.showDecrypted)
                            {
                                Console.WriteLine(globalBestCipher.Decrypt(false));
                            }
                            if (settings.showKey)
                            {
                                globalBestCipher.DisplayKey();
                            }
                        }


                        if (newScore > bestScore)
                        {
                            //Console.WriteLine($"new best score: {newScore} at iteration {it}");

                            if (settings.notifyNewBest && !settings.notifyAcceptance) //dont ping twice
                            {
                                Console.WriteLine($"New best score: {newScore} on iteration {it}, restart {restarts}. previous score: {bestScore}");
                                if (settings.showDecrypted)
                                {
                                    Console.WriteLine(globalBestCipher.Decrypt(false));
                                }
                                if (settings.showKey)
                                {
                                    globalBestCipher.DisplayKey();
                                }
                            } 

                            bestCipher = globalBestCipher.Clone();
                            bestScore = newScore;

                            lastImprov = 0;
                        }

                    }

                    lastImprov++;
                    /*
                    if (lastImprov > 10000)
                    {
                        T = 7;
                        lastImprov = 0;
                        Console.WriteLine($"No better solution found, reheating at iteration {it}");
                    }*/

                    T *= 1 - cooling; //cooling
                    if (T < 1) { T = 1; }

                }
                
                if (bestScore > globalBestScore)
                {

                    if (bestScore/globalBestScore < 0.8)
                    {
                        Console.WriteLine("I believe this is english. ending early");
                        return bestCipher;
                    }

                    globalBestScore = bestScore;
                    globalBestCipher = bestCipher.Clone();
                    
                    if(settings.notifyNewGlobalBest || settings.notifyRestart)
                    {
                        Console.WriteLine($"new global best score: {globalBestScore}, at the end of restart {restarts}");
                        if (settings.showDecrypted)
                        {
                            Console.WriteLine(globalBestCipher.Decrypt(false));
                        }
                        if (settings.showKey)
                        {
                            globalBestCipher.DisplayKey();
                        }
                    }
                    
                }
                else if(settings.notifyRestart) //if there isn't a new global best
                {
                    Console.WriteLine($"finished restart {restarts}. No new global best found, highest score: {bestScore}. Global best score: {globalBestScore}");
                    if (settings.showDecrypted)
                    {
                        Console.WriteLine(globalBestCipher.Decrypt(false));
                    }
                    if (settings.showKey)
                    {
                        globalBestCipher.DisplayKey();
                    }
                }
            }
            return globalBestCipher;

        }
    }
}
