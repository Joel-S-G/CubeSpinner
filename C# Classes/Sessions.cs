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
        public string? Name { get; set;}
        public List<SolveRecord> solves = new();
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

        public List<SolveRecord> SortByTime() //sort by time (decreasing)
        {
            return solves   
                .OrderBy(solves => solves.FinalTime ?? TimeSpan.MaxValue)
                .ToList();
        }

        public List<SolveRecord> SortByDate() //sort by date solved (newest first)
        {
            return solves   
                .OrderByDescending(solves => solves.SolvedAt)
                .ToList();
        }

        public void AddSolve(SolveRecord solve) //add new solves to the list
        {
            solves.Add(solve);
            
        }

        public void RemoveSolve(Guid solveId) //removes solve at the index pointed at by solveId
        {
            for (int i = solves.Count -1; i >= 0; i--)   //iterates through list to find the solve and then removes it at that index
            {
                if (solves[i].SolveID ==solveId)
                {
                    solves.RemoveAt(i);
                    return;
                }
            }
        }

        public SolveRecord? GetSolve(Guid solveID) //gets a solve from its Id
        {
            for (int i = solves.Count - 1; i >= 0; i--) // basically the same as RemoveSolve except it doesn't remove the solve at the index
            {
                if (solves[i].SolveID == solveID)
                {
                    return solves[i];
                }
            }   

           return null;
        }

        public SolveRecord? GetBestSolve() //return the lowest time in the session
        {
            return solves
                .Where(solve => solve.FinalTime != null)
                .OrderBy(solve => solve.FinalTime!.Value)
                .FirstOrDefault();
        
        }

        
    
    }
}