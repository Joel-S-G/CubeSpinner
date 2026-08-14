using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Validation;
using System.Linq;  


namespace CubeSpinner
{
    public abstract class GenericScrambler //generic scrambler class that contains a method for generating a random set of moves from a list
    {
        public Random random = new Random();

        public string GenerateScramble()
        {
            return ScrambleGen();
        }
        
        //abstract - use override to add in own code for each given cube
        protected abstract int GetScrambleLen(); //returns the length of the move list
        protected abstract List<string> GetMoveList(); //contains a list of moves

        protected virtual List<string> GetMoveSuffixes() //determines if a face is moved, once clockwise, anticlockwise or twice(anticlockwise) + override as mega can be turned more
        {
            return new List<string>{"", "'", "2"};
        }

        protected virtual bool ValidCheck( List<string> scramble, string potentialMove) //by defult will return true, the validity checks for NxNxN cubes is largely the same but for non cube is different - use override
        {
            
            if (scramble.Count == 0) //at the start there will be nothing in the list so it will always be valid
            {
                return true;
            }

            char prevTurn = scramble[scramble.Count - 1][0];
            char nextTurn = potentialMove[0];

            return prevTurn != nextTurn;   
        
        }
        
        protected virtual string ScrambleGen() //scramble generator
        {
            List<String> moves = GetMoveList();
            List<string> suffixes = GetMoveSuffixes();
            int length = GetScrambleLen();
            List<string> scramble = new List<string>();

            for (int i = 0; i < length; i++)
            {
               List<string> validMoves = moves
                .Where(move => ValidCheck(scramble, move))
                .ToList();

               string move = validMoves[random.Next(validMoves.Count)]; 
               string suffix = suffixes[random.Next(suffixes.Count)];

               scramble.Add(move + suffix);
        
            }
            
            return string.Join(" ", scramble);
        }
    }
}