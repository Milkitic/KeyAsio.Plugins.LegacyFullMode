# KeyAsio.Plugins.LegacyFullMode

The original music sync logic from v3. Available as a free, community-maintained plugin.

## How to Build & Install

Follow these steps to manually build and enable the Legacy FullMode sync engine:

1. **Prepare Source & Submodules**
   Initialize submodules (e.g., `git submodule update --init --recursive`) and check them out to the branch corresponding to your KeyAsio version tag.
   > *Note: For v4.x, the KeyAsio team will strive to maintain API compatibility for legacy plugins.*

2. **Publish & Deploy Plugin**
   Publish the project targeting your specific Runtime Identifier (RID), for example: `dotnet publish -r win-x64`.
   Copy the output `KeyAsio.Plugins.LegacyFullMode.dll` to the KeyAsio application root directory. 
   *(Recommendation: If prompted, do not overwrite existing core system files.)*

3. **Install Native Dependencies**
   Copy the required `SoundTouch` native DLLs from the publish output directory to the application root directory. These are required for the legacy time-stretching logic.

4. **Activate in KeyAsio**
   Restart the KeyAsio client. You will notice the mode toggle in the top-right corner is now interactive. Click to switch and activate the **Legacy Realtime.FullMode**.

---
*Disclaimer: This is a community-maintained legacy plugin. For the best experience and sub-millisecond precision, please consider using the built-in ProMix engine.*
