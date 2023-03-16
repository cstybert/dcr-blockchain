using Newtonsoft.Json;

namespace DCR;
public class BlockChainSerializer
{
    public string Serialize(BlockChain blockchain) {
        return JsonConvert.SerializeObject(blockchain, Formatting.Indented);
    }

    public BlockChain Deserialize(string json) {
        return JsonConvert.DeserializeObject<BlockChain>(json)!;
    }
}