using Cove.Server.Plugins;
using Cove.Server;
using Cove.Server.Chalk;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cove.GodotFormat;
using SkiaSharp;
using System.Text;


// The json serialize/deserialize stuff is pretty much lifted from the Persistent Chalk
// Cove plugin. https://github.com/DrMeepso/CovePlugins
// Thank you Meepso.

namespace ChalkMod
{

// for the Raw bitmap stuff below I have used the following as a base:
// https://swharden.com/blog/2022-11-04-csharp-create-bitmap/
// Thank you Scott W Harden
    public struct RawColor
    {
        public readonly byte R, G, B;

        public RawColor(byte r, byte g, byte b)
        {
            (R, G, B) = (r, g, b);
        }

        public RawColor (string hexcolor) : this()
        {
            if (hexcolor.Length > 6)
            {
                hexcolor = hexcolor.Substring(hexcolor.Length - 6);
            }
            else if (hexcolor.Length < 6)
            {
                hexcolor = hexcolor.PadLeft(6,'0');
            }
            
            R = (byte)Convert.ToInt32(hexcolor.Substring(0,2), 16);
            G = (byte)Convert.ToInt32(hexcolor.Substring(2,2), 16);
            B = (byte)Convert.ToInt32(hexcolor.Substring(4,2), 16);                
        }
    }
    public class RawBitmap
    {
        public readonly int Width;
        public readonly int Height;
        private readonly byte[] ImageBytes;

        public RawBitmap(int width, int height)
        {
            Width = width;
            Height = height;
            ImageBytes = new byte[width * height * 4];
        }

        public void SetPixel(int x, int y, RawColor color)
        {
            //Console.Write("w:{0} h:{1} x:{2} y:{3} ",Width, Height, x,y);
            if (x >= 0 && y >= 0)
            {
                int offset = ((Height - y - 1) * Width + x) * 4;
                ImageBytes[offset + 0] = color.B;
                ImageBytes[offset + 1] = color.G;
                ImageBytes[offset + 2] = color.R;
            }
        }

        public RawColor GetPixel(int x, int y)
        {
            int offset = ((Height - y - 1) * Width + x) * 4;
            return new RawColor(ImageBytes[offset + 2],ImageBytes[offset + 1],ImageBytes[offset + 0]);
        }

        public void DrawCircle(int c_x, int c_y, int radius, RawColor color)
        {
            for (int y = c_y - radius; y <=c_y+radius; y++)
            {
                for ( int x = c_x - radius; x <= c_x +radius; x++)
                {
                    if (((x-c_x)*(x-c_x)+(y-c_y)*(y-c_y))<=(radius*radius))
                    {
                        SetPixel(x,y,color);
                    }
                }
            }
        }

        public void DrawRectangle(int f_x, int f_y, int t_x, int t_y, RawColor color)
        {
            for (int y = f_y; y <= t_y; y++)
            {
                for (int x = f_x; x <= t_x; x++)
                {
                    SetPixel(x,y,color);
                }
            }
        }

        public byte[] GetBitmapBytes()
        {
            const int imageHeaderSize = 54;
            byte[] bmpBytes = new byte[ImageBytes.Length + imageHeaderSize];
            bmpBytes[0] = (byte)'B';
            bmpBytes[1] = (byte)'M';
            bmpBytes[14] = 40;
            Array.Copy(BitConverter.GetBytes(bmpBytes.Length), 0, bmpBytes, 2, 4);
            Array.Copy(BitConverter.GetBytes(imageHeaderSize), 0, bmpBytes, 10, 4);
            Array.Copy(BitConverter.GetBytes(Width), 0, bmpBytes, 18, 4);
            Array.Copy(BitConverter.GetBytes(Height), 0, bmpBytes, 22, 4);
            Array.Copy(BitConverter.GetBytes(32), 0, bmpBytes, 28, 2);
            Array.Copy(BitConverter.GetBytes(ImageBytes.Length), 0, bmpBytes, 34, 4);
            Array.Copy(ImageBytes, 0, bmpBytes, imageHeaderSize, ImageBytes.Length);
            return bmpBytes;
        }

