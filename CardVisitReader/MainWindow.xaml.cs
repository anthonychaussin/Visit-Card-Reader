using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace CardVisitReader
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private VisitCard selectedCard;

        public string Entreprise { get { return this.SelectedCard?.Entreprise; } }
        public string CName { get { return this.SelectedCard?.CName; } }
        public string Email { get { return this.SelectedCard?.Email; } }
        public string Phone { get { return this.SelectedCard?.Phone; } }
        public string Adresse { get { return this.SelectedCard?.Adresse; } }
        public string Web { get { return this.SelectedCard?.Web; } }
        public string Twiter { get { return this.SelectedCard?.Twiter; } }

        public VisitCard SelectedCard
        {
            get => selectedCard;
            set
            {
                selectedCard = value;
                this.OnPropertyChanged(nameof(this.Web));
                this.OnPropertyChanged(nameof(this.Twiter));
                this.OnPropertyChanged(nameof(this.Adresse));
                this.OnPropertyChanged(nameof(this.Email));
                this.OnPropertyChanged(nameof(this.Phone));
                this.OnPropertyChanged(nameof(this.Entreprise));
                this.OnPropertyChanged(nameof(this.CName));
            }
        }

        private string FolderPicturePath
        {
            get
            {
                return Properties.Settings.Default.PictureFolder;
            }
            set
            {
                Properties.Settings.Default.PictureFolder = value;
                Properties.Settings.Default.Save();
                this.LoadCards();
            }
        }

        private string CSVFilePath
        {
            get
            {
                return Properties.Settings.Default.CSVFile;
            }
            set
            {
                Properties.Settings.Default.CSVFile = value;
                Properties.Settings.Default.Save();
            }
        }

        /// <summary>
        /// Event when a property change
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void LoadCards()
        {
            new DirectoryInfo(this.FolderPicturePath).GetFiles("*", SearchOption.AllDirectories).ToList().ForEach(file =>
            {
                VisitCard card = new VisitCard() { PictureFile = file };
                card.CardDlicked += this.ClickCard;
                this.CardList.Items.Add(card);
            }
            );
        }

        private void ClickCard(object sender, EventArgs e)
        {
            this.SelectedCard = (VisitCard)sender;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender == this.CSV)
            {
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog() { IsFolderPicker = true })
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        this.CSVFilePath = dialog.FileName + @"\VistiCardData.csv";
                    }
                }
            }
            else if (sender == this.Picture)
            {
                this.CardList.Items.Clear();
                using (CommonOpenFileDialog dialog = new CommonOpenFileDialog() { IsFolderPicker = true })
                {
                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        this.FolderPicturePath = dialog.FileName;
                    }
                }
            }
            else if (sender == this.CreateCSV)
            {
                List<string> finalLines = new List<string>();
                Mouse.OverrideCursor = Cursors.Wait;
                this.CardList.Items.OfType<VisitCard>().ToList().ForEach(c => c.ForceLoad());
                string firstLine = "Nom;Entreprise;Email;Phone;Web;Image";
                List<string> lines = this.CardList.Items.OfType<VisitCard>().ToList().ConvertAll(c => c.CName.Replace("\n", "").Replace(";", ",") + ";" + c.Entreprise.Replace("\n", "").Replace(";", ",") + ";" + c.Email.Replace("\n", "").Replace(";", ",") + ";" + c.Phone.Replace("\n", "").Replace(";", ",") + ";" + c.Web.Replace("\n", "").Replace(";", ",") + ";" + c.File);
                if (File.Exists(this.CSVFilePath))
                {
                    finalLines.AddRange(File.ReadAllLines(this.CSVFilePath).ToList());
                    File.Delete(this.CSVFilePath);
                }
                else
                {
                    finalLines.Add(firstLine);
                }
                finalLines.AddRange(lines);
                File.WriteAllText(this.CSVFilePath, string.Join("\n", finalLines.Distinct()));
                Mouse.OverrideCursor = null;
            }
            else
            {
                throw new Exception("Erreur action inconue");
            }
        }

        /// <summary>
        /// Treat the event of a property change
        /// </summary>
        /// <param name="name"> Name of the property </param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}