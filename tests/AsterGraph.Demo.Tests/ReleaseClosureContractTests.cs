using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace AsterGraph.Demo.Tests;

public sealed class ReleaseClosureContractTests
{
    [Fact]
    public void PrereleaseNotes_LockPackageVersionAndSurfacePositiveWpfMatrixProof()
    {
        var tempRoot = CreateTempDirectory();
        var proofRoot = Path.Combine(tempRoot, "proof");
        Directory.CreateDirectory(proofRoot);

        WriteProofFile(proofRoot, "public-repo-hygiene.txt", "PUBLIC_REPO_HYGIENE_OK:True");
        WriteProofFile(proofRoot, "hostsample-packed.txt", "HOST_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "consumer-sample.txt", "CONSUMER_SAMPLE_OK:True");
        WriteProofFile(proofRoot, "demo-proof.txt", "DEMO_OK:True");
        WriteProofFile(proofRoot, "hostsample-net10-packed.txt", "HOST_SAMPLE_NET10_OK:True");
        WriteProofFile(proofRoot, "package-smoke.txt", "PACKAGE_SMOKE_OK:True");
        WriteProofFile(
            proofRoot,
            "template-smoke.txt",
            "ASTERGRAPH_TEMPLATE_SMOKE_OK:True`nTEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True`nTEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True`nTEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True");
        WriteProofFile(
            proofRoot,
            "public-api-surface.txt",
            "PUBLIC_API_SURFACE_OK:3407:net9.0`nPUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia`nPUBLIC_API_GUIDANCE_OK:True`nPUBLIC_API_DIFF_GATE_OK:True`nPUBLIC_API_USAGE_GUIDANCE_OK:True`nPUBLIC_API_STABILITY_SCOPE_OK:True");
        WriteProofFile(
            proofRoot,
            "scale-smoke.txt",
            "SCALE_TIER_BUDGET:baseline`nSCALE_PERFORMANCE_BUDGET_OK:baseline:True:none`nSCALE_AUTHORING_BUDGET_OK:baseline:True:none`nSCALE_EXPORT_BUDGET_OK:baseline:True:none`nEXPORT_PROGRESS_OK:True`nEXPORT_CANCEL_OK:True`nEXPORT_SCOPE_OK:True`nEXPORT_SELECTION_SCOPE_OK:True`nSCALE_HISTORY_CONTRACT_OK:True");
        WriteProofFile(
            proofRoot,
            "hello-world-wpf-proof.txt",
            "HOSTED_ACCESSIBILITY_BASELINE_OK:True`nHOSTED_ACCESSIBILITY_FOCUS_OK:True`nHOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True`nHOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True`nHOSTED_ACCESSIBILITY_OK:True`nADAPTER2_PERFORMANCE_BASELINE_OK:True`nADAPTER2_EXPORT_BREADTH_OK:True`nADAPTER2_PROJECTION_BUDGET_OK:True:none`nADAPTER2_COMMAND_BUDGET_OK:True:none`nADAPTER2_SCENE_BUDGET_OK:True:none`nADAPTER2_VALIDATION_SCOPE_OK:True`nADAPTER2_MATRIX_HANDOFF_OK:True`nADAPTER2_SCOPE_BOUNDARY_OK:True`nADAPTER2_WPF_SAMPLE_PROOF_OK:True`nADAPTER2_CANONICAL_ROUTE_OK:True`nADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True`nADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True`nADAPTER2_RECIPE_ALIGNMENT_OK:True`nADAPTER2_PROOF_BUDGET_OK:True`nADAPTER2_VALIDATION_HANDOFF_OK:True`nADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True`nV060_MILESTONE_PROOF_OK:True`nHELLOWORLD_WPF_OK:True");
        WriteProofFile(
            proofRoot,
            "wpf-adapter-capability-matrix.txt",
            string.Join(
                Environment.NewLine,
                [
                    "ADAPTER_CAPABILITY_MATRIX_FORMAT:1",
                    "ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS",
                    "ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS",
                ]));

        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
        var coveragePath = Path.Combine(tempRoot, "coverage-summary.json");
        File.WriteAllText(
            coveragePath,
            """
            {
              "coveredLines": 10,
              "totalLines": 20,
              "lineRate": 0.5
            }
            """);

        var outputPath = Path.Combine(tempRoot, "prerelease-notes.md");
        var process = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{Path.Combine(GetRepositoryRoot(), "eng", "write-prerelease-notes.ps1")}\" " +
            $"-RepoRoot \"{GetRepositoryRoot()}\" " +
            $"-ProofRoot \"{proofRoot}\" " +
            $"-OutputPath \"{outputPath}\" " +
            $"-PublicTag \"{publicTag}\" " +
            $"-CoverageSummaryPath \"{coveragePath}\"");

        Assert.True(
            process.ExitCode == 0,
            $"write-prerelease-notes.ps1 failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{process.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{process.StandardError}");

        var notes = File.ReadAllText(outputPath);
        Assert.Contains($"- installable package version: `{packageVersion}`", notes, StringComparison.Ordinal);
        Assert.Contains($"- matching public tag: `{publicTag}`", notes, StringComparison.Ordinal);
        Assert.Contains("## Support Story", notes, StringComparison.Ordinal);
        Assert.Contains("[Stabilization Support Matrix](./docs/en/stabilization-support-matrix.md)", notes, StringComparison.Ordinal);
        Assert.Contains("[Adapter Capability Matrix](./docs/en/adapter-capability-matrix.md)", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("- frozen support boundary:", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("- adapter capability story:", notes, StringComparison.Ordinal);
        Assert.Contains("external capability readiness gate", notes, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("[Project Status](./docs/en/project-status.md)", notes, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(notes, "externally proven", "validation-only", "bounded", "deferred"));
        Assert.True(HasLineWithAll(notes, "Adoption Readiness / Release Candidate Hygiene", "API drift", "support boundary", "release proof"));
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("V061_MILESTONE_PROOF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("0.xx alpha/beta hardening line", notes, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(notes, "API adoption proof", "PUBLIC_API_SURFACE_OK", "PUBLIC_API_GUIDANCE_OK", "generated template/plugin validation"));
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("API_RELEASE_CANDIDATE_PROOF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_HANDOFF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("RELEASE_BOUNDARY_STABILITY_OK:True", notes, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(notes, "trusted plugin proof", "CONSUMER_SAMPLE_TRUST_OK", "ASTERGRAPH_PLUGIN_VALIDATE_OK", "plugin trust contract"));
        Assert.Contains("[Beta Support Bundle](./docs/en/support-bundle.md)", notes, StringComparison.Ordinal);
        Assert.Contains("[Adoption Feedback Loop](./docs/en/adoption-feedback.md)", notes, StringComparison.Ordinal);
        Assert.Contains("[Adopter Triage Checklist](./docs/en/adopter-triage.md)", notes, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(notes, "route", "version", "proof markers", "friction", "support-bundle attachment note"));
        Assert.Contains("SCALE_EXPORT_BUDGET_OK:baseline:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_FOCUS_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_COMMAND_SURFACE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_AUTHORING_SURFACE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROJECTION_BUDGET_OK:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_COMMAND_BUDGET_OK:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_VALIDATION_SCOPE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_MATRIX_HANDOFF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCOPE_BOUNDARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_WPF_SAMPLE_PROOF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_CANONICAL_ROUTE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PROOF_BUDGET_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_VALIDATION_HANDOFF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("V060_MILESTONE_PROOF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("HELLOWORLD_WPF_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_TEMPLATE_SMOKE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SURFACE_OK:3407:net9.0", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("adapter-2 validation only", notes, StringComparison.Ordinal);
        Assert.Contains("does not widen the public publish/package boundary", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", notes, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_AUTHORING_BUDGET_OK:baseline:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("SCALE_EXPORT_BUDGET_OK:baseline:True:none", notes, StringComparison.Ordinal);
        Assert.Contains("EXPORT_PROGRESS_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("EXPORT_CANCEL_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SCOPE_OK:True", notes, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SELECTION_SCOPE_OK:True", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:MISSING", notes, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:MISSING", notes, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseChecklists_CarrySupportBoundaryStoryAndKeepWpfValidationOnly()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        Assert.Contains("frozen support boundary story", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("adapter matrix story", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("`HELLOWORLD_WPF_OK` as adapter-2 validation only", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("historical alpha reference for the current beta support story", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("external capability readiness gate", englishChecklist, StringComparison.OrdinalIgnoreCase);
        Assert.True(HasLineWithAll(englishChecklist, "externally proven", "validation-only", "bounded", "deferred"));
        Assert.True(HasLineWithAll(englishChecklist, "Adoption Readiness / Release Candidate Hygiene", "API drift", "support boundary", "release proof"));
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(englishChecklist, "xlarge", "telemetry-only", "10000-node support", "virtualization"));
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("0.xx alpha/beta hardening", englishChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(englishChecklist, "route", "version", "proof markers", "friction", "support-bundle attachment note"));
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_PROGRESS_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_CANCEL_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SCOPE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SELECTION_SCOPE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("artifacts/proof/template-smoke.txt", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("artifacts/proof/public-api-surface.txt", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_TEMPLATE_SMOKE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SURFACE_OK:...:net9.0", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("API_RELEASE_CANDIDATE_PROOF_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_HANDOFF_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("RELEASE_BOUNDARY_STABILITY_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(englishChecklist, "public API guidance proof", "template/plugin proof", "PUBLIC_API_SURFACE_OK", "PUBLIC_API_GUIDANCE_OK"));
        Assert.Contains("[Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK:True:none", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Starter.Wpf", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", englishChecklist, StringComparison.Ordinal);
        Assert.Contains("[Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)", englishChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK is Avalonia/WPF parity", englishChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK is public WPF support", englishChecklist, StringComparison.Ordinal);

        Assert.Contains("冻结的 support boundary 叙事", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("adapter matrix 叙事", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("`HELLOWORLD_WPF_OK` 只当成 adapter-2 验证通过", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("历史 alpha 参考，服务于当前 beta support story", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("外部能力就绪闸门", chineseChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(chineseChecklist, "已被外部证据证明", "仅验证通过", "受边界约束", "继续延后"));
        Assert.True(HasLineWithAll(chineseChecklist, "Adoption Readiness / Release Candidate Hygiene", "API drift", "support boundary", "release proof"));
        Assert.Contains("ADOPTION_RECOMMENDATION_CURRENT_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("CLAIM_HYGIENE_BOUNDARY_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("RELEASE_READINESS_GATE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(chineseChecklist, "xlarge", "telemetry-only", "10000 节点支持", "virtualization"));
        Assert.Contains("SCALE_RASTER_EXPORT_STRESS_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("0.xx` alpha/beta hardening 线", chineseChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(chineseChecklist, "route", "version", "proof 标记", "摩擦", "support bundle 附件备注"));
        Assert.Contains("HOSTED_ACCESSIBILITY_BASELINE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("HOSTED_ACCESSIBILITY_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_PROGRESS_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_CANCEL_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SCOPE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("EXPORT_SELECTION_SCOPE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("artifacts/proof/template-smoke.txt", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("artifacts/proof/public-api-surface.txt", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("ASTERGRAPH_TEMPLATE_SMOKE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SURFACE_OK:...:net9.0", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("API_RELEASE_CANDIDATE_PROOF_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_HANDOFF_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("RELEASE_BOUNDARY_STABILITY_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.True(HasLineWithAll(chineseChecklist, "public API guidance proof", "template/plugin proof", "PUBLIC_API_SURFACE_OK", "PUBLIC_API_GUIDANCE_OK"));
        Assert.Contains("[Adapter-2 Accessibility Recipe](./adapter-2-accessibility-recipe.md)", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_PERFORMANCE_BASELINE_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_EXPORT_BREADTH_OK:True", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_SCENE_BUDGET_OK:True:none", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.Starter.Wpf", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("AsterGraph.HelloWorld.Wpf", chineseChecklist, StringComparison.Ordinal);
        Assert.Contains("[Adapter-2 Performance Recipe](./adapter-2-performance-recipe.md)", chineseChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK 是 Avalonia/WPF parity", chineseChecklist, StringComparison.Ordinal);
        Assert.DoesNotContain("HELLOWORLD_WPF_OK 是公开 WPF support", chineseChecklist, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("docs/en/adapter-capability-matrix.md")]
    [InlineData("docs/zh-CN/adapter-capability-matrix.md")]
    public void AdapterMatrixDocs_UseLockedVocabularyAndRejectMissingWpfRows(string relativePath)
    {
        var contents = ReadRepoFile(relativePath);

        Assert.Contains("Matrix Vocabulary", contents, StringComparison.Ordinal);
        Assert.Contains("Matrix Categories", contents, StringComparison.Ordinal);
        Assert.Contains("Supported", contents, StringComparison.Ordinal);
        Assert.Contains("Partial", contents, StringComparison.Ordinal);
        Assert.Contains("Fallback", contents, StringComparison.Ordinal);

        AssertMatrixRowStatus(contents, "Canonical runtime/session route", "Supported", "Supported");
        AssertMatrixRowStatus(contents, "Hosted full editor shell", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Standalone surfaces", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Command and tool projection", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Authoring presentation", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Platform integration", "Supported", "Partial");
        AssertMatrixRowStatus(contents, "Proof and sample coverage", "Supported", "Supported");
    }

    [Fact]
    public void CiScript_ReservesMissingForAbsentWpfProofAndUsesProofMarkers()
    {
        var script = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_PROOF:MISSING", script, StringComparison.Ordinal);
        Assert.Contains("Get-ProofMarkerLine -ProofText $proofText -Marker 'HELLOWORLD_WPF_OK'", script, StringComparison.Ordinal);
        Assert.Contains("Get-ProofMarkerLine -ProofText $proofText -Marker 'COMMAND_SURFACE_OK'", script, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:$(Convert-TextToCapabilityStatus -Value $helloWorldWpfOk)", script, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:$(Convert-TextToCapabilityStatus -Value $commandSurfaceOk)", script, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseValidation_InvokesPublicVersioningGate()
    {
        var ciScript = ReadRepoFile("eng/ci.ps1");
        var releaseWorkflow = ReadRepoFile(".github/workflows/release.yml");

        Assert.Contains("validate-public-versioning.ps1", ciScript, StringComparison.Ordinal);
        Assert.Contains("Invoke-PublicVersioningValidation", ciScript, StringComparison.Ordinal);
        Assert.Contains("$arguments = @{", ciScript, StringComparison.Ordinal);
        Assert.Contains("RepoRoot = $repoRoot", ciScript, StringComparison.Ordinal);
        Assert.Contains("$arguments['PublicTag'] = $PublicTag", ciScript, StringComparison.Ordinal);
        Assert.Contains("Validate public version docs", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("$validationArgs = @{", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("RepoRoot = $PWD.Path", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains("$validationArgs['PublicTag'] = $publicTag", releaseWorkflow, StringComparison.Ordinal);
        Assert.Contains(".\\eng\\validate-public-versioning.ps1 @validationArgs", releaseWorkflow, StringComparison.Ordinal);
    }

    [Fact]
    public void ReleaseValidation_InvokesPublicApiSurfaceGate()
    {
        var ciScript = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("validate-public-api-surface.ps1", ciScript, StringComparison.Ordinal);
        Assert.Contains("Invoke-PublicApiSurfaceValidation", ciScript, StringComparison.Ordinal);
        Assert.Contains("-Framework 'net9.0'", ciScript, StringComparison.Ordinal);
        Assert.Contains("-ProofPath $publicApiSurfaceProofPath", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_SURFACE_OK:'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_GUIDANCE_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_DIFF_GATE_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_USAGE_GUIDANCE_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_STABILITY_SCOPE_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'ADOPTION_API_STABILIZATION_HANDOFF_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'ADOPTION_API_SCOPE_BOUNDARY_OK:True'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'V061_MILESTONE_PROOF_OK:True'", ciScript, StringComparison.Ordinal);

        var releaseValidationStart = ciScript.IndexOf("function Invoke-ReleaseValidation", StringComparison.Ordinal);
        Assert.True(releaseValidationStart >= 0, "ci.ps1 should contain Invoke-ReleaseValidation.");
        Assert.Contains("Invoke-PublicApiSurfaceValidation", ciScript[releaseValidationStart..], StringComparison.Ordinal);

        var validationScript = ReadRepoFile("eng/validate-public-api-surface.ps1");
        Assert.Contains("PUBLIC_API_SCOPE_OK:", validationScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_GUIDANCE_OK:True'", validationScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_DIFF_GATE_OK:True'", validationScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_USAGE_GUIDANCE_OK:True'", validationScript, StringComparison.Ordinal);
        Assert.Contains("'PUBLIC_API_STABILITY_SCOPE_OK:True'", validationScript, StringComparison.Ordinal);
    }

    [Fact]
    public void TemplateSmoke_CapturesGeneratedPluginValidationEvidence()
    {
        var ciScript = ReadRepoFile("eng/ci.ps1");
        var templateSmokeScript = ReadRepoFile("eng/template-smoke.ps1");
        var prereleaseNotesScript = ReadRepoFile("eng/write-prerelease-notes.ps1");

        Assert.Contains("$templateSmokeProofPath = Join-Path $proofArtifactsRoot 'template-smoke.txt'", ciScript, StringComparison.Ordinal);
        Assert.Contains("-ProofPath $templateSmokeProofPath", ciScript, StringComparison.Ordinal);
        Assert.Contains("Assert-ProofContains -Text $pluginValidationOutput -ExpectedText 'ASTERGRAPH_PLUGIN_VALIDATE_OK:True'", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("Assert-ProofContains -Text $pluginValidationOutput -ExpectedText 'capability_summary:'", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("Assert-ProofContains -Text $pluginValidationOutput -ExpectedText 'target_framework:'", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("Assert-ProofContains -Text $pluginValidationOutput -ExpectedText 'sha256:'", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK:True", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK:True", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_TRUST_HASH_OK:True", templateSmokeScript, StringComparison.Ordinal);
        Assert.Contains("template-smoke.txt", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("TEMPLATE_SMOKE_PLUGIN_CAPABILITY_SUMMARY_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("public-api-surface.txt", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SURFACE_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("V061_MILESTONE_PROOF_OK", prereleaseNotesScript, StringComparison.Ordinal);
        Assert.Contains("API adoption proof", prereleaseNotesScript, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicApiSurfaceValidation_PassesCurrentRepoAndRejectsMismatchedBaseline()
    {
        var scriptPath = Path.Combine(GetRepositoryRoot(), "eng", "validate-public-api-surface.ps1");

        var passProcess = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" " +
            $"-RepoRoot \"{GetRepositoryRoot()}\" " +
            "-Configuration Release " +
            "-Framework net9.0");

        Assert.True(
            passProcess.ExitCode == 0,
            $"validate-public-api-surface.ps1 failed for current repo with exit code {passProcess.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{passProcess.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{passProcess.StandardError}");
        Assert.Contains("PUBLIC_API_SURFACE_OK:", passProcess.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia", passProcess.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_GUIDANCE_OK:True", passProcess.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", passProcess.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", passProcess.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", passProcess.StandardOutput, StringComparison.Ordinal);

        var tempRoot = CreateTempDirectory();
        var baselinePath = Path.Combine(tempRoot, "public-api-baseline.txt");
        File.WriteAllText(baselinePath, "A:Baseline.Intentionally.Mismatched");

        var failProcess = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" " +
            $"-RepoRoot \"{GetRepositoryRoot()}\" " +
            $"-BaselinePath \"{baselinePath}\" " +
            "-Configuration Release " +
            "-Framework net9.0");

        Assert.NotEqual(0, failProcess.ExitCode);
        Assert.Contains("Public API surface drift detected", failProcess.StandardError, StringComparison.Ordinal);
        Assert.Contains("docs/en/public-api-inventory.md", failProcess.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicApiInventory_DefinesBaselineScopeAndRerunCommands()
    {
        var englishInventory = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseInventory = ReadRepoFile("docs/zh-CN/public-api-inventory.md");

        foreach (var contents in new[] { englishInventory, chineseInventory })
        {
            Assert.Contains("AsterGraph.Abstractions", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraph.Core", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraph.Editor", contents, StringComparison.Ordinal);
            Assert.Contains("AsterGraph.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("validate-public-api-surface.ps1", contents, StringComparison.Ordinal);
            Assert.Contains("-Framework net9.0", contents, StringComparison.Ordinal);
            Assert.Contains("-UpdateBaseline", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_SCOPE_OK:AsterGraph.Abstractions,AsterGraph.Core,AsterGraph.Editor,AsterGraph.Avalonia", contents, StringComparison.Ordinal);
            Assert.Contains("API_RELEASE_CANDIDATE_PROOF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_GUIDANCE_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("RELEASE_BOUNDARY_STABILITY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V061_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PublicApiGuidanceHandoff_StaysVisibleInStatusChecklistAndInventory()
    {
        var englishInventory = ReadRepoFile("docs/en/public-api-inventory.md");
        var chineseInventory = ReadRepoFile("docs/zh-CN/public-api-inventory.md");
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        Assert.True(HasLineWithAll(englishInventory, "Release handoff", "stable canonical", "retained migration", "compatibility-only", "obsolete"));
        Assert.True(HasLineWithAll(chineseInventory, "release handoff", "Stable canonical", "Retained migration", "Compatibility-only", "obsolete"));
        Assert.True(HasLineWithAll(englishInventory, "PUBLIC_API_SURFACE_OK", "PUBLIC_API_SCOPE_OK", "PUBLIC_API_GUIDANCE_OK", "ASTERGRAPH_TEMPLATE_SMOKE_OK", "TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK"));
        Assert.True(HasLineWithAll(chineseInventory, "PUBLIC_API_SURFACE_OK", "PUBLIC_API_SCOPE_OK", "PUBLIC_API_GUIDANCE_OK", "ASTERGRAPH_TEMPLATE_SMOKE_OK", "TEMPLATE_SMOKE_PLUGIN_VALIDATE_OK"));
        Assert.True(HasLineWithAll(englishInventory, "API_RELEASE_CANDIDATE_PROOF_OK:True", "PUBLIC_API_GUIDANCE_HANDOFF_OK:True", "RELEASE_BOUNDARY_STABILITY_OK:True"));
        Assert.True(HasLineWithAll(chineseInventory, "API_RELEASE_CANDIDATE_PROOF_OK:True", "PUBLIC_API_GUIDANCE_HANDOFF_OK:True", "RELEASE_BOUNDARY_STABILITY_OK:True"));
        Assert.True(HasLineWithAll(englishInventory, "PUBLIC_API_DIFF_GATE_OK:True", "PUBLIC_API_USAGE_GUIDANCE_OK:True", "PUBLIC_API_STABILITY_SCOPE_OK:True"));
        Assert.True(HasLineWithAll(chineseInventory, "PUBLIC_API_DIFF_GATE_OK:True", "PUBLIC_API_USAGE_GUIDANCE_OK:True", "PUBLIC_API_STABILITY_SCOPE_OK:True"));
        Assert.True(HasLineWithAll(englishInventory, "ADOPTION_API_STABILIZATION_HANDOFF_OK:True", "ADOPTION_API_SCOPE_BOUNDARY_OK:True", "V061_MILESTONE_PROOF_OK:True"));
        Assert.True(HasLineWithAll(chineseInventory, "ADOPTION_API_STABILIZATION_HANDOFF_OK:True", "ADOPTION_API_SCOPE_BOUNDARY_OK:True", "V061_MILESTONE_PROOF_OK:True"));

        Assert.True(HasLineWithAll(englishChecklist, "public API guidance proof", "PUBLIC_API_SCOPE_OK", "template/plugin proof"));
        Assert.True(HasLineWithAll(chineseChecklist, "public API guidance proof", "PUBLIC_API_SCOPE_OK", "template/plugin proof"));
        Assert.True(HasLineWithAll(englishStatus, "public API guidance proof", "PUBLIC_API_SURFACE_OK", "PUBLIC_API_SCOPE_OK", "PUBLIC_API_GUIDANCE_OK"));
        Assert.True(HasLineWithAll(chineseStatus, "public API guidance proof", "PUBLIC_API_SURFACE_OK", "PUBLIC_API_SCOPE_OK", "PUBLIC_API_GUIDANCE_OK"));
        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("API_RELEASE_CANDIDATE_PROOF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_GUIDANCE_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("RELEASE_BOUNDARY_STABILITY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_DIFF_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_USAGE_GUIDANCE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PUBLIC_API_STABILITY_SCOPE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V061_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void WpfAndGaPrepDecisionGate_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var englishAdoptionFeedback = ReadRepoFile("docs/en/adoption-feedback.md");
        var chineseAdoptionFeedback = ReadRepoFile("docs/zh-CN/adoption-feedback.md");

        Assert.True(HasLineWithAll(englishStatus, "WPF support expansion", "validation-only", "public WPF support", "3-5 real external reports"));
        Assert.True(HasLineWithAll(chineseStatus, "WPF support expansion", "validation-only", "public WPF support", "3-5 real external reports"));
        Assert.True(HasLineWithAll(englishChecklist, "WPF support expansion", "validation-only", "public WPF support", "3-5 real external reports"));
        Assert.True(HasLineWithAll(chineseChecklist, "WPF support expansion", "validation-only", "public WPF support", "3-5 real external reports"));

        Assert.True(HasLineWithAll(englishStatus, "GA prep checklist", "adoption evidence", "API drift", "support boundary", "release proof"));
        Assert.True(HasLineWithAll(chineseStatus, "GA prep checklist", "adoption evidence", "API drift", "support boundary", "release proof"));
        Assert.True(HasLineWithAll(englishChecklist, "GA prep checklist", "adoption evidence", "API drift", "support boundary", "release proof"));
        Assert.True(HasLineWithAll(chineseChecklist, "GA prep checklist", "adoption evidence", "API drift", "support boundary", "release proof"));

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishAdoptionFeedback, chineseAdoptionFeedback })
        {
            Assert.Contains("ADOPTION_READINESS_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V056_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("RELEASE_READINESS_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("SUPPORT_BOUNDARY_GATE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("BETA_CLAIM_ALIGNMENT_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_STABILIZATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADOPTION_API_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V061_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Adapter2ValidationScopeBaseline_StaysVisibleInStatusChecklistAndMatrix()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var englishMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var chineseMatrix = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishMatrix, chineseMatrix })
        {
            Assert.Contains("ADAPTER2_VALIDATION_SCOPE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_MATRIX_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "adapter-2 validation scope proof", "canonical route", "matrix vocabulary", "public WPF support", "parity claims"));
        Assert.True(HasLineWithAll(chineseStatus, "adapter-2 validation scope proof", "canonical route", "matrix vocabulary", "public WPF support", "parity claims"));
        Assert.True(HasLineWithAll(englishMatrix, "v0.60 adapter-2 validation scope baseline", "matrix handoff", "Partial", "Fallback", "Supported"));
        Assert.True(HasLineWithAll(chineseMatrix, "v0.60 adapter-2 validation scope baseline", "matrix handoff", "Partial", "Fallback", "Supported"));
    }

    [Fact]
    public void WpfProofSampleEvidence_StaysVisibleInStatusChecklistAndMatrix()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var englishMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var chineseMatrix = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishMatrix, chineseMatrix })
        {
            Assert.Contains("ADAPTER2_WPF_SAMPLE_PROOF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_CANONICAL_ROUTE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_SAMPLE_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "WPF proof sample evidence", "AsterGraph.HelloWorld.Wpf", "canonical session/runtime route", "second onboarding path"));
        Assert.True(HasLineWithAll(chineseStatus, "WPF proof sample evidence", "AsterGraph.HelloWorld.Wpf", "canonical session/runtime route", "second onboarding path"));
        Assert.True(HasLineWithAll(englishMatrix, "WPF proof sample evidence", "AsterGraph.HelloWorld.Wpf", "canonical route", "second onboarding path"));
        Assert.True(HasLineWithAll(chineseMatrix, "WPF proof sample evidence", "AsterGraph.HelloWorld.Wpf", "canonical route", "second onboarding path"));
    }

    [Fact]
    public void Adapter2PerformanceAccessibilityHandoff_StaysVisibleInStatusChecklistMatrixAndRecipes()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var englishMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var chineseMatrix = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");
        var englishAccessibilityRecipe = ReadRepoFile("docs/en/adapter-2-accessibility-recipe.md");
        var chineseAccessibilityRecipe = ReadRepoFile("docs/zh-CN/adapter-2-accessibility-recipe.md");
        var englishPerformanceRecipe = ReadRepoFile("docs/en/adapter-2-performance-recipe.md");
        var chinesePerformanceRecipe = ReadRepoFile("docs/zh-CN/adapter-2-performance-recipe.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishMatrix, chineseMatrix, englishAccessibilityRecipe, chineseAccessibilityRecipe, englishPerformanceRecipe, chinesePerformanceRecipe })
        {
            Assert.Contains("ADAPTER2_PERFORMANCE_ACCESSIBILITY_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_RECIPE_ALIGNMENT_OK:True", contents, StringComparison.Ordinal);
        }

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishMatrix, chineseMatrix, englishPerformanceRecipe, chinesePerformanceRecipe })
        {
            Assert.Contains("ADAPTER2_PROOF_BUDGET_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "adapter-2 performance/accessibility handoff proof", "WPF proof budgets", "recipes", "without widening WPF support"));
        Assert.True(HasLineWithAll(chineseStatus, "adapter-2 performance/accessibility handoff proof", "WPF proof budgets", "recipes", "不扩大 WPF support"));
        Assert.True(HasLineWithAll(englishMatrix, "Adapter-2 performance/accessibility handoff", "proof budgets", "without widening WPF support", "Partial", "Fallback"));
        Assert.True(HasLineWithAll(chineseMatrix, "Adapter-2 performance/accessibility handoff", "proof budgets", "不扩大 WPF support", "Partial", "Fallback"));
    }

    [Fact]
    public void Adapter2ValidationHandoff_StaysVisibleInStatusChecklistMatrixAndSupportBundle()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");
        var englishMatrix = ReadRepoFile("docs/en/adapter-capability-matrix.md");
        var chineseMatrix = ReadRepoFile("docs/zh-CN/adapter-capability-matrix.md");
        var englishSupportBundle = ReadRepoFile("docs/en/support-bundle.md");
        var chineseSupportBundle = ReadRepoFile("docs/zh-CN/support-bundle.md");
        var englishFeatureCatalog = ReadRepoFile("docs/en/feature-catalog.md");
        var chineseFeatureCatalog = ReadRepoFile("docs/zh-CN/feature-catalog.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus, englishMatrix, chineseMatrix, englishSupportBundle, chineseSupportBundle, englishFeatureCatalog, chineseFeatureCatalog })
        {
            Assert.Contains("ADAPTER2_VALIDATION_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("ADAPTER2_VALIDATION_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V060_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "v0.60 adapter-2 validation handoff proof", "validation-only milestone", "WPF support boundary"));
        Assert.True(HasLineWithAll(chineseStatus, "v0.60 adapter-2 validation handoff proof", "validation-only milestone", "WPF support boundary"));
        Assert.True(HasLineWithAll(englishMatrix, "v0.60 adapter-2 validation handoff", "validation-only milestone", "WPF support", "runtime API"));
        Assert.True(HasLineWithAll(chineseMatrix, "v0.60 adapter-2 validation handoff", "validation-only milestone", "WPF support", "runtime API"));
        Assert.True(HasLineWithAll(englishSupportBundle, "Adapter-2 validation handoff proof", "validation-only WPF evidence", "hosted support boundary"));
        Assert.True(HasLineWithAll(chineseSupportBundle, "Adapter-2 validation handoff proof", "validation-only WPF evidence", "hosted support boundary"));
    }

    [Fact]
    public void AuthoringDepthHandoff_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("AUTHORING_DEPTH_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("AUTHORING_DEPTH_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V058_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "authoring-depth proof", "v0.58", "without new runtime model claims"));
        Assert.True(HasLineWithAll(chineseStatus, "authoring-depth proof", "v0.58", "runtime model"));
    }

    [Fact]
    public void LargeGraphUxBaseline_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("LARGE_GRAPH_UX_POLICY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LARGE_GRAPH_UX_PROOF_BASELINE_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "large-graph UX baseline proof", "hosted performance mode", "LOD policy", "without creating a new graph-size support tier"));
        Assert.True(HasLineWithAll(chineseStatus, "large-graph UX baseline proof", "hosted performance mode", "LOD policy"));
    }

    [Fact]
    public void ViewportLodPolicy_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("VIEWPORT_LOD_POLICY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("SELECTED_HOVERED_ADORNER_SCOPE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LARGE_GRAPH_BALANCED_UX_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("VIEWPORT_LOD_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "viewport LOD proof", "selected/hovered affordances", "hosted workbench policy", "runtime graph contract"));
        Assert.True(HasLineWithAll(chineseStatus, "viewport LOD proof", "selected/hovered affordances", "hosted workbench policy", "runtime graph contract"));
    }

    [Fact]
    public void EdgeInteractionRendering_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("EDGE_INTERACTION_CACHE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("EDGE_DRAG_ROUTE_SIMPLIFICATION_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("SELECTED_EDGE_FEEDBACK_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("EDGE_RENDERING_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("NODE_DRAG_EDGE_REFRESH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("EDGE_ROUTE_REFRESH_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LIVE_CANVAS_REFRESH_AUDIT_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "edge interaction proof", "existing edge quick-tool", "hosted proof route", "runtime renderer contract"));
        Assert.True(HasLineWithAll(chineseStatus, "edge interaction proof", "edge quick-tool", "hosted proof route", "runtime renderer contract"));
    }

    [Fact]
    public void PanelProjectionEvidence_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("MINIMAP_LIGHTWEIGHT_PROJECTION_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("INSPECTOR_NARROW_PROJECTION_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LARGE_GRAPH_PANEL_SCOPE_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("PROJECTION_PERFORMANCE_EVIDENCE_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "panel projection proof", "mini-map lightweight projection", "inspector narrow projection", "broad graph subscription contract"));
        Assert.True(HasLineWithAll(chineseStatus, "panel projection proof", "mini-map lightweight projection", "inspector narrow projection", "broad graph subscription contract"));
    }

    [Fact]
    public void LargeGraphUxHandoff_StaysVisibleInStatusAndChecklist()
    {
        var englishChecklist = ReadRepoFile("docs/en/public-launch-checklist.md");
        var chineseChecklist = ReadRepoFile("docs/zh-CN/public-launch-checklist.md");
        var englishStatus = ReadRepoFile("docs/en/project-status.md");
        var chineseStatus = ReadRepoFile("docs/zh-CN/project-status.md");

        foreach (var contents in new[] { englishChecklist, chineseChecklist, englishStatus, chineseStatus })
        {
            Assert.Contains("LARGE_GRAPH_UX_HANDOFF_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("LARGE_GRAPH_UX_SCOPE_BOUNDARY_OK:True", contents, StringComparison.Ordinal);
            Assert.Contains("V059_MILESTONE_PROOF_OK:True", contents, StringComparison.Ordinal);
        }

        Assert.True(HasLineWithAll(englishStatus, "v0.59 Large Graph UX handoff proof", "phases 371-374", "existing hosted workbench evidence", "graph-size support claims"));
        Assert.True(HasLineWithAll(chineseStatus, "v0.59 Large Graph UX handoff proof", "phases 371-374", "hosted workbench", "graph-size support claims"));
    }

    [Fact]
    public void ReleaseCoverageValidation_BoundsHungTestCollectors()
    {
        var ciScript = ReadRepoFile("eng/ci.ps1");

        Assert.Contains("'--blame-hang-timeout'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'5m'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'--blame-hang-dump-type'", ciScript, StringComparison.Ordinal);
        Assert.Contains("'mini'", ciScript, StringComparison.Ordinal);
    }

    [Fact]
    public void PublicVersioningValidation_PassesCurrentRepoAndRejectsMismatchedDocs()
    {
        var packageVersion = GetPackageVersion();
        var publicTag = $"v{packageVersion}";
        var scriptPath = Path.Combine(GetRepositoryRoot(), "eng", "validate-public-versioning.ps1");

        var passProcess = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" " +
            $"-RepoRoot \"{GetRepositoryRoot()}\" " +
            $"-PublicTag \"{publicTag}\"");

        Assert.True(
            passProcess.ExitCode == 0,
            $"validate-public-versioning.ps1 failed for current repo with exit code {passProcess.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{passProcess.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{passProcess.StandardError}");
        Assert.Contains($"PUBLIC_VERSIONING_OK:{packageVersion}:{publicTag}", passProcess.StandardOutput, StringComparison.Ordinal);

        var tempRepo = CreateTempDirectory();
        WriteMinimalVersioningRepo(tempRepo, packageVersion, publicTag, readmePackageVersion: "9.9.9-beta");

        var failProcess = RunPowerShell(
            $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" " +
            $"-RepoRoot \"{tempRepo}\" " +
            $"-PublicTag \"{publicTag}\"");

        Assert.NotEqual(0, failProcess.ExitCode);
        Assert.Contains("README.md", failProcess.StandardError, StringComparison.Ordinal);
        Assert.Contains(packageVersion, failProcess.StandardError, StringComparison.Ordinal);
    }

    [Fact]
    public void WpfAdapterMatrixProof_MapsPositiveMarkersToPassNotMissing()
    {
        var tempRoot = CreateTempDirectory();
        var proofPath = Path.Combine(tempRoot, "hello-world-wpf-proof.txt");
        var outputPath = Path.Combine(tempRoot, "wpf-adapter-capability-matrix.txt");
        File.WriteAllText(proofPath, "HELLOWORLD_WPF_OK:True`nCOMMAND_SURFACE_OK:True`nADAPTER2_MATRIX_HANDOFF_OK:True".Replace("`n", Environment.NewLine, StringComparison.Ordinal));

        var script = ReadRepoFile("eng/ci.ps1");
        var helperBlock = ExtractWpfMatrixHelperBlock(script);
        var helperScriptPath = Path.Combine(tempRoot, "invoke-wpf-matrix-proof.ps1");
        File.WriteAllText(
            helperScriptPath,
            helperBlock +
            Environment.NewLine +
            "$proofArtifactsRoot = Split-Path -Parent '" + EscapePowerShellSingleQuote(outputPath) + "'" +
            Environment.NewLine +
            $"New-WpfAdapterCapabilityMatrixProof -HelloWorldProofPath '{EscapePowerShellSingleQuote(proofPath)}' -OutputPath '{EscapePowerShellSingleQuote(outputPath)}'");

        var process = RunPowerShell($"-NoProfile -ExecutionPolicy Bypass -File \"{helperScriptPath}\"");

        Assert.True(
            process.ExitCode == 0,
            $"WPF matrix proof helper failed with exit code {process.ExitCode}.{Environment.NewLine}STDOUT:{Environment.NewLine}{process.StandardOutput}{Environment.NewLine}STDERR:{Environment.NewLine}{process.StandardError}");

        var matrix = File.ReadAllText(outputPath);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX_FORMAT:1", matrix, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:PASS", matrix, StringComparison.Ordinal);
        Assert.Contains("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:PASS", matrix, StringComparison.Ordinal);
        Assert.Contains("ADAPTER2_MATRIX_HANDOFF_OK:True", matrix, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:HELLOWORLD_WPF_OK:MISSING", matrix, StringComparison.Ordinal);
        Assert.DoesNotContain("ADAPTER_CAPABILITY_MATRIX:WPF:COMMAND_SURFACE_OK:MISSING", matrix, StringComparison.Ordinal);
    }

    private static string ReadRepoFile(string relativePath)
        => File.ReadAllText(Path.Combine(GetRepositoryRoot(), relativePath));

    private static string GetPackageVersion()
    {
        var props = XDocument.Load(Path.Combine(GetRepositoryRoot(), "Directory.Build.props"));
        var version = props.Root?
            .Elements("PropertyGroup")
            .Elements("Version")
            .Select(node => node.Value)
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(version))
        {
            throw new InvalidOperationException("Could not read package version from Directory.Build.props.");
        }

        return version.Trim();
    }

    private static string GetRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Directory.Build.props")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Failed to locate repository root from test base directory.");
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "AsterGraph.Demo.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private static string ExtractWpfMatrixHelperBlock(string script)
    {
        var start = script.IndexOf("function Convert-TextToCapabilityStatus", StringComparison.Ordinal);
        var end = script.IndexOf("function Reset-Directory", StringComparison.Ordinal);

        Assert.True(start >= 0 && end > start, "The ci.ps1 helper block could not be located.");

        return script.Substring(start, end - start).TrimEnd();
    }

    private static string EscapePowerShellSingleQuote(string value)
        => value.Replace("'", "''", StringComparison.Ordinal);

    private static bool HasLineWithAll(string contents, params string[] requiredTerms)
    {
        return contents
            .Split('\n', StringSplitOptions.TrimEntries)
            .Any(line => requiredTerms.All(term => line.Contains(term, StringComparison.OrdinalIgnoreCase)));
    }

    private static void WriteProofFile(string proofRoot, string fileName, string contents)
        => File.WriteAllText(Path.Combine(proofRoot, fileName), contents.Replace("`n", Environment.NewLine, StringComparison.Ordinal));

    private static void WriteMinimalVersioningRepo(
        string repoRoot,
        string packageVersion,
        string publicTag,
        string readmePackageVersion)
    {
        WriteRepoFile(
            repoRoot,
            "Directory.Build.props",
            $"""
            <Project>
              <PropertyGroup>
                <Version>{packageVersion}</Version>
              </PropertyGroup>
            </Project>
            """);

        WriteRepoFile(
            repoRoot,
            "README.md",
            $"""
            - current installable package version: `{readmePackageVersion}`
            - matching public prerelease tag for this package line: `{publicTag}`
            - GitHub prerelease/Release entries must use the same SemVer as the NuGet packages; local planning milestones are not public release identifiers
            - package version versus historical repository-tag guidance: [Versioning](./docs/en/versioning.md)
            """);

        WriteRepoFile(
            repoRoot,
            "README.zh-CN.md",
            $"""
            - 当前可安装包版本：`{packageVersion}`
            - 与当前包版本配对的对外 SemVer prerelease 标签：`{publicTag}`
            - GitHub prerelease/Release 条目必须使用与 NuGet 包相同的 SemVer；本地规划里程碑不是公开发布标识
            - 包版本与历史仓库 tag 的关系说明：[Versioning](./docs/zh-CN/versioning.md)
            """);

        WriteRepoFile(
            repoRoot,
            "docs/en/versioning.md",
            $"""
            - package version: `{packageVersion}`
            - public tag: `{publicTag}`
            GitHub Releases and GitHub prereleases are consumer-facing release records.
            local planning-only milestone labels are private maintainer bookkeeping, not release identifiers.
            """);

        WriteRepoFile(
            repoRoot,
            "docs/zh-CN/versioning.md",
            $"""
            - 包版本：`{packageVersion}`
            - 公开 tag：`{publicTag}`
            GitHub Release 和 GitHub prerelease 是面向使用者的发布记录。
            本地规划专用里程碑标签只是维护者内部记账，不是发布标识。
            """);

        WriteRepoFile(
            repoRoot,
            ".github/ISSUE_TEMPLATE/config.yml",
            "url: https://github.com/ExplodingUFO/AsterGraph/blob/master/docs/en/versioning.md");
        WriteRepoFile(repoRoot, "docs/en/public-launch-checklist.md", "- [Versioning](./versioning.md)");
        WriteRepoFile(repoRoot, "docs/zh-CN/public-launch-checklist.md", "- [Versioning](./versioning.md)");
    }

    private static void WriteRepoFile(string repoRoot, string relativePath, string contents)
    {
        var path = Path.Combine(repoRoot, relativePath);
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, contents);
    }

    private static ProcessResult RunPowerShell(string arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "pwsh",
            Arguments = arguments,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(startInfo);
        Assert.NotNull(process);
        process!.WaitForExit();

        return new ProcessResult(
            process.ExitCode,
            process.StandardOutput.ReadToEnd(),
            process.StandardError.ReadToEnd());
    }

    private static void AssertMatrixRowStatus(string contents, string rowName, string expectedAvaloniaStatus, string expectedWpfStatus)
    {
        const string matrixHeader = "## Phase 157";
        var matrixStart = contents.IndexOf(matrixHeader, StringComparison.Ordinal);
        Assert.True(matrixStart >= 0, "The adapter capability matrix must include the Phase 157 Matrix section.");

        var matrixContents = contents[(matrixStart + matrixHeader.Length)..];
        var row = matrixContents
            .Split('\n', StringSplitOptions.TrimEntries)
            .FirstOrDefault(line => line.StartsWith($"| {rowName} |", StringComparison.Ordinal));

        Assert.NotNull(row);

        var columns = row!
            .Split('|', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();

        Assert.Equal(rowName, columns[0]);
        Assert.Equal(expectedAvaloniaStatus, columns[1].Trim('`'));
        Assert.Equal(expectedWpfStatus, columns[2].Trim('`'));
        Assert.False(string.Equals(columns[1], "MISSING", StringComparison.OrdinalIgnoreCase));
        Assert.False(string.Equals(columns[2], "MISSING", StringComparison.OrdinalIgnoreCase));
    }

    private sealed record ProcessResult(int ExitCode, string StandardOutput, string StandardError);
}
