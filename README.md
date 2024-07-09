# DetoursCustomDPI

[![NuGet version (DetoursCustomDPI)](https://img.shields.io/nuget/v/DetoursCustomDPI.svg?style=flat-square)](https://www.nuget.org/packages/DetoursCustomDPI/)

## Introduction
Override DPI for WinUI Unpackaged Applications using Detours.NET

## Supported Architectures
- x86
- x64
- ARM64

## Usage
```csharp
using DetoursCustomDPI;

// Must be called before InitializeComponent()
// After calling this method, the DPI of the application will be set to the specified value
// At this time, the DPI can't be changed dynamically after the application starts
public App()
{
	DetoursCustomDPI.Handler.OverrideDefaltDpi(192f); // 200% DPI
	InitializeComponent();
	// ...
	// ...
}
```

## Developer
`이호원 (Howon Lee) a.k.a hoyo321 or kck4156, airtaxi`

## License
MIT License
