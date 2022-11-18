﻿using Newtonsoft.Json;
using System;
using System.IO;

namespace AudioGenerator.Model.DataManifest;

internal class Parser
{
  private Manifest? manifest;
  private readonly string path = Path.Combine(AppContext.BaseDirectory, "../../../Data");

  private void ImportManifest()
  {
    manifest = JsonConvert.DeserializeObject<Manifest>(File.ReadAllText(Path.Combine(path, "./manifest.json")));
    if (manifest != null) manifest.dataPath = path;
  }

  public Manifest? getManifest()
  {
    if (manifest == null) ImportManifest();
    return manifest;
  }

  public string GetParserPath()
  {
    return path;
  }
}