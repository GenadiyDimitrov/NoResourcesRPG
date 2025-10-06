namespace NoResourcesRPG.Shared.DTOs;

public record Size(double Width, double Height, double Left, double Top, double ScaleX, double ScaleY)
{
    // Parameterless constructor calls the primary one with all zeros
    public Size() : this(0, 0, 0, 0, 0, 0)
    {
    }
};
