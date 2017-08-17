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
        Regex TypeRegex = new Regex("([^\"]+)(?=\"><img src=\"https:\\/\\/ehgt.org\\/g\\/c\\/\\1.png\")");
        WebClient myClient = new WebClient();
        string PageUrl;
        string Name;
        readonly object _CountLock = new object();
        readonly object _DisplayLock = new object();
        int ThreadCount = 0;
        bool Worked = true;

        public Downloader(string PageURL)
        {
            this.PageUrl = PageURL;
        }

        public bool StartDownload()
        {
            string BasePage;
            BasePage = myClient.DownloadString(PageUrl);
            List<string> GroupNames = new List<string>();
            List<Group> Groups = new List<Group>();
            Name = FixRegex.Replace(NameRegex.Match(BasePage).Value, " ");
            string Type = TypeRegex.Match(BasePage).Value;
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
            string infoString = "Type: " + Type + "\n";
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
            return Worked;
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
            //DisplayPage(PageNumber); Not used for UI
            if(!File.Exists(Name + "\\" + PageNumber.ToString() + ".jpg"))
            {
                try
                {
                    WebClient PageClient = new WebClient();
                    PageClient.DownloadFile(ImageUrl, Name + "\\" + PageNumber.ToString() + ".jpg");
                }catch(WebException e)
                {
                    Worked = false;
                }
            }
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
