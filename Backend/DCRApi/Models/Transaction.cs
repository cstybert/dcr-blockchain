namespace DCR;
public class Transaction
{
    private string _tmp;
    public Transaction() 
    {
        _tmp = "foo";
    }

    public string State 
    {
        get => _tmp;
    }
}