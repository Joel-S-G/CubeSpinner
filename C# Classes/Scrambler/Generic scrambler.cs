using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Validation;



namespace CubeSpinner
{
    public abstract class GenericScrambler //generic scrambler class that contains a method for generating a random set of moves from a list
    {
        public Random random = new Random();

        
        //abstract - use override to add in own code for each given cube
        protected abstract int GetScrambleLen(); //returns the length of the move list
        protected abstract List<string> GetMoveList(); //contains a list of moves

        protected virtual List<string> GetMoveSuffixes() //determines if a face is moved, once clockwise, anticlockwise or twice(anticlockwise) + override as mega can be turned more
        {
            return new List<string>{"", "'", "2"};
        }

        protected virtual bool ValidCheck(List<string> currentscramble, string nextmove) //by defult will return true, the validity checks for NxNxN cubes is largely the same but for non cube is different - use override
        {
            return true;
        }
        
        public string ScrambleGen() //scramble generator
        {
            List<String> moves = GetMoveList();
            List<string> suffixes = GetMoveSuffixes();
            int length = GetScrambleLen();
            StringBuilder scramble = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                string move = moves[random.Next(moves.Count)];
                string suffix = suffixes[random.Next(suffixes.Count)];

                string collection = move + suffix;

                scramble.Append(collection);
            }
            
            return scramble.ToString();
        }
    }
}