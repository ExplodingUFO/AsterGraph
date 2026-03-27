# Deferred Items

- 2026-03-27: `dotnet build avalonia-node-map.sln -v minimal` fails in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs` with CS0120 on `GraphClipboardPayloadSerializer.Serialize/TryDeserialize` static usage. This is pre-existing workspace test noise and outside quick task scope (docs/package publish guidance).
