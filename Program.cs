using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;

namespace ehDownloader
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Download:1\nFilter:2\nDownload List:3\nQuit:4\n");
            string input = Console.ReadLine();
            if (input == "1")
            {
                Console.Write("Page URL:");
                (new Downloader(Console.ReadLine())).StartDownload();
            }
            if (input == "2")
            {
                new InfoClass().SortManga();
            }
            if (input == "3")
            {
                if (!File.Exists("input.txt"))
                {
                    File.Create("input.txt");
                }
                else
                {
                    foreach (string inputline in File.ReadAllLines("input.txt"))
                    {
                        if(inputline.Length != 0)
                            (new Downloader(inputline)).StartDownload();
                    }
                }
            }
        }
    }

    
}
