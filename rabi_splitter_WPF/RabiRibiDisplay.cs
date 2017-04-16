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

        public int previousBlackness = -1;
        public int lastplaytime = 0;
        public bool bossbattle;
        public List<int> lastbosslist;
        public DateTime LastTMAddTime;
        

        public RabiRibiDisplay(MainContext mainContext, DebugContext debugContext, MainWindow mainWindow)
        {
            this.mainContext = mainContext;
            this.debugContext = debugContext;
            this.mainWindow = mainWindow;
            StartNewGame();
        }
        
        public void ReadMemory(Process process)
        {
            // Snapshot Game Memory
            snapshot = new MemorySnapshot(process, mainContext.veridx);

            mainContext.Text1 = "Music: " + StaticData.GetMusicName(snapshot.musicid);
            mainContext.Text2 = "Map: " +  StaticData.GetMapName(snapshot.mapid);
            mainContext.Text3 = gameState == null ? "" : ("Deaths: " + gameState.nDeaths + "\n" + "Resets: " + gameState.nRestarts);

            mainContext.Text4 = "HP: " + snapshot.hp;
            mainContext.Text5 = "MaxHP: " + snapshot.maxhp;

            mainContext.Text6 = "Amulet: " + snapshot.amulet;
            mainContext.Text7 = "Boost: " + snapshot.boost;
            mainContext.Text8 = "Mana: " + snapshot.mana;
            mainContext.Text9 = "Stamina: " + snapshot.stamina;

            mainContext.Text10 = "x: " + snapshot.px;
            mainContext.Text11 = "y: " + snapshot.py;

            #region Detect Reload

            bool reloaded = false;
            if (prevSnapshot != null) {
                reloaded = snapshot.playtime != 0 && snapshot.playtime < prevSnapshot.playtime;
                if (reloaded)
                {
                    if (InGame())
                    {
                        gameState.nRestarts++;
                    }
                    DebugLog("Reload Game!");
                }
            }

            #endregion

            #region Detect Death

            bool died = false;
            if (prevSnapshot != null)
            {
                died = snapshot.hp == 0 && prevSnapshot.hp > 0;
                if (died)
                {
                    if (InGame())
                    {
                        gameState.nDeaths++;
                    }
                    DebugLog("Death!");
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


            #region checkTM



            #endregion

            UpdateDebugArea(process);

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
