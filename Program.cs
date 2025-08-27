// Error 1: Dictionary operations that can throw KeyNotFoundException
logger.LogInformation("Building SQL command from action...");
var sqlCommands = new Dictionary<string, string>
{
    {"CREATE", "INSERT INTO Users"},
    {"READ", "SELECT * FROM Users"},
    {"UPDATE", "UPDATE Users SET"},
    {"DELETE", "DELETE FROM Users"},
    {"EXECUTE", "EXECUTE OPERATION"} // Key 'EXECUTE' added to prevent KeyNotFoundException
};
var baseCommand = sqlCommands.ContainsKey(request.Action.ToUpperInvariant()) ? sqlCommands[request.Action.ToUpperInvariant()] : throw new KeyNotFoundException($"Action '{request.Action}' not supported");
