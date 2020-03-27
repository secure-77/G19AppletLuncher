using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;


namespace G19AppletLuncher
{
    class Program
    {
        
        /*
         * G19 Applect Luncher
         * use the settings.xml to add some applications you want to 
         * start from the G19 Display Luncher
         * 
         * Autor: Marc Kimpel
         * Web: https.//secure77.de
         * Git: 
         * 
         *  --> feel free to modifiy the code as you need 
         * 
         */


        // default stuff for debug console window
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        // page lock to force use back after application execution
        static Boolean noPageLock = true;


        // global settings
        static Boolean debugOn;
        static List<Apps> listEntries;
        static int maxLineNumber;
        static int maxIndexNumber;
        static int titelColorR;
        static int titelColorG;
        static int titelColorB;
        static int lineColorR;
        static int lineColorG;
        static int lineColorB;
        static int selectedColorR;
        static int selectedColorG;
        static int selectedColorB;
        static String prefixSelector;
        static String suffixSelector;
        const String appNAME = "NAME";
        const String appPATH = "PATH";



        static void Main(string[] args)
        {

            // check and load config
            AppSettings loadedSettings = CheckandLoadConfig();
            listEntries = loadedSettings.Applications;


            // set settings
            titelColorR = utils.HexToColor(loadedSettings.GlobalSettings.TitelColor).R;
            titelColorG = utils.HexToColor(loadedSettings.GlobalSettings.TitelColor).G;
            titelColorB = utils.HexToColor(loadedSettings.GlobalSettings.TitelColor).B;
            lineColorR = utils.HexToColor(loadedSettings.GlobalSettings.LineColor).R;
            lineColorG = utils.HexToColor(loadedSettings.GlobalSettings.LineColor).G;
            lineColorB = utils.HexToColor(loadedSettings.GlobalSettings.LineColor).B;
            selectedColorR = utils.HexToColor(loadedSettings.GlobalSettings.SelectedEntryColor).R;
            selectedColorG = utils.HexToColor(loadedSettings.GlobalSettings.SelectedEntryColor).G;
            selectedColorB = utils.HexToColor(loadedSettings.GlobalSettings.SelectedEntryColor).B;
            prefixSelector = loadedSettings.GlobalSettings.PrefixSelector;
            suffixSelector = loadedSettings.GlobalSettings.SuffixSelector;
            debugOn = loadedSettings.GlobalSettings.DebugMode;


            // init device
            InitDevice(loadedSettings.GlobalSettings.AppTitle);


            // initial line numbers, indexe etc. for scrolling and paging
            maxIndexNumber = listEntries.Count - 1;
            if (maxIndexNumber >= 7) { maxLineNumber = 7; } else { maxLineNumber = maxIndexNumber; }
            int indexNumber = -1;
            int lineNumber = -1;
            int pageNumber = 0;

            // load display entries
            InitDisplayEntries();

            // main loop
            while (1 == 1)
            {
                // set time or titel
                if (loadedSettings.GlobalSettings.ShowClockInsteadOfTitel)
                { LogitechGSDK.LogiLcdColorSetTitle(DateTime.Now.ToShortTimeString(), titelColorR, titelColorG, titelColorB);}
                else
                { LogitechGSDK.LogiLcdColorSetTitle(loadedSettings.GlobalSettings.AppTitle, titelColorR, titelColorG, titelColorB); }              
                LogitechGSDK.LogiLcdUpdate();


                // go down button
                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_DOWN) & noPageLock)
                {
                    // count line number until end of page (7)
                    if (lineNumber < maxLineNumber) { lineNumber += 1; }
                    // jump to top
                    else if (lineNumber == maxIndexNumber) { lineNumber = 0; }

                    // count index until max index
                    if (indexNumber == maxIndexNumber)
                    {
                        indexNumber = 0;
                        lineNumber = 0;
                    }
                    else { indexNumber += 1; }

                    WriteDebugMessage("-> DOWN <-- Line " + lineNumber + " index number:  " + indexNumber + " on Page " + pageNumber);
                    
                    // go down on Page 0 or 1
                    if (pageNumber == 0) { Reorder(lineNumber, indexNumber, appNAME); } else { Reorder(lineNumber, indexNumber, appPATH); }
                }

