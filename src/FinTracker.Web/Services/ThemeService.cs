using Microsoft.JSInterop;

namespace FinTracker.Web.Services;

public sealed class ThemeService(IJSRuntime js)
{
    private bool _initialized;
    private bool _isDarkMode;
    private ThemeMode _mode = ThemeMode.System;

    public event Action? Changed;

    public ThemeMode Mode => _mode;

    public bool IsDarkMode => _isDarkMode;

    public async Task InitializeAsync()
    {
        if (_initialized)
            return;

        _initialized = true;

        try
        {
            var module = await js.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
            await module.InvokeVoidAsync("init", DotNetObjectReference.Create(this));
            var mode = await module.InvokeAsync<string>("getMode");
            _mode = ParseMode(mode);
            _isDarkMode = await module.InvokeAsync<bool>("apply", _mode.ToString().ToLowerInvariant());
        }
        catch (JSException)
        {
            _mode = ThemeMode.System;
            _isDarkMode = false;
        }

        Changed?.Invoke();
    }

    public async Task SetModeAsync(ThemeMode mode)
    {
        if (_mode == mode && _initialized)
            return;

        _mode = mode;

        try
        {
            var module = await js.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
            await module.InvokeVoidAsync("setMode", mode.ToString().ToLowerInvariant());
            _isDarkMode = await module.InvokeAsync<bool>("isDark");
        }
        catch (JSException)
        {
            _isDarkMode = mode == ThemeMode.Dark;
        }

        Changed?.Invoke();
    }

    [JSInvokable]
    public async Task OnSystemThemeChanged()
    {
        if (_mode != ThemeMode.System)
            return;

        try
        {
            var module = await js.InvokeAsync<IJSObjectReference>("import", "./js/theme.js");
            var dark = await module.InvokeAsync<bool>("isDark");
            if (_isDarkMode == dark)
                return;

            _isDarkMode = dark;
        }
        catch (JSException)
        {
            return;
        }

        Changed?.Invoke();
        await Task.CompletedTask;
    }

    private static ThemeMode ParseMode(string? value) => value?.ToLowerInvariant() switch
    {
        "light" => ThemeMode.Light,
        "dark" => ThemeMode.Dark,
        _ => ThemeMode.System
    };
}
