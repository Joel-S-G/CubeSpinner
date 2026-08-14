using System;

namespace CubeSpinner
{
    public class PyraScrambler : GenericScrambler
    { 
        protected override List<string> GetMoveList()
        {
            return new List<string> {"U", "L", "R", "B" };
        }

        public List<string> GetTipMoves()
        {
            return new List<string>{"u", "l", "r", "b"};
        }

        protected override int GetScrambleLen()
        {
            return random.Next(18, 25);
        }

        protected override List<string> GetMoveSuffixes()
        {
            return new List<string> {"", "'"}; // doing 2 moves in one direction on pyra is the same as doing a prime move on the same face
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