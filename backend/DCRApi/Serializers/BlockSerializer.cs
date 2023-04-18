using Newtonsoft.Json;

namespace DCR;
public class BlockSerializer
{

    public string Serialize(List<Transaction> transactions)
    {
        return JsonConvert.SerializeObject(transactions, Formatting.Indented);
    }

    public Block Deserialize(string json)
    {
        return JsonConvert.DeserializeObject<Block>(json)!;
    }
}