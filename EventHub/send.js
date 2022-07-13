const { EventHubProducerClient } = require("@azure/event-hubs");

const connectionString = "Endpoint=   " // connectionstring;    
const eventHubName = "clustersee.dbo.tabelasee2";

async function main() {

  // Create a producer client to send messages to the event hub.
  const producer = new EventHubProducerClient(connectionString, eventHubName);

  // Prepare a batch of three events.
  const batch = await producer.createBatch();
  for (let index = 0; index < 5; index++) {
    batch.tryAdd({ body: `${index} - Event`}); 
  }
  
  //batch.tryAdd({ body: "First event" });
  //batch.tryAdd({ body: "Second event" });
  //batch.tryAdd({ body: "Third event" }); 
  //batch.tryAdd({ body: "Fourth event" });  
  //batch.tryAdd({ body: "Fifth event" });     

  // Send the batch to the event hub.
  await producer.sendBatch(batch);

  // Close the producer client.
  await producer.close();

  console.log("A batch of five events have been sent to the event hub");
}

main().catch((err) => {
  console.log("Error occurred: ", err);
});