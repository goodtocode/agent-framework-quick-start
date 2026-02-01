namespace Goodtocode.AgentFramework.Core.Application.Abstractions;

public interface ISemanticPluginCompatible
{
    string PluginName { get; }
    string FunctionName { get; }
    Dictionary<string, object> Parameters { get; }
}