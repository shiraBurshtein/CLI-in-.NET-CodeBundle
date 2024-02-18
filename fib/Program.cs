using System.CommandLine;
using System.IO;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

var rootCommand = new RootCommand("rootCommand for file Bundle");
var bundleCommand = new Command("bundle", "bundle for file");
var create_rspCommand = new Command("create-rsp", "create the bundle command");
var outputOption = new Option<FileInfo>(new string[] { "--output", "-o" }, "the path & name of the file");
var languagesOption = new Option<string>(new string[] { "--languages", "-l" }, "the list of programming languages");
languagesOption.IsRequired = true;
var noteOption = new Option<bool>(new string[] { "--note", "-n" }, "whether to add the source code as a comment");
var sortOption = new Option<string>(new string[] { "--sort", "-s" }, "sort the bundle");
var removeEmptyLinesOption = new Option<bool>(new string[] { "--remove-empty-lines", "-r" }, "remove empty lines from the bundle");
var authorOption = new Option<string>(new string[] { "--author", "-a" }, "Writes the author as a comment");

rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(create_rspCommand);
bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languagesOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);

bundleCommand.SetHandler((path, author, note, languages, removeEmptyLines, sort) =>
{
    try
    {

        if (!File.Exists(path.FullName))
        {
            File.Create(path.FullName).Close();
        }
        if (author != null)
            File.AppendAllText(path.FullName, $"\n// author: {author}\n");
        if (note)
            File.AppendAllText(path.FullName, $"\n// directory:{Directory.GetCurrentDirectory()}\n");


        // Include all code files in the directory
        var extensions = new[] { ".cs", ".java", ".py",".js",".cpp",".h",".rb",".swift",".go",
                ".php",".html",".css",".ts",".sh",".pl",".r",".kt"}; // רשימת סיומות לקבצי קוד
        var Patterns = new[] { "C#", "Java", "Python","JavaScript","C++","C++","Ruby","Swift","Go","PHP",
                "HTML","CSS","TypeScript","Shell","Script","Perl","R","Kotlin"}; // רשימת שפות
        var resextensions = extensions;
        if (languages != null && !languages.Contains("all"))
        {
            resextensions = languages.Split(' ').Join(Patterns.Zip(extensions, (lang, ext) => new { Language = lang, Extension = ext }),
           lang => lang,
           langExt => langExt.Language,
           (lang, langExt) => langExt.Extension).ToArray();
        }

        var files = Directory.GetFiles(Directory.GetCurrentDirectory(), "*", SearchOption.AllDirectories)
            .Where(f => resextensions.Any(p => f.EndsWith(p))).ToArray();
        if (sort != null && sort.ToLower().Contains("type"))
        {
            Array.Sort(files, (a, b) => Path.GetExtension(a).CompareTo(Path.GetExtension(b)));
        }
        else
            Array.Sort(files);


        foreach (var file in files)
        {
            File.AppendAllText(path.FullName, "\n------------------------------------\n");
            var content = File.ReadAllText(file);
            if (removeEmptyLines)
                content = string.Join(Environment.NewLine, content.Split('\n').Where(line => !string.IsNullOrWhiteSpace(line)));

            File.AppendAllText(path.FullName, content);
        }


        Console.WriteLine("succes");
    }
    catch (DirectoryNotFoundException exception)
    {
        Console.WriteLine("the path is in valid");
    }
    catch (Exception exception)
    {
        Console.WriteLine(exception.Message);
    }


}, outputOption, authorOption, noteOption, languagesOption, removeEmptyLinesOption, sortOption);



create_rspCommand.SetHandler(() =>
{

    var responseFile = new FileInfo("responseFile.rsp");
    using (StreamWriter rspWriter = new StreamWriter(responseFile.FullName))
    {

        Console.WriteLine("enter the path of the output");

        string pathInput = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(pathInput))
        {
            Console.WriteLine("Enter the output file path: ");
            pathInput = Console.ReadLine();

        }
        rspWriter.WriteLine($"--output {pathInput}");


        Console.WriteLine("enter the laguages you want to include, for all, write all");

        string languagesInput = Console.ReadLine();
        while (string.IsNullOrWhiteSpace(languagesInput))
        {
            Console.WriteLine("Enter at list one languages or write all ");
            languagesInput = Console.ReadLine();

        }
        rspWriter.WriteLine($"--languages {languagesInput}");

        Console.WriteLine("Include source code origin as a comment? (y/n)");
        var noteInput = Console.ReadLine();
        rspWriter.WriteLine(noteInput.Trim().ToLower() == "y" ? "--note" : "");

        Console.WriteLine("Include author  as a comment? (y/n)");
        var authorInput = Console.ReadLine();
        if (authorInput.ToLower().Contains("y"))
        {
            Console.WriteLine("enter the ahuthor");
            authorInput = Console.ReadLine().Replace(" ", "-");
            rspWriter.WriteLine($"--author {authorInput}");
        }

        Console.WriteLine("Enter the sort order  ('letter' or 'type'):");
        var sortInput = Console.ReadLine();
        rspWriter.WriteLine(sortInput.ToLower().Contains("type") ? "--sort" : "");


        Console.WriteLine("remove empty lines? (y/n)");
        var removeEmptyLinesInput = Console.ReadLine();
        rspWriter.WriteLine(removeEmptyLinesInput.Trim().ToLower() == "y" ? "--remove-empty-lines" : "");

    }

    Console.WriteLine($"Response file created successfully: {responseFile.FullName}");
});
rootCommand.InvokeAsync(args);
