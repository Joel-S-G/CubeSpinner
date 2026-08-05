using System;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using Microsoft.Extensions.Validation;

namespace CubeSpinner
{
    public class ScrambleGen
    {
         
        public List<string> moves3x3or2x2 = new List<string> {"L", "R", "F", "B","U", "D"}; //for 2x2x2 and 3x3x3       
        public List<string> movessuffixNxN = new List<string> {"L", "R", "F", "B", "U", "D", "Lw", "Rw", "Fw", "Bw", "Uw", "Dw"}; //for 4x4x4 and greater NxNxN cubes
        public List<string> prefix5x5 = new List<string> {"", "1", "2"}; //refers to nummber of layers moved 5x5
        public List<string> prefix6x6 = new List<string> {"", "1", "2","3"}; //for 6x6
        public List<string> prefix7x7 = new List<string> {"", "1", "2", "3", "4"}; //for 7x7
        public List<string> directionSuffix = new List<string> {" ", "'", "2"}; // moves for cube twisty puzzles
                
        
        Random random = new Random();

        public enum puzzle
        {
            Cube2x2,
            Cube3x3,
            Cube4x4,
            Cube5x5,
            Cube6x6,
            Cube7x7,
            Megaminx,
            Pyraminx,
            Skube

        }
        
        
         public string? Scramble_Gen_Cube()
         {
            int length = random.Next(20,31);
            StringBuilder scramble = new StringBuilder();

            for (int i  = length; i > 0; i--)
            {
                int moveIndex = random.Next(0, moves3x3or2x2.Count);
                int directionIndex = random.Next(0, directionSuffix.Count);
                string collection = moves3x3or2x2[moveIndex] + directionSuffix[directionIndex];                           

                scramble.Append(collection);
                
            }

            return scramble.ToString();       
         }

        public void Validate()
        {
            
        }
         
            
         


        
    }
}