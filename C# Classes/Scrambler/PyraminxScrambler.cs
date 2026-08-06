using System;

namespace CubeSpinner
{
    public class PyraScrambler : GenericScrambler
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
            return new List<string> {"", "'", "2"};
        }

        protected override bool ValidCheck(List<string> currentscramble, string nextmove)
        {
            return base.ValidCheck(currentscramble, nextmove);
        }
    }
}