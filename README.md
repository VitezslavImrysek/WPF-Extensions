# WPF Extensions
This project extends the WPF API to unlock additional capabilities.

# Features
All features can be found inside System.Windows.Extensions namespace.

# System.Windows.Expression
Expression is at heart of the WPF property system. Things like bindings and dynamic resources are implemented using Expression. While Expression itself is a public type, its constructor and methods are internal and as such can't be used by user code directly.
For this purpose the WPF Extension project includes ExpressionBase type which defines new public methods mapping the internal methods.
Custom ExpressionBase instance can then be set to a DependencyObject using standard DependencyObject.SetValue call.

WPF Extensions uses combination of Roslyn and Mono.Cecil to implement this particular feature.
Note that all the hard work is done during compilation and there is no runtime overhead.
The used IgnoresAccessChecksToAttribute might require a newer .NET Framework CLR (Something like 4.6+ should be OK). 
Net Core 1.0+ should be fine too (mostly case for .Net Core 3.0 which has WPF implementation).

# DpiHelper
WPF is a DPI-aware UI framework. But its not really simple to test the looks of an app with different DPIs.
DpiHelper class contains two static methods to make this testing easier:
- SetApplicationDpi: Sets specified Dpi for entire application (applies to both current and new windows).
- SetWindowDpi:  Sets specified Dpi for specified window.

Note that overriding Popup Dpi is not yet supported.

# Usage
1. Getting the dll.
1.A Run the console application to produce WPFExtensions.dll.
1.B Download the already precompiled WPFExtensions.dll from releases.
2. Reference the dll from your WPF application.
3. Use any of the extensions in System.Windows.Extensions namespace.
3.A Declare your new Expression inheriting from ExpressionBase.
3.B Use DpiHelper to change Dpi.
4. Profit
