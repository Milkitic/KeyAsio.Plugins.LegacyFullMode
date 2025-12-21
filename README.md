# KeyAsio.Plugins.LegacyFullMode

The original music sync logic from v3. Available as a free, community-maintained plugin.

## How to Build & Install

Follow these steps to manually build and enable the Legacy FullMode sync engine:

1. **Prepare Source & Submodules**
   Checkout the submodules to the branch corresponding to your KeyAsio version tag. 
   > *Note: For v4.x, the KeyAsio team will strive to maintain API compatibility for legacy plugins.*

2. **Publish & Deploy Plugin**
   Publish the project targeting your specific Runtime Identifier (RID), such as `win-x64` or `win-x86`.
   Copy the output `KeyAsio.Plugins.LegacyFullMode.dll` to the KeyAsio application root directory. 
   *(Recommendation: If prompted, do not overwrite existing core system files.)*

3. **Install Native Dependencies**
   Ensure the corresponding versions of `SoundTouch` native DLL files are present in the application root directory. These are required for the legacy time-stretching logic.

4. **Activate in KeyAsio**
   Restart the KeyAsio client. You will notice the mode toggle in the top-right corner is now interactive. Click to switch and activate the **Legacy Realtime.FullMode**.

---
*Disclaimer: This is a community-maintained legacy plugin. For the best experience and sub-millisecond precision, please consider using the built-in ProMix engine.*
