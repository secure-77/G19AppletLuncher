# G19 Applet Luncher
  [LCD Applet] Application luncher for the Logitech G19 Keyboard

Smal LCD Applet to start applications from the G19 Keyboard, you can define the name and path of the applications in the settings.xml. then use the selector of the keyboard to select one entry,  press ok to start the selected application.


### Features

- Individual list of applications stored in one config
- Show details (path) on second page (right button)
- Support some styling options (colors, selector, offset etc.)
- Support scrolling 
- Digitil Clock
- Debug Mode with console output


 ![](https://img.shields.io/badge/release-v1.0-blue)



###Requirements

- G19 or similiar Keyboard with 320 x 240 color LCD and supports LCD SDK_8.57.148
- [Logitech Gaming Software (9.0X)](https://support.logi.com/hc/de/articles/360025298053-Logitech-Gaming-Software)
- .NET 4.6.1

### Installation
- Compile the c# procject or download latest release from here
- Copy all the files to your disired location bevor you start the Applet
- Run G19AppletLuncher.exe
- Create autostart entry for the G19AppletLuncher.exe

### Settings
- All settings are stored in the settings.xml, these settings are not user-dependent
- If there is no settings.xml, the applet will create one with default settings on start
- background.jpg can be replaced by any other .jpg with resolution 320 x 240px, 
- background.psd =Photoshop Template to create background.jpg
- All colors need to be in hex
- The display provides 8 lines to display content, it is possible to define mote then 8, the content then, is scrollable.
- topOffset will decrease the amount of availible lines, but scrolling is still possible


| Settings name | Description                    |
| ------------- | ------------------------------ |
| `<DebugMode>`      | Enable console and print some infos    |
| `<AppTitle>`   | Will appear on the top of the display if  `ShowClockInsteadOfTitel` == false    |
| `<TitelColor>`   |  color for the title or clock     |
| `<LineColor>`   | text color of non selected items    |
| `<SelectedEntryColor>`   | text color of prefix, selected item and suffix    |
| `<PrefixSelector>`   | string: will appear infront of the selected item    |
| `<SuffixSelector>`   | string: will appear behind the selected item    |
| `<paddingLeft>`   | string: you can use whitespaces `&#032;` to right align the text   |
| `<topOffset>`   | int 0-7: creates offset to the title, you should not use more then 7 if you still want to see content   |
| `<Apps>` `<Name>`   | The name of the application or whatever you want to start |
| `<Apps>` `<Path>`   | The path of the application or whatever you want to start |

To use special characters like <, >, space etc. take care of xml encoding
https://en.wikipedia.org/wiki/List_of_XML_and_HTML_character_entity_references




### Troubleshooting
#### If you cant see the app in your display
- make sure the G19AppletLuncher.exe is running (taskmanager)
- use debugMode to check if there is any error
- make sure  "App Luncher" is availible and enabled in the Logitech Gaming Software
- try to:
1. close the G19AppletLuncher.exe via taskmanager
2. restart the Logitech Gaming Software 
3. start the G19AppletLuncher.exe 

#### If you see more then one "App Luncher" in the LCD or the Logitech Software
This is because the Applet registration depends on the path, so if you move the G19AppletLuncher.exe after you have already startet it, it is possible that appears twice in the menu. Logitech stores this registration information in the settings.json

`C:\Users\<UserName>\AppData\Local\Logitech\Logitech Gaming Software\settings.json`

So fix this by simple deleting all json objects with the name "App Luncher", donst worry, the next start of G19AppletLuncher.exe will create a new one.

Example entry from settings.json
```json
"e728" : 
      {
        "appPath" : "C:/G19AppletLuncher/bin/x64/Debug/G19AppletLuncher.exe",
        "autostartable" : false,
        "format" : 1,
        "iconPath" : "",
        "name" : "App Luncher"
      },
```
#### If the App Luncher is not responding
- make sure only one instance of G19AppletLuncher.exe is running
1. close the G19AppletLuncher.exe via taskmanager
2. restart the Logitech Gaming Software 
3. start the G19AppletLuncher.exe 

### Developer Informations
| File name | Description                    |
| ------------- | ------------------------------ |
| LogitechLcdEnginesWrapper.dll   | c++ to c# wrapper, must be stored in the same folder as the G19AppletLuncher.exe    |
Logitech SDK can be found here: https://www.logitechg.com/de-de/innovation/developer-lab.html
http://gaming.logitech.com/sdk/LCDSDK_8.57.148.zip




### Example Images



![](https://pandao.github.io/editor.md/examples/images/4.jpg)




