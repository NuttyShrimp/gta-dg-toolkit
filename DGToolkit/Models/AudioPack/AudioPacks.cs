using System.Collections.Generic;

namespace DGToolkit.Models.AudioPack;

struct State
{
    public Manifest manifest;
}

class AudioPacks
{
    public State state;
    private Parser manifestParser;

    public AudioPacks()
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
        manifestParser.WriteManifest(state.manifest);
    }
}