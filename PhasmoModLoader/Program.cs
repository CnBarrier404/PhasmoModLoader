using AssetsTools.NET;
using AssetsTools.NET.Extra;

class Program
{
    class ModFeature
    {
        public string? Name { get; set; }
        public bool IsEnabled { get; set; }
        public Action? Execute { get; set; }
    }

    static readonly string tempDir = "./temp";

    static void Main()
    {
        AppDomain.CurrentDomain.ProcessExit += OnExit;
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            OnExit(sender, e);
        };

        string modFilePath = "./mod/modassets.assets";
        string modResourcePath = "./mod/modsounds.resource";

        if (!Directory.Exists(tempDir)) Directory.CreateDirectory(tempDir);

        Console.WriteLine("请拖入Phasmophobia_Data文件夹");
        string gameDirPath = Console.ReadLine() ?? "Null";
        var gameAssetsFiles = Directory.GetFiles(gameDirPath, "*.assets");

        var manager = new AssetsManager();
        manager.LoadClassPackage("classdata.tpk");

        Basic(manager, modFilePath, modResourcePath, gameAssetsFiles, tempDir);

        Console.WriteLine("接下来是额外功能的安装，请选择自己需要的功能");

        // ---------------- MOD功能列表 ----------------
        var features = new List<ModFeature>
        {
            new()
            {
                Name = "10号房门口道具+门口电闸+门口出生+门口发车",
                IsEnabled = true,
                Execute = () => RidgeviewCourt()
            },
            new()
            {
                Name = "门口道具附加计划",
                IsEnabled = true,
                Execute = () => EntranceItemsExtraProject()
            },
            new()
            {
                Name = "减小玩家碰撞体积",
                IsEnabled = true,
                Execute = () => ReducePlayerCollider(gameDirPath)
            },
            new()
            {
                Name = "透明白桌",
                IsEnabled = true,
                Execute = () => TransparentDiningTableAndDiningChair()
            }
        };

        // ---------------- MOD功能选择 ----------------
        foreach (var feature in features)
        {
            Console.WriteLine($"\n是否安装功能：{feature.Name}? (Y/N)");
            Console.Write("你选择：");

            var key = Console.ReadKey(true).Key;

            if (key == ConsoleKey.Y)
            {
                Console.WriteLine("是");

                if (feature.IsEnabled)
                {
                    Console.WriteLine($"正在安装......");
                    feature.Execute?.Invoke();
                }
            }
            else if (key == ConsoleKey.N)
            {
                Console.WriteLine("否");
                Console.WriteLine($"跳过安装：{feature.Name}");
            }
            else
            {
                Console.WriteLine("\n我叫你按 Y/N 你聋吗？");
            }
        }

        Console.WriteLine("所有功能安装完成！");
        Console.WriteLine("按任意退出......");
        Console.ReadKey();
    }

    // ---------------- 清理临时文件夹 ----------------
    static void OnExit(object? sender, EventArgs e)
    {
        DeleteTempFolder();
    }

    static void DeleteTempFolder()
    {
        if (Directory.Exists(tempDir))
        {
            try
            {
                var files = Directory.GetFiles(tempDir);
                foreach (var file in files)
                {
                    File.Delete(file);
                }
                Directory.Delete(tempDir, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"删除临时文件夹失败: {ex.Message}");
            }
        }
    }

    // ---------------- 基础功能 ----------------
    static void Basic(AssetsManager manager, string modFilePath, string modResourcePath, string[] gameAssetsFiles, string tempDir)
    {
        Console.WriteLine("正在安装 滤镜+肉眼灵球+发光骨头+哈基米十字架和熏香计时");
        
        var modAFileInst = manager.LoadAssetsFile(modFilePath, false);
        var modAFile = modAFileInst.file;
        manager.LoadClassDatabaseFromPackage(modAFile.Metadata.UnityVersion);

        var modAudioDict = new Dictionary<string, AssetTypeValueField>();
        foreach (var modACInfo in modAFile.GetAssetsOfType(AssetClassID.AudioClip))
        {
            try
            {
                var modACBase = manager.GetBaseField(modAFileInst, modACInfo);
                string modName = modACBase["m_Name"]?.AsString ?? "Null";
                modAudioDict[modName] = modACBase;
            }
            catch { Console.WriteLine("找不到对应的AudioClip"); }
        }
        manager.UnloadAssetsFile(modAFileInst);

        foreach (var gameFilePath in gameAssetsFiles)
        {
            string tempGamePath = Path.Combine(tempDir, Path.GetFileName(gameFilePath));
            File.Copy(gameFilePath, tempGamePath, true);

            var gameAFileInst = manager.LoadAssetsFile(tempGamePath, false);
            var gameAFile = gameAFileInst.file;
            manager.LoadClassDatabaseFromPackage(gameAFile.Metadata.UnityVersion);

            var gameAudioNames = new HashSet<string>();
            foreach (var gameACInfo in gameAFile.GetAssetsOfType(AssetClassID.AudioClip))
            {
                try
                {
                    var gameACBase = manager.GetBaseField(gameAFileInst, gameACInfo);
                    string gameName = gameACBase["m_Name"]?.AsString ?? "Null";
                    gameAudioNames.Add(gameName);
                }
                catch { }
            }

            foreach (var gameACInfo in gameAFile.GetAssetsOfType(AssetClassID.AudioClip))
            {
                try
                {
                    var gameACBase = manager.GetBaseField(gameAFileInst, gameACInfo);
                    string gameName = gameACBase["m_Name"]?.AsString ?? "Null";

                    if (modAudioDict.TryGetValue(gameName, out var modBase) && gameAudioNames.Contains(gameName))
                    {
                        Console.WriteLine($"即将使用 [{modBase["m_Name"]?.AsString}] 替换 [{gameName}]");
                        gameACInfo.SetNewData(modBase);
                    }
                }
                catch { }
            }

            using (var fs = new FileStream(gameFilePath, FileMode.Create, FileAccess.Write))
            using (var writer = new AssetsFileWriter(fs))
            {
                gameAFile.Write(writer);
            }
            manager.UnloadAssetsFile(gameAFileInst);
        }

        string targetResourcePath = Path.Combine(Path.GetDirectoryName(gameAssetsFiles[0]) ?? ".", Path.GetFileName(modResourcePath) ?? "Null");
        File.Copy(modResourcePath, targetResourcePath, true);

        Console.WriteLine("安装完成！\n");
    }

    // ---------------- 额外功能 ----------------
    static void RidgeviewCourt()
    {
        Console.WriteLine("咕咕咕");
        // 10号房一类的史山
    }

    static void EntranceItemsExtraProject()
    {
        Console.WriteLine("咕咕咕");
        // 门口道具附加计划的史山
    }
    
    static void ReducePlayerCollider(string gameDirPath)
    {
        // 减小玩家碰撞体积一类的史山
    }


    static void TransparentDiningTableAndDiningChair()
    {
        Console.WriteLine("咕咕咕");
        // 透明白桌一类的史山
    }
}

// 其实这些功能单用UABEA都有解决方法，不过我不知道怎么实现，咕咕嘎嘎，先咕着吧