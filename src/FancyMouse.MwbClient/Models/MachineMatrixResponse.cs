using System.Text.Json.Serialization;

namespace FancyMouse.MwbClient.Models;

public struct MachineMatrixResponse
{
    [JsonInclude]
    public List<string> Matrix;

    public MachineMatrixResponse(List<string> matrix)
    {
        this.Matrix = matrix ?? throw new ArgumentNullException(nameof(matrix));
    }
}
