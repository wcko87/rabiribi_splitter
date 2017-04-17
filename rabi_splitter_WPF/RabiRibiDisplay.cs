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
