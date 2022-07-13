const { EventHubConsumerClient } = require("@azure/event-hubs");
const { ContainerClient } = require("@azure/storage-blob");    
const { BlobCheckpointStore } = require("@azure/eventhubs-checkpointstore-blob");

const connectionString = // coonectionstring event; 
const eventHubName = "clustersee1.dbo.tabelasee1";
const consumerGroup = "$Default"; // name of the default consumer group


const storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=seecmspehub;AccountKey=y16Jp4UphAsmq+y/WVwJIijWVuG8YBhCbJe4R/yClXwkgOWbS+knL9Cbtp4vNVqEnJNQvw+CMBK+eovDP6Mm9w==;EndpointSuffix=core.windows.net";
const containerName = "eventhub";

async function main() {
  // Create a blob container client and a blob checkpoint store using the client.
  const containerClient = new ContainerClient(storageConnectionString, containerName);
  const checkpointStore = new BlobCheckpointStore(containerClient);

  // Create a consumer client for the event hub by specifying the checkpoint store.
  const consumerClient = new EventHubConsumerClient(consumerGroup, connectionString, eventHubName, checkpointStore);

  // Subscribe to the events, and specify handlers for processing the events and errors.
  const subscription = consumerClient.subscribe({
      processEvents: async (events, context) => {
        if (events.length === 0) {
          console.log(`No events received within wait time. Waiting for next interval`);
          return;
        }

        for (const event of events) {
          console.log(JSON.stringify(event.body));
        }
        // Update the checkpoint.
        await context.updateCheckpoint(events[events.length - 1]);
      },

      processError: async (err, context) => {
        console.log(`Error : ${err}`);
      }
    }
  );

  // After 300 seconds, stop processing.
  await new Promise((resolve) => {
    setTimeout(async () => {
      await subscription.close();
      await consumerClient.close();
      resolve();
    }, 3000000);
  });
}

main().catch((err) => {
  console.log("Error occurred: ", err);
});    