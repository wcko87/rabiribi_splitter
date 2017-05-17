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
        public BossFight currentBossFight;
        public DateTime currentBossStartTime;
        
        public BossFight lastBossFight;
        public TimeSpan lastBossFightDuration;

        public int lastNonZeroPlayTime = -1;
        public bool canReload = false;

        public InGameState()
        {
            currentActivity = InGameActivity.STARTING;
            currentBossFight = null;
            lastBossFight = null;
        }

        public bool CurrentActivityIs(InGameActivity gameActivity)
        {
            return currentActivity == gameActivity;
        }

        public bool IsGameStarted()
        {
            return !CurrentActivityIs(InGameActivity.STARTING);
        }

        public void StartBossFight(BossFight bossFight)
        {
            currentActivity = InGameActivity.BOSS_BATTLE;
            currentBossStartTime = DateTime.Now;
            currentBossFight = bossFight;
        }

        public void StopBossFight()
        {
            currentActivity = InGameActivity.WALKING;
            currentBossFight = null;
        }

        public void FinishBossFight()
        {
            lastBossFight = currentBossFight;
            lastBossFightDuration = (DateTime.Now - currentBossStartTime);
            currentActivity = InGameActivity.WALKING;
            currentBossFight = null;
        }
    }
}
