using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ehDownloader
{
    public class MangaClass
    {
        public string Name;
        public List<Group> Groups = new List<Group>();
        public List<string> GroupNames = new List<string>();

        public MangaClass(string Name)
        {
            this.Name = Name;
        }
    }

    public class Group
    {
        public string GroupName;
        public List<string> Tags = new List<string>();

        public Group(string Name)
        {
            this.GroupName = Name;
        }
    }

    public class ImageInfo
    {
        public int PageNumber;
        public string PageUrl;
        public bool Retry = false;

        public ImageInfo(int NumberInput, string UrlInput)
        {
            PageNumber = NumberInput;
            PageUrl = UrlInput;
        }
    }
}
