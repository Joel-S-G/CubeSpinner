using System;
using System.Runtime.CompilerServices;

namespace CubeSpinner
{
    class Session
    {
        
        public bool isActive;
        public string? name { get; set;}
        public List<SolveRecord> solves;
        public string? CubeType{get; set;}

        
        
        public void FormatData() // outputs data in form Session(number/name)[{penaltyStatus}, (time in ms), "scramble", "dateSolved"]
        {
            return;
        }

        public void ExportSolve() // allows the export of solves in the class
        {
            return;
        }

        public void AddSolve()
        {
            return;
        }

        public void RemoveSolve()
        {
            return;
        }

        public void GetSolve()
        {
            return;
        }

        public void GetBestSolve()
        {
            return;
        }
    }
}