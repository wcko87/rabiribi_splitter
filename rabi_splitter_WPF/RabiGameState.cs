using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    enum GameActivity
    {
        STARTING,
        WALKING,
        BOSS_BATTLE,
    }

    class RabiGameState
    {
        public int nRestarts;
        public int nDeaths;
        
        public int nDeathsAlt;

        public GameActivity currentActivity;

        public int lastNonZeroPlayTime = -1;

        public RabiGameState()
        {
            currentActivity = GameActivity.STARTING;
        }

        public bool CurrentActivityIs(GameActivity gameActivity)
        {
            return currentActivity == gameActivity;
        }

        public bool IsGameStarted()
        {
            return !CurrentActivityIs(GameActivity.STARTING);
        }


    }
}
