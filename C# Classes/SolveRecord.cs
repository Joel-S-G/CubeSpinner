using System;
using System.Data.Common;
using System.Dynamic;

namespace CubeSpinner
{
    public class SolveRecord
    {
        public Guid SolveID {get; }
        public TimeSpan RawTime {get; }
        public string? Scramble {get; }
        public DateTime SolvedAt {get; }// all the get; only variables shouldn't be changed at runtime
        public Penalty penaltyStatus { get; set;} = Penalty.OK; //set solve status as OK by defult (no penalty)
        
        
    

        public enum Penalty //defining penalty states
        {
            OK,
            Plus2,
            DNF,                    
        }

        public void FormatPenalty()
        {
            if (penaltyStatus == Penalty.OK)
            {

            }
            if (penaltyStatus == Penalty.Plus2)
            {

            }
            if (penaltyStatus == Penalty.DNF)
            {
                
            }
        }

        public TimeSpan? FinalTime => penaltyStatus switch
        {
            Penalty.DNF => null,
            Penalty.Plus2 => RawTime + TimeSpan.FromSeconds(2),
            _ => RawTime
        };

        
        public SolveRecord(TimeSpan rawTime, string scramble)
        {
            SolveID = Guid.NewGuid();
            SolvedAt = DateTime.UtcNow;
            RawTime = rawTime;
            Scramble = scramble;
            
        }   

         

        
    
    }

}
