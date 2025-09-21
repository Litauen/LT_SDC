# LT_SDC

This is proof of concept SettlementDistanceCache converter for Bannerlord from v1.2.12 to 1.3beta

1. Compile as a mod.
2. Start the Bannerlord 1.2.12 game with this mod.
3. In \Modules\LT_SDC create Data folder, put old settlements_distance_cache.bin here (exact name)
4. In the game, press CTRL+F9 to read old SDC
5. In \Modules\LT_SDC\logs you should see OldSDCParsed.CSV and OldSDCParsed_Mesh.CSV (extracted data from old SDC)
6. In the game, press CTRL+F12, nes SDC should be generated in \Modules\LT_SDC\logs as settlements_distance_cache_Default.bin


I created this as a Bannerlord mod, because I thought I would need native Bannerlord functions to generate Scene XML CSC and Scene NavMesh CRC. However, for now, those values can be anything - the game reads them but does not check.
