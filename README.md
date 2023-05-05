# CAN Frame Decoding Script

This .NET project is designed to decode CAN frames and display the decoded data in an easy-to-read format.

## Getting Started

To use this project, you'll need to have the .NET SDK installed on your system. You can download it from the official .NET website: https://dotnet.microsoft.com/download

Once you have the .NET SDK installed, you can download or clone this repository to your local machine.

### Prerequisites

This project requires the following NuGet packages:

- `System.IO.Ports`
- `System.Runtime.Serialization.Json`
- `System.Threading.Tasks.Extensions`
- `Microsoft.Extensions.Configuration`
- `Microsoft.Extensions.Configuration.Json`
- `Microsoft.Extensions.Configuration.Binder`

These packages are included in the project's `.csproj` file, so you don't need to install them manually.

### Usage

The `CANframeDecodingScript` project can be run from the command line or from within Visual Studio. 

To use it, navigate to the directory where the project is located and run the following command:

dotnet run


This script is a prototype of what should be generate by a script writing application which will be online soon. This application fills in other cofigurations parameters needed. 

The script will then start listening for incoming CAN frames on the specified device. When a frame is received, it will be decoded and the decoded data will be displayed in the console.

## License

This project is licensed under the MIT License - see the LICENSE file for details.
