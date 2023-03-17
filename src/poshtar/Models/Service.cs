using System.ComponentModel.DataAnnotations;
using poshtar.Controllers;

namespace poshtar.Models;

public class ServiceRequestModel
{
    [Required] public required ServiceName Name { get; set; }
    [Required] public required ServiceRequestType Type { get; set; }
    public bool IsInvalid(out ValidationError errorModel)
    {
        errorModel = new();

        if (Name == ServiceName.None)
            errorModel.Errors.Add(nameof(Name), "Invalid");

        if (Type == ServiceRequestType.Nothing)
            errorModel.Errors.Add(nameof(Type), "Invalid");

        return errorModel.Errors.Count > 0;
    }
}
public class ServiceResultModel
{
    public ServiceResultModel((int exitCode, string error, string output) result)
    {
        Success = result.exitCode == 0;
        Output = result.output;
        Error = result.error;
    }
    [Required] public bool Success { get; set; }
    public string? Output { get; set; }
    public string? Error { get; set; }
}