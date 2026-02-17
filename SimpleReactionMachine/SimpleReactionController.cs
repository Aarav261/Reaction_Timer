namespace SimpleReactionMachine
{
    public class SimpleReactionController : IController
    {
        private IGui gui;
        private IRandom rng;
        private GetState currentState;
        
       //states
        private interface GetState
        {
            void Enter();
            void CoinInserted();
            void GoStopPressed();
            void Tick(); 
        }

       // Convert ticks (10 ms) to seconds string with 2 decimal places
        private static string SecFromTicks(int ticks)
        {
           return (ticks / 100.00).ToString("0.00");
        }
// State setter
        private void SetState(GetState state)
        {
            this.currentState = state;
            currentState.Enter();
        } 
        
        
       // initial state for user to insert coin
        class WaitCoinState : GetState
        {
            private SimpleReactionController c;

            public WaitCoinState(SimpleReactionController controller)
            {
                this.c = controller;
            }

            public void Enter()
            {
                c.gui.SetDisplay("Insert coin");
            }

            public void CoinInserted()
            {
                c.SetState(new InsertCoinState(c));
            }

            public void GoStopPressed() { }

            public void Tick() { }
        }
        // state after coin is inserted, waiting for user to press GO

        class InsertCoinState : GetState
        {
            private readonly SimpleReactionController c;

            public InsertCoinState(SimpleReactionController controller)
            {
                this.c = controller;
            }

            public void Enter()
            {

                c.gui.SetDisplay("Press GO!");
            }

            public void CoinInserted() { }

            public void GoStopPressed()
            {
                c.SetState(new RandomDelayState(c));
            } 

            public void Tick() { }
        }
// state with random delay before user can press STOP
        private sealed class RandomDelayState : GetState
        {
            private SimpleReactionController c;
            private int delayTicks;
            private int elapsedTicks;

            public RandomDelayState(SimpleReactionController controller)
            {
                this.c = controller;
                delayTicks = c.rng.GetRandom(100, 250);
                elapsedTicks = 0;
            }

            public void Enter()
            {
                c.gui.SetDisplay("Wait...");
            }

            public void CoinInserted() { }

            public void GoStopPressed()
            {
                // Early press
                c.SetState(new WaitCoinState(c));
            }

            public void Tick()
            {
                elapsedTicks++;
                if (elapsedTicks >= delayTicks)
                {
                    c.SetState(new MeasuringReactionState(c));
                }
            }
        }
// state measuring user reaction time
        private sealed class MeasuringReactionState : GetState
        {
            private SimpleReactionController c;
            private int reactionTicks;

            public MeasuringReactionState(SimpleReactionController controller)
            {
                c = controller;
                reactionTicks = 0;
            }

            public void Enter()
            {
                c.gui.SetDisplay("0.00");   
            }

            public void CoinInserted() {  }

            public void GoStopPressed()
            {
                c.SetState(new ShowResultState(c, reactionTicks));
            }

            public void Tick()
            {

                reactionTicks++;

                // Update live display first
                c.gui.SetDisplay(SecFromTicks(reactionTicks));

                // Auto-stop at 2.00 s
                if (reactionTicks >= 200)
                {
                    c.SetState(new ShowResultState(c, reactionTicks));
                }
            }
        }
// state showing the result for a while, then back to idle
        class ShowResultState : GetState
        {
            private SimpleReactionController c;
            private int resultTicks; 
            private int TicksAfterGame = 0;

            public ShowResultState(SimpleReactionController controller, int ticks)
            {
                this.c = controller;
                this.resultTicks = ticks;
            } 

            public void Enter()
            {
                c.gui.SetDisplay(SecFromTicks(resultTicks));
            }

            public void CoinInserted() {  }

            public void GoStopPressed()
            {

                c.SetState(new WaitCoinState(c));
            }

            public void Tick()
            {
                TicksAfterGame++;
                if (TicksAfterGame >= 300)
                {
                    c.SetState(new WaitCoinState(c));
                }
            }
        }
        public void Connect(IGui gui, IRandom rng) // Connect controller to gui and rng
        {
            this.gui = gui;
            this.rng = rng;
        }

        public void Init()// Initialise the controller
        {
            if (currentState == null)
            {
                gui.Init();
            }
            SetState(new WaitCoinState(this));
        }
        public void CoinInserted() // Called whenever a coin is inserted into the machine
        {
            if (currentState == null)
            {
                Init();
            }
            currentState.CoinInserted();
        }

        public void GoStopPressed() // Called whenever the go/stop button is pressed
        
        {
            if (currentState == null)
            {
                Init();
            }
            this.currentState.GoStopPressed();
        }
        public void Tick()
        {
            this.currentState.Tick();
        }
    } 
}
  
  