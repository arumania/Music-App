using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;
using System.Media;
using System.IO.Compression;
using System.Windows.Forms;

namespace ConsoleApp27
{
    class Program
    {
        [DllImport("winmm.dll")]
        private static extern uint mciSendString(
            string command,
            StringBuilder returnValue,
            int returnLength,
            IntPtr winHandle);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Int32 i);

        const int MF_BYCOMMAND = 0x00000000;
        const int SC_MINIMIZE = 0xF020;
        const int SC_MAXIMIZE = 0xF030;
        const int SC_SIZE = 0xF000;

        [DllImport("user32.dll")]
        public static extern int DeleteMenu(IntPtr hMenu, int nPosition, int wFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        public static int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            mciSendString("close wave", null, 0, IntPtr.Zero);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }

        internal static bool truefalse = false;
        internal static bool skip = false;
        internal static int currentCount = 0;
        internal static bool change = false;
        internal static bool changeSelection = false;
        internal static int pauseSegment = 0;
        internal static string playlist = "12312312369";
        internal static SoundPlayer player = new SoundPlayer();
        internal static int selectionCount = 0;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        static void DetectKey()
        {
            KeysConverter converter = new KeysConverter();
            string text = "";
            while (true)
            {
                Thread.Sleep(20);

                for (Int32 x = 0; x < 255; x++)
                {
                    string path = Directory.GetCurrentDirectory();
                    string[] index = new string[999];
                    string[] read4 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read4.Length; i++)
                    {
                        index[i] = i + ". " + read4[i];
                    }

                    int key = GetAsyncKeyState(x);

                    if (key == 1 || key == -32767)
                    {
                        
                        text = converter.ConvertToString(x);
                        if (text == "Left")
                        {
                            selectionCount++;
                            WriteSongs(index);
                        }
                        else if (text == "Right")
                        {
                            selectionCount--;
                            WriteSongs(index);
                        }
                        else if (text == "F6")
                        {
                            changeSelection = true;
                            change = true;
                            Task.Factory.StartNew(Playlist);
                        }
                        else
                        {

                        }
                    }
                }
            }
        }

        static void Main(string[] args)
        {
            DirectoryInfo info = new DirectoryInfo(Environment.CurrentDirectory);
            FileInfo[] files = info.GetFiles("*.mp3", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string input = file + "";
            }

            if (!File.Exists("youtube-dl.exe"))
            {
                WebClient client = new WebClient();
                client.DownloadFile("https://yt-dl.org/latest/youtube-dl.exe", "youtube-dl.exe");
            }
            if (!File.Exists("ffmpeg.exe"))
            {
                if (!Directory.Exists("ffmpeg.zip"))
                {
                    WebClient client = new WebClient();
                    client.DownloadFile("https://ffmpeg.zeranoe.com/builds/win64/static/ffmpeg-20190202-6dc06e9-win64-static.zip", "ffmpeg.zip");
                }

                ZipFile.ExtractToDirectory("ffmpeg.zip", "ffmpeg");
                File.Delete("ffmpeg.zip");

                DirectoryInfo dir = new DirectoryInfo("ffmpeg");
                DirectoryInfo[] dir2 = dir.GetDirectories();
                var read = dir2[0];
                string read2 = read + "";
                read2 = @"ffmpeg\" + read2 + @"\bin\ffmpeg.exe";
                File.Copy(read2, "ffmpeg.exe");
            }

            if (Directory.Exists("ffmpeg"))
                Directory.Delete("ffmpeg", true);

            if (File.Exists("log.txt"))
            {
                string[] readString = File.ReadAllLines("log.txt");
                string[] writeString = new string[readString.Length];
                for (int i = 0; i < readString.Length; i++)
                {
                    string wdaw = readString[i];
                    if (wdaw.Contains(".mp3"))
                    {
                        string wdaw2 = wdaw.Substring(0, wdaw.IndexOf(".") + 1);
                        wdaw2 = wdaw2.Replace(".", "");
                        ExecuteCMD(Environment.CurrentDirectory + "\ffmpeg -i " + wdaw + " -acodec pcm_s16le -ar 44100 " + wdaw2 + ".wav");
                        writeString[i] = wdaw2 + ".wav";
                        File.Delete(wdaw);
                    }
                    else
                    {
                        writeString[i] = readString[i];
                    }
                }

                File.Delete("log.txt");
                for (int i = 0; i < readString.Length; i++)
                {
                    using (StreamWriter sw = File.AppendText("log.txt"))
                        sw.WriteLine(writeString[i]);
                }
            }

            Console.Title = "";
            Console.ForegroundColor = ConsoleColor.White;
            string path = Directory.GetCurrentDirectory();
            if (!File.Exists(path + "\\" + "log.txt"))
            {
                var Create = File.Create(path + "\\" + "log.txt");
                Create.Close();
            }

            if (!File.Exists(path + "\\" + "log2.txt"))
            {
                var Create = File.Create(path + "\\" + "log2.txt");
                Create.Close();
            }

            if (!File.Exists(path + "\\" + "log3.txt"))
            {
                var Create = File.Create(path + "\\" + "log3.txt");
                Create.Close();
            }

            Console.WindowWidth = 87;
            Console.WindowHeight = 50;

            string[] readFileLength = File.ReadAllLines("log.txt");
            int finalLength = readFileLength.Length + 4;

            Console.WindowHeight = finalLength;

            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MINIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_MAXIMIZE, MF_BYCOMMAND);
            DeleteMenu(GetSystemMenu(GetConsoleWindow(), false), SC_SIZE, MF_BYCOMMAND);


            Menu();
        }
        public static string[] Sort(string[] read1, int where, int to)
        {
            int where1 = where;
            int to1 = to;
            int where2 = where;
            int to2 = to;
            string read = read1[where];
            string[] read2 = new string[read1.Length];
            for (int i = to2; i < where2; i++)
            {
                read2[i] = read1[i];
            }
            for (int i = to1; i < where1; i++)
            {
                read1[i + 1] = read2[i];
            }
            read1[to] = read;
            return read1;
        }
        public static string[] Sort2(string[] read1, int where, int to)
        {
            int where1 = where;
            int to1 = to;
            int where2 = where;
            int to2 = to;
            string read = read1[where];
            string[] read2 = new string[read1.Length];
            for (int i = 0; i < read2.Length; i++)
            {
                read2[i] = read1[i];
            }
            for (int i = to1; i > where1; i--)
            {
                read1[i - 1] = read2[i];
            }
            read1[to] = read;
            return read1;
        }
        static void ExecuteCMD(string input)
        {
            int filename = 0;
            while (File.Exists(filename + ".bat"))
                filename++;

            string path2 = Directory.GetCurrentDirectory();
            string dir2 = Directory.GetCurrentDirectory() + "\\" + filename++ + ".bat";
            using (StreamWriter sw = File.AppendText(dir2))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine(input);
            }
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = dir2;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            File.Delete(dir2);
            return;
        }
        static public void Playlist()
        {
            while (0 < 1)
            {
                Thread.Sleep(10);
                string path = Directory.GetCurrentDirectory();
                string musicToBePlayed = "gayman";
                string[] matchLogInput = File.ReadAllLines(path + "\\" + "log.txt");
                for (int i = 0; i < matchLogInput.Length; i++)
                {
                    if (currentCount == i)
                    {
                        musicToBePlayed = matchLogInput[i];
                        break;
                    }
                }
                if (musicToBePlayed == "gayman")
                {
                    currentCount = 0;
                }
                else
                {
                    ExecuteCMD("ffmpeg -i " + musicToBePlayed + " 2> output.txt");
                    string readFile = File.ReadAllText("output.txt");
                    File.Delete("output.txt");
                    string part = readFile.Substring(0, readFile.IndexOf("Duration: "));
                    string part2 = readFile.Replace(part, "");
                    string part3 = part2.Substring(0, part2.IndexOf(", bitrate:"));
                    part3 = part3.Replace("Duration: ", "");
                    part3 = part3.Replace(" ", "");
                    string hours = part3.Substring(0, part3.IndexOf(":"));

                    string minutes = part3.Replace(hours + ":", "");
                    minutes = minutes.Substring(0, minutes.IndexOf(":"));
                    string secounds = part3.Replace(hours + ":" + minutes + ":", "");
                    secounds = secounds.Substring(0, secounds.IndexOf("."));
                    int hours2 = Convert.ToInt32(hours);
                    int minutes2 = Convert.ToInt32(minutes);
                    int secounds2 = Convert.ToInt32(secounds);

                    int finalTime = (hours2 * 60 * 60 * 1000) + (minutes2 * 60 * 1000) + (secounds2 * 1000);

                    string[] matchnameinput = File.ReadAllLines(path + "\\" + "log2.txt");

                    string writeOutput = matchnameinput[currentCount];
                    string[] index2 = new string[999];
                    char[] readIndex = writeOutput.ToCharArray();
                    bool next = false;
                    string write = "";
                    for (int y = 0; y < readIndex.Length; y++)
                    {
                        if (readIndex[y] == ' ')
                        {
                            next = true;
                            write += " ";
                        }
                        else
                        {
                            if (next == true)
                            {
                                next = false;
                                string writeChar = readIndex[y] + "";
                                write += writeChar.ToUpper();
                            }
                            else
                            {
                                string writeWdaw = readIndex[y] + "";
                                writeWdaw = writeWdaw.ToLower();
                                string writeWdaw2 = writeWdaw.ToUpper();
                                if (y == 0)
                                    write += writeWdaw2;
                                else
                                {
                                    write += writeWdaw;
                                }
                            }
                        }
                    }

                    Play(musicToBePlayed);

                    string[] index = new string[999];
                    string[] read4 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read4.Length; i++)
                    {
                        index[i] = i + ". " + read4[i];
                    }
                    WriteSongs(index);

                    int thread = finalTime;
                    int thread2 = thread;
                    while (0 < 1)
                    {
                        if (truefalse == true)
                        {
                            truefalse = false;
                            return;
                        }
                        else if (thread == 0)
                        {
                            break;
                        }
                        else if (change == true)
                        {
                            change = false;
                            if (changeSelection == true)
                            {
                                changeSelection = false;
                                currentCount = selectionCount - 1;
                            }
                            break;
                        }
                        else if (skip == true)
                        {
                            skip = false;
                            break;
                        }
                        else
                        {
                            thread = thread - 200;
                            Thread.Sleep(200);
                        }
                    }
                    player.Stop();
                    if (change != true)
                    {
                        currentCount++;
                    }
                }
            }
        }

        static string ToUpper(string input)
        {
            string output = "";
            char[] readIndex = input.ToArray();
            bool next = false;
            string write = "";
            for (int y = 0; y < readIndex.Length; y++)
            {
                if (readIndex[y] == ' ')
                {
                    next = true;
                    write += " ";
                }
                else
                {
                    if (next == true)
                    {
                        next = false;
                        string writeChar = readIndex[y] + "";
                        write += writeChar.ToUpper();
                    }
                    else
                    {
                        string writeWdaw = readIndex[y] + "";
                        writeWdaw = writeWdaw.ToLower();
                        string writeWdaw2 = writeWdaw.ToUpper();
                        if (y == 0)
                            write += writeWdaw2;
                        else
                        {
                            write += writeWdaw;
                        }
                    }
                }
            }
            output = write;
            return output;
        }

        static void WriteSongs(string[] index)
        {
            Thread.Sleep(200);
            Console.Clear();
            Console.WriteLine("Number      Artist                        Song                     Extra\n");
            for (int i = 0; i < index.Length; i++)
            {
                string read = index[i];
                if (read == null)
                {
                    break;
                }
                else
                {
                    string[] index2 = new string[999];
                    char[] readIndex = read.ToCharArray();
                    bool next = false;
                    string write = "";
                    for (int y = 0; y < readIndex.Length; y++)
                    {
                        if (readIndex[y] == ' ')
                        {
                            next = true;
                            write += " ";
                        }
                        else
                        {
                            if (next == true)
                            {
                                next = false;
                                string writeChar = readIndex[y] + "";
                                write += writeChar.ToUpper();
                            }
                            else
                            {
                                string writeWdaw = readIndex[y] + "";
                                writeWdaw = writeWdaw.ToLower();
                                string writeWdaw2 = writeWdaw.ToUpper();
                                if (y == 0)
                                    write += writeWdaw2;
                                else
                                {
                                    write += writeWdaw;
                                }
                            }
                        }
                    }
                    string writeInt = write.Substring(0, write.IndexOf("."));
                    int writeIntInt = Convert.ToInt32(writeInt);
                    string output = "";
                    output = write.Replace(writeInt, "");
                    char[] outputCharArray = output.ToArray();
                    output = "";
                    
                    for (int x = 0; x < outputCharArray.Length; x++)
                    {
                        if ((x != 0) && (x != 1))
                        {
                            output += outputCharArray[x];
                        }
                    }

                    bool green = false;
                    bool yellow = false;
                    bool red = false;
                    bool blue = false;
                    bool white = false;
                    bool activated = false;

                    string[] readColorcode = File.ReadAllLines("log3.txt");

                    string outputColor = "";
                    for (int wdaw = 0; wdaw < readColorcode.Length; wdaw++)
                    {
                        string readColorcodeString = readColorcode[wdaw];
                        string subtractedColorcodeString = readColorcodeString.Substring(0, readColorcodeString.IndexOf("::"));
                        string outputToLower = output.ToLower();
                        readColorcodeString = readColorcodeString.Replace(subtractedColorcodeString + "::", "");
                        if (outputToLower.Contains(readColorcodeString))
                        {
                            activated = true;
                            outputColor = subtractedColorcodeString;
                            break;
                        }
                    }

                    if (outputColor == "g")
                    {
                        green = true;
                    }

                    if (outputColor == "y")
                    {
                        yellow = true;
                    }

                    if (outputColor == "r")
                    {
                        red = true;
                    }

                    if (outputColor == "b")
                    {
                        blue = true;
                    }

                    if (outputColor == "w")
                    {
                        white = true;
                    }

                    string extra = "";

                    if (output.Contains("::"))
                    {
                        string remove = output.Substring(0, output.LastIndexOf("::") + 1);
                        remove = remove.Replace(" :", "");
                        extra = output.Replace(remove + " ::", "");
                        output = remove;
                    }

                    if (Convert.ToInt32(writeInt) == currentCount)
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        string[] readLog = File.ReadAllLines("log2.txt");
                    }
                    Console.Write(writeInt + ".       ");
                    if (Convert.ToInt32(writeInt) < 10)
                        Console.Write(" ");
                    if (Convert.ToInt32(writeInt) < 100)
                        Console.Write("  ");
                    int writeInt2 = Convert.ToInt32(writeInt);
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.White;

                    bool artists = false;
                    string output2 = "";
                    if (output.Contains("-"))
                    {
                        output2 = output.Substring(output.IndexOf("-"));
                        output2 = output2.Replace("- ", "");
                        output2 = output.Replace(" - " + output2, "");
                        artists = true;
                    }

                    if (activated == false)
                    {
                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;

                        if (artists == true)
                        {
                            artists = false;
                            int lenght = output2.Length;
                            int lenghtFinal = 30 - lenght;
                            output = output.Replace(output2 + " - ", "");

                            string wdaw22 = "";
                            int finalLenghtSong = 25 - output.Length;
                            for (int x = 0; x < finalLenghtSong; x++)
                                wdaw22 += " ";

                            Console.Write(output2);
                            for (int f = 0; f < lenghtFinal; f++)
                                Console.Write(" ");
                            Console.Write(output + wdaw22);
                        }
                        else
                            Console.Write(output);

                        char[] extraChar = extra.ToArray();
                        if (extra != "")
                        {
                            string wdawRead = extraChar[0] + "";
                            wdawRead = wdawRead.ToUpper();
                            char[] extraChar2 = wdawRead.ToArray();
                            extraChar[0] = extraChar2[0];
                        }

                        for (int x = 0; x < extraChar.Length; x++)
                        {
                            Console.Write(extraChar[x]);
                        }
                        int extra2 = 20 - extra.Length;
                        for (int x = 0; x < extra2; x++)
                        {
                            Console.Write(" ");
                        }

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White; 
                        Console.Write("\n");
                    }
                    else
                    {
                        activated = false;
                        Console.ForegroundColor = ConsoleColor.White;
                        if (red == true)
                        {
                            red = false;
                            Console.BackgroundColor = ConsoleColor.Red;
                        }

                        if (green == true)
                        {
                            green = false;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Green;
                        }

                        if (white == true)
                        {
                            white = false;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.White;
                        }

                        if (yellow == true)
                        {
                            yellow = false;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.BackgroundColor = ConsoleColor.Yellow;
                        }

                        if (blue == true)
                        {
                            blue = false;
                            Console.BackgroundColor = ConsoleColor.Blue;
                        }

                        if (artists == true)
                        {
                            artists = false;
                            int lenght = output2.Length;
                            int lenghtFinal = 30 - lenght;
                            output = output.Replace(output2 + " - ", "");

                            string wdaw22 = "";
                            int finalLenghtSong = 25 - output.Length;
                            for (int x = 0; x < finalLenghtSong; x++)
                                wdaw22 += " ";

                            Console.Write(output2);
                            for (int f = 0; f < lenghtFinal; f++)
                                Console.Write(" ");
                            Console.Write(output + wdaw22);
                        }
                        else
                            Console.Write(output);

                        char[] extraChar = extra.ToArray();
                        if (extra != "")
                        {
                            string wdawRead = extraChar[0] + "";
                            wdawRead = wdawRead.ToUpper();
                            char[] extraChar2 = wdawRead.ToArray();
                            extraChar[0] = extraChar2[0];
                        }

                        for (int x = 0; x < extraChar.Length; x++)
                        {
                            Console.Write(extraChar[x]);
                        }
                        int extra2 = 20 - extra.Length;
                        for (int x = 0; x < extra2; x++)
                        {
                            Console.Write(" ");
                        }

                        Console.BackgroundColor = ConsoleColor.Black;
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write("\n");
                    }
                }
            }
            return;
        }
        static void Menu()
        {
            string path = Directory.GetCurrentDirectory();
            string[] index = new string[999];
            while (0 < 1)
            {
                string[] read4 = File.ReadAllLines(path + "\\" + "log2.txt");
                for (int i = 0; i < read4.Length; i++)
                {
                    index[i] = i + ". " + read4[i];
                }

                WriteSongs(index);

                Thread.Sleep(100);
                string input = Console.ReadLine();

                if (input.Contains("dl "))
                {
                    input = input.Replace("dl ", "");
                    string dir = DonwloadKeyword(input);
                    dir = path + "\\" + dir;
                    string dir2 = dir;
                    dir = dir.Replace(".mp3", ".wav");
                    using (StreamWriter sw = File.AppendText(path + "\\" + "log.txt"))
                        sw.WriteLine(dir);

                    ExecuteCMD(Environment.CurrentDirectory + @"\ffmpeg -i " + dir2 + " -acodec pcm_s16le -ar 44100 " + dir);
                    File.Delete(dir2);

                    using (StreamWriter sw = File.AppendText(path + "\\" + "log2.txt"))
                        sw.WriteLine(input);
                    string[] read2 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read2.Length; i++)
                    {
                        index[i] = i + ". " + read2[i];
                    }
                }
                else if (input == "skip")
                {
                    skip = true;
                }
                else if (input.Contains("sort "))
                {
                    input = input.Replace("sort ", "");
                    string from = input.Substring(0, input.IndexOf(" to "));
                    string to = input.Replace(from, "");
                    to = to.Replace(" to ", "");
                    int fromInt = Convert.ToInt32(from);
                    int toInt = Convert.ToInt32(to);

                    string[] dlString = File.ReadAllLines("log.txt");
                    string[] dlString2 = File.ReadAllLines("log2.txt");

                    if (fromInt > toInt)
                    {
                        dlString = Sort(dlString, fromInt, toInt);
                        dlString2 = Sort(dlString2, fromInt, toInt);
                    }
                    else
                    {
                        dlString = Sort2(dlString, fromInt, toInt);
                        dlString2 = Sort2(dlString2, fromInt, toInt);
                    }

                    File.Delete("log.txt");
                    File.Delete("log2.txt");

                    using (StreamWriter sw = File.AppendText("log.txt"))
                    {
                        for (int i = 0; i < dlString.Length; i++)
                        {
                            sw.WriteLine(dlString[i]);
                        }
                    }

                    using (StreamWriter sw = File.AppendText("log2.txt"))
                    {
                        for (int i = 0; i < dlString2.Length; i++)
                        {
                            sw.WriteLine(dlString2[i]);
                        }
                    }
                }
                else if (input.Contains("changesong "))
                {
                    input = input.Replace("changesong ", "");
                    string from = input.Substring(0, input.IndexOf(" to "));
                    string to = input.Replace(from, "");
                    to = to.Replace(" to ", "");
                    int fromInt = Convert.ToInt32(from);
                    int toInt = Convert.ToInt32(to);
                    ChangeSong(fromInt, toInt);
                }
                else if (input.Contains("changename "))
                {
                    string[] readcurrent = File.ReadAllLines("log2.txt");
                    input = input.Replace("changename ", "");
                    int overwrite = Convert.ToInt32(input);
                    Console.WriteLine("Old: " + readcurrent[overwrite]);
                    Console.Write("NEW NAME: >> ");
                    string readinput = Console.ReadLine();
                    readcurrent[overwrite] = readinput;
                    File.Delete("log2.txt");
                    var create = File.Create("log2.txt");
                    create.Close();
                    using (StreamWriter sw = File.AppendText("log2.txt"))
                    {
                        for (int i = 0; i < readcurrent.Length; i++)
                        {
                            sw.WriteLine(readcurrent[i]);
                        }
                    }
                }
                else if (input == "colorcode")
                {
                    Console.WriteLine("Input color: green (G), yellow (Y), red (R), blue (B), white (W)");
                    Console.Write("COLOR: >> ");
                    string inputReadColor = Console.ReadLine();
                    inputReadColor = inputReadColor.ToLower();
                    Console.Write("SEARCH TERM: >> ");
                    string inputSearchTerm = Console.ReadLine();
                    using (StreamWriter sw = File.AppendText("log3.txt"))
                    {
                        sw.WriteLine(inputReadColor + "::" + inputSearchTerm);
                    }
                }
                else if (input == "clear")
                {
                    Console.Clear();
                }
                else if (input.Contains("dldirect "))
                {
                    input = input.Replace("dldirect ", "");
                    string dir = DonwloadKeyword2(input);
                    dir = path + "\\" + dir;
                    using (StreamWriter sw = File.AppendText(path + "\\" + "log.txt"))
                        sw.WriteLine(dir);
                    using (StreamWriter sw = File.AppendText(path + "\\" + "log2.txt"))
                        sw.WriteLine(input);
                    string[] read2 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read2.Length; i++)
                    {
                        index[i] = i + ". " + read2[i];
                    }
                }
                else if (input.Contains("play "))
                {
                    input = input.Replace("play ", "");
                    int inputReadInt = Convert.ToInt32(input);
                    string[] matchLogInput = File.ReadAllLines(path + "\\" + "log.txt");
                    for (int i = 0; i < matchLogInput.Length; i++)
                    {
                        if (inputReadInt == i)
                        {
                            Play(matchLogInput[i]);
                            break;
                        }
                    }

                    index = new string[999];
                    string[] read3 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read3.Length; i++)
                    {
                        index[i] = i + ". " + read3[i];
                    }
                }
                else if (input == "stop")
                {
                    truefalse = true;
                    currentCount = 0;

                    player.Stop();

                    Thread.Sleep(200);
                    if (truefalse == true)
                        truefalse = false;
                }
                else if (input == "start playlist")
                {
                    Task.Factory.StartNew(Playlist);
                }
                else if (input.Contains("start specific "))
                {
                    input = input.Replace("start specific ", "");
                    currentCount = Convert.ToInt32(input);
                    Task.Factory.StartNew(Playlist);
                }
                else if (input.Contains("dlstart "))
                {
                    input = input.Replace("dlstart ", "");
                    string dir = DonwloadKeyword(input);
                    dir = path + "\\" + dir;
                    string dir2 = dir;
                    dir = dir.Replace(".mp3", ".wav");
                    using (StreamWriter sw = File.AppendText(path + "\\" + "log.txt"))
                        sw.WriteLine(dir);

                    ExecuteCMD(Environment.CurrentDirectory + @"\ffmpeg -i " + dir2 + " -acodec pcm_s16le -ar 44100 " + dir);
                    File.Delete(dir2);

                    using (StreamWriter sw = File.AppendText(path + "\\" + "log2.txt"))
                        sw.WriteLine(input);

                    string[] readDl = File.ReadAllLines("log.txt");
                    int inputReadInt = readDl.Length - 1;
                    string[] matchLogInput = File.ReadAllLines(path + "\\" + "log.txt");
                    for (int i = 0; i < matchLogInput.Length; i++)
                    {
                        if (inputReadInt == i)
                        {
                            Play(matchLogInput[i]);
                            break;
                        }
                    }

                    index = new string[999];
                    string[] read3 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read3.Length; i++)
                    {
                        index[i] = i + ". " + read3[i];
                    }
                }
                else if (input.Contains("change "))
                {
                    input = input.Replace("change ", "");
                    int inputInt = Convert.ToInt32(input);
                    currentCount = inputInt - 1;
                    change = true;
                }
                else if (input.Contains("delcolorcode "))
                {
                    input = input.Replace("delcolorcode ", "");
                    input = input + "::";
                    string[] readArray = File.ReadAllLines("log3.txt");
                    string[] outputArray = new string[999];
                    int outputCount = 0;
                    for (int i = 0; i < readArray.Length; i++)
                    {
                        if (!readArray[i].Contains(input))
                        {
                            outputArray[i] = readArray[i];
                            outputCount++;
                        }
                    }
                    File.Delete("log3.txt");
                    using (StreamWriter sw = File.AppendText("log3.txt"))
                    {
                        for (int i = 0; i < outputArray.Length; i++)
                        {
                            if (outputArray[i] != null)
                            {
                                sw.WriteLine(outputArray[i]);
                            }
                        }
                    }
                }
                else if (input.Contains("del "))
                {
                    try
                    {
                        bool after = false;
                        input = input.Replace("del ", "");
                        int del = Convert.ToInt32(input);
                        string[] readDel = File.ReadAllLines("log.txt");
                        string[] writeDel = new string[readDel.Length - 1];

                        File.Delete(readDel[del]);

                        for (int i = 0; i < readDel.Length; i++)
                        {
                            int y = i;
                            if (i == del)
                                after = true;
                            if (after == true)
                            {
                                y++;
                            }
                            if (y == readDel.Length)
                                break;
                            writeDel[i] = readDel[y];
                        }
                        File.Delete("log.txt");
                        var create = File.Create("log.txt");
                        create.Close();
                        for (int i = 0; i < writeDel.Length; i++)
                        {
                            using (StreamWriter sw = File.AppendText("log.txt"))
                                sw.WriteLine(writeDel[i]);
                        }

                        bool after2 = false;
                        string[] readDel2 = File.ReadAllLines("log2.txt");
                        string[] writeDel2 = new string[readDel2.Length - 1];
                        for (int i = 0; i < readDel2.Length; i++)
                        {
                            int y = i;
                            if (i == del)
                                after2 = true;
                            if (after2 == true)
                            {
                                y++;
                            }
                            if (y == readDel2.Length)
                                break;
                            writeDel2[i] = readDel2[y];
                        }
                        File.Delete("log2.txt");
                        var create2 = File.Create("log2.txt");
                        create2.Close();
                        for (int i = 0; i < writeDel2.Length; i++)
                        {
                            using (StreamWriter sw = File.AppendText("log2.txt"))
                                sw.WriteLine(writeDel2[i]);
                        }
                    }
                    catch
                    {

                    }

                    index = new string[999];
                    string[] read3 = File.ReadAllLines(path + "\\" + "log2.txt");
                    for (int i = 0; i < read3.Length; i++)
                    {
                        index[i] = i + ". " + read3[i];
                    }
                }
                else
                {

                }
            }
        }
        static void ChangeSong(int fromInt, int toInt)
        {
            string[] log1 = File.ReadAllLines("log.txt");
            string[] log2 = File.ReadAllLines("log2.txt");

            string log1Read1From = log1[fromInt];
            string log1Read2To = log1[toInt];

            string log2Read1From = log2[fromInt];
            string log2Read2To = log2[toInt];

            log1[fromInt] = log1Read2To;
            log1[toInt] = log1Read1From;

            log2[fromInt] = log2Read2To;
            log2[toInt] = log2Read1From;

            File.Delete("log.txt");
            File.Delete("log2.txt");

            using (StreamWriter sw = File.AppendText("log.txt"))
            {
                for (int i = 0; i < log1.Length; i++)
                {
                    sw.WriteLine(log1[i]);
                }
            }

            using (StreamWriter sw = File.AppendText("log2.txt"))
            {
                for (int i = 0; i < log2.Length; i++)
                {
                    sw.WriteLine(log2[i]);
                }
            }
            return;
        }
        static string DonwloadKeyword(string input)
        {
            string path = Directory.GetCurrentDirectory();
            string output = Query(input);
            string dir = Download(output);
            return dir;
        }
        static string DonwloadKeyword2(string input)
        {
            string path = Directory.GetCurrentDirectory();
            string dir = Download(input);
            return dir;
        }
        static void Play(string dir)
        {
            player = new SoundPlayer(dir);
            player.Play();

            return;
        }
        static string Download(string link)
        {
            Random rand = new Random();
            int filename = rand.Next(9, 9999);
            while (File.Exists(filename + ".mp3"))
            {
                Thread.Sleep(10);
                filename = filename + 5;
            }

            string path = Directory.GetCurrentDirectory();
            string dir2 = Directory.GetCurrentDirectory() + "\\" + filename++ + ".bat";
            string dl = path + "\\" + "youtube-dl.exe -o " + path + "\\" + filename + ".mp3 --extract-audio --audio-format mp3 " + link;
            using (StreamWriter sw = File.AppendText(dir2))
            {
                sw.WriteLine("@echo off");
                sw.WriteLine(dl);
            }
            Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = dir2;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            File.Delete(dir2);
            return filename + ".mp3";
        }
        static string Query(string input)
        {
            input = input.Replace(" ", "+");
            input = input.Replace("'", "%27");
            string queryString = "https://www.youtube.com/results?search_query=" + input;
            WebClient client = new WebClient();
            string dl = client.DownloadString(queryString);

            string pattern = @"<(a|link).*?href=(\""|')(.+?)(\""|').*?>";
            Regex reg = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regMatch = reg.Matches(dl);
            int count = 0;
            string output = "";
            foreach (Match match in regMatch)
            {
                if (count == 49)
                {
                    output = match + "";
                }
                count++;
            }

            output = output.Substring(output.IndexOf(" href") + 1);
            output = output.Remove(output.IndexOf(" class=\"") + 1);
            output = output.Replace("href", "");
            output = output.Replace("class=", "");
            output = output.Replace("\"", "");
            string output2 = "";
            for (int i = 1; i < output.Length; i++)
            {
                output2 += output[i];
            }
            output2 = "https://www.youtube.com" + output2;
            output2 = output2.Replace(" ", "");
            return output2;
        }
    }
}
