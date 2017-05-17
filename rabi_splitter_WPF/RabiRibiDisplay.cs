using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    enum GameStatus
    {
        INGAME,
        MENU
    }

    class RabiRibiDisplay
    {
        private MainContext mainContext;
        private DebugContext debugContext;
        private MainWindow mainWindow;

        private GameStatus gameStatus = GameStatus.MENU;
        private RabiGameState gameState;
        private MemorySnapshot prevSnapshot;
        private MemorySnapshot snapshot;

        private string[] datastrings;
        

        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, MainWindow mainWindow)
        {
            this.datastrings = new string[StaticData.EnenyEntitySize[mainContext.veridx]];
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.mainWindow = mainWindow;
            StartNewGame();
        }
        
        public void ReadMemory(Process process)
        {
            // Snapshot Game Memory
            snapshot = new MemorySnapshot(process, mainContext.veridx);

            #region Game State Machine

            if (gameState.CurrentActivityIs(GameActivity.STARTING)) {
                // Detect start game
                if (snapshot.playtime < 200)
                {
                    gameState.currentActivity = GameActivity.WALKING;
                    DebugLog("IGT start?");
                }
            } else {
                if (MusicChanged())
                {
                    if (StaticData.IsBossMusic(snapshot.musicid))
                    {
                        gameState.currentActivity = GameActivity.BOSS_BATTLE;
                    }
                    else
                    {
                        gameState.currentActivity = GameActivity.WALKING;
                    }
                }
            }

            #endregion
            
            #region Detect Reload
            
            bool reloaded = (prevSnapshot != null) && (snapshot.playtime < prevSnapshot.playtime);
            if (gameState.IsGameStarted() && snapshot.playtime > 0)
            {
                if (snapshot.playtime < gameState.lastNonZeroPlayTime)
                {
                    if (InGame())
                    {
                        gameState.nRestarts++;
                    }
                    DebugLog("Reload Game! " + snapshot.playtime + " <- " + gameState.lastNonZeroPlayTime);
                }
                gameState.lastNonZeroPlayTime = snapshot.playtime;
            }
                
            if (gameState.IsGameStarted() && prevSnapshot != null)
            {
                // Issue: This sometimes detects warps as resets too.
                if (snapshot.animationFrame < prevSnapshot.animationFrame)
                {
                    if (InGame())
                    {
                        gameState.nRestartsAlt++;
                    }
                    DebugLog("Reload Game (Alt)!");
                }
            }

            #endregion

            #region Detect Death
            
            if (prevSnapshot != null)
            {
                if (snapshot.hp == 0 && prevSnapshot.hp > 0)
                {
                    if (InGame())
                    {
                        gameState.nDeaths++;
                    }
                    DebugLog("Death!");
                }
            }

            if (prevSnapshot != null)
            {
                if (snapshot.IsDeathSprite() && !prevSnapshot.IsDeathSprite())
                {
                    if (InGame())
                    {
                        gameState.nDeathsAlt++;
                    }
                    DebugLog("Death (Alt)!");
                }
            }

            #endregion

            #region Detect Start Game

            if (prevSnapshot != null && (snapshot.CurrentMusicIs(Music.MAIN_MENU) || snapshot.CurrentMusicIs(Music.ARTBOOK_INTRO))
                && prevSnapshot.blackness == 0 && snapshot.blackness >= 100000)
            {
                // Sudden increase by 100000
                DebugLog("Start Game!");
                StartNewGame();
            }

            #endregion
           
            if (prevSnapshot == null || prevSnapshot.mapid != snapshot.mapid)
            {
                DebugLog("newmap: " + snapshot.mapid + ":" + StaticData.GetMapName(snapshot.mapid));
            }

            mainContext.Text1 = "Music: " + StaticData.GetMusicName(snapshot.musicid);
            mainContext.Text2 = "Map: " + StaticData.GetMapName(snapshot.mapid);
            mainContext.Text3 = gameState == null ? "" : ("Deaths: " + gameState.nDeaths// + " [" + gameState.nDeathsAlt + "]"
                                                 + "\n" + "Resets: " + gameState.nRestarts);// + " [" + gameState.nRestartsAlt + "]");

            mainContext.Text4 = "HP: " + snapshot.hp + " / " + snapshot.maxhp;
            mainContext.Text5 = "Amulet: " + snapshot.amulet + "\n" + "Boost: " + snapshot.boost;
            mainContext.Text6 = "MP: " + snapshot.mana + "\n" + "SP: " + snapshot.stamina;

            var nextHammer = StaticData.GetNextHammerLevel(snapshot.hammerXp);
            var nextRibbon = StaticData.GetNextRibbonLevel(snapshot.ribbonXp);
            var nextCarrot = StaticData.GetNextCarrotLevel(snapshot.carrotXp);
            mainContext.Text7 = "Hammer: " + snapshot.hammerXp + (nextHammer == null ? "" : ("/" + nextHammer.Item1 + "\n" + "NEXT: " + nextHammer.Item2));
            mainContext.Text8 = "Ribbon: " + snapshot.ribbonXp + (nextRibbon == null ? "" : ("/" + nextRibbon.Item1 + "\n" + "NEXT: " + nextRibbon.Item2));
            mainContext.Text9 = "Carrot: " + snapshot.carrotXp + (nextCarrot == null ? "" : ("/" + nextCarrot.Item1 + "\n" + "NEXT: " + nextCarrot.Item2));

            mainContext.Text10 = "x: " + snapshot.px + "\n" + "y: " + snapshot.py;
            mainContext.Text11 = "[A/H/M/P/R] ups:\n" + snapshot.nAttackUps + "/" + snapshot.nHpUps + "/" + snapshot.nManaUps + "/" + snapshot.nPackUps + "/" + snapshot.nRegenUps;


            mainContext.Text12 = "Entities: " + snapshot.entityArraySize + "\n" + "Active: " + snapshot.nActiveEntities;

            mainContext.Text13 = "Sprite: " + snapshot.GetCurrentSprite() + "\n" + "Action: " + snapshot.GetCurrentAction();
            mainContext.Text14 = "AnimFrame: " + snapshot.animationFrame;
            {
                string bosstext = "Boss Fight: " + (gameState.currentActivity == GameActivity.BOSS_BATTLE) + "\n";
                bosstext += "Bosses: " + snapshot.bossList.Count + "\n";
                foreach (var boss in snapshot.bossList)
                {
                    bosstext += "[" + boss.entityArrayIndex + "] " + StaticData.GetBossName(boss.id) + ": " + boss.hp + "/" + boss.maxHp + "\n";
                }

                mainContext.Text16 = bosstext;
            }



//// NEW CODE


                if (mainContext.veridx < 0) return;

                #region Read IGT

                int igt = MemoryHelper.GetMemoryValue<int>(process, StaticData.IGTAddr[mainContext.veridx]);
                if (igt > 0 && mainContext.Igt)
                {
                    sendigt((float)igt / 60);
                }

                #endregion

                #region Detect Reload

                bool reloaded = false;
                {
                    // When reloading, the frame numbers look like this:
                    // Case 1: (PLAYTIME frame steps down briefly before going to 0)
                    //         1108, 1109, 1110, 1110, 1110, 540, 0, 0, 0, 0, 0, 540, 540, 541, 542, 544,
                    // Case 2: (PLAYTIME frame goes straight to 0)
                    //         1108, 1109, 1110, 1110, 1110, 0, 0, 0, 0, 0, 540, 540, 541, 542, 544,
                    // This can sometimes cause reloads to be detected twice (which is usually not a problem though, but ocd lol)
                    // So we use a switch to "prime" the canReload flag whenever it detects the PLAYTIME increasing.
                    // the canReload flag is unset when a reload is detected, and remains unset until PLAYTIME starts increasing again.

                    int playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.PlaytimeAddr[mainContext.veridx]);
                    reloaded = playtime < mainContext.lastplaytime;

                    if (playtime > mainContext.lastplaytime) mainContext.canReload = true;
                    if (mainContext.canReload && playtime < mainContext.lastplaytime)
                    {
                        PracticeModeSendTrigger(SplitTrigger.Reload);
                        DebugLog("Game Reloaded!");
                        mainContext.canReload = false;
                    }
                    mainContext.lastplaytime = playtime;
                }
                // Forces splits regardless of reload status. Placed here so that reloads still run through the debug log.
                if (!mainContext.DontSplitOnReload)
                {
                    reloaded = false;
                }

                #endregion

                int mapid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MapAddress[mainContext.veridx]);
                if (mainContext.lastmapid != mapid)
                {
                    PracticeModeMapChangeTrigger(mainContext.lastmapid, mapid);
                    DebugLog("New Map: [" + mapid + "] " + StaticData.GetMapName(mapid));
                }

                int musicaddr = StaticData.MusicAddr[mainContext.veridx];
                int musicid = MemoryHelper.GetMemoryValue<int>(process, musicaddr);

                #region Detect Start Game

                if (musicid == 53)
                {
                    int blackness = MemoryHelper.GetMemoryValue<int>(process, StaticData.BlacknessAddr[mainContext.veridx]);
                    if (mainContext.previousBlackness == 0 && blackness >= 100000)
                    {
                        // Sudden increase by 100000
                        // Have to be careful, though. I don't know whether anything else causes blackness to increase by 100000
                        if (mainContext.AutoStart) SpeedrunSendStartTimer();
                        DebugLog("[Start] Game Started!");
                        mainContext.LastBossEnd = DateTime.Now;
                    }
                    mainContext.previousBlackness = blackness;
                }

                #endregion

                #region Non-Music Splits

                // Side Chapter
                if (mainContext.SplitOnSideCh)
                {
                    if (mainContext.lastmapid == 5 && mainContext.lastmusicid == 44 && (mapid != 5 || musicid != 44))
                    {
                        DebugLog("[Split] Finished Side Chapter");
                        SpeedrunSendSplit();
                    }
                }

                //Computer
                if (mainContext.SplitOnComputer)
                {
                    var newmoney = MemoryHelper.GetMemoryValue<int>(process, StaticData.MoneyAddress[mainContext.veridx]);
                    if (newmoney - mainContext.lastmoney == 17500)
                    {
                        DebugLog("[Split] Found Computer");
                        SpeedrunSendSplit();
                    }
                    mainContext.lastmoney = newmoney;
                }

                //Cyber Flower
                if (mainContext.SplitOnCyberFlower)
                {
                    int hasFlower = MemoryHelper.GetMemoryValue<int>(process, StaticData.CyberFlowerAddr[mainContext.veridx]);
                    if (hasFlower == 1 && mainContext.lastHasFlower == 0)
                    {
                        DebugLog("[Split] Obtained Cyber Flower");
                        mainContext.SplitOnCyberFlower = false;
                        SpeedrunSendSplit();
                    }
                    mainContext.lastHasFlower = hasFlower;
                }

                #endregion

                #region Music Splits

                if (musicid > 0)
                {
                    if (mainContext.lastmusicid != musicid)
                    {
                        PracticeModeMusicChangeTrigger(mainContext.lastmusicid, musicid);
                        DebugLog("New Music: [" + musicid + "] " + StaticData.GetMusicName(musicid));
                        mainContext.GameMusic = StaticData.GetMusicName(musicid);

                        if (musicid == 45 || musicid == 46 || musicid == 53)
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
                            if (mainContext.SplitOnCocoa1 && (mainContext.lastmusicid == 44 && musicid == 1))
                            {
                                if (!reloaded)
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
                            if (mainContext.SplitOnRibbon && (mainContext.lastmusicid == 44 && musicid == 2))
                            {
                                if (!reloaded)
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
                            if (mainContext.SplitOnAshuri1 && (mainContext.lastmusicid == 44 && musicid == 9))
                            {
                                if (!reloaded)
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
                            if ((mainContext.SplitOnBigBox || mainContext.SplitOnHoloMaid) && mainContext.lastmusicid == 33 && musicid == 19)
                            {
                                int entityArrayPtr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnemyPtrAddr[mainContext.veridx]);
                                float xPosition = MemoryHelper.GetMemoryValue<float>(process, entityArrayPtr + StaticData.EnemyEntityXPositionOffset[mainContext.veridx], false);

                                // 19830 is the rough X-Coordinate of the Save Point next to Sliding Powder
                                // Mr. Big Box
                                if (mainContext.SplitOnBigBox && xPosition < 19830)
                                {
                                    if(!reloaded)
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
                                if (mainContext.SplitOnHoloMaid && xPosition > 19830)
                                {
                                    if(!reloaded)
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
                            if (mainContext.SplitOnHoM && (mainContext.lastmusicid == 30 && musicid != 30))
                            {
                                DebugLog("[Split] Hall of Memories Completed");
                                mainContext.SplitOnHoM = false;
                                SpeedrunSendSplit();
                            }

                            // Forgotten Cave II
                            if (mainContext.SplitOnFC2 && (mainContext.lastmusicid == 6 && musicid != 6))
                            {
                                DebugLog("[Split] Forgotten Cave II Completed");
                                mainContext.SplitOnFC2 = false;
                                SpeedrunSendSplit();
                            }

                            // Library  
                            if (mainContext.SplitOnLibrary && (mapid == 1 && mainContext.lastmusicid != 42 && musicid == 42))
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


                        #region Old Code
                        /*else
                        {
                            var bossmusicflag = StaticData.BossMusics.Contains(musicid);
                            if (bossmusicflag)
                            {
                                if (mainContext.Bossbattle)
                                {
                                    if (mainContext.Noah1Reload && (mainContext.lastmusicid == 52 || musicid == 52))
                                    {
                                        DebugLog("noah 1 reload? ignore");
                                    }
                                    else
                                    {
                                        if (mainContext.MusicStart || mainContext.MusicEnd)
                                        {
                                            if (!mainContext.DontSplitOnReload || !reloaded) SpeedrunSendSplit();
                                            if (!reloaded) PracticeModeSendTrigger(SplitTrigger.BossEnd);
                                            DebugLog("new boss music, " + (reloaded ? "don't split (reload)" : "split"));
                                        }
                                        if (musicid == 37)
                                        {
                                            mainContext.Noah1Reload = true;
                                            DebugLog("noah1 music start, ignore MR forever");
                                        }
                                    }

                                    mainContext.lastmusicid = musicid;
                                    return;
                                }
                            }
                            if (!mainContext.Bossbattle)
                            {

                                if (musicid == 54 && mainContext.Alius1 && !mainContext.ForceAlius1)
                                {
                                    mainContext.Bossbattle = false;
                                    mainContext.Alius1 = false;
                                    DebugLog("Alius music, ignore once");

                                }
                                else if (musicid == 42 && mapid == 1 && mainContext.Irisu1)
                                {
                                    mainContext.Bossbattle = false;
                                    DebugLog("Irisu P1, ignore");

                                }
                                else
                                {
                                    if (bossmusicflag)
                                    {
                                        if (mapid == 5 && musicid == 44 && mainContext.SideCh)
                                        {
                                            mainContext.Bossbattle = false;
                                            DebugLog("sidechapter, ignore");

                                        }
                                        else
                                        {
                                            PracticeModeSendTrigger(SplitTrigger.BossStart);
                                            mainContext.Bossbattle = true;
                                            mainContext.lastbosslist = new List<int>();
                                            mainContext.lastnoah3hp = -1;
                                            if (musicid == 37)
                                            {
                                                mainContext.Noah1Reload = true;
                                                DebugLog("noah1 music start, ignore MR forever");
                                            }
                                            if (mainContext.MusicStart)
                                            {
                                                SpeedrunSendSplit();
                                                DebugLog("music start, split");

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!bossmusicflag) //boss music end!
                                {
                                    mainContext.Bossbattle = false;
                                    if (mainContext.MusicEnd)
                                    {
                                        if (!mainContext.DontSplitOnReload || !reloaded) SpeedrunSendSplit();
                                        if (!reloaded) PracticeModeSendTrigger(SplitTrigger.BossEnd);
                                        DebugLog(reloaded ? "music end, don't split (reload)" : "music end, split");

                                    }
                                }
                            }
                        }
                        mainContext.lastmusicid = musicid;*/
                        #endregion
                    }
                }
                else
                {
                    mainContext.GameMusic = "N/A";
                }
                mainContext.lastmusicid = musicid;
                mainContext.lastmapid = mapid;

                #endregion

                #region Old Stuff P.2

                if (mainContext.Bossbattle)
                {
                    if (mainContext.MiruDe || false)//todo noah3 option
                    {
                        int Noah3HP = -1;

                        if (StaticData.IsValidMap(mapid))
                        {
                            int ptr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnemyPtrAddr[mainContext.veridx]);
                            List<int> bosses = new List<int>();
                            for (var i = 0; i < 50; i++)
                            {
                                ptr = ptr + StaticData.EnemyEntitySize[mainContext.veridx];

                                var emyid = MemoryHelper.GetMemoryValue<int>(process,
                                    ptr + StaticData.EnemyEntityIDOffset[mainContext.veridx], false);
                                if (StaticData.IsBoss(emyid))
                                {
                                    bosses.Add(emyid);
                                    if (emyid == 1053)
                                    {
                                        Noah3HP = MemoryHelper.GetMemoryValue<int>(process,
                                            ptr + StaticData.EnemyEntityHPOffset[mainContext.veridx], false);
                                    }

                                }

                            }
                            if (mainContext.MiruDe && mapid==8)
                            {
                                foreach (var boss in mainContext.lastbosslist)
                                {
                                    if (boss == 1043)
                                    {
                                        if (!bosses.Contains(boss)) //despawn
                                        {
                                            SpeedrunSendSplit();
                                            DebugLog("miru despawn, split");
                                            mainContext.Bossbattle = false;
                                        }
                                    }
                                }
                            }

                            if (mainContext.Tm2 && musicid == 8)
                            {
                                bool f = true;
                                foreach (var boss in mainContext.lastbosslist)
                                {

                                    if (boss == 1024)
                                    {
                                        if (!bosses.Contains(boss)) //despawn
                                        {
                                            SpeedrunSendSplit();
                                            DebugLog("nixie despawn, split");
                                            mainContext.Bossbattle = false;
                                            f = false;
                                            break;
                                        }
                                    }
                                }

                                int newTM = MemoryHelper.GetMemoryValue<int>(process, StaticData.TownMemberAddr[mainContext.veridx]);
                                if (newTM - mainContext.lastTM == 1 && f) //for after 1.71 , 1.71 isn't TM+2 at once when skip Nixie, it's TM+1 twice
                                {
                                    if (DateTime.Now - mainContext.LastTMAddTime < TimeSpan.FromSeconds(1))
                                    {
                                        var d = DateTime.Now - mainContext.LastTMAddTime;
                                        mainContext.Bossbattle = false;
                                        SpeedrunSendSplit();
                                        DebugLog("TM+2 in " + d.TotalMilliseconds + " ms, split");
                                    }
                                    mainContext.LastTMAddTime = DateTime.Now;
                                }
                                else if (newTM - mainContext.lastTM == 2 && f)//for 1.65-1.70
                                {
                                    mainContext.Bossbattle = false;
                                    SpeedrunSendSplit();
                                    DebugLog("TM+2, split");
                                }
                                mainContext.lastTM = newTM;
                            }
                            mainContext.lastbosslist = bosses;
                            mainContext.lastnoah3hp = Noah3HP;
                        }
                    }
                }

                #endregion



//// END NEW CODE

            UpdateDebugArea(process);
            UpdateEntityData(process);

            prevSnapshot = snapshot;
        }

        private void StartNewGame()
        {
            gameState = new RabiGameState();
            gameStatus = GameStatus.INGAME;
        }

        private void ReturnToMenu()
        {
            gameStatus = GameStatus.MENU;
            gameState = null;
        }

        private bool InGame()
        {
            return gameStatus == GameStatus.INGAME;
        }

        private bool MusicChanged()
        {
            return prevSnapshot != null && prevSnapshot.musicid != snapshot.musicid;
        }

        private bool MusicChangedTo(Music music)
        {
            return MusicChanged() && snapshot.CurrentMusicIs(music);
        }
        
        private void UpdateEntityData(Process process)
        {
            // Read entire entity data for specific entity
            {
                var entityStatsList = debugContext.EntityStatsListData;

                int entitySize = StaticData.EnenyEntitySize[mainContext.veridx];
                int baseArrayPtr = snapshot.entityArrayPtr + entitySize * debugContext.EntityAnalysisIndex;
                int[] entitydataint = new int[entitySize / 4];
                float[] entitydatafloat = new float[entitySize / 4];

                debugContext.targetEntityListSize = entitySize / 4;
                int length = Math.Min(entitySize, entityStatsList.Count * 4);
                for (int i = 0; i < length; i += 4)
                {
                    int index = i / 4;
                    int value_int = MemoryHelper.GetMemoryValue<int>(process, baseArrayPtr + i, false);
                    float value_float = MemoryHelper.GetMemoryValue<float>(process, baseArrayPtr + i, false);
                    entityStatsList[index].IntVal = value_int;
                    entityStatsList[index].FloatVal = value_float;
                }
            }
        }

        private void UpdateDebugArea(Process process)
        {
            int ptr = snapshot.entityArrayPtr;
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                debugContext.BossList[i].BossID = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyHPOffset[mainContext.veridx], false);
                ptr += StaticData.EnenyEntitySize[mainContext.veridx];
            }

            debugContext.BossEvent = gameState.currentActivity == GameActivity.BOSS_BATTLE;
        }

        private void DebugLog(string log)
        {
            this.debugContext.Log(log);
        }

        private void sendsplit()
        {
            mainWindow.SendMessage("split\r\n");
        }

        private void sendreset()
        {
            mainWindow.SendMessage("reset\r\n");
        }

        private void sendstarttimer()
        {
            mainWindow.SendMessage("starttimer\r\n");
        }

        private void sendigt(float time)
        {
            mainWindow.SendMessage($"setgametime {time}\r\n");
        }

    }
}
