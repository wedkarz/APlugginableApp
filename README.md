# APlugginableApp (APA)

A Plugginable App demonstrates ability to run plugins that transform text input to some other text.


Invoking *apa.exe* will give you hint what is available.

```powershell
Usage: apa [command] [options]

Options:
  --version     Show version information
  -?|-h|--help  Show help information

Commands:
  interactive   Runs interactive interpreter aka. REPL
  list          Lists available plugins
  run           Runs single plugin with argument

Run 'apa [command] -?|-h|--help' for more information about a command.
  ```
  
  
  Basically you have 3 options:
  
  1) list all available plugins: *apa list*. Depending on installed plugins you will see a list of them. e.g.
  
  ```powershell
Politics        Hides names of most popuplar political parties in Poland
Count           Counts characters in input string
Reverse         Reverses input
Sum             Splits input by `+' sign, parses numbers and calculates sum of them
  ```
  
  There are no limitations on plugin names so you have to take care yourself to name plugins uniqly as apa will choose the first plugin it finds.
  Plugin name is derived from plugin class by convention: `public class PoliticsPlugin : IPlugin` will produce a `Politics` plugin. etc.
  
  
  2) run plugin with argument to see how it works. Two parameters are required *-p* for plugin and *-a* for its argument e.g `apa run -p Sum -a "100+123"`
  ```powershell
  PS C:\apa> .\apa.exe run -p Count -a "Litwo, Ojczyzno moja, ty jestes jak zdrowie"
  43
  ```
  
  3) Run in an interactive shell mode/REPL environment: `apa interactive`. To quit, just type exit.
  ```powershell
  PS C:\apa> .\apa.exe interactive   Enter command to execute. Type exit to exit.
apa> Politics("I love all political parties. Especially PO and PiS")
     I love all political parties. Especially ***** ** and ***** ***
apa> Sum("20+123")
     143
apa> Count("To be or not to be")
     18
apa> Reverse("It was a pleasure meeting you")
     uoy gniteem erusaelp a saw tI
apa> exit
```

## Creating Plugins
To provide your own plugins you need to implement interface. Interface type has to be present in your assembly and all types implementing `IPlugin` will be discovered as separate plugins. Interface does not have to be referenced from application assembly (although it would simplify relflection that is going underneath). Plugins have to be placed under `Plugins` directory. Plugis directory is expected in the same location as `apa.exe`.
Build of solution will provide you with a directory already created and SamplePlugins.dll present in Plugins directory.

```csharp
public interface IPlugin
{
    string Description { get; }
    string Execute(string input);
}
```
