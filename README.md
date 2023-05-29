# Readme DCR Blockchain

## Running locally
Before running the files run the `dotnet build` command inside the `backend/DCREngine` directory

You should also have a DNS server running, you do this by running the `dotnet run` command indside the dns directory.

You can now start miners and fullnodes by using the `start-miner.sh` and `start-node.sh` scripts.

### Start-Miner
The `start-miner.sh` script is used like so: `./start-miner.sh [Port]`
Example use case: `./start-node.sh 4300`
It starts a miner process running on the port provided.

### Start-Node
The `start-node.sh` script is used like so: `./start-node.sh [FrontEndPort] [FullNodePort]`
Example use case: `./start-node.sh 8080 4300`
It starts a miner process running on the port provided.

**It is important that the first miner or fullnode uses port 4300.** Afterwards when spinning up more nodes, you can use whatever port you would like
You can see the state of the nodes in the `blockchain[port].json` files, which track the state of the blockchain in the nodes with the listed port number.

## Running Tests
The tests can be ran in either the `DCRApi` or `DCREngine` directories by using the `dotnet test` command. 
To change the values of the evaluation tests in `DCRApi` change the following values in the constructor of `EvaluationCompare.cs`
````
_settings = new Settings()
{
    TimeToSleep = 0,
    SizeOfBlocks = int.MaxValue,
    NumberNeighbours = 1,
    Difficulty = 0,
    IsEval = true
};
_numBlocks = 50;
_sizeOfBlock = 1000;
_numEvalTransactions = 50000;
````
