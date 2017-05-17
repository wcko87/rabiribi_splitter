using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace rabi_splitter_WPF
{
    enum GameStatus
    {
        INGAME,
        MENU
    }

    class RabiRibiState
    {
        public GameStatus gameStatus = GameStatus.MENU;
        public int lastValidMusicId = -1;
    }
}
