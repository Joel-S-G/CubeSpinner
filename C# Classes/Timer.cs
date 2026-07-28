using System;

namespace CubeSpinner
{

    public enum timerState //the timer will be designed as a simple state machine, the following are the states, originally they were going to be bool flag but i realised that is that happened there was a possibility that two flag could be triggered simultaneusly and that is not possible (mutually exclusive e.g. isStop and isStart cannot happen at the same time )
       {
            isIdle, //nothing happening
            isReady, //this is for when they are holding down the spacebar and ready to solve
            isInspecting, //inspection timer, might not be used
            isRunning, // timer is running
            isStopped // spacebar is pressed again

       }
    public class Timer
    { 
     
        public timerState State {get; private set;} = timerState.isIdle;
        public TimeSpan ElapsedTime {get; private set;} 
        public int startDelayMS {get; set;} = 300; //defult value is 300ms but allows it to be changed by the user



        private DateTime holdStartedAt;
        private DateTime sovleStartedAt;
        System.Threading.Timer pollTimer;
        public event Action Statechanged;


        public void spaceDown() //behaviours for when the spacebar is held/pressed down
        {
            switch (State)
            {
                case timerState.isIdle: //arms timer from idle
                {

                    State = timerState.isReady;
                    StartPolling();
                    break;
                }

                case timerState.isStopped: //arms timer from stop
                {
                    State = timerState.isReady; //arms the timer
                    holdStartedAt = DateTime.UtcNow;
                    StartPolling();
                    break;
                }


                case timerState.isInspecting: //arms timer from inspection - reasearch how to toggle this based on if inspection feature is enabled, keep enabled?
                {
                    State = timerState.isReady;
                    holdStartedAt = DateTime.UtcNow;
                    StartPolling();
                    break;
                }         

                case timerState.isRunning: //stops the timer if running
                {  
                    Stop();
                    break;
                }

            }
        }

        public void spaceUp() // behaviours for when the spacebar is released 
        {
            switch (State)
            {
                case timerState.isReady: //if full delay is waited out -> starts timer
                {                    
                    Start();
                    break;
                }

                case timerState.isIdle: //if full delay is not met -> aborted attempt
                {
                    State = timerState.isIdle;
                    StopPolling();
                    break;
                }   
            }

        }

        private void Start() //starting the timer
        {
            
            State = timerState.isRunning;
            sovleStartedAt = DateTime.UtcNow;
            StartPolling();
        }

        private void Stop() //stopping the timer
        {
            State = timerState.isStopped;
            ElapsedTime = DateTime.UtcNow - sovleStartedAt; //calculates elapsed time
            StopPolling();
        }

        private void StartPolling()
        {
            pollTimer?.Dispose();
            pollTimer = new System.Threading.Timer(_ => Poll(), null, 0, 30); //disposes of any running polling timers, 0 - start immediately, 30 - refresh rate (ms) -> updates the value on the webpage 
        }

        private void StopPolling()
        {
            pollTimer?.Dispose();
        }

        private void Poll()
        {
           if (State == timerState.isReady && (DateTime.UtcNow - holdStartedAt).TotalMilliseconds >= startDelayMS) //check if delay time has been met
           {
             State = timerState.isReady;
           }

           if (State == timerState.isRunning) //recalculates if the program is still running
           {
             ElapsedTime = DateTime.UtcNow - sovleStartedAt;
           }

           Statechanged?.Invoke();
        }
    }

}
