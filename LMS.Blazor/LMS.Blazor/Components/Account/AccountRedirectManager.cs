using Microsoft.AspNetCore.Components;

namespace LMS.Blazor.Components.Account;

internal sealed class AccountRedirectManager(NavigationManager navigationManager)
{
    public void RedirectTo(string? uri)
    {
        // Ingen returnUrl: gå till "/" - där skickas inloggad användare
        // vidare till teacher-dashboard eller student-dashboard beroende på roll.
        if (string.IsNullOrWhiteSpace(uri))
        {
            navigationManager.NavigateTo("");
            return;
        }

        if (Uri.IsWellFormedUriString(uri, UriKind.Relative) &&
            !uri.StartsWith("//", StringComparison.Ordinal) &&
            !uri.StartsWith('\\'))
        {
            navigationManager.NavigateTo(uri);
            return;
        }

        var baseUri = new Uri(navigationManager.BaseUri);

        if (Uri.TryCreate(uri, UriKind.Absolute, out var absoluteUri) &&
            baseUri.IsBaseOf(absoluteUri))
        {
            navigationManager.NavigateTo(
                navigationManager.ToBaseRelativePath(absoluteUri.ToString()));
            return;
        }

        navigationManager.NavigateTo("");
    }
}