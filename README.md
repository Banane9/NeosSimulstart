Simulstart
==========

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that makes it possible to launch other processes together with Neos.  
Processes can be launched with (a request for) administrator privileges and can automatically be closed with the game as well.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
2. Place [Simulstart.dll](https://github.com/Banane9/NeosSimulstart/releases/latest/download/Simulstart.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
3. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## Usage

Since NeosModSettings can't handle custom types, this mod requires editing its settings file to add or remove processes.

It should look as follows:
```JSON
{
    "version":"1.0.0",
    "values":
    {
        "ProcessesToLaunch":
        [
            {
                "Administrator":true,
                "Arguments":"-extra LaunchArguments",
                "KeepOpen":true,
                "Path":"C:\\Path\\To\\AdministratorExecutable.exe",
                "WorkingDirectory": "C:\\Path\\To\\WorkingDirectory"
            },
            {
                "Restart": true,
                "Path":"C:\\Path\\To\\SimpleExecutable.exe"
            }
        ]
    }
}
```


| Field  | Usage |
| :---: | :--- |
| `Administrator` | Launch process with administrator privileges if `true`. Default: `false` |
| `Arguments` | Launch process with extra parameters. Default: none |
| `KeepOpen` | Leave process running when Neos is getting shut down. Default: `false` |
| `Path` | The path of the executable file to launch. Required value. |
| `Restart` | Restart process if it exits while Neos is still running. Default: `false` |
| `WorkingDirectory` | The directory to execute the file in. Default: The executable's directory. |