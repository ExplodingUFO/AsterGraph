# Deferred Items

- Build verification for quick task `260327-sx0` is blocked by pre-existing unrelated test compile failures in `tests/AsterGraph.Serialization.Tests/SerializationCompatibilityTests.cs`:
  - CS0120 on `GraphClipboardPayloadSerializer.Serialize(GraphSelectionFragment)`
  - CS0120 on `GraphClipboardPayloadSerializer.TryDeserialize(string?, out GraphSelectionFragment?)`
- These errors are outside the onboarding-doc scope and were not modified in this quick pass.
