using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;

namespace EHDownloader_UI
{
    public partial class Form1 : Form
    {
        string[] CurrentList = Directory.GetDirectories(Directory.GetCurrentDirectory());
        List<MangaClass> MangaList = new List<MangaClass>();
        Regex TagRegex = new Regex("(?<= )[^\\n,]+");
        HashSet<string> ToShow = new HashSet<string>();
        HashSet<string> FilteredItems = new HashSet<string>();
        bool FinishedFiltering = false;
        string Type;

        public Form1()
        {
            InitializeComponent();
        }

        public void InitalizeComponent()
        {
           
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 0)
            {
                if (textBox1.Text.Contains("https://e-hentai.org/"))
                {
                    label1.Text = "Downloading";
                    label1.Refresh();
                    Downloader MyDownloader = new Downloader(textBox1.Text);
                    bool worked = MyDownloader.StartDownload();
                    if (worked)
                    {
                        label1.Text = "Done";
                    }else
                    {
                        MessageBox.Show("Error while downloading. Please try again.");
                    }
                }else
                {
                    label1.Text = "Invalid Entry";
                    label1.Refresh();
                }
            }
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            FinishedFiltering = false;
            SortManga("");
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            listBox1.EndUpdate();
            listBox1.BeginUpdate();
            foreach (string x in ToShow)
            {
                listBox1.Items.Add(x);
            }
            listBox1.EndUpdate();

        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (!File.Exists("input.txt"))
            {
                label2.Text = "Creating input.txt";
                label2.Refresh();
                File.Create("input.txt");
                label2.Text = "Please close and re-open the program";
                label2.Refresh();
            }
            else
            {
                bool Worked = true;
                label2.Text = "Downloading";
                label2.Refresh();
                foreach (string inputline in File.ReadAllLines("input.txt"))
                {
                    if (inputline.Length != 0)
                    {
                        Downloader myDownloader = new Downloader(inputline);
                        if(myDownloader.StartDownload() == false)
                        {
                            Worked = false;
                        }
                    }
                }
                if (Worked)
                {
                    label2.Text = "Done";
                }else
                {
                    MessageBox.Show("Error while downloading. Please try again.");
                }
            }
        }

        public void SortManga(string UserInput)
        {
            GetInfo(CurrentList);
            if (UserInput.Length != 0)
            {
                List<string> NewList = new List<string>();

                string GroupSelect = UserInput.Substring(0, UserInput.IndexOf(':'));
                string TagSelect = UserInput.Substring(UserInput.IndexOf(':') + 1);
                List<MangaClass> FilterManga = new List<MangaClass>();
                foreach (MangaClass CurrentManga in MangaList)
                {
                    if (CurrentManga.GroupNames.Contains(GroupSelect))
                    {
                        if (CurrentManga.Groups[CurrentManga.GroupNames.IndexOf(GroupSelect)].Tags.Contains(TagSelect))
                        {
                            FilterManga.Add(CurrentManga);
                            NewList.Add(CurrentManga.Name);
                        }
                    }
                }
                MangaList = FilterManga;
                CurrentList = NewList.ToArray();
            }
        }

        void GetInfo(string[] MangaInput)
        {
            MangaClass AllTags = new MangaClass("AllTags");
            foreach (string manga in MangaInput)
            {
                MangaClass ThisManga = new MangaClass(manga);
                foreach (string InfoLine in File.ReadAllLines(manga + "\\" + "info.txt"))
                {
                    string GroupName = InfoLine.Substring(0, InfoLine.IndexOf(':'));
                    Group ThisGroup = new Group(GroupName);
                    foreach (Match TagMatch in TagRegex.Matches(InfoLine))
                    {
                        ThisGroup.Tags.Add(TagMatch.Value);
                    }
                    ThisManga.GroupNames.Add(GroupName);
                    ThisManga.Groups.Add(ThisGroup);
                }
                MangaList.Add(ThisManga);
            }
            foreach (MangaClass ThisManga in MangaList)
            {
                foreach (Group ThisGroup in ThisManga.Groups)
                {
                    if (!AllTags.GroupNames.Contains(ThisGroup.GroupName))
                    {
                        AllTags.GroupNames.Add(ThisGroup.GroupName);
                        AllTags.Groups.Add(new Group(ThisGroup.GroupName));
                    }
                    foreach (string ThisTag in ThisGroup.Tags)
                    {
                        if (!AllTags.Groups[AllTags.GroupNames.IndexOf(ThisGroup.GroupName)].Tags.Contains(ThisTag))
                        {
                            AllTags.Groups[AllTags.GroupNames.IndexOf(ThisGroup.GroupName)].Tags.Add(ThisTag);
                        }
                    }
                }
            }

            ToShow = new HashSet<string>();
            foreach (Group ThisGroup in AllTags.Groups)
            {
                
                foreach (string ThisTag in ThisGroup.Tags)
                {
                    string ItemToAdd = ThisGroup.GroupName + ":" + ThisTag;
                     ToShow.Add(ItemToAdd);
                }
            }


        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (FinishedFiltering == false)
            {
                if (listBox1.SelectedItem != null)
                {
                    string SelectedItem = listBox1.SelectedItem.ToString();
                    FilteredItems.Add(SelectedItem);
                    listBox2.BeginUpdate();
                    listBox2.Items.Add(SelectedItem);
                    listBox2.EndUpdate();
                    SortManga(SelectedItem);
                    ToShow.Remove(SelectedItem);
                    listBox1.BeginUpdate();
                    listBox1.Items.Clear();
                    foreach (string x in ToShow)
                    {
                        if (!FilteredItems.Contains(x))
                        {
                            listBox1.Items.Add(x);
                        }
                    }
                    listBox1.EndUpdate();
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            FinishedFiltering = true;
            listBox1.BeginUpdate();
            listBox1.Items.Clear();
            listBox1.EndUpdate();
            listBox1.BeginUpdate();
            HashSet<string> ErrorList = new HashSet<string>();
            foreach(string x in CurrentList)
            {
                ErrorList.Add(x);
            }
            foreach (string x in ErrorList)
            {

                string y = x.Substring(x.LastIndexOf("\\")+1);
                listBox1.Items.Add(y);
            }
            listBox1.EndUpdate();
        }

        private void listBox2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
