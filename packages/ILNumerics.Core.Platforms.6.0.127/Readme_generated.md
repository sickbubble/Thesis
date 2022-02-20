# ILNumerics Ultimate VS 
This package is part of [ILNumerics Ultimate VS](https://ilnumerics.net) - a powerful framework for technical application development on .NET. 
ILNumerics provides packages for the convenient authoring of: 
- Fast numerical algorithms in Matlab(R) and numpy style
    - Efficient n-d array classes: [ILNumerics.Computing](https://www.nuget.org/packages/ILNumerics.Computing) ([online docu](https://ilnumerics.net/docs-core.html))
    - Large selection of fundamental, [mathematical array functions](https://ilnumerics.net/apidoc/?topic=html/Methods_T_ILNumerics_ILMath.htm)
- Toolboxes for 
    - Interpolation: [ILNumerics.Toolboxes.Interpolation](https://www.nuget.org/packages/ILNumerics.Toolboxes.Interpolation) ([online docu](https://ilnumerics.net/ilnumerics-interpolation-toolbox.html#))  
    - Optimization: [ILNumerics.Toolboxes.Optimization](https://www.nuget.org/packages/ILNumerics.Toolboxes.Optimization) ([online docu](https://ilnumerics.net/ilnumerics-optimization-toolbox.html))
    - Statistics: [ILNumerics.Toolboxes.Statistics](https://www.nuget.org/packages/ILNumerics.Toolboxes.Statistics) ([online docu](https://ilnumerics.net/ilnumerics-statistics-toolbox.html))
    - HDF5: [ILNumerics.IO.HDF5](https://www.nuget.org/packages/ILNumerics.IO.HDF5) ([online docu](https://ilnumerics.net/hdf5-interface.html))
- Flexible, good looking, fast and robust 2D and 3D visualizations.
    + ILNumerics Visualization Engine: [ILNumerics.Drawing](https://www.nuget.org/packages/ILNumerics.Drawing): ([online docu](https://ilnumerics.net/Visualization-API.html))
        - Scene graph based rendering of line-, scatter-, XY-, contour-, surface plots. 
        - Compose complex scenes built of simple point-, lines-, and triangle-shapes. 
        - Output to common image formats and SVG.
    + Windows Forms Controls: [ILNumerics.Drawing.Platforms](https://www.nuget.org/packages/ILNumerics.Drawing.Platforms)
        - Interactivity on Windows.Forms: ILNumerics.Drawing.Panel 
        - Hardware acceleration (OpenGL) 
        - Fully featured GDI+ fallback.
    - ILNumerics Drawing Extensions: [ILNumerics.Toolboxes.Drawing2](https://www.nuget.org/packages/ILNumerics.Toolboxes.Drawing2)
        - [Bar-](https://ilnumerics.net/bar-plots.html), [spline-](https://ilnumerics.net/spline-interpolated-lines.html), region-, error-, candlestick-, fill area-, stacked area-, box plots.
        - More and [faster](https://ilnumerics.net/interactive-fast-surface-visualization.html) surfaces, [smooth surface](https://ilnumerics.net/scattered-data-smooth-surface.html). 
- Visual Studio Tools: get the free [Array Visualizer](https://ilnumerics.net/visualstudio-extension.html) from [MS Marketplace](https://marketplace.visualstudio.com/items?itemName=ILNumericsGmbH.ilnumericsVS600)

### Compatibility
ILNumerics is compatible with all .NET runtimes since .NET Framework 4.6.1., including .NET Core, .NET 5.0, .NET 6.0. 

We've invested great effort to translate and keep up to date the quasi standard algorithms for all computational routines, including [linear algebra](https://netlib.org/lapack), [FFT](https://www2.cisl.ucar.edu/resources/legacy/fft5) and [optimization](https://netlib.org/minpack/index.html) methods. These _purely managed_ implementations are the first to bring professional robustness and precision to .NET. Hence, ILNumerics runs efficiently on any platform supported by .NET! Optionally, native libraries can be used to replace the managed default implementation. For the Windows platform, such native, optimized libraries are provided, too.

### Performance 
ILNumerics apps are typically much faster than alternatives: faster to implement and faster during execution. With ILNumerics the size of your data is not [limited](https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/gcallowverylargeobjects-element) by the managed heap. ILNumerics n-dimensional Array&lt;T> bridges the gap from high-level mathematical expressions down to native memory robustly, efficiently, and transparently. It allows to connect your data to any 3rd party without hassle. Further, it is able to host your data on any computational resource found, including CPUs and GPUs - key for [heterogeneous, parallel computing](https://ilnumerics.net/blog/ilnumerics-comes-next). 

### About this Package
This package supports the ILNumerics infrastructure. Users commonly don't reference 
this package directly. Instead, start with the [Computing Engine](https://www.nuget.org/packages/ILNumerics.Computing) or the [Visualization Engine](https://www.nuget.org/packages/ILNumerics.Drawing).

### General Preparation
- install the [ILNumerics Visual Studio extension](https://marketplace.visualstudio.com/items?itemName=ILNumericsGmbH.ilnumericsVS600)
- download your [trial license](https://ilnumerics.net/download.html).
- [activate your seat](https://ilnumerics.net/license-activation.html).

### Quick Start (Computing Engine)
- reference the [Computing Engine](https://www.nuget.org/packages/ILNumerics.Computing) package in your project. 
- include the following 'using' directives (C#): 

        using ILNumerics; 
        using static ILNumerics.ILMath; 
        using static ILNumerics.Globals; 

- read the [beginners guide](https://ilnumerics.net/computing-basics.html) and start implementing your algorithms! 

### Documentation
- https://ilnumerics.net/general-usage_v5.html#

### Examples 
[ILNumerics example collection](https://ilnumerics.net/examples.html)
