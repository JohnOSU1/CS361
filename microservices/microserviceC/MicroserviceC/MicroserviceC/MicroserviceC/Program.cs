using NetMQ;
using NetMQ.Sockets;
using System;
using System.IO;
using System.Reflection.Metadata;
using System.Collections.Generic;
using System.Text;

class Program
{
    static void Main()
    {
        Console.WriteLine($"Starting MicroserviceC");
        string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "AppData", "LocalLow");                        //Get path for where game data is stored ******Move before final submission
        string filePath = Path.Combine(userPath, "DefaultCompany", "My project", "sounds.txt");                                                         //Search for sounds.txt
        Dictionary<string, string> mapedText = MapFile(filePath);                                                                                       //Map sound.txt to mapedText
        //Console.WriteLine($"File written successfully! Path: {Path.GetFullPath(filePath)}");
        using (var responder = new ResponseSocket("@tcp://localhost:5151"))                                                                             //start connection
        {
            while (true)                                                                                                                                //connection loop, runs until server shuts down
            {
                string trigger = responder.ReceiveFrameString();                                                                                        //recive request with button name
                try
                {
                    if (File.Exists(filePath))  
                    {
                        if (mapedText.TryGetValue(trigger, out string soundFile))                                                                       //If a value can be found for the button name, send the sound name that matches
                        {
                            Console.WriteLine($"Found sound: {trigger} -> {soundFile}");
                            responder.SendFrame(soundFile);
                        }
                        else
                        {
                            Console.WriteLine("Sound name not found.");
                            responder.SendFrame("empty");
                        }
                    }
                    else
                    {
                        responder.SendFrame("empty");
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Console.WriteLine("Error: You don't have permission to write to this file.");
                    responder.SendFrame("Failed to find sound data");
                }
            }
        }
    }
    static Dictionary<string, string> MapFile(string path)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (string line in File.ReadAllLines(path))
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                var parts = line.Split(',');                                                                //search for seperating comma in text
                string soundName = parts[0].Trim();                                                         //trim removes unwanted whitespace 
                string soundFile = parts[1].Trim();
                if (!map.ContainsKey(soundName))                                                            //map values
                {
                    map[soundName] = soundFile;
                }
            }
        }
        return map;
    }
}