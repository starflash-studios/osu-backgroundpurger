![Banner](https://user-images.githubusercontent.com/16698604/75624773-613fb600-5bf2-11ea-91e1-678ef262bb6f.png)
Liberate your storage space, and the eyes of onlookers, with this automated osu! background / video remover. Also doubles as a background blurrer - for when your underage anime grills are just a little too underage ;)


![HeaderRequired](https://user-images.githubusercontent.com/16698604/75624946-55ed8a00-5bf4-11ea-91a2-ddcdd45f2092.png)
# Requirements
- [x] [.NET Framework 4.7.2 (Runtime)](https://dotnet.microsoft.com/download/dotnet-framework)
- [x] Windows 10, 8.1, 8, 7 SP1, Vista / XP (Untested)
- [ ] A life


![HeaderInstall](https://user-images.githubusercontent.com/16698604/75624945-5423c680-5bf4-11ea-98a0-a1a3d2a6d1c8.png)
# Installation
1. [See Requirements](#requirements)
2. Download the latest stable version's 'Release.zip' file from the [Releases](https://github.com/starflash-studios/osu-backgroundpurger/releases) page.
3. Extract the archive to it's own folder, and move that folder to where you would like the program to be located.
4. Create any shortcuts to the program you would like (i.e. in `C:\ProgramData\Microsoft\Windows\Start Menu\Programs`).
5. Run 'no!background.exe' to start the program

# IMPORTANT
Please note: The below is an outdated readme.md file from versions 0.1.1.0 and below. Though the functionality remains almost the same, all below information is outdated and not relevant to the latest version. An updated readme.md file will be released soon.

*With that out of the way, let's begin.*

![HeaderModes](https://user-images.githubusercontent.com/16698604/75624881-b7612900-5bf3-11ea-8f3a-6080751e55df.png)
# Modes
* Disable
* Enable
* Delete
* Blur

## Disable
This mode renames all used background media files by appending .bkp to the filename, whilst simultaneously commenting out the related lines in the .osu file. This ensures that the files are preserved, whilst also making sure osu! isn't annoyed by not finding the file. [This process can be undone](#enable).

## Enable
This mode renames all backup files created in the [previous process](#disable) by removing the .bkp from the end of the file name. This process also ensures to uncomment the related lines in the .osu file to allow osu! to know that the backgrounds are available again.

## Delete
This mode deletes all background media files, whilst simultaneously comment out the related lines in the .osu file. This process saves space, but **can not be undone**. For a process that can be undone, see [disable](#disable) and [enable](#enable).

## Blur
This mode blurs all supported background image files (*.png, *.jpg & *.bmp). Original files have .bkp appended to the end of the file so that the process can be undone by overriding the new files. It is recommended to delete all .bkp files to save space if you are happy with the results.


![HeaderUI](https://user-images.githubusercontent.com/16698604/75624880-b62ffc00-5bf3-11ea-814b-f32f03965525.png)
# UI
Once a mode has been selected by clicking on the relevant name, the user may choose to either process a [select](#select) amount of beatmaps, or [all](#auto) beatmaps found within a folder

## Select
![SelectButton](https://user-images.githubusercontent.com/16698604/75624884-bb8d4680-5bf3-11ea-9d48-ad93192acd03.png)

Upon clicking on the 'SELECT' button, a file browsing dialog will open. The user may then choose the beatmap folders they would like to process. In windows, holding CTRL or SHIFT will allow the user to select multiple folders at once.

## Auto
![AutoButton](https://user-images.githubusercontent.com/16698604/75625433-e4fca100-5bf8-11ea-80fb-c6f84ed375a1.png)

Upon clicking on the 'AUTO' button, a file browsing dialog will open. The user must then select the folder **CONTAINING** all the beatmap folders. The program will then iterate through **ALL** found beatmaps and apply the selected mode.


![License](https://user-images.githubusercontent.com/16698604/75624962-7b7a9380-5bf4-11ea-9395-6c818508d7eb.png)
# License
`GNU GPL v3: `
> This program is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version. This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details. You should have received a copy of the GNU General Public License along with this program. If not, see <https://www.gnu.org/licenses/>.
