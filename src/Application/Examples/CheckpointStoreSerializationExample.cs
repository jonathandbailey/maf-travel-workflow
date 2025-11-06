using Application.Workflows.Conversations;
using System.Text.Json;

namespace Application.Examples;

/// <summary>
/// Example usage of ConversationCheckpointStore serialization
/// </summary>
public static class CheckpointStoreSerializationExample
{
    /// <summary>
    /// Demonstrates how to serialize a checkpoint store to JSON
    /// </summary>
    public static async Task<string> SerializeCheckpointStore()
    {
        var store = new ConversationCheckpointStore();
        
        // Add some checkpoints
        await store.CreateCheckpointAsync(
            "run-1", 
            JsonDocument.Parse("{\"state\": \"active\"}").RootElement);
        
        await store.CreateCheckpointAsync(
            "run-1", 
            JsonDocument.Parse("{\"state\": \"completed\"}").RootElement);

        // Serialize to JSON
        var json = JsonSerializer.Serialize(store, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });

        return json;
    }

    /// <summary>
    /// Demonstrates how to deserialize a checkpoint store from JSON
    /// </summary>
    public static ConversationCheckpointStore? DeserializeCheckpointStore(string json)
    {
        var store = JsonSerializer.Deserialize<ConversationCheckpointStore>(json);
        return store;
    }

    /// <summary>
    /// Demonstrates full round-trip serialization
    /// </summary>
    public static async Task<bool> RoundTripExample()
    {
        // Create and populate store
        var originalStore = new ConversationCheckpointStore();
        var checkpoint = await originalStore.CreateCheckpointAsync(
            "run-123", 
            JsonDocument.Parse("{\"data\": \"example\"}").RootElement);

        // Serialize
        var json = JsonSerializer.Serialize(originalStore);

        // Deserialize
        var restoredStore = JsonSerializer.Deserialize<ConversationCheckpointStore>(json);

        // Verify
        if (restoredStore is null) return false;

        var element = await restoredStore.RetrieveCheckpointAsync("run-123", checkpoint);
        return element.GetProperty("data").GetString() == "example";
    }
}
