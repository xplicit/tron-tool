using System.IO.Compression;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text;
using Interfaces;

namespace tron;

public class CommandHandler : ICommandHandler
{
    private const string templatePrefix = "$appname";
    private const string templateExtension = ".tmpl";

    public async Task HandleCommands(params string [] args)
    {
        var nameOption = new Option<string>(
            aliases: new[] { "-n", "--name" },
            description: "Adds an application's name."
        )
        {
            IsRequired = true,
            Arity = ArgumentArity.ExactlyOne
        };

        var basicAppCommand = new Command("basic", "Creates pure JS/CSS/HTML application.");
        var reactAppCommand = new Command("react-app", "Creates new React application.");

        basicAppCommand.Add(nameOption);
        reactAppCommand.Add(nameOption);

        basicAppCommand.SetHandler(() =>
        {
            var temporaryDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Temporary");
            DirectoryInfo temporaryDirectoryInfo = new(temporaryDirectory);
            if (!temporaryDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(temporaryDirectory);
            }

            var basicTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,"templates/basic/");
            var temporaryBasicTemplateDirectory = Path.Combine(temporaryDirectory, "basic");

            CopyDirectory(basicTemplatePath, temporaryBasicTemplateDirectory, args[3], true);

            var basicTemplateDestinationPath = Path.Combine(temporaryBasicTemplateDirectory, args[3]);

            DirectoryInfo basicAppTemplateDirInfo = new(basicTemplateDestinationPath);
            var basicTemplateZipPath = Path.Combine(temporaryDirectory, "basic.zip");
            if (basicAppTemplateDirInfo.Exists)
            {
                CompressFiles(basicTemplateDestinationPath, basicTemplateZipPath);
            }

            var decompressionTarget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), args[3]);
            FileInfo basicTemplateArchiveInfo = new(basicTemplateZipPath);
            if (basicTemplateArchiveInfo.Exists)
            {
                DecompressFiles(basicTemplateZipPath, decompressionTarget);
            }

            Console.WriteLine($"New folder {args[3]} is created. You can find is there: {decompressionTarget}");

            try
            {
                Directory.Delete(temporaryDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine($"Your new project {args[3]} is ready to code!");
            }
        });

        reactAppCommand.SetHandler(() =>
        {
            var temporaryDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Temporary");;
            DirectoryInfo temporaryDirectoryInfo = new(temporaryDirectory);
            if (!temporaryDirectoryInfo.Exists)
            {
                Directory.CreateDirectory(temporaryDirectory);
            }

            var reactTemplatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory ,"templates/react-app/");
            var temporaryReactTemplateDirection = Path.Combine(temporaryDirectory, "react-app");

            CopyDirectory(reactTemplatePath, temporaryReactTemplateDirection, args[3], true);

            var reactTemplateDestinationPath = Path.Combine(temporaryReactTemplateDirection, args[3]);

            DirectoryInfo basicAppTemplateDirInfo = new(reactTemplateDestinationPath);
            var reactTemplateZipPath = Path.Combine(temporaryDirectory, "react.zip");;
            if (basicAppTemplateDirInfo.Exists)
            {
                CompressFiles(reactTemplateDestinationPath, reactTemplateZipPath);
            }

            var decompressionTarget = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), args[3]);
            FileInfo reactAppTemplateArchiveInfo = new(reactTemplateZipPath);
            if (reactAppTemplateArchiveInfo.Exists)
            {
                DecompressFiles(reactTemplateZipPath, decompressionTarget);
            }

            Console.WriteLine($"New folder {args[3]} is created. You can find is there: {decompressionTarget}");

            try
            {
                Directory.Delete(temporaryDirectory, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.WriteLine($"Your new project {args[3]} is ready to code!");
            }
        });

        var newCommand = new Command("new", "Creates new application template");
        newCommand.AddCommand(basicAppCommand);
        newCommand.AddCommand(reactAppCommand);

        var root = new RootCommand("CLI scaffolding app");
        root.AddCommand(newCommand);

        var parser = new CommandLineBuilder(root).UseDefaults().Build();

        try
        {
            await parser.InvokeAsync(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private static void CompressFiles(string source, string archive)
    {
        ZipFile.CreateFromDirectory(source, archive);

        if (archive != null)
        {
            Console.WriteLine("New archive is created.");
        }
    }

    private static void DecompressFiles(string archive, string extractionTarget)
    {
        DirectoryInfo extractionTargetInfo = new (extractionTarget);
        if (extractionTargetInfo.Exists)
        {
            Console.WriteLine($"Unable to create new project because directory {extractionTarget} already exist.");
        }

        ZipFile.ExtractToDirectory(archive, extractionTarget);
    }

    private static void CopyDirectory(string source, string target, string appName, bool hasSubdirectories = false)
    {
        DirectoryInfo sourceDirInfo = new(source);
        DirectoryInfo targetDirInfo = new(target);

        if (targetDirInfo.FullName == sourceDirInfo.FullName)
        {
            return;
        }

        if (!targetDirInfo.Exists)
        {
            Directory.CreateDirectory(target);
        }

        if (!sourceDirInfo.Exists)
        {
            Console.WriteLine(new DirectoryNotFoundException($"Directory {sourceDirInfo.FullName} is not exist"));
        }

        foreach (FileInfo fileInfo in sourceDirInfo.GetFiles())
        {
            var targetPath = Path.Combine(targetDirInfo.FullName, fileInfo.Name);
            StringBuilder targetPathSb = new(targetPath);
            if (targetPath.Contains(templatePrefix) && targetPath.Contains(templateExtension))
            {
                targetPathSb.Replace(templatePrefix, appName);
                targetPathSb.Replace(templateExtension, string.Empty);
            }
            fileInfo.CopyTo(targetPathSb.ToString(), true);
        }

         if (hasSubdirectories)
        {
            foreach (DirectoryInfo sourceSubDirInfo in sourceDirInfo.GetDirectories()) 
            {
                DirectoryInfo targetSubDir = targetDirInfo.CreateSubdirectory(sourceSubDirInfo.Name);
                StringBuilder targetNameSb = new(targetSubDir.FullName);
                if (targetSubDir.FullName.Contains(templatePrefix))
                {
                    targetNameSb.Replace(templatePrefix, appName);
                }
                CopyDirectory(sourceSubDirInfo.FullName, targetNameSb.ToString(), appName, true);
            }
        }
    }
}
