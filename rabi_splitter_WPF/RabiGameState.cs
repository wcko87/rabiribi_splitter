using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    enum GameActivity
    {
        WALKING,
        BOSS_BATTLE,
    }

    class RabiGameState
    {
        public int nRestarts;
        public int nDeaths;

        public GameActivity currentActivity;
        public int currentBoss;

        public RabiGameState()
        {
            currentActivity = GameActivity.WALKING;
        }


    }
}
