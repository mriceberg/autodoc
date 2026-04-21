using AutoDoc.Cli.Commands;
using System.CommandLine;

var root = new RootCommand("AutoDoc — Power Platform documentation generator");
root.AddCommand(GenerateCommand.Build());

return await root.InvokeAsync(args);
