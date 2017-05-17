using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    partial class RabiRibiDisplay
    {
        private MainContext mainContext;
        private DebugContext debugContext;
        private PracticeModeContext practiceModeContext;
        private MainWindow mainWindow;
        
        private RabiRibiState rabiRibiState;
        private InGameState inGameState;
        private MemorySnapshot prevSnapshot;
        private MemorySnapshot snapshot;

        // Variables used for tracking frequency of memory reads.
        private static readonly DateTime UNIX_START = new DateTime(1970, 1, 1);
        private double readFps = -1;
        private long previousFrameMillisecond = -1;

        // internal frame counter.
        private int memoryReadCount;

        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, PracticeModeContext practiceModeContext, MainWindow mainWindow)
        {
            this.rabiRibiState = new RabiRibiState();
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.practiceModeContext = practiceModeContext;
            this.mainWindow = mainWindow;
            this.memoryReadCount = 0;
            StartNewGame();
        }

        public void ReadMemory(Process process)
        {
            ++memoryReadCount;
            practiceModeContext.ResetSendTriggers();
            var memoryHelper = new MemoryHelper(process);

            // Snapshot Game Memory
            snapshot = new MemorySnapshot(memoryHelper, mainContext.veridx);

            Update();
            UpdateDebugArea(memoryHelper);
            UpdateFps();

            if (snapshot.musicid >= 0) rabiRibiState.lastValidMusicId = snapshot.musicid;
            if (snapshot.playtime > 0) inGameState.lastNonZeroPlayTime = snapshot.playtime;
            prevSnapshot = snapshot;

            SendPracticeModeMessages();
            memoryHelper.Dispose();
        }
        
        private void UpdateFps()
        {
            long currentFrameMillisecond = (long)(DateTime.Now - UNIX_START).TotalMilliseconds;
            if (previousFrameMillisecond != -1)
            {
                double newFps = 1000.0 / (currentFrameMillisecond - previousFrameMillisecond);
                if (readFps < 0) readFps = newFps;
                else readFps = 0.9 * readFps + 0.1 * newFps;

                var fpsDisplay = $"Reads Per Second:\n{readFps:.0.00}";
            }
            previousFrameMillisecond = currentFrameMillisecond;
        }

        /// <summary>
        /// Only runs if prevSnapshot != null.
        /// </summary>
        private void Update()
        {
            // Do nothing if prevSnapshot == null. We want to take one more snapshot before we start doing stuff.
            if (prevSnapshot == null) return;

            #region Game State Machine

            if (inGameState.CurrentActivityIs(InGameActivity.STARTING))
            {
                // Detect start game
                if (0 < snapshot.playtime && snapshot.playtime < 200 ||
                    (prevSnapshot.playtime < snapshot.playtime && snapshot.playtime < prevSnapshot.playtime + 200))
                {
                    inGameState.currentActivity = InGameActivity.WALKING;
                    DebugLog("IGT start?");
                }
            }
            else
            {
            }

            #endregion

            #region Detect Reload

            // This variable is used to block splits while reloading.
            bool reloading = snapshot.playtime == 0 || snapshot.playtime < prevSnapshot.playtime;
            // Forces splits regardless of reload status.
            if (!mainContext.DontSplitOnReload) reloading = false;

            // Detect reload events (separate from reloading variable)
            if (inGameState.IsGameStarted())
            {
                // When reloading, the frame numbers look like this:
                // Case 1: (PLAYTIME frame steps down briefly before going to 0)
                //         1108, 1109, 1110, 1110, 1110, 540, 0, 0, 0, 0, 0, 540, 540, 541, 542, 544,
                // Case 2: (PLAYTIME frame goes straight to 0)
                //         1108, 1109, 1110, 1110, 1110, 0, 0, 0, 0, 0, 540, 540, 541, 542, 544,
                // This can sometimes cause reloads to be detected twice (which is usually not a problem though, but ocd lol)
                // So we use a switch to "prime" the canReload flag whenever it detects the PLAYTIME increasing.
                // the canReload flag is unset when a reload is detected, and remains unset until PLAYTIME starts increasing again.

                if (snapshot.playtime > prevSnapshot.playtime) inGameState.canReload = true;
                if (InGame() && inGameState.canReload && snapshot.playtime < prevSnapshot.playtime)
                {
                    PracticeModeSendTrigger(SplitTrigger.Reload);
                    DebugLog("Game Reloaded!");
                    inGameState.canReload = false;
                    inGameState.nRestarts++;
                }
            }
            #endregion

            #region Detect Music change

            if (MusicChanged() && !ValidMusicChanged())
            {
                DebugLog($"Invalid Music Change: {StaticData.GetMusicName(prevSnapshot.musicid)} -> {StaticData.GetMusicName(snapshot.musicid)}");
            }

            if (ValidMusicChanged())
            {
                DebugLog($"Valid Music Change: {StaticData.GetMusicName(rabiRibiState.lastValidMusicId)} -> {StaticData.GetMusicName(snapshot.musicid)}");
            }

            #endregion

            #region Detect Minimap Change

            if (prevSnapshot.minimapPosition != snapshot.minimapPosition)
            {
                DebugLog($"Minimap Shift! {prevSnapshot.minimapPosition} -> {snapshot.minimapPosition}");
                if (snapshot.minimapPosition == 1)
                {
                    var bossFight = BossFightIdentifier.IdentifyBossFight(snapshot);
                    DebugLog($"BOSS FIGHT: {bossFight.displayName}");
                    DebugLog($"Fighting Bosses: {string.Join(", ", snapshot.bossList.Select(boss => StaticData.GetBossName(boss.id)))}");

                    inGameState.StartBossFight(bossFight);
                }
                else // snapshot.minimapPosition == 0
                {
                    if (reloading)
                    {
                        inGameState.StopBossFight();
                    }
                    else
                    {
                        inGameState.FinishBossFight();
                    }
                }
            }

            #endregion

            #region Detect Boss Change

            {
                var currBosses = new HashSet<int>(snapshot.bossList.Select(bossStats => bossStats.id));
                var prevBosses = new HashSet<int>(prevSnapshot.bossList.Select(bossStats => bossStats.id));

                foreach (var enteringBoss in currBosses.Except(prevBosses))
                {
                    DebugLog($"Boss Enters: {StaticData.GetBossName(enteringBoss)}");
                }

                foreach (var leavingBoss in prevBosses.Except(currBosses))
                {
                    DebugLog($"Boss Leaves: {StaticData.GetBossName(leavingBoss)}");
                }
            }

            #endregion

            #region Detect Death
            // Original death detection code.
            if (snapshot.hp == 0 && prevSnapshot.hp > 0)
            {
                if (InGame())
                {
                    inGameState.nDeaths++;
                }
                DebugLog("Death!");
            }

            // Alternate death detection code.
            if (snapshot.IsDeathSprite() && !prevSnapshot.IsDeathSprite())
            {
                if (InGame())
                {
                    inGameState.nDeathsAlt++;
                }
                DebugLog("Death (Alt)!");
            }
            #endregion

            #region Detect Start Game From Menu

            if (prevSnapshot != null && (snapshot.CurrentMusicIs(Music.MAIN_MENU) || snapshot.CurrentMusicIs(Music.ARTBOOK_INTRO))
                && prevSnapshot.blackness == 0 && snapshot.blackness >= 100000)
            {
                // Sudden increase by 100000
                if (mainContext.AutoStart) SpeedrunSendStartTimer();
                DebugLog("[Start] Game Started!");
                StartNewGame();
                //mainContext.LastBossEnd = DateTime.Now;
            }

            #endregion

            #region Read IGT
            if (snapshot.t_playtime > 0 && mainContext.Igt)
            {
                SendIgt((float)snapshot.t_playtime / 60);
            }
            #endregion


            #region Detect Map Change
            if (prevSnapshot.mapid != snapshot.mapid)
            {
                PracticeModeMapChangeTrigger(prevSnapshot.mapid, snapshot.mapid);
                DebugLog($"New Map: [{snapshot.mapid}] {StaticData.GetMapName(snapshot.mapid)}");
            }
            #endregion

            #region Non-Music Splits

            // Side Chapter
            if (mainContext.SplitOnSideCh)
            {
                // TODO???: WIP
                if (false)
                {
                    DebugLog("[Split] Finished Side Chapter");
                    SpeedrunSendSplit();
                }
            }

            //Computer
            if (mainContext.SplitOnComputer)
            {
                if (snapshot.money - prevSnapshot.money == 17500)
                {
                    DebugLog("[Split] Found Computer");
                    SpeedrunSendSplit();
                }
            }

            //Cyber Flower
            if (mainContext.SplitOnCyberFlower)
            {
                if (snapshot.cyberFlower == 1 && prevSnapshot.cyberFlower == 0)
                {
                    DebugLog("[Split] Obtained Cyber Flower");
                    mainContext.SplitOnCyberFlower = false;
                    SpeedrunSendSplit();
                }
            }

            #endregion

            #region Music Splits

            if (ValidMusicChanged())
            {
                int prevMusicId = rabiRibiState.lastValidMusicId;
                Music prevMusic = StaticData.GetMusic(prevMusicId).Value;
                int currMusicId = snapshot.musicid;
                Music currMusic = StaticData.GetMusic(currMusicId).Value;

                PracticeModeMusicChangeTrigger(prevMusicId, currMusicId);
                DebugLog($"New Music: [{currMusicId}] {StaticData.GetMusicName(currMusicId)}");
                mainContext.GameMusic = StaticData.GetMusicName(currMusicId);

                if (currMusic == Music.THEME_OF_RABI_RIBI_8BIT || currMusic == Music.THEME_OF_RABI_RIBI ||
                    currMusic == Music.MAIN_MENU)
                {
                    DebugLog("Title Music Detected");
                    if (mainContext.AutoReset)
                    {
                        DebugLog("[Reset]");
                        SpeedrunSendReset();
                    }
                    //Run "Preset Refresh" here.
                }
                else
                {
                    #region Prologue
                    //Prologue Cocoa
                    if (mainContext.SplitOnCocoa1 &&
                        (prevMusic == Music.GET_ON_WITH_IT && currMusic == Music.ADVENTURE_STARTS_HERE))
                    {
                        if (!reloading)
                        {
                            DebugLog("[Split] Cocoa 1 Defeated");
                            SpeedrunSendSplit();
                        }
                        else
                        {
                            DebugLog("Reloaded Cocoa 1; Don't Split");
                        }
                    }

                    //Prologue Ribbon
                    if (mainContext.SplitOnRibbon &&
                        (prevMusic == Music.GET_ON_WITH_IT && currMusic == Music.SPECTRAL_CAVE))
                    {
                        if (!reloading)
                        {
                            DebugLog("[Split] Ribbon Defeated");
                            SpeedrunSendSplit();
                        }
                        else
                        {
                            DebugLog("Reloaded Ribbon; Don't Split");
                        }
                    }

                    //Prologue Ashuri
                    if (mainContext.SplitOnAshuri1 &&
                        (prevMusic == Music.GET_ON_WITH_IT && currMusic == Music.RABI_RABI_BEACH))
                    {
                        if (!reloading)
                        {
                            DebugLog("[Split] Ashuri 1 Defeated");
                            SpeedrunSendSplit();
                        }
                        else
                        {
                            DebugLog("Reloaded Ashuri 1; Don't Split");
                        }
                    }

                    #endregion

                    #region Miscellaneous

                    // Mr. Big Box & Holo-Defense System
                    if ((mainContext.SplitOnBigBox || mainContext.SplitOnHoloMaid) &&
                        (prevMusic == Music.MIDBOSS_BATTLE && currMusic == Music.EXOTIC_LABORATORY))
                    {
                        // 19830 is the rough X-Coordinate of the Save Point next to Sliding Powder
                        // Mr. Big Box
                        if (mainContext.SplitOnBigBox && snapshot.px < 19830)
                        {
                            if (!reloading)
                            {
                                DebugLog("[Split] Mr. Big Box Defeated");
                                SpeedrunSendSplit();
                            }
                            else
                            {
                                DebugLog("Reloaded Mr. Big Box; Don't Split");
                            }
                        }

                        // Holo-Defense System
                        if (mainContext.SplitOnHoloMaid && snapshot.px > 19830)
                        {
                            if (!reloading)
                            {
                                DebugLog("[Split] Holo-Defense System Defeated");
                                SpeedrunSendSplit();
                            }
                            else
                            {
                                DebugLog("Reloaded Holo-Defense System; Don't Split");
                            }
                        }
                    }

                    // Hall of Memories
                    if (mainContext.SplitOnHoM &&
                        (prevMusic == Music.ANOTHER_D && currMusic != Music.ANOTHER_D))
                    {
                        DebugLog("[Split] Hall of Memories Completed");
                        mainContext.SplitOnHoM = false;
                        SpeedrunSendSplit();
                    }

                    // Forgotten Cave II
                    if (mainContext.SplitOnFC2 &&
                        (prevMusic == Music.FORGOTTEN_CAVE_II && currMusic != Music.FORGOTTEN_CAVE_II))
                    {
                        DebugLog("[Split] Forgotten Cave II Completed");
                        mainContext.SplitOnFC2 = false;
                        SpeedrunSendSplit();
                    }

                    // Library  
                    if (mainContext.SplitOnLibrary && snapshot.CurrentMapIs(Map.WesternCoast) &&
                        (prevMusic != Music.RFN_III && currMusic == Music.RFN_III))
                    {
                        DebugLog("[Split] Library Completed");
                        mainContext.SplitOnLibrary = false;
                        SpeedrunSendSplit();
                    }

                    #endregion

                    #region Bosses

                    //
                    //
                    //

                    #endregion
                }
                #endregion
            }
        }
        
        private void StartNewGame()
        {
            inGameState = new InGameState();
            rabiRibiState.gameStatus = GameStatus.INGAME;
        }

        private void ReturnToMenu()
        {
            rabiRibiState.gameStatus = GameStatus.MENU;
            inGameState = null;
        }

        private bool InGame()
        {
            return rabiRibiState.gameStatus == GameStatus.INGAME;
        }

        private bool MusicChanged()
        {
            return prevSnapshot != null && prevSnapshot.musicid != snapshot.musicid;
        }

        private bool ValidMusicChanged()
        {
            return rabiRibiState.lastValidMusicId >= 0 && snapshot.musicid >= 0 && rabiRibiState.lastValidMusicId != snapshot.musicid;
        }
        
        private void UpdateDebugArea(MemoryHelper memoryHelper)
        {
            int ptr = snapshot.entityArrayPtr;
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                debugContext.BossList[i].BossID = memoryHelper.GetMemoryValue<int>(
                    ptr + StaticData.EnemyEntityIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = memoryHelper.GetMemoryValue<int>(
                    ptr + StaticData.EnemyEntityHPOffset[mainContext.veridx], false);
                ptr += StaticData.EnemyEntitySize[mainContext.veridx];
            }

            debugContext.BossEvent = inGameState.currentActivity == InGameActivity.BOSS_BATTLE;
        }

        private void DebugLog(string log)
        {
            this.debugContext.Log($"[ {memoryReadCount:D8}] {log}");
        }

        #region LiveSplit Messages
        private void SpeedrunSendSplit()
        {
            if (!mainContext.PracticeMode) SendMessage("split\r\n");
        }

        private void SpeedrunSendReset()
        {
            if (!mainContext.PracticeMode) SendMessage("reset\r\n");
        }

        private void SpeedrunSendStartTimer()
        {
            if (!mainContext.PracticeMode) SendMessage("starttimer\r\n");
        }

        private void SendIgt(float time)
        {
            SendMessage($"setgametime {time}\r\n");
        }

        private void PracticeModeSendTrigger(SplitTrigger trigger)
        {
            if (mainContext.PracticeMode) DebugLog("Practice Mode Trigger " + (trigger.ToString()));
            practiceModeContext.SendTrigger(SplitCondition.Trigger(trigger));
        }

        private void PracticeModeMapChangeTrigger(int oldMapId, int newMapId)
        {
            if (mainContext.PracticeMode) DebugLog("Practice Mode Trigger Map Change " + oldMapId + " -> " + newMapId);
            practiceModeContext.SendTrigger(SplitCondition.MapChange(oldMapId, newMapId));
        }

        private void PracticeModeMusicChangeTrigger(int oldMusicId, int newMusicId)
        {
            if (mainContext.PracticeMode) DebugLog("Practice Mode Trigger Music Change " + oldMusicId + " -> " + newMusicId);
            practiceModeContext.SendTrigger(SplitCondition.MusicChange(oldMusicId, newMusicId));
        }

        private void SendPracticeModeMessages()
        {
            if (!mainContext.PracticeMode) return;
            if (practiceModeContext.SendStartTimerThisFrame())
            {
                SendMessage("starttimer\r\n");
            }
            if (practiceModeContext.SendSplitTimerThisFrame())
            {
                SendMessage("split\r\n");
            }
            if (practiceModeContext.SendResetTimerThisFrame())
            {
                SendMessage("reset\r\n");
            }
        }

        private void SendMessage(string message)
        {
            mainWindow.SendMessage(message);
        }
        #endregion
    }
}
