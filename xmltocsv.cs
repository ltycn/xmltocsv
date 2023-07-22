using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml;

class XMLtoCSVConverterAndMerger
{
    static void Main(string[] args)
    {
        string folderPath;
        string logo = @"

*******************************************************************

██╗  ██╗███╗   ███╗██╗  ████████╗ ██████╗  ██████╗███████╗██╗   ██╗
╚██╗██╔╝████╗ ████║██║  ╚══██╔══╝██╔═══██╗██╔════╝██╔════╝██║   ██║
 ╚███╔╝ ██╔████╔██║██║     ██║   ██║   ██║██║     ███████╗██║   ██║
 ██╔██╗ ██║╚██╔╝██║██║     ██║   ██║   ██║██║     ╚════██║╚██╗ ██╔╝
██╔╝ ██╗██║ ╚═╝ ██║███████╗██║   ╚██████╔╝╚██████╗███████║ ╚████╔╝ 
╚═╝  ╚═╝╚═╝     ╚═╝╚══════╝╚═╝    ╚═════╝  ╚═════╝╚══════╝  ╚═══╝  


                                                  github.com/ltycn
*******************************************************************
        ";

        if (args.Length == 0)
        {
            Console.WriteLine(logo);
            
            Console.WriteLine(@"For accuracy concern, please confirm:
1. ONLY XML files exist in this path.
2. NO other CSV files in this path.
 =(●'v'●)=
");
            Console.WriteLine("Please drag a folder to here:");
            folderPath = Console.ReadLine();
        }
        else
        {
            folderPath = args[0];
        }

        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine("You give a wrong path! QAQ");
            return;
        }

        XmlToCsv(folderPath);
        MergeCsvFiles(folderPath);

        Console.WriteLine("Done a big job, you wanna continue？  _(:3 」∠ )_ （Y/N）");
        char response = Console.ReadKey().KeyChar;
        if (response == 'Y' || response == 'y')
        {
            Process.Start(System.Reflection.Assembly.GetExecutingAssembly().Location);
            Environment.Exit(0);
        }
    }

    static void XmlToCsv(string folderPath)
    {
        string[] xmlFiles = Directory.GetFiles(folderPath, "*.xml");

        if (xmlFiles.Length == 0)
        {
            Console.WriteLine("NO XML Files found here~  Is this a correct path?");
            return;
        }

        foreach (string xmlFilePath in xmlFiles)
        {
            string csvFileName = Path.GetFileNameWithoutExtension(xmlFilePath) + ".csv";
            string csvFilePath = Path.Combine(folderPath, csvFileName);

            try
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                using (StreamWriter csvWriter = new StreamWriter(csvFilePath))
                {
                    ConvertProcess(xmlDoc.DocumentElement, csvWriter, 0);
                }

                Console.WriteLine($"Convert DONE: {xmlFilePath} -> {csvFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Something went wrong...: {ex.Message}");
            }
        }
    }

    static void ConvertProcess(XmlNode node, StreamWriter writer, int depth)
    {
        if (node.NodeType == XmlNodeType.Element)
        {
            if (depth > 0 && node.HasChildNodes && node.FirstChild.NodeType == XmlNodeType.Text)
            {
                writer.WriteLine($"{node.Name},{node.InnerText}");
            }
            else if (depth > 0)
            {
                writer.WriteLine(node.Name);
            }

            foreach (XmlAttribute attr in node.Attributes)
            {
                writer.WriteLine($"{attr.Name},{attr.Value}");
            }
        }

        foreach (XmlNode childNode in node.ChildNodes)
        {
            ConvertProcess(childNode, writer, depth + 1);
        }
    }

    static void MergeCsvFiles(string folderPath)
    {
        string[] csvFiles = Directory.GetFiles(folderPath, "*.csv");

        if (csvFiles.Length == 0)
        {
            Console.WriteLine("NO CSV Files Found! Is this a correct path?");
            return;
        }

        var dataDict = new Dictionary<string, List<string>>();
        var fileNamesWithoutExtensions = new List<string>();

        foreach (string csvFilePath in csvFiles)
        {
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(csvFilePath);
            fileNamesWithoutExtensions.Add(fileNameWithoutExtension);

            using (StreamReader csvReader = new StreamReader(csvFilePath))
            {
                string line;
                var seenKeys = new HashSet<string>();
                while ((line = csvReader.ReadLine()) != null)
                {
                    string[] parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();

                        if (!seenKeys.Contains(key))
                        {
                            seenKeys.Add(key);
                            if (!dataDict.ContainsKey(key))
                            {
                                dataDict[key] = new List<string>();
                            }
                            dataDict[key].Add(value);
                        }
                    }
                }
            }
        }

        string outputCsvFilePath = Path.Combine(folderPath, "output.csv");
        using (StreamWriter csvWriter = new StreamWriter(outputCsvFilePath))
        {
            // Write an empty cell for the first column in the header row
            csvWriter.Write(",");

            // Write the file names without extensions in the first row
            string header = string.Join(",", fileNamesWithoutExtensions);
            csvWriter.WriteLine(header);

            foreach (var kvp in dataDict)
            {
                string line = $"{kvp.Key},{string.Join(",", kvp.Value)}";
                csvWriter.WriteLine(line);
            }
        }

        Console.WriteLine($"We're ALL DONE! New file here~：{outputCsvFilePath}");
        Process.Start(outputCsvFilePath);
    }
}
