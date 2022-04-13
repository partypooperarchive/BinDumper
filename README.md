# BinDumper

Tool for dumping BinOutput files from GI as JSONs

NOTE: dumping is only possible with the help of UNOBFUSCATED DummyDlls!
You might try to combine older DummyDlls with newer binary files, but your mileage might vary.

Full dump from 2.2.0 is possible, except for following files:

- `LevelDesign/Meta/LevelMetaData` (reason known - hashed fields order, hackaround required, isn't worth it)
- `Voice/Lut/Lut` (reason known - keys in file are integers while tool assume string keys in general case, isn't worth fixing)
- `IndexDic/NpcFreeTalkIndex`, `IndexDic/CoopTalkIndex`, `IndexDic/MainQuestIndex`, `IndexDic/MainQuestBriefIndex` (all files doesn't
  represent types from assembly, just `Dictionary<uint, uint>` while tool only supports assembly classes as the root class, isn't worth
  fixing)

Unlike other dumper tools, this tool expects most of parameters on standart input; this is a deliberate choice to make batch processing **much**
faster (reconstructing type lookup table takes a lot of time). You can pump as much lines on stdin as required to process multiple files.

Helper script `get_class_mode` is provided to help determining correct class name and mode of processing for the specific file.
