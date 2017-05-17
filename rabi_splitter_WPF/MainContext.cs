using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Data;
using rabi_splitter_WPF.Annotations;
using System.Windows;

namespace rabi_splitter_WPF
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        // Code taken from http://stackoverflow.com/a/2427307
        enum Parameter
        {
            VisibleWhenTrue, VisibleWhenFalse
        }

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            var boolValue = (bool)value;
            var direction = (Parameter)Enum.Parse(typeof(Parameter), (string)parameter);

            if (direction == Parameter.VisibleWhenTrue) return boolValue ? Visibility.Visible : Visibility.Collapsed;
            else return boolValue ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            return null;
        }
        #endregion
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter,
            System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
    public class BossData:INotifyPropertyChanged
    {
        private int _bossIdx;
        private int _bossId;
        private int _bossHp;

        public int BossIdx
        {
            get { return _bossIdx; }
            set
            {
                if (value == _bossIdx) return;
                _bossIdx = value;
                OnPropertyChanged(nameof(BossIdx));
            }
        }

        public int BossID
        {
            get { return _bossId; }
            set
            {
                if (value == _bossId) return;
                _bossId = value;
                OnPropertyChanged(nameof(BossID));
            }
        }

        public int BossHP
        {
            get { return _bossHp; }
            set
            {
                if (value == _bossHp) return;
                _bossHp = value;
                OnPropertyChanged(nameof(BossHP));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class DebugContext : INotifyPropertyChanged
    {
        private bool _bossEvent;
        private string _debugLog;

        public ObservableCollection<BossData> BossList = new ObservableCollection<BossData>();

        public DebugContext()
        {
            BossList=new ObservableCollection<BossData>();
            for (int i = 0; i < 50; i++)
            {
                BossList.Add(new BossData()
                {
                    BossIdx = i
                });
            }
        }

        public bool BossEvent
        {
            get { return _bossEvent; }
            set
            {
                if (value == _bossEvent) return;
                _bossEvent = value;
                OnPropertyChanged(nameof(BossEvent));
            }
        }

        public string DebugLog
        {
            get { return _debugLog; }
            set
            {
                if (value == _debugLog) return;
                _debugLog = value;
                OnPropertyChanged(nameof(DebugLog));
            }
        }

        public void Log(string message)
        {
            this.DebugLog += message + "\n";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    class MainContext : INotifyPropertyChanged
    {
        #region _general
        public bool _autoStart;
        public bool _autoReset;
        public bool _advancedSettings;
        private bool _practiceMode;
        #endregion

        #region _advanced
        private bool _debugMode;
        private bool _dontSplitOnReload;
        private bool _splitOnSideCh;
        private bool _splitOnCyberFlower;
        private bool _splitOnShRita;
        private bool _splitOnShPandora;
        #endregion

        #region _prologue
        private bool _splitOnCocoa1;
        private bool _splitOnRibbon;
        private bool _splitOnAshuri1;
        #endregion

        #region _miscellaneous
        private bool _splitOnBigBox;
        private bool _splitOnHoloMaid;
        private bool _splitOnComputer;
        private bool _splitOnHoM;
        private bool _splitOnFC2;
        private bool _splitOnLibrary;
        #endregion

        #region _bosses
        //Bosses 1
        private bool _splitOnRumi;
        private bool _splitOnRita;
        private bool _splitOnNieve;
        private bool _splitOnNixie;
        private bool _splitOnAruraune;
        private bool _splitOnPandora;
        private bool _splitOnIrisu;
        private bool _splitOnSaya;
        private bool _splitOnCicini;
        private bool _splitOnSyaro;

        //Bosses 2
        private bool _splitOnCocoa2;
        private bool _splitOnAshuri2;
        private bool _splitOnLilith1;
        private bool _splitOnLilith2;
        private bool _splitOnVanilla;
        private bool _splitOnChocolate;
        private bool _splitOnKotri1;
        private bool _splitOnKotri2;
        private bool _splitOnKotri3;
        private bool _splitOnKeke;

        //Bosses 3
        private bool _splitOnSeana1;
        private bool _splitOnSeana2;
        private bool _splitOnMiriam;
        private bool _splitOnMiru;
        private bool _splitOnNoah1;
        private bool _splitOnNoah2;
        private bool _splitOnNoah3;
        private bool _splitOnAlius1;
        private bool _splitOnAlius2;
        private bool _splitOnAlius3;
        #endregion

        #region _unsorted
        private bool _musicStart;
        private bool _musicEnd;
        private bool _miruDe; 
        private bool _alius1;
        private bool _tm2;
        private bool _irisu1;
        #endregion
        
        private int _serverPort;
        private string _gameVer;
        private string _gameMusic;
        private bool _igt;


        public string BOSSTimer
        {
            get
            {
                if (LastBossStart == null) return "N/A";
                else
                {
                    var dt = DateTime.Now;
                    if (LastBossEnd.HasValue && LastBossStart.Value<LastBossEnd.Value)
                    {
                        dt = LastBossEnd.Value;
                    }
                    var ts = dt- LastBossStart.Value;
                    return ts.ToString(@"mm\:ss\.f");
                }
                
            }
        }
        public string RoutingTimer
        {
            get
            {
                if (LastBossEnd == null) return "N/A";
                else
                {
                    var dt = DateTime.Now;
                    if (LastBossStart.HasValue && LastBossStart.Value > LastBossEnd.Value)
                    {
                        dt = LastBossStart.Value;
                    }
                    var ts = dt - LastBossEnd.Value;
                    return ts.ToString(@"mm\:ss\.f");
                }
            }
        }
        public DateTime? LastBossStart;
        public DateTime? LastBossEnd;

        #region General
        public bool AutoStart
        {
            get { return _autoStart; }
            set
            {
                if (value == _autoStart) return;
                _autoStart = value;
                OnPropertyChanged(nameof(AutoStart));
            }
        }
        public bool AutoReset
        {
            get { return _autoReset; }
            set
            {
                if (value == _autoReset) return;
                _autoReset = value;
                OnPropertyChanged(nameof(AutoReset));

            }
        }
        public bool AdvancedSettings
        {
            get { return _advancedSettings; }
            set
            {
                if (value == _advancedSettings) return;
                _advancedSettings = value;
                OnPropertyChanged(nameof(AdvancedSettings));
            }
        }
        public bool PracticeMode
        {
            get { return _practiceMode; }
            set
            {
                if (value == _practiceMode) return;
                _practiceMode = value;
                OnPropertyChanged(nameof(PracticeMode));
            }
        }
        #endregion

        #region Advanced
        public bool DebugMode
        {
            get { return _debugMode; }
            set
            {
                if (value == _debugMode) return;
                _debugMode = value;
                OnPropertyChanged(nameof(DebugMode));
            }
        }
        public bool DontSplitOnReload
        {
            get { return _dontSplitOnReload; }
            set
            {
                if (value == _dontSplitOnReload) return;
                _dontSplitOnReload = value;
                OnPropertyChanged(nameof(DontSplitOnReload));
            }
        }
        public bool SplitOnSideCh
        {
            get { return _splitOnSideCh; }
            set
            {
                if (value == _splitOnSideCh) return;
                _splitOnSideCh = value;
                OnPropertyChanged(nameof(SplitOnSideCh));
            }
        }
        public bool SplitOnCyberFlower
        {
            get { return _splitOnCyberFlower; }
            set
            {
                if (value == _splitOnCyberFlower) return;
                _splitOnCyberFlower = value;
                OnPropertyChanged(nameof(SplitOnCyberFlower));
            }
        }
        public bool SplitOnShRita
        {
            get { return _splitOnShRita; }
            set
            {
                if (value == _splitOnShRita) return;
                _splitOnShRita = value;
                OnPropertyChanged(nameof(SplitOnShRita));
            }
        }
        public bool SplitOnShPandora
        {
            get { return _splitOnShPandora; }
            set
            {
                if (value == _splitOnShPandora) return;
                _splitOnShPandora = value;
                OnPropertyChanged(nameof(SplitOnShPandora));
            }
        }
        #endregion

        #region Prologue
        public bool SplitOnCocoa1
        {
            get { return _splitOnCocoa1; }
            set
            {
                if (value == _splitOnCocoa1) return;
                _splitOnCocoa1 = value;
                OnPropertyChanged(nameof(SplitOnCocoa1));
            }
        }
        public bool SplitOnRibbon
        {
            get { return _splitOnRibbon; }
            set
            {
                if (value == _splitOnRibbon) return;
                _splitOnRibbon = value;
                OnPropertyChanged(nameof(SplitOnRibbon));
            }
        }
        public bool SplitOnAshuri1
        {
            get { return _splitOnAshuri1; }
            set
            {
                if (value == _splitOnAshuri1) return;
                _splitOnAshuri1 = value;
                OnPropertyChanged(nameof(SplitOnAshuri1));
            }
        }
        #endregion

        #region Miscellaneous
        public bool SplitOnBigBox
        {
            get { return _splitOnBigBox; }
            set
            {
                if (value == _splitOnBigBox) return;
                _splitOnBigBox = value;
                OnPropertyChanged(nameof(SplitOnBigBox));
            }
        }
        public bool SplitOnHoloMaid
        {
            get { return _splitOnHoloMaid; }
            set
            {
                if (value == _splitOnHoloMaid) return;
                _splitOnHoloMaid = value;
                OnPropertyChanged(nameof(SplitOnHoloMaid));
            }
        }
        public bool SplitOnComputer
        {
            get { return _splitOnComputer; }
            set
            {
                if (value == _splitOnComputer) return;
                _splitOnComputer = value;
                OnPropertyChanged(nameof(SplitOnComputer));
            }
        }
        public bool SplitOnHoM
        {
            get { return _splitOnHoM; }
            set
            {
                if (value == _splitOnHoM) return;
                _splitOnHoM = value;
                OnPropertyChanged(nameof(SplitOnHoM));
            }
        }
        public bool SplitOnFC2
        {
            get { return _splitOnFC2; }
            set
            {
                if (value == _splitOnFC2) return;
                _splitOnFC2 = value;
                OnPropertyChanged(nameof(SplitOnFC2));
            }
        }
        public bool SplitOnLibrary
        {
            get { return _splitOnLibrary; }
            set
            {
                if (value == _splitOnLibrary) return;
                _splitOnLibrary = value;
                OnPropertyChanged(nameof(SplitOnLibrary));
            }
        }
        #endregion

        #region Bosses
        //Bosses 1
        public bool SplitOnRumi
        {
            get { return _splitOnRumi; }
            set
            {
                if (value == _splitOnRumi) return;
                _splitOnRumi = value;
                OnPropertyChanged(nameof(SplitOnRumi));
            }
        }
        public bool SplitOnRita
        {
            get { return _splitOnRita; }
            set
            {
                if (value == _splitOnRita) return;
                _splitOnRita = value;
                OnPropertyChanged(nameof(SplitOnRita));
            }
        }
        public bool SplitOnNieve
        {
            get { return _splitOnNieve; }
            set
            {
                if (value == _splitOnNieve) return;
                _splitOnNieve = value;
                OnPropertyChanged(nameof(SplitOnNieve));
            }
        }
        public bool SplitOnNixie
        {
            get { return _splitOnNixie; }
            set
            {
                if (value == _splitOnNixie) return;
                _splitOnNixie = value;
                OnPropertyChanged(nameof(SplitOnNixie));
            }
        }
        public bool SplitOnAruraune
        {
            get { return _splitOnAruraune; }
            set
            {
                if (value == _splitOnAruraune) return;
                _splitOnAruraune = value;
                OnPropertyChanged(nameof(SplitOnAruraune));
            }
        }
        public bool SplitOnPandora
        {
            get { return _splitOnPandora; }
            set
            {
                if (value == _splitOnPandora) return;
                _splitOnPandora = value;
                OnPropertyChanged(nameof(SplitOnPandora));
            }
        }
        public bool SplitOnIrisu
        {
            get { return _splitOnIrisu; }
            set
            {
                if (value == _splitOnIrisu) return;
                _splitOnIrisu = value;
                OnPropertyChanged(nameof(SplitOnIrisu));
            }
        }
        public bool SplitOnSaya
        {
            get { return _splitOnSaya; }
            set
            {
                if (value == _splitOnSaya) return;
                _splitOnSaya = value;
                OnPropertyChanged(nameof(SplitOnSaya));
            }
        }
        public bool SplitOnCicini
        {
            get { return _splitOnCicini; }
            set
            {
                if (value == _splitOnCicini) return;
                _splitOnCicini = value;
                OnPropertyChanged(nameof(SplitOnCicini));
            }
        }
        public bool SplitOnSyaro
        {
            get { return _splitOnSyaro; }
            set
            {
                if (value == _splitOnSyaro) return;
                _splitOnSyaro = value;
                OnPropertyChanged(nameof(SplitOnSyaro));
            }
        }
        //Bosses 2
        public bool SplitOnCocoa2
        {
            get { return _splitOnCocoa2; }
            set
            {
                if (value == _splitOnCocoa2) return;
                _splitOnCocoa2 = value;
                OnPropertyChanged(nameof(SplitOnCocoa2));
            }
        }
        public bool SplitOnAshuri2
        {
            get { return _splitOnAshuri2; }
            set
            {
                if (value == _splitOnAshuri2) return;
                _splitOnAshuri2 = value;
                OnPropertyChanged(nameof(SplitOnAshuri2));
            }
        }
        public bool SplitOnLilith1
        {
            get { return _splitOnLilith1; }
            set
            {
                if (value == _splitOnLilith1) return;
                _splitOnLilith1 = value;
                OnPropertyChanged(nameof(SplitOnLilith1));
            }
        }
        public bool SplitOnLilith2
        {
            get { return _splitOnLilith2; }
            set
            {
                if (value == _splitOnLilith2) return;
                _splitOnLilith2 = value;
                OnPropertyChanged(nameof(SplitOnLilith2));
            }
        }
        public bool SplitOnVanilla
        {
            get { return _splitOnVanilla; }
            set
            {
                if (value == _splitOnVanilla) return;
                _splitOnVanilla = value;
                OnPropertyChanged(nameof(SplitOnVanilla));
            }
        }
        public bool SplitOnChocolate
        {
            get { return _splitOnChocolate; }
            set
            {
                if (value == _splitOnChocolate) return;
                _splitOnChocolate = value;
                OnPropertyChanged(nameof(SplitOnChocolate));
            }
        }
        public bool SplitOnKotri1
        {
            get { return _splitOnKotri1; }
            set
            {
                if (value == _splitOnKotri1) return;
                _splitOnKotri1 = value;
                OnPropertyChanged(nameof(SplitOnKotri1));
            }
        }
        public bool SplitOnKotri2
        {
            get { return _splitOnKotri2; }
            set
            {
                if (value == _splitOnKotri2) return;
                _splitOnKotri2 = value;
                OnPropertyChanged(nameof(SplitOnKotri2));
            }
        }
        public bool SplitOnKotri3
        {
            get { return _splitOnKotri3; }
            set
            {
                if (value == _splitOnKotri3) return;
                _splitOnKotri3 = value;
                OnPropertyChanged(nameof(SplitOnKotri3));
            }
        }
        public bool SplitOnKeke
        {
            get { return _splitOnKeke; }
            set
            {
                if (value == _splitOnKeke) return;
                _splitOnKeke = value;
                OnPropertyChanged(nameof(SplitOnKeke));
            }
        }
        //Bosses 3
        public bool SplitOnSeana1
        {
            get { return _splitOnSeana1; }
            set
            {
                if (value == _splitOnSeana1) return;
                _splitOnSeana1 = value;
                OnPropertyChanged(nameof(SplitOnSeana1));
            }
        }
        public bool SplitOnSeana2
        {
            get { return _splitOnSeana2; }
            set
            {
                if (value == _splitOnSeana2) return;
                _splitOnSeana2 = value;
                OnPropertyChanged(nameof(SplitOnSeana2));
            }
        }
        public bool SplitOnMiriam
        {
            get { return _splitOnMiriam; }
            set
            {
                if (value == _splitOnMiriam) return;
                _splitOnMiriam = value;
                OnPropertyChanged(nameof(SplitOnMiriam));
            }
        }
        public bool SplitOnMiru
        {
            get { return _splitOnMiru; }
            set
            {
                if (value == _splitOnMiru) return;
                _splitOnMiru = value;
                OnPropertyChanged(nameof(SplitOnMiru));
            }
        }
        public bool SplitOnNoah1
        {
            get { return _splitOnNoah1; }
            set
            {
                if (value == _splitOnNoah1) return;
                _splitOnNoah1 = value;
                OnPropertyChanged(nameof(SplitOnNoah1));
            }
        }
        public bool SplitOnNoah2
        {
            get { return _splitOnNoah2; }
            set
            {
                if (value == _splitOnNoah2) return;
                _splitOnNoah2 = value;
                OnPropertyChanged(nameof(SplitOnNoah2));
            }
        }
        public bool SplitOnNoah3
        {
            get { return _splitOnNoah3; }
            set
            {
                if (value == _splitOnNoah3) return;
                _splitOnNoah3 = value;
                OnPropertyChanged(nameof(SplitOnNoah3));
            }
        }
        public bool SplitOnAlius1
        {
            get { return _splitOnAlius1; }
            set
            {
                if (value == _splitOnAlius1) return;
                _splitOnAlius1 = value;
                OnPropertyChanged(nameof(SplitOnAlius1));
            }
        }
        public bool SplitOnAlius2
        {
            get { return _splitOnAlius2; }
            set
            {
                if (value == _splitOnAlius2) return;
                _splitOnAlius2 = value;
                OnPropertyChanged(nameof(SplitOnAlius2));
            }
        }
        public bool SplitOnAlius3
        {
            get { return _splitOnAlius3; }
            set
            {
                if (value == _splitOnAlius3) return;
                _splitOnAlius3 = value;
                OnPropertyChanged(nameof(SplitOnAlius3));
            }
        }
        #endregion

        #region Unsorted
        public bool MiruDe
        {
            get { return _miruDe; }
            set
            {
                if (value == _miruDe) return;
                _miruDe = value;
                OnPropertyChanged(nameof(MiruDe));
            }
        }
        private bool _forceAlius1;
        public bool Alius1
        {
            get { return _alius1; }
            set
            {
                if (value == _alius1) return;
                _alius1 = value;
                OnPropertyChanged(nameof(Alius1));
            }
        }
        public bool Tm2
        {
            get { return _tm2; }
            set
            {
                if (value == _tm2) return;
                _tm2 = value;
                OnPropertyChanged(nameof(Tm2));
            }
        }
        public bool Irisu1
        {
            get { return _irisu1; }
            set
            {
                if (value == _irisu1) return;
                _irisu1 = value;
                OnPropertyChanged(nameof(Irisu1));
            }
        }
        public bool Noah1Reload
        {
            get { return _noah1Reload; }
            set
            {
                if (value == _noah1Reload) return;
                _noah1Reload = value;
                OnPropertyChanged(nameof(Noah1Reload));
            }
        }
        public bool MusicStart
        {
            get { return _musicStart; }
            set
            {
                if (value == _musicStart) return;
                _musicStart = value;
                OnPropertyChanged(nameof(MusicStart));
            }
        }
        public bool MusicEnd
        {
            get { return _musicEnd; }
            set
            {
                if (value == _musicEnd) return;
                _musicEnd = value;
                OnPropertyChanged(nameof(MusicEnd));
            }
        }
        public int ServerPort
        {
            get { return _serverPort; }
            set
            {
                if (value == _serverPort) return;
                _serverPort = value;
                OnPropertyChanged(nameof(ServerPort));
            }
        }
        public string GameVer
        {
            get { return _gameVer; }
            set
            {
                if (value == _gameVer) return;
                _gameVer = value;
                OnPropertyChanged(nameof(GameVer));
            }
        }
        public string GameMusic
        {
            get { return _gameMusic; }
            set
            {
                if (value == _gameMusic) return;
                _gameMusic = value;
                OnPropertyChanged(nameof(GameMusic));
            }
        }
        public bool Igt
        {
            get { return _igt; }
            set
            {
                if (value == _igt) return;
                _igt = value;
                OnPropertyChanged(nameof(Igt));
            }
        }
        public bool ForceAlius1
        {
            get { return _forceAlius1; }
            set
            {
                if (value == _forceAlius1) return;
                _forceAlius1 = value;
                OnPropertyChanged(nameof(ForceAlius1));
            }
        }
        public bool Bossbattle
        {
            get { return bossbattle; }
            set
            {
                if (value == bossbattle) return;
                bossbattle = value;
                if (value)
                {
                    this.LastBossStart = DateTime.Now;
                }
                else
                {
                    this.LastBossEnd=DateTime.Now;
                }
               
               
            }
        }
        
        public string oldtitle;
        public int veridx;
        /*public int lastHasFlower;

        private bool bossbattle;
        public List<int> lastbosslist;
        public int lastnoah3hp;
        public int lastTM;
        public DateTime LastTMAddTime;
        private bool _noah1Reload;*/
        #endregion


        public MainContext()
        {
            //General Settings
            this.AutoStart = false;
            this.AutoReset = false;
            this.AdvancedSettings = false;
            this.ServerPort = 16834;

            //Additional Settings
            this.DebugMode = false;
            this.DontSplitOnReload = true;
            this.SplitOnSideCh = false;
            this.SplitOnCyberFlower = true;
            this.SplitOnShRita = false;
            this.SplitOnShPandora = false;

            //Prologue
            this.SplitOnCocoa1 = true;
            this.SplitOnRibbon = true;
            this.SplitOnAshuri1 = true;

            //Miscellaneous
            this.SplitOnBigBox = false;
            this.SplitOnHoloMaid = false;
            this.SplitOnComputer = true;
            this.SplitOnHoM = false;
            this.SplitOnFC2 = false;
            this.SplitOnLibrary = false;

            //Bosses 1
            this.SplitOnRumi = false;
            this.SplitOnRita = true;
            this.SplitOnNieve = true;
            this.SplitOnNixie = true;
            this.SplitOnAruraune = false;
            this.SplitOnPandora = true;
            this.SplitOnIrisu = false;
            this.SplitOnSaya = false;
            this.SplitOnCicini = true;
            this.SplitOnSyaro = true;

            //Bosses 2
            this.SplitOnCocoa2 = true;
            this.SplitOnAshuri2 = true;
            this.SplitOnLilith1 = false;
            this.SplitOnLilith2 = false;
            this.SplitOnVanilla = true;
            this.SplitOnChocolate = true;
            this.SplitOnKotri1 = true;
            this.SplitOnKotri2 = true;
            this.SplitOnKotri3 = true;
            this.SplitOnKeke = false;

            //Bosses 3
            this.SplitOnSeana1 = false;
            this.SplitOnSeana2 = false;
            this.SplitOnMiriam = false;
            this.SplitOnMiru = true;
            this.SplitOnNoah1 = true;
            this.SplitOnNoah2 = false;
            this.SplitOnNoah3 = true;
            this.SplitOnAlius1 = false;
            this.SplitOnAlius2 = true;
            this.SplitOnAlius3 = true;

            //Other + Unsorted
            this.MusicEnd = true;
            this.MusicStart = false;
            this.MiruDe = true;          
            this.Alius1 = true;
            this.Tm2 = true;
            this.Irisu1 = true;      
            this.Igt = true;
            this.Noah1Reload = false;
            this.ForceAlius1 = false;
        }

        public void NotifyTimer()
        {
            OnPropertyChanged(nameof(BOSSTimer));
            OnPropertyChanged(nameof(RoutingTimer));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

