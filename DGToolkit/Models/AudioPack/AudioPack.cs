namespace DGToolkit.Models.AudioPack;

struct State
{
    public Manifest manifest;
}

class AudioPack
{
    private State state;
    private Parser manifestParser;

    public AudioPack()
    {
        manifestParser = new Parser();
    }

    public void Load()
    {
        //Load info from manifest
        state = new State()
        {
            manifest = manifestParser.ImportManifest(),
        };
    }

    public void Save()
    {
    }
}