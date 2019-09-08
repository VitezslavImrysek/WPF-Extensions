# WPF Extensions - Public System.Windows.Expression API
WPF Extensions uses the power of Roslyn and Mono.Cecil to unlock internal extensibility points of the WPF.

# Introduction
Expression is at hearth of the WPF property system. Things like bindings and dynamic resources are built using Expression. While Expression itself is a public type, its constructor and methods are internal and as such can't be used by user code directly.

This project produces WPFExtensions.dll which contains new ExpressionBase type which defines new public methods for all the internal methods. 

# Usage
1. Run the console application to produce WPFExtensions.dll
2. Reference the created dll from your WPF application.
3. Declare your new Expression inheriting from ExpressionBase (System.Windows.Extensions namespace).
4. Profit

# Notes
Note that all the hard work is done during compilation and there is no runtime overhead.
The used IgnoresAccessChecksToAttribute might require a newer .NET Framework CLR (Something like 4.6+ should be OK). 
Net Core 1.0+ should be fine too (looking at you .Net Core 3.0).
