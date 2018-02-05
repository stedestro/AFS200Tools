# AFS Volume 2.00 specification
Thanks to Stickman (http://forum.xentax.com/viewtopic.php?f=10&t=11410) for their work.

AFS Volume is an archive format, found in all versions of the playstation game "Tenchu".
Files in the japanese version of the game tells us the archives were created using the
software ```AFS Volume Maker Version 2.00```. The data is encoded as big endian.

Known files using this format are:

| Filename | Game | Serial Code | Filesize (Bytes) | CRC32 |
| --- | --- | --- | --- | --- |
| ```TENCHU.VOL``` | Rittai Ninja Katsugeki: Tenchu | SLPS-01272 (Version 1.1) | 19531300 | 9A83CF46 |
| ```DATA.VOL``` | Tenchu: Stealth Assassins | SLUS-00706 (Version 1.1) | 31714732 | 1F83A85A |
| ```DATA.VOL``` | Tenchu: Stealth Assassins | SLES-01374 | 56699440 | 6890ACB9 |

## File Structure
The header is 40 or 42 bytes long
### Header
| Index | Name   | Length (Bytes)| 
| --- | ------ | -------- | 
| 0 | Magic "AFS_VOL_200\0" (0x4146535F564F4C5F32303000) | 12 |
| 12 |  Number of files | 4 | 
| 16 | Index of file table | 4 |
| 20 | Zeroes | 20 (might be 22) | 
### File Entry
Each entry is 36 bytes long  

| Index | Name | Length (Bytes) | Note |
| --- | --- | --- | --- |
| 0 | Magic (0x4958) | 2 | "IX" |
| 2 | Entry Type | 2 | 1 if a file, 2 if a directory |
| 4 | File index | 4 | entry is a directory if value is 0xCDCDCDCD |
| 8 | File size | 4 | entry is a directory if value is 0xCDCDCDCD |
| 12 | Clone of file size | 4 | - |
| 16 | Filename | 20 | See info on filenames below |

## Files information
The file table is located at the end of the archive.

### Filename format (speculation)
File are named with the following format ```@x_filename``` where ```x``` is a number
representing the arborescence of the file. When inserting entries in a new directory,
```x``` has for value the numbers of entries already present in the archive.
#### Exemple
   * ROOTFOLDER
      * @0_FOLDER0
         * @1_FOLDER1
            * @2_file1.ext
            * @2_file2.ext
         * @1_FOLDER2
	        * @5_file3.ext
		    * @5_file4.ext
         * @1_file5.ext

The filename went from ```@2_file2.ext``` to ```@5_file3.ext``` because there were
already 5 entries before ```file_3.ext``` (```@0_FOLDER0, @1_FOLDER1, @2_file1.ext, @2_file2.ext, @1_FOLDER2```). This implies that whenever the user add or remove files from the archive,
the filenames need to be renamed. These archive were most likely static.

In the tenchu files, the root folder is always named ```K:``` as if it was a drive and isn't used 
in the filename prefix calculation.
The filenames are ascii encoded.