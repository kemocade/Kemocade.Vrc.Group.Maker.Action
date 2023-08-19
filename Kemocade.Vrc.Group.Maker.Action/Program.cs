using CommandLine;
using Kemocade.Vrc.Group.Maker.Action;
using OtpNet;
using VRChat.API.Api;
using VRChat.API.Client;
using VRChat.API.Model;
using static System.Console;

// Configure Cancellation
using CancellationTokenSource tokenSource = new();
CancelKeyPress += delegate { tokenSource.Cancel(); };

// Configure Inputs
ParserResult<ActionInputs> parser = Parser.Default.ParseArguments<ActionInputs>(args);
if (parser.Errors.ToArray() is { Length: > 0 } errors)
{
    foreach (CommandLine.Error error in errors)
    { WriteLine($"{nameof(error)}: {error.Tag}"); }
    Environment.Exit(2);
    return;
}
ActionInputs inputs = parser.Value;

// Authentication credentials
Configuration config = new()
{
    Username = inputs.Username,
    Password = inputs.Password,
    UserAgent = "kemocade/0.0.1 admin%40kemocade.com"
};

// Create instances of APIs we'll need
AuthenticationApi authApi = new(config);
GroupsApi groupsApi = new(config);

try
{
    // Log in
    WriteLine("Logging in...");
    CurrentUser currentUser = authApi.GetCurrentUser();
    await WaitSeconds(1);

    // Check if 2FA is needed
    if (currentUser == null)
    {
        WriteLine("2FA needed...");

        // Generate a 2FA code with the stored secret
        string key = inputs.Key.Replace(" ", string.Empty);
        Totp totp = new(Base32Encoding.ToBytes(key));

        // Make sure there's enough time left on the token
        int remainingSeconds = totp.RemainingSeconds();
        if (remainingSeconds < 5)
        {
            WriteLine("Waiting for new token...");
            await WaitSeconds(remainingSeconds + 1);
        }

        // Verify 2FA
        WriteLine("Using 2FA code...");
        authApi.Verify2FA(new(totp.ComputeTotp()));
        await WaitSeconds(1);

        currentUser = authApi.GetCurrentUser();
        if (currentUser == null)
        {
            WriteLine("Failed to validate 2FA!");
            Environment.Exit(2);
        }
    }
    WriteLine($"Logged in as {currentUser.DisplayName}");

    Group group = groupsApi.CreateGroup(new(name: "Kemocade", shortCode: "KEMO", roleTemplate: GroupRoleTemplate.Default));
    string groupKey = $"{group.ShortCode}.{group.Discriminator}";
    WriteLine($"Created Group: {groupKey}");
    await WaitSeconds(1);

    string[] discriminators = inputs.Discriminators.Split('.');
    WriteLine($"Checking for {discriminators.Length} discriminators: {string.Join(".", discriminators)}");
    if (!discriminators.Contains(group.Discriminator))
    {
        WriteLine($"{group.Discriminator} is not one of the target discriminators.");
        groupsApi.DeleteGroup(group.Id);
        WriteLine($"Deleted Group: {groupKey}");
        await WaitSeconds(1);
    }
    else
    {
        WriteLine($"Got target Discriminator: {group.Discriminator}!");
    }
}
catch (ApiException e)
{
    WriteLine("Exception when calling API: {0}", e.Message);
    WriteLine("Status Code: {0}", e.ErrorCode);
    WriteLine(e.ToString());
    Environment.Exit(2);
    return;
}

WriteLine("Done!");
Environment.Exit(0);

static async Task WaitSeconds(int seconds) =>
    await Task.Delay(TimeSpan.FromSeconds(seconds));