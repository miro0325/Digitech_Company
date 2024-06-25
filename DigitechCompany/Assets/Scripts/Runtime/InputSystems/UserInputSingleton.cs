public partial class UserInput
{
    private static UserInput _input;
    public static UserInput input => _input ??= new();
}