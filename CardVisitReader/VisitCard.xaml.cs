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
        private string entreprise;
        private string phone;
        private string email;
        private string web;
        private string cname;
        private string adresse;

        public string File { get { return PictureFile.FullName; } }
        public string CName
        {
            get
            {
                if (string.IsNullOrEmpty(this.cname))
                {
                    return Regex.Match(this.Text + "", @"([a-zA-Z]{2,}\s[a-zA-Z]{1,}'?-?[a-zA-Z]{2,}\s?([a-zA-Z]{1,})?)").Value.Split('\n')[0];
                }
                else
                {
                    return this.cname;
                }
            }
            set
            {
                this.cname = value;
            }
        }
        public string Email
        {
            get
            {
                if (string.IsNullOrEmpty(this.email))
                {
                    return Regex.Match(this.Text + "", @"[\w-\.]+@([\w-]+\.)+[\w-]{2,4}").Value.Split('\n')[0];
                }
                else
                {
                    return email;
                }
            }
            set
            {
                this.email = value;
            }
        }
        public string Phone
        {
            get
            {
                if (string.IsNullOrEmpty(this.phone))
                {
                    return Regex.Match(this.Text + "", @"[+]*[(]{0,1}[0-9]{1,4}[)]{0,1}[-\s\./0-9]*").Value.Split('\n')[0];
                }
                else
                {
                    return phone;
                }
            }
            set
            {
                this.phone = value;
            }
        }
        public string Adresse
        {
            get
            {
                if (string.IsNullOrEmpty(this.adresse))
                {
                    return Regex.Match(this.Text + "", @".*\d{1,3}.*[\n ,]\d{4,5}([- ]\d{4,5})? [^\n]+([\n ,](France|Suisse|Allemagne|Italie|Germany|Deutschland|Italia))?").Value.Replace("\n", ", ");
                }
                else
                {
                    return this.adresse;
                }
            }
            set
            {
                this.adresse = value;
            }
        }
        public string Web
        {
            get
            {
                if (string.IsNullOrEmpty(this.web))
                {
                    return Regex.Match(this.Text + "", @"(?:http(s)?:\/\/)?[\w.-]+(?:\.[\w\.-]+)+[\w\-\._~:/?#[\]@!\$&'\(\)\*\+,;=.]+").Value.Replace("@", ".").Split('\n')[0];
                }
                else
                {
                    return this.web;
                }
            }
            set
            {
                this.web = value;
            }
        }
        public string Twiter
        {
            get;
            set;
        }

        public string Entreprise
        {
            get
            {
                if (string.IsNullOrEmpty(this.entreprise))
                {
                    List<string> lines = this.Text?.Split('\n')?.ToList();
                    if (lines != null && lines.Count > 0)
                    {
                        string firstTry = lines[lines.IndexOf(lines.Where(l => l.Contains(this.CName)).First()) + 1];
                        if (string.IsNullOrEmpty(firstTry) && !string.IsNullOrEmpty(this.Email))
                        {
                            string domain = this.Email.Split('@')[1];
                            return domain.Substring(0, domain.LastIndexOf('.'));
                        }
                        else
                        {
                            return firstTry;
                        }
                    }
                    else
                    {
                        return "";
                    }
                }
                else
                {
                    return this.entreprise;
                }

            }
            set
            {
                this.entreprise = value;
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
                                        this.Text = page.GetText().Replace(" | ", "\n");
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