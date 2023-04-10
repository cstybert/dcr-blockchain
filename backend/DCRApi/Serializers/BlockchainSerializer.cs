using Newtonsoft.Json;

namespace DCR;
public class BlockchainSerializer
{
    public string Serialize(Blockchain blockchain)
    {
        return JsonConvert.SerializeObject(blockchain, Formatting.Indented);
    }

    public string Serialize(Block block)
    {
        return JsonConvert.SerializeObject(block, Formatting.Indented);
    }

    public Blockchain Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Blockchain>(json)!;
    }
}