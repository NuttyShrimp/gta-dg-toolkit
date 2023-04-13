using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;
using DGToolkit.Models.Clothing.Options;
using DGToolkit.Models.Clothing.YMT;

namespace DGToolkit.Models.Clothing;

public class ClothingStore
{
    public static ClothingStore _instance = new();
    public static ClothingStore Instance => _instance;

    public ClothingOptions Options;
    public ObservableCollection<ClothData> Clothes { get; set; }
    public ObservableCollection<ShopPedApparel> availableDLCS { get; set; }
    public ShopPedApparel? selectedDLC;
    internal LogStore LogStore = new();
    internal bool unsavedChanges = false;
    internal ClothData? selectedCloth;

    public ClothingStore()
    {
        Options = new ClothingOptions(this);
        Clothes = new ObservableCollection<ClothData>();
        availableDLCS = new ObservableCollection<ShopPedApparel>();
        loadAvailableDLCS();
        Clothes.CollectionChanged += (sender, args) => { unsavedChanges = true; };
    }

    private void loadAvailableDLCS()
    {
        // Look for all folders in Options.data.StreamFolder
        // Add them to availableDLCS
        var resourceFolderInfo = new DirectoryInfo(Options.data.ResourceFolder);
        if (!resourceFolderInfo.Exists)
        {
            Debug.WriteLine($"Directory {Options.data.ResourceFolder} does not exist");
            return;
        }

        var directories = resourceFolderInfo.EnumerateFiles();
        foreach (var file in directories)
        {
            if (file.Extension != ".meta") continue;
            ShopPedApparel pedApparel;
            var serializer = new XmlSerializer(typeof(ShopPedApparel));
            using var stream = new FileStream(file.FullName, FileMode.Open);
            using var reader = XmlReader.Create(stream);
            pedApparel = (ShopPedApparel) serializer.Deserialize(reader);
            if (pedApparel == null) continue;
            availableDLCS.Add(pedApparel);
        }
    }

    public void SelectDLC(ShopPedApparel dlcName)
    {
        if (selectedDLC == dlcName)
        {
            return;
        }

        if (unsavedChanges)
        {
            var result = MessageBox.Show("You have unsaved changes. Do you want to save them?", "Unsaved changes",
                MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel)
            {
                return;
            }

            if (result == DialogResult.Yes)
            {
                Options.SaveOptions();
            }
        }

        selectedDLC = dlcName;
        Clothes.Clear();
        Loader.GenerateData(Options.data, selectedDLC.fullDlcName).ForEach(d => { Clothes.Add(d); });
        unsavedChanges = false;
    }

    private int GetNextIndexForComponent(Types.DrawableTypes type)
    {
        var clothOfType = Clothes.ToList().FindAll(cloth => cloth.DrawableType == type);
        if (clothOfType.Count == 0)
        {
            return 0;
        }

        var maxIndex = 0;
        foreach (var clothData in clothOfType)
        {
            if (clothData.CurrentComponentIndex <= maxIndex)
            {
                maxIndex = clothData.CurrentComponentIndex + 1;
            }
        }

        Debug.WriteLine(maxIndex);
        return maxIndex;
    }

    public void addMixedFiles(List<string> filePaths)
    {
        if (selectedDLC == null)
        {
            MessageBox.Show("Please select a DLC first");
            return;
        }

        for (var i = 0; i < filePaths.Count; i++)
        {
            var filePath = filePaths[i];
            var fileName = Path.GetFileName(filePath);
            var componentRegex = new Regex(@"(p_)?(\w+)_(\d{3})(_\w+)?\.ydd$");
            var textureRegex = new Regex(@"(p_)?(\w+)_diff_(\d{3})(_\w+)?\.ytd$");
            if (componentRegex.IsMatch(fileName))
            {
                var compRegMatch = componentRegex.Match(fileName);

                var nameResolver = new ClothNameResolver(filePath);

                var componentIndex = GetNextIndexForComponent(nameResolver.DrawableType);
                var postfix = compRegMatch.Groups[4].Value;
                var newFileName =
                    $"{selectedDLC.fullDlcName}^{ClothNameResolver.DrawableTypeToString(nameResolver.DrawableType)}_{componentIndex.ToString().PadLeft(3, '0')}";
                if (postfix != "")
                {
                    newFileName += postfix;
                }

                newFileName += ".ydd";

                // Copy file to stream folder
                var newFilePath = Path.Combine(Options.data.ResourceFolder, "stream", selectedDLC.fullDlcName,
                    newFileName);
                if (File.Exists(newFilePath))
                {
                    File.Delete(newFilePath);
                }

                File.Copy(filePath, newFilePath);
                // Move the textures if any
                filePaths.ToList().ForEach(fp =>
                {
                    if (!textureRegex.IsMatch(fp)) return;
                    var textureRegMatch = textureRegex.Match(fp);
                    var texNameResolver = new ClothNameResolver(fp);
                    if (texNameResolver.DrawableType != nameResolver.DrawableType) return;
                    if (compRegMatch.Groups[3].Value != textureRegMatch.Groups[3].Value) return;
                    var newTextureFileName =
                        $"{selectedDLC.fullDlcName}^{ClothNameResolver.DrawableTypeToString(nameResolver.DrawableType)}_diff_{componentIndex.ToString().PadLeft(3, '0')}";
                    if (textureRegMatch.Groups[4].Value != "")
                    {
                        newTextureFileName += textureRegMatch.Groups[4].Value;
                    }

                    newTextureFileName += ".ytd";
                    var newTextureFilePath = Path.Combine(Options.data.ResourceFolder, "stream",
                        selectedDLC.fullDlcName,
                        newTextureFileName);

                    if (File.Exists(newTextureFilePath))
                    {
                        File.Delete(newTextureFilePath);
                    }

                    File.Copy(fp, newTextureFilePath);
                    filePaths.Remove(fp);
                });
                var clothData = new ClothData(Path.GetRelativePath(Options.data.ResourceFolder, newFilePath),
                    nameResolver.ClothType == Types.ClothTypes.Component
                        ? selectedDLC.fullDlcName
                        : selectedDLC.fullPropsDlcName,
                    nameResolver.ClothType,
                    nameResolver.DrawableType, componentIndex, postfix, "");
                Clothes.Add(clothData);
                clothData.SearchForTextures(Options.data.ResourceFolder);
                LogStore.AddLogEntry(
                    $"Imported {nameResolver.DrawableType} {componentIndex} with {clothData.Textures.Count} textures");
                continue;
            }

            if (textureRegex.IsMatch(fileName))
            {
                var textureRegMatch = textureRegex.Match(fileName);

                var nameResolver = new ClothNameResolver(filePath);
                var clothData = GetDrawable(nameResolver.DrawableType, int.Parse(textureRegMatch.Groups[3].Value));
                if (clothData == null)
                {
                    LogStore.AddLogEntry($"Failed to import {filePath} no component found");
                    continue;
                }

                clothData.AddTexture(filePath);
                LogStore.AddLogEntry($"Imported texture {filePath} to {clothData.Name}");
                continue;
            }

            LogStore.AddLogEntry($"Failed to import {filePath} not a component or a texture");
        }

        Options.SaveOptions();
    }

    public ClothData? GetDrawable(Types.DrawableTypes type, int index) => Clothes.ToList()
        .Find(cloth => cloth.DrawableType == type && cloth.CurrentComponentIndex == index);

    public ShopPedApparel? GetDLC(string dlcName) => availableDLCS.ToList().Find(dlc => dlc.fullDlcName == dlcName);
}