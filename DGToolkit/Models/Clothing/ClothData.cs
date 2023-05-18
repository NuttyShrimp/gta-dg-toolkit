using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DGToolkit.Models.Clothing.Options;
using DGToolkit.Models.Clothing.YMT;
using Newtonsoft.Json;

namespace DGToolkit.Models.Clothing;

public class TextureData : INotifyPropertyChanged
{
    public static string TextureMissing = "⚠";
    [JsonIgnore] public bool FileExists = false;
    public string IsMissing => FileExists ? "" : TextureMissing;

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

    public string DisplayName
    {
        get
        {
            var shortFileName = Regex.Replace(FileName, @"^.*\^", "");
            return Name != null ? $"{Name} ({shortFileName})" : shortFileName;
        }
    }


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
    public static char OffsetLetter = 'a';
    private static readonly string[] TypeIcons = {"🧥", "👓"};

    // TODO: Move to shoppedApparel so we can use prop dlc names
    private ShopPedApparel dlcInfo;
    private string dlcName => IsPedProp() ? dlcInfo.fullPropsDlcName : dlcInfo.fullDlcName;

    private string fileDirectory =>
        Path.Combine(ClothingStore.Instance.Options.data.ResourceFolder, "stream", dlcInfo.fullDlcName);


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

    public bool FileExists { get; set; } = false;
    public double[] ExpressionMods;
    public int PropMask = 1;

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

    public ClothData(string dlcName, Types.ClothTypes clothType,
        Types.DrawableTypes drawableType,
        int compIdx, string postFix, string description)
    {
        this.dlcInfo = ClothingStore.Instance.GetDLC(dlcName);
        _currentComponentIndex = compIdx;
        PostFix = postFix;

        ClothType = clothType;
        DrawableType = drawableType;
        Name = DrawableType + "_" + ComponentNumerics;
        Description = description;
        ExpressionMods = new[] {0, 0, 0, 0, 0.0};
        PropertyChanged += updateFileName;
        ValidateFileExisting();
    }

    private void updateFileName(object sender, PropertyChangedEventArgs args)
    {
        if (args.PropertyName == "CurrentComponentIndex" || args.PropertyName == "DrawableType")
        {
            var oldFilePath = Loader.GetDrawableFileName(dlcName, _currentComponentIndex, DrawableType, PostFix);
            var newFile =
                $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_{ComponentNumerics}{PostFix}.ydd";

            if (oldFilePath == newFile) return;

            if (File.Exists(Path.Combine(fileDirectory, newFile)))
            {
                File.Delete(Path.Combine(fileDirectory, newFile));
            }

            File.Move(Path.Combine(fileDirectory, oldFilePath), Path.Combine(fileDirectory, newFile));

            Textures.ToList().ForEach(texData =>
            {
                var oldTexPath = Path.Combine(fileDirectory, texData.FileName);
                var newTexFile =
                    $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{texData.OffsetLetter}";
                if (!IsPedProp())
                {
                    newTexFile += IsPostfix_U() ? "_uni" : "_whi";
                }

                newTexFile += ".ytd";
                texData.FileName = Path.GetFileName(newTexFile);

                if (File.Exists(Path.Combine(fileDirectory, newTexFile)))
                {
                    File.Delete(Path.Combine(fileDirectory, newTexFile));
                }

                File.Move(Path.Combine(fileDirectory, oldTexPath), Path.Combine(fileDirectory, newTexFile));
            });
            OnPropertyChanged("DisplayName");
        }
    }

    public void SearchForTextures()
    {
        Textures.Clear();
        if (IsComponent())
        {
            var baseFileName =
                $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_";
            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(fileDirectory,
                    $"{baseFileName}{(char) (OffsetLetter + i)}_uni.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FileExists = true,
                    FileName = Path.GetFileName(relPath),
                    OffsetLetter = (char) (OffsetLetter + i)
                });
            }

            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(fileDirectory,
                    $"{baseFileName}{(char) (OffsetLetter + i)}_whi.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FileExists = true,
                    FileName = Path.GetFileName(relPath),
                    OffsetLetter = (char) (OffsetLetter + i)
                });
            }
        }
        else
        {
            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(fileDirectory,
                    $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{(char) (OffsetLetter + i)}.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    FileExists = true,
                    FileName = Path.GetFileName(relPath),
                    OffsetLetter = (char) (OffsetLetter + i)
                });
            }
        }
    }

    public void AddTexture(string path)
    {
        // Get next available offset letter
        // Update filename to match new offset letter, current drawable type, and component index
        // Add texture to list

        var offsetLetter = (char) (OffsetLetter + Textures.Count);
        var newFileName =
            $"{dlcName}^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{offsetLetter}";
        if (!IsPedProp())
        {
            newFileName += IsPostfix_U() ? "_uni" : "_whi";
        }

        newFileName += ".ytd";
        var newPath = Path.Combine(fileDirectory, newFileName);

        File.Copy(path, newPath);

        if (Textures.ToList().Find(texData => texData.FileName == newFileName) == null)
            Textures.Add(new TextureData()
            {
                FileExists = true,
                FileName = newFileName,
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

        var tempFileName = Path.Combine(fileDirectory, $"rem_{Path.GetFileName(texData.FileName)}");

        if (File.Exists(tempFileName))
        {
            File.Delete(tempFileName);
        }

        File.Move(Path.Combine(fileDirectory, texData.FileName), tempFileName);

        // Shift all textures after the deleted one
        var charReplRE = new Regex(@"((?:p_)?\w*_diff_\d{3}_)([a-z])(.*\.ytd)$");
        for (int i = index + 1; i < Textures.Count; i++)
        {
            var oldFileName = (string) Textures[i].FileName.Clone();
            var filePathMatch = charReplRE.Match(oldFileName);
            if (!filePathMatch.Success)
            {
                MessageBox.Show($"Failed to parse texture file name {oldFileName}, aborting");
                break;
            }

            Textures[i].OffsetLetter = (char) (Textures[i].OffsetLetter - 1);

            // Update the file name
            var newFileName = Regex.Replace(oldFileName, @"((?:p_)?\w*_diff_\d{3}_)([a-z])(.*?\.ytd)$",
                "$1" + Textures[i].OffsetLetter + "$3");
            Textures[i].FileName = newFileName;
            File.Move(Path.Combine(fileDirectory, oldFileName),
                Path.Combine(fileDirectory, newFileName));
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

    public void ValidateFileExisting()
    {
        var fileName = Path.Combine(fileDirectory,
            Loader.GetDrawableFileName(dlcName, _currentComponentIndex, DrawableType, PostFix));
        FileExists = File.Exists(fileName);
        foreach (var textureData in Textures)
        {
            var filePath = Path.Combine(fileDirectory, textureData.FileName);
            textureData.FileExists = File.Exists(filePath);
        }
    }
}