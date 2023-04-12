using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DGToolkit.Models.Clothing;

public class TextureData
{
    public string file { get; set; }
    public string? name { get; set; }
}

public class ClothData
    : INotifyPropertyChanged
{
    private static char _offsetLetter = 'a';
    private static readonly string[] TypeIcons = {"🧥", "👓"};

    private string dlcName;
    public Types.ClothTypes ClothType { get; set; }
    public Types.DrawableTypes DrawableType { get; set; }
    public string MainPath { get; set; }
    public int[] ExpressionMods;
    public readonly ObservableCollection<TextureData> Textures = new();
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
            _currentComponentIndex = value;
            OnPropertyChanged("DisplayName");
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
    }

    public void SearchForTextures(string rootFolder)
    {
        Textures.Clear();
        string rootPath = Path.Combine(rootFolder, Path.GetDirectoryName(MainPath));
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
                    file = Path.GetRelativePath(rootFolder, relPath)
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
                    file = Path.GetRelativePath(rootFolder, relPath)
                });
            }
        }
        else
        {
            for (int i = 0;; ++i)
            {
                string relPath = Path.Combine(rootPath,
                    $"{dlcName}^p^{ClothNameResolver.DrawableTypeToString(DrawableType)}_diff_{ComponentNumerics}_{(char) (_offsetLetter + i)}.ytd");
                if (!File.Exists(relPath))
                    break;
                Textures.Add(new TextureData()
                {
                    file = Path.GetRelativePath(rootFolder, relPath)
                });
            }
        }
    }

    public void AddTexture(string path)
    {
        if (Textures.ToList().Find(texData => texData.file == path) == null)
            Textures.Add(new TextureData()
            {
                file = path
            });
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
        return PostFix == "u" ? true : false;
    }

    public void SetCustomPostfix(string newPostfix)
    {
        PostFix = newPostfix;
    }
}