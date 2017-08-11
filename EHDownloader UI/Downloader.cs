using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace EHDownloader_UI
{


    public class Downloader
    {

        Regex GroupRegex = new Regex("(?<=toggle_tagmenu\\(')([^:]+)(?=:[^']+',this)");
        Regex TagRegex = new Regex("(?<=toggle_tagmenu\\('[^:]+:)([^']+)(?=',this)");
        Regex NameRegex = new Regex("(?<=<title>).*?(?= - E-Hentai Galleries<\\/title>)");
        Regex FirstPageRegex = new Regex("(?<=<a href=\")[^\"]+(?=\"><img alt=)");
        Regex PictureRegex = new Regex("(?<=id=\"img\" src=\")[^\"]+");
        Regex NextRegex = new Regex("[^\"]+(?=\"><img src=\"https:\\/\\/ehgt\\.org\\/g\\/n.png\")");
        Regex FixRegex = new Regex("[\\/:*?\"<>|]");
        WebClient myClient = new WebClient();
        string PageUrl;
        string Name;
        readonly object _CountLock = new object();
        readonly object _DisplayLock = new object();
        int ThreadCount = 0;

        public Downloader(string PageURL)
        {
            this.PageUrl = PageURL;
        }

        public void StartDownload()
        {
            string BasePage;
            BasePage = myClient.DownloadString(PageUrl);
            List<string> GroupNames = new List<string>();
            List<Group> Groups = new List<Group>();
            Name = FixRegex.Replace(NameRegex.Match(BasePage).Value, " ");
            Console.WriteLine(Name);
            for (int x = 0; x < TagRegex.Matches(BasePage).Count; x++)
            {
                string GroupName = GroupRegex.Matches(BasePage)[x].Value;
                string TagName = TagRegex.Matches(BasePage)[x].Value;
                if (!GroupNames.Contains(GroupName))
                {
                    GroupNames.Add(GroupName);
                    Groups.Add(new Group(GroupName));
                }
                Groups[GroupNames.IndexOf(GroupName)].Tags.Add(TagName);
            }
            string infoString = "";
            foreach (Group theGroup in Groups)
            {
                infoString = infoString + theGroup.GroupName + ": ";
                foreach (string Tag in theGroup.Tags)
                {
                    infoString = infoString + Tag + ", ";
                }
                infoString = infoString.Trim(new[] { ' ', ',' });
                infoString = infoString + "\n";
            }
            Directory.CreateDirectory(Name);
            File.WriteAllText(Name + "\\" + "info.txt", infoString);
            DownloadImage(BasePage);
        }

        void DownloadImage(string BasePage)
        {
            string currentpage = FirstPageRegex.Match(BasePage).Value;
            string nextpage = currentpage;
            int track = 1;
            do
            {
                currentpage = nextpage;
                string webpage = myClient.DownloadString(currentpage);
                string imageurl = PictureRegex.Match(webpage).Value;
                nextpage = NextRegex.Match(webpage).Value;
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadDownload), new ImageInfo(track, imageurl));
                Thread.Sleep(250);
                track++;
            } while (currentpage != nextpage);
            do
            {
                Thread.Sleep(1000);
            } while (GetRunning() != 0);
        }

        void ThreadDownload(Object PageInfo)
        {
            ImageInfo TheInfo = (ImageInfo)PageInfo;
            int PageNumber = TheInfo.PageNumber;
            string ImageUrl = TheInfo.PageUrl;
            AddThread();
            DisplayPage(PageNumber);
            WebClient PageClient = new WebClient();
            PageClient.DownloadFile(ImageUrl, Name + "\\" + PageNumber.ToString() + ".jpg");
            EndThread();
        }

        void DisplayPage(int PageNumber)
        {
            lock (_DisplayLock)
            {
                Console.WriteLine(PageNumber);
            }
        }

        void AddThread()
        {
            lock (_CountLock)
            {
                ThreadCount++;
            }
        }

        void EndThread()
        {
            lock (_CountLock)
            {
                ThreadCount--;
            }
        }

        int GetRunning()
        {
            int RunningThreads;
            lock (_CountLock)
            {
                RunningThreads = ThreadCount;
            }
            return RunningThreads;
        }

    }
}
