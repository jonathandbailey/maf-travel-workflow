using Application.Workflows.Conversations;
using Microsoft.Agents.AI.Workflows;
using System.Text.Json;
using Xunit;

namespace Tests;

public class ConversationCheckpointStoreSerializationTests
{
    [Fact]
    public async Task CanSerializeAndDeserializeCheckpointStore()
    {
        // Arrange
        var originalStore = new ConversationCheckpointStore();
        var runId = "test-run-123";
        
        // Create some checkpoints
        var checkpoint1 = await originalStore.CreateCheckpointAsync(
            runId, 
            JsonDocument.Parse("{\"data\": \"test1\"}").RootElement);
        
        var checkpoint2 = await originalStore.CreateCheckpointAsync(
            runId, 
            JsonDocument.Parse("{\"data\": \"test2\"}").RootElement);

        // Act - Serialize
        var json = JsonSerializer.Serialize(originalStore, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        // Act - Deserialize
        var deserializedStore = JsonSerializer.Deserialize<ConversationCheckpointStore>(json);

        // Assert
        Assert.NotNull(deserializedStore);
        Assert.Equal(2, deserializedStore.CheckpointElements.Count);
        
        var index = await deserializedStore.RetrieveIndexAsync(runId);
        Assert.Equal(2, index.Count());
        
        // Verify we can retrieve the checkpoints
        var retrievedElement1 = await deserializedStore.RetrieveCheckpointAsync(runId, checkpoint1);
        Assert.Equal("test1", retrievedElement1.GetProperty("data").GetString());
        
        var retrievedElement2 = await deserializedStore.RetrieveCheckpointAsync(runId, checkpoint2);
        Assert.Equal("test2", retrievedElement2.GetProperty("data").GetString());
    }

    [Fact]
    public void CanSerializeEmptyCheckpointStore()
    {
        // Arrange
        var store = new ConversationCheckpointStore();

        // Act
        var json = JsonSerializer.Serialize(store);
        var deserialized = JsonSerializer.Deserialize<ConversationCheckpointStore>(json);

        // Assert
        Assert.NotNull(deserialized);
        Assert.Empty(deserialized.CheckpointElements);
    }
}
