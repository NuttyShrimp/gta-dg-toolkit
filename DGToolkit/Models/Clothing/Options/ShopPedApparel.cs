namespace DGToolkit.Models.Clothing.Options;

public class ShopPedApparel
{
    public string pedName { get; set; }
    public string dlcName { get; set; }
    public string fullDlcName { get; set; }

    public string fullPropsDlcName
    {
        get { return $"{pedName}_p_{dlcName}"; }
    }

    public string eCharacter { get; set; }

    public string creatureMetaData { get; set; }

    // Not actually string but just needs something
    public string pedOutfits { get; set; }
    public string pedComponents { get; set; }
    public string pedProps { get; set; }
}