                // go up button
                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_UP) & noPageLock)
                {

                    // discount line number until start page (0)
                    if (lineNumber > -1 & indexNumber <= maxLineNumber) { lineNumber += -1; }

                    // discount until index or line -1
                    if (indexNumber == -1 | lineNumber == -1)
                    {
                        indexNumber = maxIndexNumber;
                        lineNumber = maxLineNumber;
                    }
                    else { indexNumber += -1; }

                    WriteDebugMessage("-> UP <-- Line " + lineNumber + " index number:  " + indexNumber + " on Page " + pageNumber);
                    
                    // go up Page 0 or 1
                    if (pageNumber == 0) { Reorder(lineNumber, indexNumber, appNAME); } else { Reorder(lineNumber, indexNumber, appPATH); }
                }

                // hit enter
                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_OK) & noPageLock)
                {
                    WriteDebugMessage("-> OK <-- Line " + lineNumber);
                    SelectEntryOK(lineNumber);
                }

                // go back (left)
                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_LEFT))
                {
                    WriteDebugMessage("-> BACK (left) <-- Line " + lineNumber);
                    pageNumber = 0;
                    Reorder(lineNumber, indexNumber, appNAME);
                    noPageLock = true;

                }

                // show path (right)
                if (LogitechGSDK.LogiLcdIsButtonPressed(LogitechGSDK.LOGI_LCD_COLOR_BUTTON_RIGHT) & noPageLock)
                {
                    WriteDebugMessage("-> SHOW PATH (right) <-- Line " + lineNumber);
                    Reorder(lineNumber, indexNumber, appPATH);
                    pageNumber = 1;
                }

                Thread.Sleep(100);
            }

        }

        //debug method
        static public void WriteDebugMessage(String message)
        {
            if (debugOn)
            {
                Console.WriteLine(message);
            }

        }

        // set background
        static public void SetBackground()
        {

            try
            {
                // use background from folder
                Image img = Image.FromFile(AppDomain.CurrentDomain.BaseDirectory + @"test.jpg");
                Bitmap bitmapImage = new Bitmap(img);

                // convert to byte
                byte[] pix = utils.GetPixel(bitmapImage);

                //set background           
                WriteDebugMessage("Background Set: " + LogitechGSDK.LogiLcdColorSetBackground(pix));
            }
            catch (Exception ex)
            {
                WriteDebugMessage("Could not load background image: " + ex.ToString());
            }

            LogitechGSDK.LogiLcdUpdate();
        }

        // init stuff
        static public void InitDevice(string AppTitle)
        {
            // init aPP 
            Boolean init = LogitechGSDK.LogiLcdInit(AppTitle, LogitechGSDK.LOGI_LCD_TYPE_COLOR);
            WriteDebugMessage("Initial status: " + init);

            // check for connection
            if (init == false)
            {
                Console.WriteLine("No device found, please check connection");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // hide console if no debug
            var handle = GetConsoleWindow();
            if (debugOn) { WriteDebugMessage("Connect status: " + LogitechGSDK.LogiLcdIsConnected(LogitechGSDK.LOGI_LCD_TYPE_COLOR)); }
            else { ShowWindow(handle, SW_HIDE); }

            // set background
            SetBackground();
        }


        // load or create Config
        static public AppSettings CheckandLoadConfig()
        {
            AppSettings settings;
            if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + @"\settings.xml"))
            {
                settings = AppSettings.Load();
                WriteDebugMessage("Settings loaded");
            }
            else
            {
                WriteDebugMessage("No settings.xml found");
                settings = new AppSettings();
                settings.Applications = new List<Apps>();
                settings.Applications.Add(new Apps { Name = "Menu Item 0", Path = @"C:\Windows\System32\calc.exe" });
                settings.Applications.Add(new Apps { Name = "Menu Item 1", Path = @"C:\Windows\System32\calc.exe" });
                settings.Applications.Add(new Apps { Name = "Menu Item 2", Path = @"C:\Windows\System32\calc.exe" });
                settings.Applications.Add(new Apps { Name = "Menu Item 3", Path = @"C:\Windows\System32\calc.exe" });
                settings.Applications.Add(new Apps { Name = "Menu Item 4", Path = @"C:\Windows\System32\calc.exe" });

                /* Some more default config entries for testing the scolling
                  settings.Applications.Add(new Apps { Name = "Menu Item 5", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 6", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 7", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 8", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 9", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 10", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 11", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 12", Path = @"C:\Windows\System32\calc.exe" });
                  settings.Applications.Add(new Apps { Name = "Menu Item 13", Path = @"C:\Windows\System32\calc.exe" });
                  */

                settings.GlobalSettings = new GlobalSettings();
                settings.GlobalSettings.DebugMode = false;
                settings.GlobalSettings.AppTitle = "App Luncher";
                settings.GlobalSettings.ShowClockInsteadOfTitel = true;
                settings.GlobalSettings.LineColor = "#FFFFFF";
                settings.GlobalSettings.SelectedEntryColor = "#FFA500";
                settings.GlobalSettings.TitelColor = "#FFA500";
                settings.GlobalSettings.PrefixSelector = "<";
                settings.GlobalSettings.SuffixSelector = ">";

                AppSettings.Save(settings);
                WriteDebugMessage("default settings created: settings.xml");
            }

            return settings;
        }

        // create initial display entries
        public static void InitDisplayEntries()
        {
           WriteDebugMessage(listEntries.Count + " apps found.. add them");

            for (int i = 0; i <= maxLineNumber; i++)
            {

                LogitechGSDK.LogiLcdColorSetText(i, listEntries[i].Name, lineColorR, lineColorG, lineColorB);
                WriteDebugMessage("Add Item: " + listEntries[i].Name);
            }

            LogitechGSDK.LogiLcdUpdate();
        }

         public static void Reorder(int line, int index, string valueType)
        {

            // remove alle entries
            RemoveEntries();

            // end of page, scroll down
            if (index >= 8)
            {
                Scroll(valueType, line, index, maxLineNumber);
            }
            // scroll up
            else if (line == 0 & index > 7)
            {
                Scroll(valueType, line, index, 0);           
            }
            // normal, no scrolling 
            else
            {
                NoScroll(valueType, line);
            }

            LogitechGSDK.LogiLcdUpdate();
        }



        // start external application
        public static void SelectEntryOK(int line)
        {

            // remove alle entries
            RemoveEntries();

            try
            {
                System.Diagnostics.Process.Start(listEntries[line].Path);
                WriteDebugMessage("Start Application: " + listEntries[line].Path);
                
                // display execute path
                LogitechGSDK.LogiLcdColorSetText(1, "Excute... " + listEntries[line].Name, selectedColorR, selectedColorG, selectedColorB);
                LogitechGSDK.LogiLcdColorSetText(3, "@ " + listEntries[line].Path, lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(5, "(go back with <- )", lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdUpdate();
            }
            catch (Exception ex)
            {

                var first = ex.Message.Substring(0, (int)(ex.Message.Length / 3));
                var second = ex.Message.Substring((int)(ex.Message.Length / 3), (int)(ex.Message.Length / 3));
                var third = ex.Message.Substring((ex.Message.Length / 3) + (ex.Message.Length / 3), (int)(ex.Message.Length / 3));

                WriteDebugMessage("Faled to start Application: " + ex.ToString() + " " + listEntries[line].Path);
                LogitechGSDK.LogiLcdColorSetText(0, "failed: " + listEntries[line].Name, selectedColorR, selectedColorG, selectedColorB);
                LogitechGSDK.LogiLcdColorSetText(1, "Error:", lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(2, first, lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(3, second, lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(4, third, lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(6, "@ " + listEntries[line].Path, lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdColorSetText(7, "(go back with <- )", lineColorR, lineColorG, lineColorB);
                LogitechGSDK.LogiLcdUpdate();

            }
            finally
            {
                noPageLock = false;

            }
        }
        // remove entries method
        public static void RemoveEntries()
        {
            for (int i = 0; i < 8; i++)
            {
                LogitechGSDK.LogiLcdColorSetText(i, "", 255, 255, 255);
            }

        }

        // get entry of type method
        public static string getListEntryValue(int index, string valueType)
        {
            if (valueType.Equals("PATH")) { return listEntries[index].Path; } else { return listEntries[index].Name; }
        }

        // scroll down method
        public static void Scroll(string valueType, int line, int index, int until)
        {
            for (int i = 0; i <= maxLineNumber; i++)
            {
                if (i == until) { LogitechGSDK.LogiLcdColorSetText(i, prefixSelector + getListEntryValue(index, valueType) + suffixSelector, selectedColorR, selectedColorG, selectedColorB); }
                else { LogitechGSDK.LogiLcdColorSetText(i, getListEntryValue(index + i - maxLineNumber, valueType), lineColorR, lineColorG, lineColorB); }
                LogitechGSDK.LogiLcdUpdate();
            }

        }

        // no scroling, just change Entry
        public static void NoScroll(string valueType, int line)
        {
            for (int i = 0; i < listEntries.Count; i++)
            {
                if (i == line) { LogitechGSDK.LogiLcdColorSetText(i, prefixSelector + getListEntryValue(i, valueType) + suffixSelector, selectedColorR, selectedColorG, selectedColorB); }
                else { LogitechGSDK.LogiLcdColorSetText(i, getListEntryValue(i, valueType), lineColorR, lineColorG, lineColorB); }
            }

        }

    }
}


