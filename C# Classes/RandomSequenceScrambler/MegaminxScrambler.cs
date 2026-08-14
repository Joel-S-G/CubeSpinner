using System;

namespace CubeSpinner
{
    public class MegaScrambler : GenericScrambler
    {
        protected override List<string> GetMoveList()
        {
            return new List<string> {"U","U'", "R++", "R--", "D++", "D--"};
        }

        protected override int GetScrambleLen()
        {
            return random.Next(79, 100);
        }

        protected override List<string> GetMoveSuffixes()
        {
            return new List<string> {""};
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
