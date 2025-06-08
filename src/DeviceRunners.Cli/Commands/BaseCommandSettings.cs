using System.ComponentModel;
using DeviceRunners.Cli.Models;
using Spectre.Console.Cli;

namespace DeviceRunners.Cli.Commands;

public abstract class BaseCommandSettings : CommandSettings
{
    [Description("Output format (json, xml, text)")]
    [CommandOption("--output")]
    public OutputFormat OutputFormat { get; set; } = OutputFormat.Default;
}