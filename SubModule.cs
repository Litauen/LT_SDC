using LT.Logger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem.GameState;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;


namespace LT_SDC
{
    public class SubModule : MBSubModuleBase
    {


        static string FullName = Directory.GetParent(Directory.GetParent(Directory.GetParent("LT_SDC").FullName).FullName).FullName;
        static string DataLocation = System.IO.Path.Combine(FullName, "Modules\\LT_SDC\\Data\\");

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

        }



        protected override void OnApplicationTick(float dt)
        {



            if (Game.Current != null && Game.Current.GameStateManager.ActiveState != null)
            {


                // this looks expensive to run on each tick
                if (Game.Current.GameStateManager.ActiveState.GetType() == typeof(MapState) && !Game.Current.GameStateManager.ActiveState.IsMenuState && !Game.Current.GameStateManager.ActiveState.IsMission)
                {



                    if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyReleased(InputKey.F9))
                    {

                        LTLogger.IMTAGreen("Ctrl-F9 pressed - parsing Old SDC");


                        string oldSDCFileName = "settlements_distance_cache.bin";
                        string fullPath = System.IO.Path.Combine(DataLocation, oldSDCFileName);

                        // Check if file exists
                        if (File.Exists(fullPath))
                        {
                            ParseOldSDC(oldSDCFileName, fullPath);
                        }
                        else
                        {
                            LTLogger.IMTARed($"Old SDC file does not exist: {fullPath}");
                        }


                    }

                    //if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyReleased(InputKey.F10))
                    //{

                    //    LTLogger.IMTAGreen("Ctrl-F10 pressed - parsing old Settlements.xml");

                    //    string fileName = "settlements.xml";
                    //    string fullPath = System.IO.Path.Combine(DataLocation, fileName);

                    //    // Check if file exists
                    //    if (File.Exists(fullPath))
                    //    {
                    //        //ParseSettlementsToCSV(fileName, fullPath);
                    //    }
                    //    else
                    //    {
                    //        LTLogger.IMTARed($"Old settleents.xml file does not exist: {fullPath}");
                    //    }


                    //}



                    if (Input.IsKeyDown(InputKey.LeftControl) && Input.IsKeyReleased(InputKey.F12))
                    {

                        string LOG_PATH = @"..\\..\\Modules\\LT_SDC\\logs\\";

                        LTLogger.IMTAGreen("Ctrl-F12 pressed - let's construct new SDC bin!");

                        // ---------------- check for data file presence -------------------
                        string fileName1 = "OldSDCParsed.CSV";
                        string fullPath1 = LOG_PATH + fileName1;
                        if (!File.Exists(fullPath1))
                        {
                            LTLogger.IMTARed($"File {fileName1} does not exist: {fullPath1}");
                            return;
                        } else { LTLogger.IMTAGreen($"  {fileName1} present"); }
                        string fileName2 = "OldSDCParsed_Mesh.CSV";
                        string fullPath2 = LOG_PATH + fileName2;
                        if (!File.Exists(fullPath2))
                        {
                            LTLogger.IMTARed($"File {fileName2} does not exist: {fullPath2}");
                            return;
                        }
                        else { LTLogger.IMTAGreen($"  {fileName2} present"); }
                        //string fileName3 = "OldSettlementsXMLParsed.CSV";
                        //string fullPath3 = LOG_PATH + fileName3;
                        //if (!File.Exists(fullPath3))
                        //{
                        //    LTLogger.IMTARed($"File {fileName3} does not exist: {fullPath3}");
                        //    return;
                        //} else { LTLogger.IMTAGreen($"  {fileName3} present"); }
                        // ----------------------------------------------------------


                        //int totalSettlements = File.ReadLines(fullPath3).Count();
                        int faceMapCount = File.ReadLines(fullPath2).Count();


                        //LTLogger.IMTAGreen($"Total settlements [{totalSettlements}]");
                        LTLogger.IMTAGreen($"Face Map Count [{faceMapCount}]");


                        // ----------- read settlement distances ----------
                        Dictionary<string, List<(string settlement, float distance)>> settlementDistances = new Dictionary<string, List<(string settlement, float distance)>>();
                        LoadSettlementDistances(fullPath1, settlementDistances);

                        // Test - show all connections from castle_tiedra_valladolid
                        //if (settlementDistances.ContainsKey("castle_tiedra_valladolid"))
                        //{
                        //    LTLogger.IMYellow("Connections from castle_tiedra_valladolid:");
                        //    foreach (var (settlement, distance) in settlementDistances["castle_tiedra_valladolid"])
                        //    {
                        //        LTLogger.IMYellow($"  -> {settlement}: {distance:F2}");
                        //    }
                        //}


                        Dictionary<string, bool> settlementTypes = new Dictionary<string, bool>();
                        //LoadSettlementTypes(fullPath3, settlementTypes);

                        // Test
                        //LTLogger.IMYellow($"castle_guejar is fortification: {settlementTypes["castle_guejar"]}"); // True
                        //LTLogger.IMYellow($"town_seville_cadiz is fortification: {settlementTypes["town_seville_cadiz"]}"); // False


                        Dictionary<int, string> faceMeshDict = new Dictionary<int, string>();
                        LoadFaceMeshMapping(fullPath2, faceMeshDict);

                        // Test
                        //LTLogger.IMYellow($"Face 24 maps to: {faceMeshDict[24]}"); // town_tbilisi
                        //LTLogger.IMYellow($"Face 26 maps to: {faceMeshDict[26]}"); // hideout_turk_22



                        // ----------------- create new bin file --------------

                        List<(string, string)> fortificationNeighbors = new List<(string, string)>();
                        //LoadFortificationNeighbors("fort_neighbors.csv", fortificationNeighbors);

                        // Generate binary file
                        //uint sceneXmlCrc = 0x08B6DBE2;    // Example CRC
                        //uint sceneNavMeshCrc = 0xAF678EB4; // Example CRC

                        uint sceneXmlCrc = 0x08B6DBE3;    // Custom CRC
                        uint sceneNavMeshCrc = 0xAF678EB5; // Custom CRC

                        GenerateSettlementCacheBinary(
                            LOG_PATH+"settlements_distance_cache_Default.bin",
                            sceneXmlCrc,
                            sceneNavMeshCrc,
                            settlementDistances,
                            settlementTypes,
                            fortificationNeighbors,
                            faceMeshDict
                        );

                    }


                }





            }



        }



        public static void GenerateSettlementCacheBinary(
            string outputPath,
            uint sceneXmlCrc,
            uint sceneNavMeshCrc,
            Dictionary<string, List<(string settlement, float distance)>> settlementDistances,
            Dictionary<string, bool> settlementTypes,
            List<(string fort1, string fort2)> fortificationNeighbors,
            Dictionary<int, string> faceMeshMapping)
        {


            string outputFileName = "debug_bin.log";

            using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
            using (BinaryWriter writer = new BinaryWriter(fileStream))
            {
                // 1. Write Header
                writer.Write(sceneXmlCrc);      // Scene XML CRC (4 bytes)
                writer.Write(sceneNavMeshCrc);  // Scene NavMesh CRC (4 bytes)
                writer.Write(settlementDistances.Count); // Primary Count (4 bytes)

                // 2. Write Main Distance Table
                foreach (var kvp in settlementDistances)
                {
                    string originName = kvp.Key;
                    //settlementTypes.TryGetValue(originName, out bool originIsFortification);

                    //LTLogger.WriteToFile(outputFileName, $"{originName},{kvp.Value.Count}");

                    writer.Write(originName);                    // Origin Name (string)
                    writer.Write(false);                         // bool isPortUsed = false;
                    writer.Write(kvp.Value.Count);               // Neighbor Count (int32)

                    foreach (var (neighborName, distance) in kvp.Value)
                    {
                        settlementTypes.TryGetValue(neighborName, out bool neighborIsFortification);

                        writer.Write(neighborName);              // Neighbor Name (string)
                        writer.Write(false);   // bool isPortUsed = false;
                        writer.Write(distance);                  // Distance (float)
                                                                 // Note: No landRatio for Land/Naval mode


                        //LTLogger.WriteToFile(outputFileName, $"  {neighborName},{distance}");
                    }
                }

                // 3. Write Fortification Neighbors
                writer.Write(fortificationNeighbors.Count);     // Fortification Edge Count
                //foreach (var (fort1, fort2) in fortificationNeighbors)
                //{
                //    writer.Write(fort1);                         // Fort A (string)
                //    writer.Write(fort2);                         // Fort B (string)
                //}

                // 4. Write Face-to-Settlement Mappings
                writer.Write(faceMeshMapping.Count);            // Face Map Count
                foreach (var kvp in faceMeshMapping)
                {
                    int faceIndex = kvp.Key;
                    string settlementName = kvp.Value;

                    writer.Write(faceIndex);                    // Face Index (int32)
                    writer.Write(settlementName);               // Settlement Name (string)
                    writer.Write(false);              // ool isPortUsed = false;
                }
            }

            LTLogger.IMTAGreen($"Binary file written to: {outputPath}");
        }



        public static void LoadFaceMeshMapping(string csvPath, Dictionary<int, string> faceMeshDict)
        {
            var lines = Array.Empty<string>();
            try
            {
                lines = File.ReadAllLines(csvPath);
            }
            catch (IOException)
            {
                LTLogger.IMTARed($"Error reading CSV file: IOException");
                return;
            }
            catch (Exception ex)
            {
                LTLogger.IMTARed($"Error reading CSV file: {ex.Message}");
                return;
            }

            foreach (var line in lines)
            {
                // Skip empty lines or comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Split by comma
                var parts = line.Split(',');

                if (parts.Length >= 2)
                {
                    int faceIndex = int.Parse(parts[0].Trim());
                    string settlementId = parts[1].Trim();

                    faceMeshDict[faceIndex] = settlementId;
                }
            }
        }


        public static void LoadSettlementTypes(string csvPath, Dictionary<string, bool> settlementTypes)
        {
            var lines = Array.Empty<string>();
            try
            {
                lines = File.ReadAllLines(csvPath);
            }
            catch (IOException)
            {
                LTLogger.IMTARed($"Error reading CSV file: IOException");
                return;
            }
            catch (Exception ex)
            {
                LTLogger.IMTARed($"Error reading CSV file: {ex.Message}");
                return;
            }

            foreach (var line in lines)
            {
                // Skip empty lines or comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Split by comma
                var parts = line.Split(',');

                if (parts.Length >= 2)
                {
                    string settlementId = parts[0].Trim();
                    int typeValue = int.Parse(parts[1].Trim());
                    bool isFortification = typeValue == 1;

                    settlementTypes[settlementId] = isFortification;
                }
            }
        }




        public static void LoadSettlementDistances(string csvPath, Dictionary<string, List<(string settlement, float distance)>> settlementDistances)
        {
            var lines = Array.Empty<string>();
            try
            {
                lines = File.ReadAllLines(csvPath);

            }
            catch (IOException)
            {
                LTLogger.IMTARed($"Error reading CSV file: IOException");
                return;
            }
            catch (Exception ex)
            {
                LTLogger.IMTARed($"Error reading CSV file: {ex.Message}");
                return;
            }


            foreach (var line in lines)
            {
                // Skip empty lines or comments
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                // Split by comma
                var parts = line.Split(',');

                if (parts.Length >= 3)
                {
                    string settlement1 = parts[0].Trim();
                    string settlement2 = parts[1].Trim();
                    float distance = float.Parse(parts[2].Trim());

                    int num = string.Compare(settlement1, settlement2, StringComparison.Ordinal);
                    if (num >= 0)
                    {
                        // need to switch pair
                        string temp = settlement1;
                        settlement1 = settlement2;
                        settlement2 = temp;
                    }

                    // Add settlement1 -> settlement2
                    if (!settlementDistances.ContainsKey(settlement1))
                    {
                        settlementDistances[settlement1] = new List<(string, float)>();
                    }
                    settlementDistances[settlement1].Add((settlement2, distance));

                    // Add settlement2 -> settlement1 (bidirectional)
                    //if (!settlementDistances.ContainsKey(settlement2))
                    //{
                    //    settlementDistances[settlement2] = new List<(string, float)>();
                    //}
                    //settlementDistances[settlement2].Add((settlement1, distance));
                }
            }
        }



        public static void ParseSettlementsToCSV(string fileName, string fullPath)
        {

            // Check if file exists
            if (!File.Exists(fullPath))
            {
                LTLogger.IMTARed($"File does not exist: {fullPath}");
                return;
            }

            string outputFileName = "OldSettlementsXMLParsed.CSV";
            LTLogger.DeleteFile(outputFileName);

            try
            {
                // Load the XML document
                XDocument doc = XDocument.Load(fullPath);

                // Create a list to store results
                List<string> csvLines = new List<string>();

                // Add CSV header (optional, remove if not needed)
                // csvLines.Add("settlement_id,is_fortification");

                // Find all Settlement elements
                var settlements = doc.Descendants("Settlement");

                foreach (var settlement in settlements)
                {
                    // Get the settlement ID
                    string settlementId = settlement.Attribute("id")?.Value;

                    if (string.IsNullOrEmpty(settlementId))
                        continue;

                    // Check if it's a fortification (has Town component) or village
                    int isFortification = 0;

                    var components = settlement.Element("Components");
                    if (components != null)
                    {
                        // Check if it has a Town element (fortification)
                        if (components.Element("Town") != null)
                        {
                            isFortification = 1;
                        }
                        // If it has a Village element, it's not a fortification (0)
                        else if (components.Element("Village") != null)
                        {
                            isFortification = 0;
                        }
                    }

                    // Add to CSV lines
                    //csvLines.Add($"{settlementId},{isFortification}");
                    LTLogger.WriteToFile(outputFileName, $"{settlementId},{isFortification}");

                }

                // Write to CSV file
                //File.WriteAllLines(csvOutputPath, csvLines);

                //Console.WriteLine($"Successfully parsed {csvLines.Count} settlements to {csvOutputPath}");
            }
            catch (Exception ex)
            {
                LTLogger.IMRed($"Error parsing XML: {ex.Message}");
            }
        }



        private static void ParseOldSDC(string oldSDCFileName, string fullPath)
        {
            // Check if file exists
            if (!File.Exists(fullPath))
            {
                LTLogger.IMTARed($"File does not exist: {fullPath}");
                return;
            }


            string outputFileName = "OldSDCParsed.CSV";
            string outputFileNameMesh = "OldSDCParsed_Mesh.CSV";

            // Get file info
            FileInfo fileInfo = new FileInfo(fullPath);

            // Get size in bytes
            long sizeInBytes = fileInfo.Length;

            // Or you can get various size formats
            //Console.WriteLine($"File exists: {fullPath}");
            //Console.WriteLine($"Size in bytes: {sizeInBytes}");
            //Console.WriteLine($"Size in KB: {sizeInBytes / 1024.0:F2}");
            LTLogger.IMTAGreen($"Old SDC: {oldSDCFileName} size {sizeInBytes / (1024.0 * 1024.0):F2}M");


            FileStream fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(fileStream);

            LTLogger.DeleteFile(outputFileName);

            if (reader != null)
            {
                LTLogger.IMTAGreen("BinaryReader initialized");
            } else
            {
                LTLogger.IMTARed("Failed to initialize BinaryReader");
                return;
            }

            int num3 = reader.ReadInt32();

            LTLogger.IMTAGreen($"First Int32 [{num3}] - total settlements");

            for (int l = 0; l < num3; l++)
            {

                string settlementId1 = reader.ReadString();
                //LTLogger.IMTAGreen($"settlementId1 [{settlementId1}]");

                for (int m = l + 1; m < num3; m++)
                {
                    string settlementId2 = reader.ReadString();
                    float distance = reader.ReadSingle();

                    LTLogger.WriteToFile(outputFileName, $"{settlementId1},{settlementId2},{distance}");
                }
            }


            //int n1 = reader.ReadInt32();
            //int n2 = reader.ReadInt32();

            //LTLogger.IMGreen($"n1 [{n1}]  n2 [{n2}]");

            for (int n = reader.ReadInt32(); n >= 0; n = reader.ReadInt32())
            {
                string settlementId3 = reader.ReadString();
                //this._navigationMeshClosestSettlementCache[n] = settlement7;
                LTLogger.WriteToFile(outputFileNameMesh, $"{n},{settlementId3}");
            }

            reader.Close();  // This also closes the underlying FileStream
        }
    }
}