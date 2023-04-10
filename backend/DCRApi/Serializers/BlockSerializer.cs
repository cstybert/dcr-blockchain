using Newtonsoft.Json;

namespace DCR;
public class BlockSerializer
{

    public string Serialize(Block block)
    {
        return JsonConvert.SerializeObject(block, Formatting.Indented);
    }

    public Block Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Block>(json)!;
    }
}