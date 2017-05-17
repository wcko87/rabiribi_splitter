using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    class RabiRibiDisplay
    {
        private MainContext mainContext;
        private DebugContext debugContext;
        private MainWindow mainWindow;

        public int previousBlackness = -1;
        public int lastmoney;
        public int lastmapid;
        public int lastmusicid;
        public int lastplaytime = 0;
        public bool bossbattle;
        public List<int> lastbosslist;
        public int lastnoah3hp;
        public int lastTM;
        public DateTime LastTMAddTime;
        private bool _noah1Reload;


        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, MainWindow mainWindow)
        {
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.mainWindow = mainWindow;
        }

        public void ReadMemory(Process process)
        {
            #region read igt

            int igt = MemoryHelper.GetMemoryValue<int>(process, StaticData.IGTAddr[mainContext.veridx]);
            if (igt > 0 && mainContext.Igt)
            {
                sendigt((float)igt / 60);
            }

            #endregion

            #region Detect Reload

            bool reloaded = false;
            {
                int playtime = MemoryHelper.GetMemoryValue<int>(process, StaticData.PlaytimeAddr[mainContext.veridx]);
                reloaded = playtime != 0 && playtime < lastplaytime;
                if (reloaded) DebugLog("Reload Game!");
                lastplaytime = playtime;
            }

            #endregion

            #region Detect Start Game

            {
                int blackness = MemoryHelper.GetMemoryValue<int>(process, StaticData.BlacknessAddr[mainContext.veridx]);
                if (previousBlackness == 0 && blackness >= 100000)
                {
                    // Sudden increase by 100000
                    // Have to be careful, though. I don't know whether anything else causes blackness to increase by 100000
                    DebugLog("Start Game!");
                }
                previousBlackness = blackness;
            }

            #endregion

            #region CheckMoney
            
            {
                var newmoney = MemoryHelper.GetMemoryValue<int>(process, StaticData.MoneyAddress[mainContext.veridx]);
                if (newmoney - lastmoney == 17500)
                {
                    sendsplit();
                    DebugLog("get 17500 en, split");
                }
                lastmoney = newmoney;
            }

            #endregion

            int mapid = MemoryHelper.GetMemoryValue<int>(process, StaticData.MapAddress[mainContext.veridx]);
            if (lastmapid != mapid)
            {
                DebugLog("newmap: " + mapid + ":" + StaticData.MapNames[mapid]);
                lastmapid = mapid;
            }


            #region checkTM



            #endregion



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
        }

        private void UpdateDebugArea(Process process)
        {
            int ptr = MemoryHelper.GetMemoryValue<int>(process, StaticData.EnenyPtrAddr[mainContext.veridx]);
            //                    List<int> bosses = new List<int>();
            //                    List<int> HPS = new List<int>();
            //                    this.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() => debugContext.BossList.Clear()));
            //                    ptr += StaticData.EnenyEntitySize[mainContext.veridx] * 3;
            for (var i = 0; i < 50; i++)
            {
                ptr += StaticData.EnenyEntitySize[mainContext.veridx];
                debugContext.BossList[i].BossID = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyIDOffset[mainContext.veridx], false);
                debugContext.BossList[i].BossHP = MemoryHelper.GetMemoryValue<int>(process,
                    ptr + StaticData.EnenyEnitiyHPOffset[mainContext.veridx], false);
            }

            debugContext.BossEvent = bossbattle;
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
