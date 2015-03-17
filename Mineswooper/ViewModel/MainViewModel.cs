﻿using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Mineswooper.DAL;
using Mineswooper.Model;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;

namespace Mineswooper.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        #region Privates
        private ObservableCollection<ScoreEntry> scores;
        private const int cellSize = 40;
        private bool scorePopupOpen = false;
        private bool rulesPopupOpen = false;
        private bool victoryOpen = false;
        private bool isUIEnabled = true;
        private string gameRules = "It's pacman m8, rack up those pellets to score and don't get eaten.";
        private GameField field = new GameField(23, 16);
        #endregion
        public MainViewModel()
        {
            PlayerScore = new ScoreEntry { Name = "Anonymous", Score = 0 };
            ShutdownCommand = new RelayCommand(() =>
            {
                //StringBuilder map = new StringBuilder();
                //foreach (var tile in field.Tiles)
                //{
                //    if (tile.IsTraversable) map.Append("T");
                //    else map.Append("W");
                //}
                //string strMap = map.ToString();
                //System.IO.File.WriteAllText(@"C:\Users\asus.pc\Desktop\job\task_projects\Mineswooper\Mineswooper\Model\defaultMap.txt", strMap);
                Application.Current.Shutdown();
            });
            //GameTileClick = new RelayCommand<Point>((p) => { field.ToggleTraversable(p); });
            FieldResetCommand = new RelayCommand(() => { field.ResetField(); });
            ShowRules = new RelayCommand(() => { RulesOpen = true; IsUIEnabled = false; });
            CloseRules = new RelayCommand(() => { RulesOpen = false; IsUIEnabled = true; });
            ShowScores = new RelayCommand(() => { ScoreOpen = true; IsUIEnabled = false; });
            CloseScores = new RelayCommand(() => { ScoreOpen = false; IsUIEnabled = true; });
            SaveScore = new RelayCommand(() =>
            {
                var entry = new ScoreEntry { Name = PlayerScore.Name, Date = PlayerScore.Date, Score = PlayerScore.Score };
                scores.Add(entry);
                scores = new ObservableCollection<ScoreEntry>(scores.OrderBy(e => e.Score).ToList());
                RaisePropertyChanged("ScoreEntries");
                using (var dc = new ScoreContext())
                {
                    dc.ScoreEntries.Add(entry);
                    dc.SaveChanges();
                }
                VictoryOpen = false;
                IsUIEnabled = true;
            });
            field.PropertyChanged += GameFieldEvent;
            using (var dc = new ScoreContext())
            {
                scores = new ObservableCollection<ScoreEntry>(dc.ScoreEntries.Select(e => e).OrderBy(e => e.Score).ToList());
            }

        }
        #region Relay commands
        public RelayCommand ShutdownCommand { get; set; }
        public RelayCommand<Point> GameTileClick { get; set; }
        public RelayCommand FieldResetCommand { get; set; }
        public RelayCommand ShowRules { get; set; }
        public RelayCommand ShowScores { get; set; }
        public RelayCommand CloseRules { get; set; }
        public RelayCommand CloseScores { get; set; }
        public RelayCommand SaveScore { get; set; }
        #endregion
        #region Public properties
        public ObservableCollection<ScoreEntry> ScoreEntries
        {
            get { return scores; }
            set
            {
                if (scores == value) return;
                scores = value;
                RaisePropertyChanged("ScoreEntries");
            }
        }
        public bool IsUIEnabled
        {
            get { return isUIEnabled; }
            set
            {
                if (isUIEnabled == value) return;
                isUIEnabled = value;
                RaisePropertyChanged("IsUIEnabled");
            }
        }
        public bool RulesOpen
        {
            get { return rulesPopupOpen; }
            set
            {
                if (rulesPopupOpen == value) return;
                rulesPopupOpen = value;
                RaisePropertyChanged("RulesOpen");
            }
        }
        public bool ScoreOpen
        {
            get { return scorePopupOpen; }
            set
            {
                if (scorePopupOpen == value) return;
                scorePopupOpen = value;
                RaisePropertyChanged("ScoreOpen");
            }
        }
        public bool VictoryOpen
        {
            get { return victoryOpen; }
            set
            {
                if (victoryOpen == value) return;
                victoryOpen = value;
                RaisePropertyChanged("VictoryOpen");
            }
        }
        public int FieldWidth { get { return cellSize * (int)field.Size.X; } }
        public int FieldHeight { get { return cellSize * (int)field.Size.Y; } }
        public ScoreEntry PlayerScore { get; set; }
        public string Rules { get { return gameRules; } }
        public int CellSize { get { return cellSize; } }
        public ObservableCollection<GameTile> Field { get { return field.Tiles; } }
        public int Rows { get { return (int)field.Size.Y; } }
        public int Cols { get { return (int)field.Size.X; } }
        public int Score { get { return field.Score; } }
        #endregion
        private void GameFieldEvent(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Score":
                    RaisePropertyChanged("Score");
                    break;
                case "PresumedMines":
                    RaisePropertyChanged("Mines");
                    break;
                case "Victory":
                    PlayerScore.Score = field.Score;
                    PlayerScore.Date = DateTime.Now;
                    VictoryOpen = true;
                    IsUIEnabled = false;
                    break;
                case "Defeat":
                    break;
            }
        }
    }
}