# Requirements: AsterGraph v1.4

**Defined:** 2026-04-08
**Milestone:** v1.4 Plugin Loading and Automation Execution
**Core Value:** Hosts can integrate only the graph-editor pieces they need, replace default UI and behavior seams safely, and keep building on a stable public API instead of patching internal implementation details.

## Milestone Requirements

### Plugin Loading

- [ ] **PLUG-01**: Host can load one or more runtime plugins through a public composition path rooted in `AsterGraphEditorFactory` / `AsterGraphEditorOptions` instead of internal editor or Avalonia object access
- [ ] **PLUG-02**: Loaded plugins can contribute additive services, menus, presentation, diagnostics, or related host seams through explicit contracts that stay compatible with the canonical session/runtime boundary
- [ ] **PLUG-03**: Host can inspect loaded plugin descriptors, availability, and failures through canonical runtime inspection or diagnostics surfaces

### Automation Execution

- [ ] **AUTO-01**: Host can execute automation or macro steps against canonical command IDs, query snapshots, and batched mutation paths without depending on `GraphEditorViewModel` methods
- [ ] **AUTO-02**: Host can observe automation progress, failures, and results through typed runtime diagnostics/events suitable for non-Avalonia or headless consumers

### Proof And Samples

- [ ] **PROOF-01**: `HostSample`, `PackageSmoke`, and focused regression coverage prove the plugin composition and automation execution story from the canonical host boundary
- [ ] **PROOF-02**: `ScaleSmoke` or equivalent large-graph proof shows the automation path remains credible on larger sessions, and docs point hosts to the canonical plugin/automation path

## Future Requirements

### Showcase Follow-ons

- **PRESET-01**: User can save and recall named showcase presets for common host integration narratives
- **TOUR-01**: User can step through a guided capability tour that highlights specific seams in sequence

### Deferred Platform Work

- **TRUST-01**: Host can enforce plugin trust, signing, version policy, or isolation rules beyond the first in-process loader baseline
- **MARKET-01**: Host can discover, install, or update plugins through marketplace/feed-oriented workflows
- **SCRIPT-01**: Host can author automation through a dedicated scripting language, script host, or workflow designer instead of command-step composition only

## Out of Scope

| Feature | Reason |
|---------|--------|
| New graph-editing end-user features unrelated to plugin or automation delivery | This milestone is about extending the host/runtime platform, not broadening the editor feature set |
| Another demo-showcase-only milestone for presets or guided tours | The highest remaining product risk is real extension delivery, not more presentation polish |
| Plugin marketplace, remote distribution, trust UI, or isolation policy design | The first milestone should prove the public loader boundary before expanding into deployment and security product work |
| Dedicated scripting language or workflow-designer UI | The first automation baseline should stay descriptor-first and command-driven |
| Replacing Avalonia or rewriting retained compatibility hosts | This milestone should build on the shipped kernel/session boundary rather than reopening stack or migration strategy |

## Traceability

Roadmap mapping will be filled during milestone roadmapping.

**Coverage:**
- milestone requirements: 7 total
- mapped to phases: 0
- unmapped: 7

---
*Requirements defined: 2026-04-08*
