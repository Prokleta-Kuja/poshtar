namespace poshtar.Models;

public class ValidationError
{
    public ValidationError() { }
    public ValidationError(Dictionary<string, string> errors)
    {
        Errors = errors;
    }
    public ValidationError(string key, string message)
    {
        Errors.Add(key, message);
    }
    public string Message { get; set; } = "Validation error";
    public Dictionary<string, string> Errors { get; set; } = new();
}