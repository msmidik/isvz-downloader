using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using VestnikVZClient.zakazky;
using System.Globalization;

namespace VestnikVZClient
{
    public class Program
    {


        static void Main(string[] args)
        {
            Console.WriteLine("Zakázkový klient");
            Console.WriteLine("zadejte (počáteční písmeno):");
            Console.WriteLine(" v. version (test - zobrazení verze)");
            Console.WriteLine(" l. list (stažení seznamu zakázek za období do seznam.txt)");
            Console.WriteLine(" d. document (stažení zakázek dle seznam.txt do složky export v xml)");
            string text = Console.ReadLine();
            var exportClient = new ExportClient();

            switch (text[0])
            {
                case 'v':
                    exportClient.GetVersion();
                    break;

                case 'l':
                    Console.WriteLine("datum od (výchozí 1.1.2013)");
                    string fromDate = Console.ReadLine();
                    if (fromDate == "") { fromDate = "1.1.2013"; }
                    Console.WriteLine("datum do (výchozí 2.1.2013)");
                    string toDate = Console.ReadLine();
                    if (toDate == "") { toDate = "2.1.2013"; }
                    
                    exportClient.GetList(DateTime.Parse(fromDate), DateTime.Parse(toDate));
                    break;

                case 'd':
                    exportClient.GetForm();
                    break;

                default:
                    Console.WriteLine("chybný parametr");
                    Console.ReadLine();
                    break;
            }


        }
    }
}
