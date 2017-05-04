using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    enum InGameActivity
    {
        STARTING,
        WALKING,
        BOSS_BATTLE,
    }

    class InGameState
    {
        public int nRestarts;
        public int nDeaths;
        
        public int nDeathsAlt;

        public InGameActivity currentActivity;

        public int lastNonZeroPlayTime = -1;

        public InGameState()
        {
            currentActivity = InGameActivity.STARTING;
        }

        public bool CurrentActivityIs(InGameActivity gameActivity)
        {
            return currentActivity == gameActivity;
        }

        public bool IsGameStarted()
        {
            return !CurrentActivityIs(InGameActivity.STARTING);
        }


    }
}
