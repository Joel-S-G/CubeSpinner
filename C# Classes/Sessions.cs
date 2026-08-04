// format solve -> newSolve append-> List solves


using System;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace CubeSpinner
{
    class Session
    {
        
        public bool isActive;
        public string? name { get; set;}
        public required List<SolveRecord> solves;
        public string? CubeType{get; set;}
                    
        
        public List<string> FormatData() // outputs data in form Session(number/name)[{penaltyStatus}, (time in ms), "scramble", "dateSolved"]
        {
                       
            List<string> formattedOutput = new();


            foreach (SolveRecord solve in solves) //iterates through solves and formats them
            {
                formattedOutput.Add($"{solve.penaltyStatus}, {solve.FinalTime?.TotalMilliseconds}ms, {solve.Scramble}, {solve.SolvedAt}");
            } 

            return formattedOutput;
        }

        public List<SolveRecord> SortbyTime()
        {
            return solves   
                .OrderBy(solves => solves.FinalTime ?? TimeSpan.MaxValue)
                .ToList();
        }

        public List<SolveRecord> SortbyDate()
        {
            return solves   
                .OrderByDescending(solves => solves.SolvedAt)
                .ToList();
        }

        public void AddSolve(SolveRecord solve)
        {
            if (solves ==null) //creates a new list if empty
            {
                solves = new List<SolveRecord>();
            }
            
            solves.Add(solve);
            
        }

        public void RemoveSolve(SolveRecord solve, Guid solveId)
        {
            for (int i = solves.Count -1; i > 0; i--)   //iterates through list to find the solve and then removes it at that index
            {
                if (solves[i].SolveID ==solveId)
                {
                    solves.RemoveAt(i);
                    return;
                }
            }
        }

        public SolveRecord? GetSolve(Guid solveID)
        {
            for (int i = solves.Count - 1; i > 0; i--) // basically the same as RemoveSolve except it doesn't remove the solve at the index
            {
                if (solves[i].SolveID == solveID)
                {
                     return solves[i];
                }
            }   

            return null;
        }

        public SolveRecord? GetBestSolve()
        {
            var validsolve = solves
                            .Where(solve => solve.FinalTime != null); //ensures the solve is valid by making sure that it is not null (DNF)
            var lowestTime = validsolve
                            .Where(solve => solve.FinalTime != null)  //solve doesn't = null, removes DNFs
                            .OrderBy(solves => solves.FinalTime!.Value) // orders values 
                            .FirstOrDefault(); //returns lowest value (or null if list is empty), this will return the value lowest in the list and by extension will have all the properties asscociated (e.g. the id, scramble, finaltime etc.)
                      
            return lowestTime;
        
        }

    
    }
}