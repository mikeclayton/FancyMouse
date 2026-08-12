; Unshipped analyzer release
; https://github.com/dotnet/roslyn-analyzers/blob/main/src/Microsoft.CodeAnalysis.Analyzers/ReleaseTrackingAnalyzers.Help.md

### New Rules

Rule ID | Category | Severity | Notes
--------|----------|----------|-------
WIN32GEN001 | FancyMouse.Win32Gen | Info | Win32ApiGeneratorDiagnostics.NoTemplateFound, a NativeMethods.txt entry with no matching wrapper template and no win32metadata classification either
WIN32GEN002 | FancyMouse.Win32Gen | Error | Win32ApiGeneratorDiagnostics.FunctionMissingTemplate, a NativeMethods.txt entry win32metadata confirms is a real function with no wrapper template yet
WIN32RESULT001 | FancyMouse.Win32Gen.Analyzers | Error | Win32ResultAnalyzerDiagnostics.UnhandledResult, a Win32Result/Win32ReturnCode-returning call not chained to .ThrowIfFailed() or .IgnoreFailure()
WIN32RESULT002 | FancyMouse.Win32Gen.Analyzers | Error | Win32ResultAnalyzerDiagnostics.ValuePropertyUsed, a direct .Value read on a Win32Result/Win32ReturnCode instead of .GetValue()
