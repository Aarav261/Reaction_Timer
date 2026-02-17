namespace SimpleReactionMachine
{
    public class EnhancedReactionController : IController
    {
        private IGui gui;
        private IRandom rng;
        private GetState currentState;

        private int gamesPlayed;
        private int[] reactionTimes = new int[3];

        private interface GetState
        {
            void Enter();
            void CoinInserted();
            void GoStopPressed();
            void Tick();
        }

        private static string SecFromTicks(int ticks)
        {
            return (ticks / 100.00).ToString("0.00");
        }

        private void SetState(GetState state)
        {
            this.currentState = state;
            this.currentState.Enter();
        }

        // Wait for coin
        class WaitCoinState : GetState
        {
            private EnhancedReactionController c;

            public WaitCoinState(EnhancedReactionController controller)
            {
                this.c = controller;
            }

            public void Enter()
            {
                c.gui.SetDisplay("Insert coin");
            }
            public void CoinInserted()
            {
                c.gamesPlayed = 0;
                c.SetState(new WaitGoState(c));
            }
            public void GoStopPressed() { }
            public void Tick() { }
        }

        // Wait for Go/Stop, with 10s timeout
        class WaitGoState : GetState
        {
            private EnhancedReactionController c;
            private int ticks;

            public WaitGoState(EnhancedReactionController controller)
            {
                c = controller; ticks = 0;
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
            public void Tick()
            {
                ticks++;
                if (ticks >= 1000) // 10s timeout
                    c.SetState(new WaitCoinState(c));
            }
        }

        // Wait random delay, abort on early press
        class RandomDelayState : GetState
        {
            private EnhancedReactionController c;
            private int delayTicks;
            private int elapsedTicks;
            public RandomDelayState(EnhancedReactionController controller)
            {
                c = controller;
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
                c.SetState(new WaitCoinState(c));
            } // abort 
            public void Tick()
            {
                elapsedTicks++;
                if (elapsedTicks >= delayTicks)
                    c.SetState(new MeasuringReactionState(c));
            }
        }

        // Measure reaction time
        class MeasuringReactionState : GetState
        {
            private EnhancedReactionController c;
            private int reactionTicks;
            public MeasuringReactionState(EnhancedReactionController controller)
            {
                c = controller;
                reactionTicks = 0;
            }

            public void Enter()
            {
                c.gui.SetDisplay("0.00");
            }
            public void CoinInserted() { }
            public void GoStopPressed()
            {
                c.reactionTimes[c.gamesPlayed] = reactionTicks;
                c.SetState(new ShowResultState(c));
            }
            public void Tick()
            {
                reactionTicks++;
                c.gui.SetDisplay(SecFromTicks(reactionTicks));
                if (reactionTicks >= 200)
                {
                    c.reactionTimes[c.gamesPlayed] = reactionTicks;
                    c.SetState(new ShowResultState(c));
                }
            }
        }

        // Show result for 3s or skip on button press
        class ShowResultState : GetState
        {
            private EnhancedReactionController c;
            private int ticks;
            public ShowResultState(EnhancedReactionController controller)
            {
                c = controller;
                ticks = 0;
            }
            public void Enter()
            {
                c.gui.SetDisplay(SecFromTicks(c.reactionTimes[c.gamesPlayed]));
            }
            public void CoinInserted() { }
            public void GoStopPressed()
            {
                c.gamesPlayed++;
                if (c.gamesPlayed < 3)
                    c.SetState(new RandomDelayState(c));
                else
                    c.SetState(new ShowAverageState(c));
            }
            public void Tick()
            {
                ticks++;
                if (ticks >= 300)
                {
                    c.gamesPlayed++;
                    if (c.gamesPlayed < 3)
                        c.SetState(new RandomDelayState(c));
                    else
                        c.SetState(new ShowAverageState(c));
                }
            }
        }

        // Show average for 5s or skip on button press
        class ShowAverageState : GetState
        {
            private EnhancedReactionController c;
            private int ticks;
            private string avg;
            public ShowAverageState(EnhancedReactionController controller)
            {
                c = controller;
                ticks = 0;
                avg = "Average = " + SecFromTicks((c.reactionTimes[0] + c.reactionTimes[1] + c.reactionTimes[2]) / 3);
            }

            public void Enter()
            {
                c.gui.SetDisplay(avg);
            }
            public void CoinInserted() { }

            public void GoStopPressed()
            {
                c.SetState(new WaitCoinState(c)); 
            }
            public void Tick()
            {
                ticks++;
                if (ticks >= 500)
                    c.SetState(new WaitCoinState(c));
            }
        }

        public void Connect(IGui gui, IRandom rng)
        {
            this.gui = gui;
            this.rng = rng;
        }

        public void Init()
        {
            if (currentState == null)
            {
                gui.Init();
            }
            SetState(new WaitCoinState(this));
        }

        public void CoinInserted()
        {
            if (currentState == null)
            {
                Init();
            }
            this.currentState.CoinInserted();
        }

        public void GoStopPressed()
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
