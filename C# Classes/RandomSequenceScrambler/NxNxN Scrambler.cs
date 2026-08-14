using System;

namespace CubeSpinner
{
    public class NxNxN_Scrambler : GenericScrambler
    {
       
        public int CubeSize { get; set; } //refers to the dimesions of the cube
 
        public NxNxN_Scrambler(int cubeSize = 3) //defults to 3x3x3
        {
            CubeSize = cubeSize;
        }

        public List<string> GetFaces()
        {
          return new List<string>{"U", "D", "L", "R", "F", "B"};   
        }

       
       
        protected override List<string> GetMoveList()
        {
            List<string> moves = new List<string>{"U", "D", "L", "R", "F", "B"};

            if (CubeSize >= 4)
            {
                moves.AddRange(new List<string>{"Uw", "Dw", "Lw", "Rw", "Fw", "Bw"} ); //wide moves for 4x4 and greater
            }

            for(int depth = 3; depth <= CubeSize/2; depth++) //adds further wide turns for larger N cubes (4x4 and greater)
            {
                foreach (string face in GetFaces() )
                {
                    moves.Add($"{depth}{face}w");
                }
            }

            return moves;

        }


        protected override int GetScrambleLen()
        {
            return CubeSize switch
            {
                <= 3 => random.Next(18, 22),
                4 => random.Next(40, 46),
                5 => random.Next(60, 71),
                6 => random.Next(80, 91),
                _ => random.Next(100, 111)
            };

            
        }

        protected override List<string> GetMoveSuffixes()
        {
            return new List<string> {"", "'"};
        }

        protected override bool ValidCheck(List<string> scramble, string potentialMove)
        {
            return base.ValidCheck(scramble, potentialMove);
        }

        protected override string ScrambleGen()
        {
            return base.ScrambleGen();
        
        }
    }
}