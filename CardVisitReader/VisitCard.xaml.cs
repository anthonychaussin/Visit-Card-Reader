using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tesseract;

namespace CardVisitReader
{
    /// <summary>
    /// Logique d'interaction pour VisitCard.xaml
    /// </summary>
    public partial class VisitCard : UserControl, INotifyPropertyChanged
    {
        private string text;

        public string File { get { return PictureFile.FullName; } }
        public string CName { get { return Regex.Match(this.Text + "", @"([a-zA-Z]{2,}\s[a-zA-Z]{1,}'?-?[a-zA-Z]{2,}\s?([a-zA-Z]{1,})?)").Value.Split('\n')[0]; } }
        public string Email { get { return Regex.Match(this.Text + "", @"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").Value.Split('\n')[0]; } }
        public string Phone { get { return Regex.Match(this.Text + "", @"[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*").Value.Split('\n')[0]; } }
        public string Adresse { get; set; }
        public string Web { get { return Regex.Match(this.Text + "", @"(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+").Value.Replace("@", ".").Split('\n')[0]; } }
        public string Twiter { get; set; }

        public string Entreprise
        {
            get
            {
                List<string> lines = this.Text?.Split('\n')?.ToList();
                if (lines != null && lines.Count > 0)
                {
                    return lines[lines.IndexOf(lines.Where(l => l.Contains(this.CName)).First()) + 1];
                }
                else
                {
                    return "";
                }
            }
        }

        public FileInfo PictureFile { get; set; }

        public string Text
        {
            get => text;
            set
            {
                text = value;
                this.OnPropertyChanged(nameof(this.Web));
                this.OnPropertyChanged(nameof(this.Twiter));
                this.OnPropertyChanged(nameof(this.Adresse));
                this.OnPropertyChanged(nameof(this.Email));
                this.OnPropertyChanged(nameof(this.Phone));
                this.OnPropertyChanged(nameof(this.CName));
                this.OnPropertyChanged(nameof(this.Entreprise));
            }
        }

        public event EventHandler CardDlicked;

        /// <summary>
        /// Event when a property change
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        public VisitCard()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ReadCard()
        {
            try
            {
                using (TesseractEngine engine = new TesseractEngine(@"./tessdata", "fra", EngineMode.Default))
                {
                    try
                    {
                        using (Pix img = Pix.LoadFromFile(PictureFile.FullName))
                        {
                            try
                            {
                                using (Tesseract.Page page = engine.Process(img))
                                {
                                    try
                                    {
                                        this.Text = page.GetText();
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Unexpected Error: " + e.Message);
                                        Console.WriteLine("Details: ");
                                        Console.WriteLine(e.ToString());
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Unexpected Error: " + e.Message);
                                Console.WriteLine("Details: ");
                                Console.WriteLine(e.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unexpected Error: " + e.Message);
                        Console.WriteLine("Details: ");
                        Console.WriteLine(e.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected Error: " + e.Message);
                Console.WriteLine("Details: ");
                Console.WriteLine(e.ToString());
            }
        }

        internal void ForceLoad()
        {
            this.UserControl_Loaded(this, null);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.ReadCard();
        }

        /// <summary>
        /// Treat the event of a property change
        /// </summary>
        /// <param name="name"> Name of the property </param>
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Card_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.CardDlicked?.Invoke(this, e);
        }
    }
}