using System;
using System.Security.Principal;
using Microsoft.Extensions.FileProviders;

namespace CubeSpinner
{
    class StatisticsCalc
    {
        //methods : mo3, ao5, ao12, ao100, rolling mean of 100
        //implement 100 item FIFO structure - no need for more than 100 solves at any given time -> same 
        //attributes: finaltime, solveRecords solves


        //circular queue managent
        public const int RollingWindow = 100;
        private readonly List<TimeSpan> sortedTimes = new();
        private readonly Queue<SolveRecord> window = new();
        private TimeSpan runningSum = TimeSpan.Zero;
        public int Count
        {
            get {return window.Count;}
        }


        private void InsertSorted(TimeSpan? time) //keeps times within the 100 solve window
        {
            if (time is not TimeSpan t)
            {
                return;
            }

            runningSum += t;
            int index = sortedTimes.BinarySearch(t);
            if (index < 0)
            {
                index = (index * -1) - 1;
            }   

            sortedTimes.Insert(index, t);
        }

        private void RemoveSorted(TimeSpan? time) //like the previous function, keeps solves in the queue
        {
            if (time is not TimeSpan t)
            {
                return;
            }

            runningSum -= t;
            int index = sortedTimes.BinarySearch(t);
            if (index >= 0 )
            {
                sortedTimes.RemoveAt(index);
            }
        }
        
        public void RecordSolve(SolveRecord solve) //inserts a solve into the queue, ensures that it is not full and if it is it will remove the 
        {
            window.Enqueue(solve);

            if (window.Count > RollingWindow)
            {
                window.Dequeue(); //remove oldest solve
            }
        } 

        public void Rebuild(List<SolveRecord> allSolves) // recreated the list for when solves are removed from the middle
        {
            window.Clear();

            int startindex = allSolves.Count - RollingWindow;

            if (startindex > 0)
            {
                startindex = 0; //in case a session is cleared with <100 solves
            }

            for (int i = startindex; i < allSolves.Count; i++)
            {
                window.Enqueue(allSolves[i]);
            }
        }


      //calculation helpers
      
       private TimeSpan? CalculateMean(List<TimeSpan> times) 
        {
            long totalTicks = 0;

            foreach (TimeSpan time in times)
            {
                totalTicks += time.Ticks; // "ticks" are just a very small, precise unit of time
            }
 
            long averageTicks = totalTicks / times.Count;
            return TimeSpan.FromTicks(averageTicks);
        }

       
        private TimeSpan? CalculateTrimmedAverge(List<SolveRecord> solves, int trim)
        {
            if (solves == null || solves.Count == 0)
            {
                return null;
            }

            int totalSolves = solves.Count;
            int dnfCount = 0;

            foreach (SolveRecord solve in solves)
            {
                if (solve.FinalTime == null)
                {
                    dnfCount++;
                }
            }

            if (dnfCount > trim)
            {
                return null;
            }

            List<TimeSpan> times = new List<TimeSpan>();

            foreach (SolveRecord solve in solves) //builds a list of items to be processed
            {
                times.Add(solve.FinalTime ?? TimeSpan.MaxValue);
            }

            times.Sort(); //sorts the list by value

            List<TimeSpan> keep = new List<TimeSpan>();
            int startIndex = trim;
            int endIndex = times.Count - trim;

            if (endIndex <= startIndex)
            {
                return null;
            }

            for (int i = startIndex; i < endIndex; i++) //iterates through and builds a list of kept times
            {
                keep.Add(times[i]);
            }

            if (keep.Count == 0)
            {
                return null;
            }

            return CalculateMean(keep);
        }

        private List<TimeSpan> GetNonDNF()
        {
            List<TimeSpan> times = new List<TimeSpan>();

            foreach (SolveRecord solve in window)
            {
                if(solve.FinalTime == null)
                {
                    times.Add(solve.FinalTime.Value);
                }
            }

            return times;
        }

        private List<SolveRecord> GetLastN(int n) //returns the last n solves to calculate statistics
        {
            List<SolveRecord> allInWindow = new List<SolveRecord>(window);
            List<SolveRecord> result = new List<SolveRecord>();

            if (allInWindow.Count < n)
            {
                return result;
            }

            int startIndex = allInWindow.Count - n;
            for (int i = startIndex; i < allInWindow.Count; i++)
            {
                result.Add(allInWindow[i]);
            }

            return result;
        }



        //following solves averages

        public TimeSpan? Mo3
        {
           get { return CalculateTrimmedAverge(GetLastN(3), 0);}
        }

        public TimeSpan? Ao5
        {
            get { return CalculateTrimmedAverge(GetLastN(5), 1);}
        }

        public TimeSpan? Ao10
        {
            get { return CalculateTrimmedAverge(GetLastN(10), 1);}
        }

        public TimeSpan? Ao100
        {
            get { return CalculateTrimmedAverge(GetLastN(100), 5);}
        }

        public TimeSpan? RollingMean100 //rolling mean of 100
        {
            get
            {
                List<TimeSpan> times = GetNonDNF();
 
                if (times.Count == 0)
                {
                    return null;
                }
 
                return CalculateMean(times);
            }
        }

        public TimeSpan? GetPercentile(double percentile, List<TimeSpan> sortedTimes)
        {
            if (sortedTimes.Count == 1)
            {
                return sortedTimes[0];
            }

            double rank = percentile/100 * (sortedTimes.Count-1);

            int lowerindex = (int)Math.Floor(rank);
            int upperindex = (int)Math.Ceiling(rank);

            if (lowerindex == upperindex)
            {
                return sortedTimes[lowerindex]; //rank landed on exactly one item
            }

            double fraction = rank - lowerindex;
            long lowerTicks = sortedTimes[lowerindex].Ticks;
            long upperTicks = sortedTimes[upperindex].Ticks;
            long blend = lowerTicks + (long)((upperTicks-lowerTicks) * fraction);

            return TimeSpan.FromTicks(blend);

        }
        

        
    }   
}