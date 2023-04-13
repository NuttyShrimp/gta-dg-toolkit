using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace DGToolkit.Models.Clothing;

public class TextureData : INotifyPropertyChanged
{
    private string _filePath = "";

    public string FilePath
    {
        get => _filePath;
        set
        {
            _filePath = value;
            OnPropertyChanged("DisplayName");
        }
    }

    private string _fileName = "";

    public string FileName
    {
        get => _fileName;
        set
        {
            _fileName = value;
            OnPropertyChanged("DisplayName");
        }
    }

    public char OffsetLetter { get; set; }
    private string? _name { get; set; }

    public string? Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged("DisplayName");
        }
    }

    [JsonIgnore] public string DisplayName => Name != null ? $"{Name} ({FileName})" : FileName;

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new(propertyName));
    }
}

public class ClothData
    : INotifyPropertyChanged
{
    private static char _offsetLetter = 'a';
    private static readonly string[] TypeIcons = {"🧥", "👓"};

    private string dlcName;
    private Types.ClothTypes _clothType;

    public Types.ClothTypes ClothType
    {
        get => _clothType;
        set
        {
            _clothType = value;
            OnPropertyChanged();
        }
    }

    private Types.DrawableTypes _drawableType;

    public Types.DrawableTypes DrawableType
    {
        get => _drawableType;
        set
        {
            _drawableType = value;
            Name = DrawableType + "_" + ComponentNumerics;
            OnPropertyChanged();
        }
    }

    // Includes the file name
    public string MainPath { get; set; }
    public int[] ExpressionMods;

    public readonly ObservableCollection<TextureData> Textures = new();

    // includes '_'
    public string PostFix = "";
    public string Description = "";

    public string Type => TypeIcons[(int) ClothType];

    private int _currentComponentIndex;

    public string ComponentNumerics
    {
        get => _currentComponentIndex.ToString().PadLeft(3, '0');
    }

    public int CurrentComponentIndex
    {
        get => _currentComponentIndex;
        set
        {
            if (ClothingStore.Instance.GetDrawable(DrawableType, value) != null)
            {
                return;
            }

            _currentComponentIndex = value;
            Name = DrawableType + "_" + ComponentNumerics;
            OnPropertyChanged("DisplayName");
            OnPropertyChanged();
        }
    }

    private string _name = "";

    public string Name
    {
        get => _name;
        set
        {
            _name = value;
            OnPropertyChanged("DisplayName");
        }
    }

    public string DisplayName => $"{Name} (ID: {CurrentComponentIndex}) ({DrawableType})";

    public ClothData(string path, string dlcName, Types.ClothTypes clothType,
        Types.DrawableTypes drawableType,
        int compIdx, string postFix, string description)
    {
        this.dlcName = dlcName;
        _currentComponentIndex = compIdx;
        PostFix = postFix;

        ClothType = clothType;
        DrawableType = drawableType;
        Name = DrawableType + "_" + ComponentNumerics;
        MainPath = path;
        Description = description;
        ExpressionMods = new[] {0, 0, 0, 0, 0};
        PropertyChanged += updateFileName;
    }

    private void updateFileName(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "CurrentComponentIndex" || args.PropertyName == "DrawableType")
        {
            var rootPath = Path.Combine(ClothingStore.Instance.Options.data.ResourceFolder,
                Path.GetDirectoryName(MainPath));

            var oldFilePath = Path.GetFileName(MainPath);
            var newFile =
                $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_{ComponentNumerics}{PostFix}.ydd";

            MainPath = Path.Combine(Path.GetDirectoryName(MainPath) ?? "", newFile);

            if (oldFilePath == newFile) return;

            if (File.Exists(Path.Combine(rootPath, newFile)))
            {
                File.Delete(Path.Combine(rootPath, newFile));
            }

            File.Move(Path.Combine(rootPath, oldFilePath), Path.Combine(rootPath, newFile));

            Textures.ToList().ForEach(texData =>
            {
                var oldTexPath = Path.GetFileName(texData.FilePath);
                var newTexFile =
                    $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{texData.OffsetLetter}";
                if (!IsPedProp())
                {
                    newTexFile += IsPostfix_U() ? "_uni" : "_whi";
                }

                newTexFile += ".ytd";
                texData.FilePath = Path.Combine(Path.GetDirectoryName(texData.FilePath) ?? "", newTexFile);
                texData.FileName = Path.GetFileNameWithoutExtension(newTexFile).Replace(dlcName + "^", "");

                if (File.Exists(Path.Combine(rootPath, newTexFile)))
                {
                    File.Delete(Path.Combine(rootPath, newTexFile));
                }

                File.Move(Path.Combine(rootPath, oldTexPath), Path.Combine(rootPath, newTexFile));
            });
            OnPropertyChanged("DisplayName");
        }
    }

    public void SearchForTextures(string resourceFolder)
    {
        Textures.Clear();
        string rootPath = Path.Combine(resourceFolder,
            Path.GetDirectoryName(MainPath));
        if (IsComponent())
        {
            var baseFileName =
                $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_";
            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(rootPath,
                    $"{baseFileName}{(char) (_offsetLetter + i)}_uni.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FilePath = Path.GetRelativePath(resourceFolder, relPath),
                    FileName = Path.GetFileNameWithoutExtension(relPath).Replace(dlcName + "^", ""),
                    OffsetLetter = (char) (_offsetLetter + i)
                });
            }

            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(rootPath,
                    $"{baseFileName}{(char) (_offsetLetter + i)}_whi.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FilePath = Path.GetRelativePath(resourceFolder, relPath),
                    FileName = Path.GetFileNameWithoutExtension(relPath).Replace(dlcName + "^", ""),
                    OffsetLetter = (char) (_offsetLetter + i)
                });
            }
        }
        else
        {
            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(rootPath,
                    $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{(char) (_offsetLetter + i)}.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FilePath = Path.GetRelativePath(resourceFolder, relPath),
                    FileName = Path.GetFileNameWithoutExtension(relPath).Replace(dlcName + "^", ""),
                    OffsetLetter = (char) (_offsetLetter + i)
                });
            }
        }
    }

    public void AddTexture(string path)
    {
        // Get next available offset letter
        // Update filename to match new offset letter, current drawable type, and component index
        // Add texture to list

        var offsetLetter = (char) (_offsetLetter + Textures.Count);
        var fileName = Path.GetFileNameWithoutExtension(path);
        var newFileName =
            $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{offsetLetter}";
        if (!IsPedProp())
        {
            newFileName += IsPostfix_U() ? "_uni" : "_whi";
        }

        newFileName += ".ytd";
        var newPath = Path.Combine(ClothingStore.Instance.Options.data.ResourceFolder, Path.GetDirectoryName(MainPath),
            newFileName);

        File.Copy(path, newPath);
        var relPath = Path.GetRelativePath(ClothingStore.Instance.Options.data.ResourceFolder, newPath);

        if (Textures.ToList().Find(texData => texData.FilePath == relPath) == null)
            Textures.Add(new TextureData()
            {
                FilePath = relPath,
                FileName = Path.GetFileNameWithoutExtension(relPath).Replace(dlcName + "^", ""),
                OffsetLetter = offsetLetter
            });
    }

    public void RemoveTexture(TextureData texData)
    {
        var index = Textures.ToList().FindIndex(tex => tex == texData);
        if (index == -1)
        {
            MessageBox.Show("Texture not found");
            return;
        }

        var result = MessageBox.Show($"Are you sure you want to delete {texData.FileName}?", "Delete Texture",
            MessageBoxButtons.YesNo);
        if (result == DialogResult.No)
            return;

        var dataResourceFolder = ClothingStore.Instance.Options.data.ResourceFolder;
        Debug.WriteLine(Path.Combine(dataResourceFolder, texData.FilePath));
        var tempFileName = Path.Combine(dataResourceFolder, Path.GetDirectoryName(texData.FilePath),
            $"rem_{Path.GetFileName(texData.FilePath)}");

        if (File.Exists(tempFileName))
        {
            File.Delete(tempFileName);
        }

        File.Move(Path.Combine(dataResourceFolder, texData.FilePath), tempFileName);

        // Shift all textures after the deleted one
        var charReplRE = new Regex(@"^(.+\^(?:p_)?\w*_diff_\d{3}_)([a-z])(.*\.ytd)$");
        for (int i = index + 1; i < Textures.Count; i++)
        {
            var oldFilePath = Path.GetFileName(Textures[i].FilePath);
            var filePathMatch = charReplRE.Match(oldFilePath);
            if (!filePathMatch.Success)
            {
                MessageBox.Show($"Failed to parse texture file name {oldFilePath}, aborting");
                break;
            }

            Textures[i].OffsetLetter = (char) (Textures[i].OffsetLetter - 1);

            // Update the file name
            var newFileName = Regex.Replace(oldFilePath, @"(^.+^(?:p_)?\w*_diff_\d{3}_)([a-z])(.*?\.ytd)$",
                "$1" + Textures[i].OffsetLetter + "$3");
            File.Move(Path.Combine(dataResourceFolder, Textures[i].FilePath),
                Path.Combine(dataResourceFolder, Path.GetDirectoryName(Textures[i].FilePath), newFileName));
        }

        File.Delete(tempFileName);
        Textures.Remove(texData);
    }

    public override string ToString()
    {
        return _name;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        var handler = PropertyChanged;
        handler?.Invoke(this, new(propertyName));
    }

    public bool IsComponent()
    {
        if (DrawableType <= Types.DrawableTypes.Top)
            return true;
        return false;
    }

    public byte GetComponentTypeId()
    {
        return IsComponent()
            ? (byte) DrawableType
            : (byte) 255;
    }

    public bool IsPedProp()
    {
        return !IsComponent();
    }

    public byte GetPedPropTypeId()
    {
        if (IsPedProp())
            return (byte) ((int) DrawableType - (int) Types.DrawableTypes.PropHead);
        return 255;
    }

    public string GetPrefix()
    {
        return ClothNameResolver.DrawableTypeToString(DrawableType);
    }

    public bool IsPostfix_U()
    {
        return PostFix == "_u";
    }

    public void SetCustomPostfix(string newPostfix)
    {
        PostFix = newPostfix;
    }
}