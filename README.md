# DCR Blockchain
## Setup
Before running any nodes, the environment has to be set up. This requires the following steps:
1. In the `frontend` directory, run command `npm i` to install the frontend dependencies.
2. In the `backend/DCREngine` directory, run command `dotnet build` to build the DCR engine.
3. In the `dns` directory, run command `dotnet run` to start a DNS seeder server. This DNS server functions to supply a pre-defined list of semi-permanent seed nodes (stored in `dns/network.json`) to requesting nodes. By default, this list contains only a single node, running on IP address `localhost:4300`.

## Starting a node
There are two types of nodes in this blockchain environment: Miner and FullNode. **It is important that the first miner or full node uses port 4300.** Afterwards when spinning up more nodes, you can use whatever port you would like.

### Miner
A miner is responsible for receiving transactions, arranging these in blocks, and mining and propagating blocks throughout the network.
To start a miner, run the `start-miner.sh` script like so: `./start-miner.sh [Port]`.
Example use case: `./start-miner.sh 4300` starts a miner process on port 4300 (`localhost:4300`).

### FullNode
A full node is responsible for generating transactions and propagating these throughout the network. This node consists of a frontend user interface and a backend node process.
To start a full node, run the `start-node.sh` script like so: `./start-node.sh [FrontendPort] [BackendPort]`
Example use case: `./start-node.sh 8080 4300` starts a frontend running on port `8080` (`localhost:8080`) and a full node process on port 4300 (`localhost:4300`).

### Node Blockchain State
You can see the state of a node's blockchain in the `blockchain[Port].json` file, where `Port` is the port of the node which the blockchain relates to.

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
