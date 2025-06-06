using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class FishSpawnGroup
{
    public Randomizer<FishAIModel> prefabs = new(new List<FishAIModel>());
    public RangeInt countRange = new(5, 10);

    public void Prepare()
    {
        prefabs.Shuffle();
    }
}