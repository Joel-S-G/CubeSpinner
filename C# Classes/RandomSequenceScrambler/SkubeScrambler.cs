using System;

namespace CubeSpinner
{
    public class SkubeScrambler : GenericScrambler
    {
        protected override List<string> GetMoveList()
        {
            return new List<string> {"U", "L", "R", "B" };
        }

        protected override int GetScrambleLen()
        {
            return random.Next(18, 25);
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