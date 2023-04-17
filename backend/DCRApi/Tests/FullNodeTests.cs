using NUnit.Framework;
using System.Net.Http;
using DCR;
using Microsoft.AspNetCore.Mvc;

namespace Tests;

public class FullNodeTests
{
    private BlockchainSerializer _blockchainSerializer;
    private BlockSerializer _blockSerializer;
    private NetworkSerializer _networkSerializer;

    [SetUp]
    public void Setup()
    {
        _blockchainSerializer = new BlockchainSerializer();
        _blockSerializer = new BlockSerializer();
        _networkSerializer = new NetworkSerializer();
    }

    [Test]
    public void Controller_Test()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var testClient = new NetworkClient("localhost", 4300);
        var node = new FullNode(loggerFactory.CreateLogger<FullNode>(), testClient);
        var controller = new BlockchainController(loggerFactory.CreateLogger<BlockchainController>(), testClient, node);

        var result = controller.GetBlockchain() as OkObjectResult;
        var blockchain = _blockchainSerializer.Deserialize(result.Value.ToString());

        var newBlock = "{\"PreviousBlockHash\": \"000ijG+10JYNq35Kd5HAg1gjmONRHMMDmHauh+7ZAnA=\",\"Hash\": \"000arcgqV11T9LpxiC5dyEuybjjrK2cwLd/bMZwcLh8=\",\"Nonce\": 134934,\"Index\": 3,\"Timestamp\": \"0001-01-01T00:00:00\",\"Transactions\": [{\"Id\": \"da97371a-eae1-421a-9f85-e3ce0c31dd7a\",\"Actor\": \"Foo\",            \"Action\": 0,            \"EntityTitle\": \"\",            \"Graph\": {                \"Id\": \"310b330a-b05b-4c7e-af10-97c09746c241\",                \"Activities\": [                {                    \"Title\": \"Select papers\",                    \"Pending\": false,                    \"Included\": false,                    \"Executed\": true,                    \"Enabled\": false                },                {                    \"Title\": \"Write introduction\",                    \"Pending\": true,                    \"Included\": true,                    \"Executed\": false,                    \"Enabled\": true                },                {                    \"Title\": \"Write abstract\",                    \"Pending\": true,                    \"Included\": true,                    \"Executed\": false,                    \"Enabled\": true                },                {                    \"Title\": \"Write conclusion\",                    \"Pending\": true,                    \"Included\": true,                    \"Executed\": false,                    \"Enabled\": true                }                ],                \"Relations\": [                {                    \"Type\": 2,                    \"Source\": \"Select papers\",                    \"Target\": \"Select papers\"                },                {                    \"Type\": 0,                    \"Source\": \"Select papers\",                    \"Target\": \"Write introduction\"                },                {                    \"Type\": 0,                    \"Source\": \"Select papers\",                    \"Target\": \"Write abstract\"                },                {                    \"Type\": 0,                    \"Source\": \"Select papers\",                    \"Target\": \"Write conclusion\"                },                {                    \"Type\": 1,                    \"Source\": \"Write introduction\",                    \"Target\": \"Write abstract\"                },                {                    \"Type\": 1,                    \"Source\": \"Write conclusion\",                    \"Target\": \"Write abstract\"                }                ],                \"Accepting\": false            }            }        ]}";
        var block = _blockSerializer.Deserialize(newBlock);
        Console.WriteLine(_blockSerializer.Serialize(block.Transactions));
        var request = new ShareBlockRequest(block, testClient.ClientNode);
        controller.ReceiveBlock(request);

        // Wait 5 seconds
        System.Threading.Tasks.Task.Delay(5000).Wait();

        result = controller.GetHeadBlock() as OkObjectResult;
        var headBlock = _blockSerializer.Deserialize(result.Value.ToString());
        Console.WriteLine(headBlock.Hash);

        Assert.AreEqual(result , result);
    }
    
    [Test]
    public async Task Simple_Test()
    {
        CancellationTokenSource cts = new CancellationTokenSource();
        TestHelper.RunNode(4300, cts.Token);

        await Task.Delay(TimeSpan.FromSeconds(10));

        TestHelper.RunNode(4301, cts.Token);

        await Task.Delay(TimeSpan.FromSeconds(10));

        var testClient = new NetworkClient("localhost", 4300);
        var neighborBlockchain = await testClient.GetBlockchain(new NetworkNode("localhost", 4300));
        cts.Cancel();

        await Task.WhenAll();
        cts.Cancel();
    }
}