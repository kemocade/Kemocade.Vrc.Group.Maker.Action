using CommandLine;

namespace Kemocade.Vrc.Group.Maker.Action;

internal record ActionInputs
{
    [Option('u', "username", Required = true)]
    public string Username { get; init; } = null!;

    [Option('p', "password", Required = true)]
    public string Password { get; init; } = null!;

    [Option('k', "key", Required = true)]
    public string Key { get; init; } = null!;

    [Option('n', "name", Required = true)]
    public string Name { get; init; } = null!;

    [Option('s', "shortcode", Required = true)]
    public string Shortcode { get; init; } = null!;

    [Option('d', "discriminators", Required = true)]
    public string Discriminators { get; init; } = null!;
}