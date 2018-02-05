using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFS200Tools
{
    class Program
    {
        static void Main(string[] args)
        {
            AFS200.AFS200Volume vol;
            if(args.Count() < 2)
            {
                Console.WriteLine("AFS200Tools.exe <volume filename> -options");
                Console.WriteLine("\t <volume filename> -l or --list : List all files in archive");
                Console.WriteLine("\t <volume filename> -i or --info : List the number of files in archive");
                Console.WriteLine("\t <volume filename> -xi X or --extract-index X : Extract the file at index X in the current folder");
                Environment.Exit(1);
            }
            switch (args[1])
            {
                case "-l":
                case "--list":
                    vol = new AFS200.AFS200Volume(args[0]);
                    List<AFS200.AFS200Volume.AFS200FileEntry> filelist = vol.GetFileList();
                    Console.WriteLine("Number of files : " + vol.GetHeader().fileNumber);
                    Console.WriteLine("Index\tFilename\tFilesize");
                    for (int i = 0; i < filelist.Count; i++)
                    {
                        if (filelist[i].entryType == 2)
                        {
                            Console.WriteLine(i + "\t" + filelist[i].filename + "\t is a directory");
                        }
                        else
                        {
                            Console.WriteLine(i + "\t" + filelist[i].filename + "\t"+ filelist[i].fileSize.ToString());
                        }
                    }
                    break;
                case "-i":
                case "--info":
                    vol = new AFS200.AFS200Volume(args[0]);
                    Console.WriteLine("Number of files : " + vol.GetHeader().fileNumber);
                    break;
                case "-xi":
                case "--extract-index":
                    vol = new AFS200.AFS200Volume(args[0]);
                    int index = Convert.ToInt32(args[2]);
                    AFS200.AFS200Volume.AFS200FileEntry entry = vol.GetFileEntry(index);
                    vol.ExtractFileAtIndex(index, entry.filename.Substring(entry.filename.IndexOf('_')+1));
                    break;
                case "-h":
                case "--help":
                default:
                    Console.WriteLine("AFS200Tools.exe <volume filename> -options");
                    Console.WriteLine("\t <volume filename> -l or --list : List all files in archive");
                    Console.WriteLine("\t <volume filename> -i or --info : List the number of files in archive");
                    Console.WriteLine("\t <volume filename> -xi X or --extract-index X : Extract the file at index X in the current folder");
                    break;
            }
        }
    }
}