        public void Save(string filename, string format = "BMP", int quality = 100)
        {
            format = format.ToUpper();
            byte[] bytes = GetBitmapBytes();
            if (format == "BMP")
            {
                File.WriteAllBytes(filename, bytes);
            }
            else if (format == "PNG")
            {
                using SKBitmap bmp = SKBitmap.Decode(bytes);
                using SKBitmap scaledbmp = new SKBitmap(Width*2, Height*2);
                bmp.ScalePixels(scaledbmp, new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None));
                using SKFileWStream fs = new(filename);
                scaledbmp.Encode(fs, SKEncodedImageFormat.Png, quality: quality);
            }
        }
    }

    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override Vector2 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            float x = root.GetProperty("X").GetSingle();
            float y = root.GetProperty("Y").GetSingle();

            return new Vector2(x, y);
        }

        public override void Write(Utf8JsonWriter writer, Vector2 value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteNumber("X", value.x);
            writer.WriteNumber("Y", value.y);
            writer.WriteEndObject();
        }
    }

    public class ChalkCanvasConverter : JsonConverter<ChalkCanvas>
    {
        public override ChalkCanvas Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            var root = doc.RootElement;

            long canvasID = root.GetProperty("CanvasID").GetInt64();
            ChalkCanvas canvas = new ChalkCanvas(canvasID);

            if (root.TryGetProperty("ChalkImage", out JsonElement chalkImageElement))
            {
                var chalkImage = JsonSerializer.Deserialize<Dictionary<string, int>>(chalkImageElement.GetRawText(), options);

                var deserializedChalkImage = chalkImage.ToDictionary(
                    kvp => JsonSerializer.Deserialize<Vector2>(kvp.Key, options),
                    kvp => kvp.Value
                );

                canvas.chalkImage = deserializedChalkImage;
            }

            return canvas;
        }

        public override void Write(Utf8JsonWriter writer, ChalkCanvas value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            writer.WriteNumber("CanvasID", value.canvasID);

            writer.WritePropertyName("ChalkImage");
            writer.WriteStartObject();

            foreach (var kvp in value.chalkImage)
            {
                var key = JsonSerializer.Serialize(kvp.Key, options);
                writer.WritePropertyName(key);
                writer.WriteNumberValue(kvp.Value);
            }

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
    public class ChalkModSettings
    {
        public required string webhookuser { get; set; }
        public required string webhookurl { get; set; }
        public required string checkseconds { get; set; }
        public required string[] palette { get; set; }
    }
    public class ChalkMod : CovePlugin
    {
        public ChalkMod(CoveServer server) : base(server) { }
        private string currentDir = Directory.GetCurrentDirectory();
        private const string ChalkLog = "chalkLogs.txt";
        private ChalkModSettings chalkModSettings;
        public int timeToCheck;

        //private const string ChalkFile = "chalk.json";

        public override void onInit()
        {
            base.onInit();

            string jsonString = File.ReadAllText(Path.Combine(currentDir,"chalkmod.json"));
            chalkModSettings = JsonSerializer.Deserialize<ChalkModSettings>(jsonString);

            // Cant be less than 10 seconds. Default is 300 seconds.
            if (string.IsNullOrEmpty(chalkModSettings.checkseconds) || chalkModSettings.checkseconds == "")
            {
                timeToCheck = 300;
            }
            else if (Convert.ToInt32(chalkModSettings.checkseconds) >= 10)
            {
                timeToCheck = Convert.ToInt32(chalkModSettings.checkseconds);
            }
            else
            {
                timeToCheck = 300;
            }

            if (string.IsNullOrEmpty(chalkModSettings.webhookuser) || chalkModSettings.webhookuser == "")
                chalkModSettings.webhookuser = "WFChalk";

            Log("ChalkMod working! Backup delay set to "+timeToCheck.ToString()+" seconds.");

            RegisterCommand("backupchalk", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }

                backupChalk();
                SendPlayerChatMessage(player, "The chalk has been backed up!");
            });
            SetCommandDescription("backupchalk", "Backs up the chalk data");
        
            RegisterCommand("loadchalk", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }

                string filename = "";
                int canvasid = 4;

                if (args.Length == 0)
                {
                    Log("loadchalk command failed - you must provide a chalk json filename.");
                    SendPlayerChatMessage(player, "loadchalk command failed - you must provide a chalk json filename.");
                    return;
                }
                
                try
                {
                    if (Convert.ToInt32(args[args.Length-1]) >= 0 && Convert.ToInt32(args[args.Length-1]) <= 3 && args.Length >= 2)
                    {
                        canvasid = Convert.ToInt32(args[args.Length-1]);
                        filename = String.Join(" ",args,0,args.Length-1);
                    }
                    else
                    {
                        filename = String.Join(" ",args);
                    }
                }
                catch
                {
                    filename = String.Join(" ",args);
                }
                
                if (args[0] == "" || args[0] == null || !File.Exists(Path.Combine(currentDir,filename)) || filename.Length < 6 || filename.Substring(filename.Length - 5) != ".json" || filename.Contains(".."))
                {
                    Log("loadchalk command failed - file \"" + filename + "\" not found.");
                    SendPlayerChatMessage(player, "loadchalk command failed - file \"" + filename + "\" not found.");
                    return;
                }
                
                if (canvasid == 4)
                {
                    Log("loadchalk command attempting to load data from \"" + filename + "\"");
                    SendPlayerChatMessage(player, "loadchalk command attempting to load data from \"" + filename + "\"");
                }
                else
                {
                    Log("loadchalk command attempting to load data from canvas " + canvasid.ToString() + " in \"" + filename + "\"");
                    SendPlayerChatMessage(player, "loadchalk command attempting to load data from canvas " + canvasid.ToString() + " in \"" + filename + "\"");
                }
                loadChalk(currentDir, filename, canvasid);
            });
            SetCommandDescription("loadchalk", "loads chalk data from a json file");

            RegisterCommand("clearchalk", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }

                // Using -2 has a "all canvases" value because why not
                int canvasid = -2;

                if (args.Length > 0)
                {
                    if (args[0] != "" || args[0] != null)
                    {
                        try
                        {
                            canvasid = Convert.ToInt32(args[0]);
                            Log("clearing all the chalk on canvas ID " + canvasid.ToString() + ".");
                            SendPlayerChatMessage(player, "clearing all the chalk on canvas ID " + canvasid.ToString() + ".");
                        }
                        catch
                        {
                            Log("Invalid canvas ID!");
                            SendPlayerChatMessage(player, "Invalid canvas ID!");
                            return;
                        }
                    }
                }
                else
                {
                    Log("clearing all the chalk on all canvases.");
                    SendPlayerChatMessage(player, "clearing all the chalk on all canvases.");

                }

                clearChalk(canvasid);
            });
            SetCommandDescription("clearchalk", "Clears all or one canvas.");

            RegisterCommand("cleanupchalk", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }

                Log("cleaning up the chalk.");
                SendPlayerChatMessage(player, "cleaning up the chalk.");

                cleanupChalk();
            });
            SetCommandDescription("cleanupchalk", "Removes non-standard data from the chalk data");

            RegisterCommand("chalkmod", (player, args) =>
            {
                if (!IsPlayerAdmin(player))
                {
                    SendPlayerChatMessage(player, "You do not have permission to use this command!");
                    return;
                }

                Log("ChalkMod commands:");
                Log("backupchalk: Creates a backup of the current chalk into a timestamped JSON file. Also creates an image of the current chalk into a timestamped PNG file. If you have a Discord webhook URL in chalkmod.json, it will send the PNG to that URL.");
                Log("loadchalk: Loads the chalk data from the chalk JSON file into memory. File must be in the Cove server folder. After the filename you can specify a canvas number (0 to 3) to load only that canvas.");
                Log("clearchalk: Removes all chalk data from memory, unless you specify a canvas ID (0 to 3), then it will only clear that canvas.");
                Log("cleanupchalk: Removes all non-standard canvas data from memory. Non-standard is defined as canvas ID values of less than 0 or more than 3.");
            });
            SetCommandDescription("chalkmod", "Explains the ChalkMod commands.");
        }

        public long lastBackup = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // now
        public long lastCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds(); // now
        public long lastChalkUpdate = 0;
        public bool hadOfflineUpdate = false;
        public override void onUpdate()
        {
            base.onUpdate();

            if (ParentServer.AllPlayers.Count > 0)
                // At least 1 player is online, reset hadOfflineUpdate
                hadOfflineUpdate = false;
            
            if (hadOfflineUpdate)
                return;

            // I felt I shouldn't do a bunch of things multiple times a second, so instead
            // once every 30 seconds seemed better in my head
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastCheck < 30)
                return;

            lastCheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastBackup <= timeToCheck)
                return;

            lastChalkUpdate = new DateTimeOffset(File.GetLastWriteTimeUtc(Path.Combine(currentDir, ChalkLog))).ToUnixTimeSeconds();

            if (lastChalkUpdate > lastBackup)
            {
                Log("Chalk was updated in the past " + timeToCheck.ToString() + " seconds.");
                lastBackup = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                backupChalk();

                if (ParentServer.AllPlayers.Count == 0 && !hadOfflineUpdate)
                {
                    // We're doing a final update, mark as such
                    hadOfflineUpdate = true;
                }
            }
        }

        public override void onEnd()
        {
            base.onEnd();

            UnregisterCommand("backupchalk");
            UnregisterCommand("loadchalk");
            UnregisterCommand("clearchalk");
            UnregisterCommand("cleanupchalk");
            UnregisterCommand("chalkmod");
        }

        private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions
        {
            IncludeFields = true,
            Converters = { new Vector2Converter(), new ChalkCanvasConverter()}
        };

        public void backupChalk()
        {
            string[] palette = chalkModSettings.palette;

            string timestamp = string.Format("{0:yyyy-MM-dd_HH-mm-ss}", DateTime.Now);

            // making a copy of the chalk data to work with
            List<ChalkCanvas> chalkData = ParentServer.chalkCanvas;

            if (chalkData != null)
            {
                RawBitmap blankbmp = new RawBitmap(400, 400);
                blankbmp.DrawRectangle(0, 0, 399, 399, new RawColor("#6B8E23"));
                blankbmp.DrawCircle(99, 99, 100, new RawColor("#DEB887"));
                blankbmp.DrawCircle(299, 99, 50, new RawColor("#DEB887"));
                blankbmp.DrawCircle(99, 299, 90, new RawColor("#DEB887"));
                blankbmp.DrawCircle(299, 299, 90, new RawColor("#DEB887"));

                RawBitmap chalkbmp = blankbmp;

                foreach (var canvas in chalkData)
                {
                    //Log("CanvasID: {0}", canvas.canvasID);

                    int offsetX = 0;
                    int offsetY = 0;

                    switch (canvas.canvasID)
                    {
                        case 1:
                            offsetX = 200;
                            break;
                        case 2:
                            offsetX = 200;
                            offsetY = 200;
                            break;
                        case 3:
                            offsetY = 200;
                            break;
                    }

                    if (canvas.canvasID <= 4 && canvas.canvasID >= 0)
                    {
                        foreach (KeyValuePair<Vector2, int> entry in canvas.chalkImage.ToList())
                        {
                            //Log("Canvas{0},{1},{2}", canvas.canvasID, entry.Key, entry.Value);

                            if (entry.Key.x >= 1 && entry.Key.x <= 200 && entry.Key.y >= 1 && entry.Key.y <= 200)
                            {
                                int myx = Convert.ToInt32(entry.Key.x) + offsetX - 1;
                                int myy = Convert.ToInt32(entry.Key.y) + offsetY - 1;

                                // -1 is eraser
                                if (entry.Value == -1)
                                {
                                    chalkbmp.SetPixel(myx,myy,blankbmp.GetPixel(myx,myy));
                                }
                                else if (entry.Value <= 61 && entry.Value >= 0)
                                {
                                    chalkbmp.SetPixel(myx,myy,new RawColor(palette[entry.Value]));
                                }
                                else if (entry.Value > 61)
                                {
                                    string hexValue = entry.Value.ToString("X").PadLeft(6,'0');
                                    chalkbmp.SetPixel(myx,myy,new RawColor(hexValue));
                                }
                            }
                        }
                    }
                    // non-standard canvas IDs (usually from stamps mod) will get a separate PNG generated
                    // also those PNGs will not get pushed to the Discord webhook
                    else if (canvas.canvasID >= 4 || canvas.canvasID < -1)
                    {
                        RawBitmap stampbmp = new RawBitmap(400, 400);
                        stampbmp.DrawRectangle(0, 0, 399, 399, new RawColor("#DEB887"));

                        foreach (KeyValuePair<Vector2, int> entry in canvas.chalkImage.ToList())
                        {
                            //Console.WriteLine("{0},{1}", entry.Value + 1, palette.Length - 1);
                            if (entry.Value <= 6 && entry.Value >= 0)
                            {
                                stampbmp.SetPixel(Convert.ToInt32(entry.Key.x) + 10, Convert.ToInt32(entry.Key.y) + 10,new RawColor(palette[entry.Value]));
                            }
                        }
                        stampbmp.Save(canvas.canvasID.ToString() + "_" + "chalk_" + timestamp + ".png","PNG");
                    }
                }
                chalkbmp.Save(Path.Combine(currentDir,"chalk_" + timestamp + ".png"),"PNG");
                Log("Chalk Data Converted and saved to chalk_" + timestamp + ".png");

                postChalk(currentDir,"chalk_" + timestamp + ".png");

                // use the json formatter to serialize the chalk data
                string json = JsonSerializer.Serialize(chalkData, jsonOptions);

                // write the json string to a file
                File.WriteAllText(Path.Combine(currentDir,"chalk_" + timestamp + ".json"), json);
                Log("Chalk Data saved to chalk_" + timestamp + ".json");
            } else
            {
                Log("Failed to convert chalk data, chalk file is corrupt");
            }
        }
        public void loadChalk(string dirname, string chalkfilename, int canvasid)
        {
            byte[] chalkData = File.ReadAllBytes(Path.Combine(dirname, chalkfilename));
            Log("Chalk data file found. Loading chalk data...");
            
            List<ChalkCanvas> chalk = new List<ChalkCanvas>();

            // deserialize the chalk data
            try {
                chalk = JsonSerializer.Deserialize<List<ChalkCanvas>>(chalkData, jsonOptions);
            } catch
            {
                Log("Failed to restore chalk data, chalk file is corrupt");
            }

            if (chalk != null)
            {
                if (canvasid >= 0 && canvasid <= 3)
                {
                    foreach (var canvas in chalk)
                    {
                        if (canvas.canvasID == canvasid)
                        {
                            int index = ParentServer.chalkCanvas.FindIndex(a => a.canvasID == canvasid);
                            if (index == -1)
                            {
                                var newcanvas = new ChalkCanvas(canvasid);
                                newcanvas = canvas;
                            }
                            else
                            {
                                ParentServer.chalkCanvas[index] = canvas;   
                            }
                        }
                    }
                    Log("Restored Chalk Data for canvas "+ canvasid.ToString());
                } 
                else
                {
                    // set the chalk data to the server's chalk data
                    ParentServer.chalkCanvas = chalk;
                    Log("Restored Chalk Data");
                }
            } else
            {
                Log("Failed to restore chalk data, chalk file is corrupt or empty");
            }
        }

        public void clearChalk(int canvasid)
        {
            // using value less than -1 as a "all canvases" value
            if (canvasid < -1)
            {
                ParentServer.chalkCanvas.Clear();
                Log("Cleared all chalk data.");
            }
            else
            {
                int index = ParentServer.chalkCanvas.FindIndex(a => a.canvasID == canvasid);
                if (index != -1)
                {
                    ParentServer.chalkCanvas.RemoveAt(index);
                    Log("Cleared chalk data for canvas " + canvasid.ToString());
                }
            }
        }

        public void cleanupChalk()
        {
            List<long> canvasList = new List<long>();

            foreach (var canvas in ParentServer.chalkCanvas)
            {
                if (canvas.canvasID < 0 || canvas.canvasID > 3)
                {
                    canvasList.Add(canvas.canvasID);
                }
            }

            foreach (long canvasid in canvasList)
            {
                int index = ParentServer.chalkCanvas.FindIndex(a => a.canvasID == canvasid);
                if (index != -1)
                {
                    ParentServer.chalkCanvas.RemoveAt(index);
                    Log("Removed canvas ID " + canvasid.ToString() + " from the chalk data.");
                }
            }
        }
        public async void postChalk(string dirname, string chalkfilename)
        {
            if (String.IsNullOrEmpty(chalkModSettings.webhookurl) || chalkModSettings.webhookurl == "")
                return;

            using var httpClient = new HttpClient();
            using var content = new MultipartFormDataContent();

            byte[] fileBytes = File.ReadAllBytes(Path.Combine(dirname,chalkfilename));
            var fileContent = new ByteArrayContent(fileBytes);

            fileContent.Headers.TryAddWithoutValidation("Content-Type", "image/png");
            content.Add(fileContent, "file", chalkfilename);

            var messageData = new
            {
                username = chalkModSettings.webhookuser,
                content = chalkfilename
            };

            string jsonData = JsonSerializer.Serialize(messageData, jsonOptions);
            var jsonContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
            content.Add(jsonContent, "payload_json");
            
            Log("Sending " + chalkfilename + " to webhook");

            await httpClient.PostAsync(chalkModSettings.webhookurl, content);

            httpClient.Dispose();
        }
    }
}
