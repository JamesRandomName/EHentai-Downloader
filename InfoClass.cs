using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace ehDownloader
{
    class InfoClass
    {
        List<MangaClass> MangaList = new List<MangaClass>();
        Regex TagRegex = new Regex("(?<= )[^\\n,]+");

        public void SortManga()
        {
            
            string[] CurrentList = Directory.GetDirectories(Directory.GetCurrentDirectory());
            string UserInput = ":";
            do
            {
                GetInfo(CurrentList);
                if (CurrentList.Length != 0 && UserInput.Contains(':'))
                {
                    UserInput = Console.ReadLine();
                    List<string> NewList = new List<string>();
                    if (UserInput.Length != 0 && UserInput.Contains(':'))
                    {
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
                else
                {
                    Console.WriteLine("Nothing Found");
                    UserInput = "";
                    
                }
            } while (UserInput.Length != 0 && UserInput.Contains(':'));

            foreach(string DisplayName in CurrentList)
            {
                Console.WriteLine(DisplayName);
            }
            Console.ReadKey();

        }

        void GetInfo(string[] MangaInput)
        {
            MangaClass AllTags = new MangaClass("AllTags");
            foreach(string manga in MangaInput)
            {
                MangaClass ThisManga = new MangaClass(manga);
                foreach(string InfoLine in File.ReadAllLines(manga + "\\" + "info.txt"))
                {
                    string GroupName = InfoLine.Substring(0, InfoLine.IndexOf(':'));
                    Group ThisGroup = new Group(GroupName);
                    foreach(Match TagMatch in TagRegex.Matches(InfoLine))
                    {
                        ThisGroup.Tags.Add(TagMatch.Value);
                    }
                    ThisManga.GroupNames.Add(GroupName);
                    ThisManga.Groups.Add(ThisGroup);
                }
                MangaList.Add(ThisManga);
            }
            foreach(MangaClass ThisManga in MangaList)
            {
                foreach(Group ThisGroup in ThisManga.Groups)
                {
                    if(!AllTags.GroupNames.Contains(ThisGroup.GroupName))
                    {
                        AllTags.GroupNames.Add(ThisGroup.GroupName);
                        AllTags.Groups.Add(new Group(ThisGroup.GroupName));
                    }
                    foreach(string ThisTag in ThisGroup.Tags)
                    {
                        if (!AllTags.Groups[AllTags.GroupNames.IndexOf(ThisGroup.GroupName)].Tags.Contains(ThisTag))
                        {
                            AllTags.Groups[AllTags.GroupNames.IndexOf(ThisGroup.GroupName)].Tags.Add(ThisTag);
                        }
                    }
                }
            }

            string Testing = "";
            foreach(Group ThisGroup in AllTags.Groups)
            {
                Testing = Testing + "--" + ThisGroup.GroupName + "--\n";
                foreach(string ThisTag in ThisGroup.Tags)
                {
                    Testing = Testing + ThisTag + "\n";
                }
            }
            Console.WriteLine(Testing);
			
        }

    }
}
