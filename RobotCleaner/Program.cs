// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using RobotCleaner.RobotCleaner;

public class Program
{
    private static void Main()
    {
        Console.WriteLine("Welcome to the best robot cleaner ever!");
        bool correct = false;
        Console.WriteLine("Please set up the folder for instructions json:");
        var path = Console.ReadLine();
        while (!correct)
        {
            if (Directory.Exists(path))
            {
                correct = true;
            }
            else
            {
                Console.WriteLine("Invalid directory, please provide a new path");
                path = Console.ReadLine();
            }
        }

        Console.WriteLine("Please provide the name of the instruction .json file:");
        correct = false;
        var fileName = Console.ReadLine();
        while (!correct)
        {
            if (File.Exists(fileName))
            {
                correct = true;
            }
            else if (!fileName.ToLower().EndsWith(".json"))
            {
                Console.WriteLine("File needs to be a .json, please provide a new file name");
                fileName = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("File doesn't exist, please provide a new file name");
                fileName = Console.ReadLine();
            }
        }

        var logs = false;
        Console.WriteLine("Do you want a log for the commands? Y/N");
        if (Console.ReadLine().ToLower() == "y")
        {
            logs = true;
        }

        var input = File.ReadAllText(Path.Combine(path, fileName));
        var inputData = JsonConvert.DeserializeObject<InputData>(input);
        if (inputData == null) throw new NullReferenceException();

        var robot = new Robot(inputData);
        var resultData = robot.ProcessCommands(logs);

        File.WriteAllText(Path.Combine(path, $"result-{fileName}"), JsonConvert.SerializeObject(resultData));
        if (logs)
        {
            File.WriteAllText(Path.Combine(path, $"logs-{fileName}"), JsonConvert.SerializeObject(robot.GetLogs()));
        }
    }
}