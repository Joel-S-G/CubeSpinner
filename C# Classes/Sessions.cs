// format solve -> newSolve append-> List solves


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
        public record newSolve;


        
        
        public List<string> FormatData() // outputs data in form Session(number/name)[{penaltyStatus}, (time in ms), "scramble", "dateSolved"]
        {
                       
            List<string> formattedOutput = new();


            foreach (SolveRecord solve in solves) //iterates through solves and formats them
            {
                formattedOutput.Add($"{solve.penaltyStatus}, {solve.FinalTime?.TotalMilliseconds}ms, {solve.Scramble}, {solve.SolvedAt}");
            } 

            return formattedOutput;
        }
        

        /*public void ExportSolve(SolveRecord solve) // allows the export of solves in the class
        {
            return;
        }*/

        public void AddSolve()
        {

            
        }

        public void RemoveSolve(Guid solveId)
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